using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using vc.Ifx.Messaging;

namespace vc.Ifx.Messaging.Kafka;

/// <summary>Consumes Kafka records and commits offsets only after successful handling.</summary>
public sealed class KafkaMessageConsumer<TMessage> : IMessageConsumer, IAsyncDisposable
    where TMessage : IMessage
{
    private static readonly TimeSpan pollInterval = TimeSpan.FromMilliseconds(200);
    private readonly KafkaOptions options;
    private readonly Func<ConsumerConfig, IConsumer<Ignore, byte[]>> consumerFactory;
    private readonly IMessageHandler<TMessage> handler;
    private readonly string subscriptionName;
    private readonly ILogger<KafkaMessageBus> logger;
    private readonly object sync = new();
    private CancellationTokenSource? activeCancellation;
    private Task? worker;

    internal KafkaMessageConsumer(
        KafkaOptions options,
        Func<ConsumerConfig, IConsumer<Ignore, byte[]>> consumerFactory,
        IMessageHandler<TMessage> handler,
        string subscriptionName,
        ILogger<KafkaMessageBus> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(consumerFactory);
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);

        this.options = options.Validate();
        this.consumerFactory = consumerFactory;
        this.handler = handler;
        this.subscriptionName = subscriptionName;
        this.logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            if (worker is not null)
            {
                return Task.CompletedTask;
            }

            activeCancellation = new CancellationTokenSource();
            CancellationToken token = activeCancellation.Token;
            worker = Task.Run(() => ConsumeAsync(token), CancellationToken.None);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        CancellationTokenSource? cancellation;
        Task? activeWorker;
        lock (sync)
        {
            cancellation = activeCancellation;
            activeWorker = worker;
            cancellation?.Cancel();
        }

        if (activeWorker is null)
        {
            return;
        }

        try
        {
            await activeWorker.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (activeWorker.IsCompleted)
            {
                lock (sync)
                {
                    if (ReferenceEquals(worker, activeWorker))
                    {
                        worker = null;
                        activeCancellation = null;
                    }
                }

                cancellation?.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await StopAsync().ConfigureAwait(false);

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        string topic = options.GetTopicName<TMessage>();
        IConsumer<Ignore, byte[]>? consumer = null;

        try
        {
            consumer = consumerFactory(options.CreateConsumerConfig(subscriptionName));
            consumer.Subscribe(topic);
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, byte[]>? result = consumer.Consume(pollInterval);
                if (result is null)
                {
                    continue;
                }

                cancellationToken.ThrowIfCancellationRequested();
                KafkaEnvelope<TMessage> envelope = KafkaEnvelope<TMessage>.Deserialize(result.Message.Value);
                if (envelope.Message is null)
                {
                    throw new InvalidDataException("The Kafka message envelope did not contain a message.");
                }

                await handler.HandleAsync(envelope.Message, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                consumer.Commit(result);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Kafka consumer failed for {MessageType} on {Topic}", typeof(TMessage).FullName, topic);
            throw;
        }
        finally
        {
            if (consumer is not null)
            {
                try
                {
                    consumer.Close();
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Kafka consumer could not close for {MessageType} on {Topic}", typeof(TMessage).FullName, topic);
                    throw;
                }
                finally
                {
                    consumer.Dispose();
                }
            }
        }
    }
}
