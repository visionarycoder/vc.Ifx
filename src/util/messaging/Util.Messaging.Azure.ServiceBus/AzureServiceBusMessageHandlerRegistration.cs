using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Util.Messaging.Azure.ServiceBus;

internal sealed class AzureServiceBusMessageHandlerRegistration<THandler, TMessage>(
    AzureServiceBusMessageBus bus,
    THandler handler,
    string subscriptionName) : IHostedService
    where THandler : IMessageHandler<TMessage>
    where TMessage : IMessage
{
    private IMessageConsumer? consumer;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        consumer = bus.CreateConsumer(subscriptionName, handler);
        await consumer.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (consumer is not null)
        {
            await consumer.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        if (consumer is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (consumer is IDisposable disposable)
        {
            disposable.Dispose();
        }

        consumer = null;
    }
}
