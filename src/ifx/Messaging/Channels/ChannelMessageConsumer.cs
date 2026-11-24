using System.Threading.Channels;
using vc.Ifx.Messaging;

namespace vc.Ifx.Messaging.Channels;

public sealed class ChannelMessageConsumer<TMessage> : IMessageConsumer, IDisposable
    where TMessage : IMessage
{
    private readonly Channel<IMessage> channel;
    private readonly IMessageHandler<TMessage> handler;
    private readonly Action unsubscribe;
    private readonly CancellationTokenSource cancellation = new();
    private readonly CancellationToken consumerToken;
    private readonly object stateLock = new();
    private Task? consuming;

    public ChannelMessageConsumer(Channel<IMessage> channel, IMessageHandler<TMessage> handler, Action unsubscribe)
    {
        this.channel = channel ?? throw new ArgumentNullException(nameof(channel));
        this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
        this.unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
        consumerToken = cancellation.Token;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (stateLock)
        {
            consuming ??= Task.Run(() => ConsumeAsync(consumerToken), consumerToken);
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
        catch (OperationCanceledException exception) when (
            consumingTask.IsCanceled && cancellation.IsCancellationRequested && exception.CancellationToken == consumerToken)
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
