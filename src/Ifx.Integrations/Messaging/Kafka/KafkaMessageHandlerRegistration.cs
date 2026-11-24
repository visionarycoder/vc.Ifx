using Microsoft.Extensions.Hosting;
using vc.Ifx.Messaging;

namespace vc.Ifx.Messaging.Kafka;

internal sealed class KafkaMessageHandlerRegistration<THandler, TMessage>(
    KafkaMessageBus messageBus,
    THandler handler,
    string subscriptionName) : IHostedService, IAsyncDisposable
    where THandler : class, IMessageHandler<TMessage>
    where TMessage : IMessage
{
    private IMessageConsumer? consumer;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        consumer = messageBus.CreateConsumer(subscriptionName, handler);
        await consumer.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (consumer is not null)
        {
            await consumer.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (consumer is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync().ConfigureAwait(false);
        }
    }
}
