using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using vc.Ifx.Messaging;

namespace vc.Ifx.Messaging.Channels;

internal sealed class ChannelMessageHandlerRegistration<THandler, TMessage>(
    ChannelMessageBus bus,
    IServiceScopeFactory scopeFactory,
    string subscriptionName) : IHostedService
    where THandler : IMessageHandler<TMessage>
    where TMessage : IMessage
{
    private IMessageConsumer? consumer;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        consumer = bus.CreateConsumer(subscriptionName, new ScopedHandler(scopeFactory));
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

    private sealed class ScopedHandler(IServiceScopeFactory scopeFactory) : IMessageHandler<TMessage>
    {
        public async Task HandleAsync(TMessage message, CancellationToken cancellationToken)
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            THandler handler = scope.ServiceProvider.GetRequiredService<THandler>();
            await handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }
}
