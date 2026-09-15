using Microsoft.Extensions.Hosting;
using Ifx.Messaging.Abstractions;

namespace Util.Messaging.Channels;

internal sealed class ChannelMessageHandlerRegistration<THandler, TMessage>(
    ChannelMessageBus bus,
    THandler handler,
    string subscriptionName) : IHostedService
    where THandler : IMessageHandler<TMessage>
    where TMessage : IMessage
{
    private IMessageConsumer? consumer;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        consumer = bus.CreateConsumer(subscriptionName, handler);
        await consumer.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (consumer is not null)
        {
            await consumer.StopAsync(cancellationToken);
        }

        if (consumer is IDisposable disposable)
        {
            disposable.Dispose();
        }

        consumer = null;
    }
}
