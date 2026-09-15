using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Util.Messaging.RabbitMQ;

internal sealed class RabbitMqMessageHandlerRegistration<THandler, TMessage> : IHostedService
    where THandler : IMessageHandler<TMessage>
    where TMessage : IMessage
{
    private readonly RabbitMqMessageBus bus;
    private readonly THandler handler;
    private readonly string subscriptionName;
    private IMessageConsumer? consumer;

    public RabbitMqMessageHandlerRegistration(
        RabbitMqMessageBus bus,
        THandler handler,
        string subscriptionName)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
        this.subscriptionName = string.IsNullOrWhiteSpace(subscriptionName)
            ? throw new ArgumentException("Subscription name must not be null, empty, or whitespace.", nameof(subscriptionName))
            : subscriptionName;
    }

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

        if (consumer is IDisposable disposable)
        {
            disposable.Dispose();
        }

        consumer = null;
    }
}
