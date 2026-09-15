using System.Threading.Channels;
using Ifx.Messaging.Abstractions;

namespace Util.Messaging.Channels;

public sealed class ChannelMessageConsumer<TMessage>(
    Channel<IMessage> channel,
    IMessageHandler<TMessage> handler,
    Action unsubscribe) : IMessageConsumer, IDisposable
    where TMessage : IMessage
{
    private readonly CancellationTokenSource cancellation = new();
    private Task? consuming;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (consuming is not null)
        {
            return Task.CompletedTask;
        }

        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(
            cancellation.Token,
            cancellationToken);

        consuming = Task.Run(() => ConsumeAsync(cancellation.Token), linked.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (consuming is null)
        {
            return;
        }

        await cancellation.CancelAsync();

        try
        {
            await consuming.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
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
