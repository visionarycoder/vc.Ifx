using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Util.Messaging.Msmq;

/// <summary>
/// Receives MSMQ messages for a single subscription and dispatches them to a handler.
/// </summary>
/// <typeparam name="TMessage">The message type to consume.</typeparam>
public sealed class MsmqMessageConsumer<TMessage> : IMessageConsumer, IDisposable
    where TMessage : IMessage
{
    private readonly CancellationTokenSource cancellation = new();
    private readonly IMsmqQueueClient queueClient;
    private readonly IOptions<MsmqOptions> options;
    private readonly IMessageHandler<TMessage> handler;
    private readonly string queuePath;
    private readonly Action unsubscribe;
    private Task? consuming;

    internal MsmqMessageConsumer(
        IMsmqQueueClient queueClient,
        IOptions<MsmqOptions> options,
        IMessageHandler<TMessage> handler,
        string queuePath,
        Action unsubscribe)
    {
        this.queueClient = queueClient;
        this.options = options;
        this.handler = handler;
        this.queuePath = queuePath;
        this.unsubscribe = unsubscribe;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (consuming is not null)
        {
            return Task.CompletedTask;
        }

        consuming = Task.Run(() => ConsumeAsync(cancellation.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (consuming is null)
        {
            return;
        }

        await cancellation.CancelAsync().ConfigureAwait(false);

        try
        {
            await consuming.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            consuming = null;
        }
    }

    /// <summary>
    /// Releases the consumer subscription and internal cancellation resources.
    /// </summary>
    public void Dispose()
    {
        unsubscribe();
        cancellation.Cancel();
        cancellation.Dispose();
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ReceivedMsmqMessage? receivedMessage = null;

            await RetryPolicy.ExecuteAsync(
                async token => receivedMessage = await queueClient.ReceiveAsync(queuePath, options.Value.ReceiveTimeout, token).ConfigureAwait(false),
                options.Value.MaxRetryAttempts,
                options.Value.InitialRetryDelay,
                cancellationToken).ConfigureAwait(false);

            if (receivedMessage is null)
            {
                continue;
            }

            MessageEnvelope envelope = MsmqEnvelopeSerializer.Deserialize(receivedMessage.Body);

            if (envelope.Message is not TMessage typedMessage)
            {
                throw new InvalidOperationException(
                    $"Received message type '{envelope.Message.GetType().FullName}' does not match consumer type '{typeof(TMessage).FullName}'.");
            }

            await handler.HandleAsync(typedMessage, cancellationToken).ConfigureAwait(false);
        }
    }
}
