using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Util.Messaging.RabbitMQ;

/// <summary>
/// Consumes RabbitMQ deliveries for a specific message type.
/// </summary>
/// <typeparam name="TMessage">The message type handled by this consumer.</typeparam>
public sealed class RabbitMqMessageConsumer<TMessage> : IMessageConsumer, IDisposable
    where TMessage : IMessage
{
    private readonly IRabbitMqChannelFactory channelFactory;
    private readonly RabbitMqOptions options;
    private readonly IMessageHandler<TMessage> handler;
    private readonly string subscriptionName;
    private readonly SemaphoreSlim stateLock = new(1, 1);
    private IRabbitMqChannel? channel;
    private string? consumerTag;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqMessageConsumer{TMessage}"/> class.
    /// </summary>
    /// <param name="channelFactory">The channel factory.</param>
    /// <param name="options">The configured RabbitMQ options snapshot.</param>
    /// <param name="handler">The typed handler.</param>
    /// <param name="subscriptionName">The logical subscription name.</param>
    public RabbitMqMessageConsumer(
        RabbitMqChannelFactory channelFactory,
        IOptions<RabbitMqOptions> options,
        IMessageHandler<TMessage> handler,
        string subscriptionName)
        : this((IRabbitMqChannelFactory)channelFactory, GetOptions(options), handler, subscriptionName)
    {
    }

    internal RabbitMqMessageConsumer(
        IRabbitMqChannelFactory channelFactory,
        RabbitMqOptions options,
        IMessageHandler<TMessage> handler,
        string subscriptionName)
    {
        ArgumentNullException.ThrowIfNull(channelFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);

        this.channelFactory = channelFactory;
        this.options = options.Validate();
        this.handler = handler;
        this.subscriptionName = subscriptionName;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await stateLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (channel is not null)
            {
                return;
            }

            IRabbitMqChannel createdChannel = await channelFactory.CreateChannelAsync(cancellationToken).ConfigureAwait(false);
            RabbitMqTopology topology = RabbitMqTopology.Create<TMessage>(options, subscriptionName);

            try
            {
                await RetryPolicy.ExecuteAsync(
                    token => createdChannel.DeclareConsumerTopologyAsync(topology, options, token),
                    options.MaxRetryAttempts,
                    options.InitialRetryDelay,
                    cancellationToken).ConfigureAwait(false);

                string? createdConsumerTag = null;

                await RetryPolicy.ExecuteAsync(
                    async token =>
                    {
                        createdConsumerTag = await createdChannel.StartConsumerAsync(
                            topology.QueueName,
                            (delivery, callbackToken) => HandleDeliveryAsync(createdChannel, delivery, callbackToken),
                            token).ConfigureAwait(false);
                    },
                    options.MaxRetryAttempts,
                    options.InitialRetryDelay,
                    cancellationToken).ConfigureAwait(false);

                channel = createdChannel;
                consumerTag = createdConsumerTag;
            }
            catch
            {
                await createdChannel.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }
        finally
        {
            stateLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        IRabbitMqChannel? channelToDispose;
        string? consumerTagToCancel;

        await stateLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            channelToDispose = channel;
            consumerTagToCancel = consumerTag;
            channel = null;
            consumerTag = null;
        }
        finally
        {
            stateLock.Release();
        }

        if (channelToDispose is null)
        {
            return;
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(consumerTagToCancel))
            {
                await RetryPolicy.ExecuteAsync(
                    token => channelToDispose.CancelConsumerAsync(consumerTagToCancel, token),
                    options.MaxRetryAttempts,
                    options.InitialRetryDelay,
                    cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            await channelToDispose.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        channel?.Dispose();
        stateLock.Dispose();
    }

    private static RabbitMqOptions GetOptions(IOptions<RabbitMqOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Value.Validate();
    }

    private async Task HandleDeliveryAsync(
        IRabbitMqChannel activeChannel,
        RabbitMqDelivery delivery,
        CancellationToken cancellationToken)
    {
        RabbitMqEnvelope<TMessage>? envelope;

        try
        {
            envelope = RabbitMqEnvelopeSerializer.Deserialize<TMessage>(delivery.Body);
        }
        catch
        {
            await activeChannel.RejectAsync(delivery.DeliveryTag, requeue: false).ConfigureAwait(false);
            return;
        }

        if (envelope is null || envelope.Message is not TMessage message)
        {
            await activeChannel.RejectAsync(delivery.DeliveryTag, requeue: false).ConfigureAwait(false);
            return;
        }

        try
        {
            await RetryPolicy.ExecuteAsync(
                token => handler.HandleAsync(message, token),
                options.MaxRetryAttempts,
                options.InitialRetryDelay,
                cancellationToken).ConfigureAwait(false);

            await activeChannel.AcknowledgeAsync(delivery.DeliveryTag).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await activeChannel.NegativeAcknowledgeAsync(delivery.DeliveryTag, requeue: true).ConfigureAwait(false);
        }
        catch
        {
            await activeChannel.RejectAsync(delivery.DeliveryTag, requeue: false).ConfigureAwait(false);
        }
    }
}
