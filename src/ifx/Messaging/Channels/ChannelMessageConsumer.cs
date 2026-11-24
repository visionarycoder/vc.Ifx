using System.Threading.Channels;
using vc.Ifx.Messaging;

namespace vc.Ifx.Messaging.Channels;

public sealed class ChannelMessageConsumer<TMessage>(
    Channel<IMessage> channel,
    IMessageHandler<TMessage> handler,
    Action unsubscribe) : IMessageConsumer, IDisposable
    where TMessage : IMessage
{
    private readonly CancellationTokenSource cancellation = new();
    private readonly object stateLock = new();
    private Task? consuming;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (stateLock)
        {
            consuming ??= Task.Run(() => ConsumeAsync(cancellation.Token), cancellation.Token);
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Task? consumingTask;
        lock (stateLock)
        {
            consumingTask = consuming;
        }

        if (consumingTask is null) return;

        await cancellation.CancelAsync();

        try
        {
            await consumingTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (consumingTask.IsCanceled)
        {
            // The consumer stopped because this instance requested cancellation.
        }
    }

    public void Dispose()
    {
        unsubscribe();
        cancellation.Cancel();
        cancellation.Dispose();
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        await foreach (IMessage message in channel.Reader.ReadAllAsync(cancellationToken))
        {
            if (message is TMessage typedMessage)
            {
                await handler.HandleAsync(typedMessage, cancellationToken);
            }
        }
    }
}
