using vc.Ifx.Messaging;
using vc.Ifx.Messaging.ProviderSupport;
using Microsoft.Extensions.Options;

namespace vc.Ifx.Messaging.Msmq;

/// <summary>
/// Receives MSMQ messages for a single subscription and dispatches them to a handler.
/// </summary>
/// <typeparam name="TMessage">The message type to consume.</typeparam>
public sealed class MsmqMessageConsumer<TMessage> : IMessageConsumer, IDisposable
    where TMessage : IMessage
{
    private CancellationTokenSource cancellation = new();
    private readonly object lifecycleLock = new();
    private bool disposed;
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

        lock (lifecycleLock)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            if (consuming is not null)
            {
                return Task.CompletedTask;
            }

            CancellationToken token = cancellation.Token;
            consuming = Task.Run(() => ConsumeAsync(token), CancellationToken.None);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Task? current;
        CancellationTokenSource currentCancellation;

        lock (lifecycleLock)
        {
            if (disposed || consuming is null)
            {
                return;
            }

            current = consuming;
            currentCancellation = cancellation;
        }

        await currentCancellation.CancelAsync().ConfigureAwait(false);

        try
        {
            await current.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (
            !cancellationToken.IsCancellationRequested &&
            currentCancellation.IsCancellationRequested &&
            current.IsCanceled)
        {
        }
        finally
        {
            if (current.IsCompleted)
            {
                lock (lifecycleLock)
                {
                    if (ReferenceEquals(consuming, current))
                    {
                        consuming = null;
                        currentCancellation.Dispose();

                        if (!disposed)
                        {
                            cancellation = new CancellationTokenSource();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Releases the consumer subscription and internal cancellation resources.
    /// </summary>
    public void Dispose()
    {
        Task? current;
        CancellationTokenSource currentCancellation;

        lock (lifecycleLock)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            current = consuming;
            currentCancellation = cancellation;
            currentCancellation.Cancel();
        }

        unsubscribe();

        if (current is null)
        {
            currentCancellation.Dispose();
        }
        else
        {
            _ = current.ContinueWith(
                _ => currentCancellation.Dispose(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
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
