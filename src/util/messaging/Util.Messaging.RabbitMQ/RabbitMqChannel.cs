using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Util.Messaging.RabbitMQ;

internal sealed class RabbitMqChannel : IRabbitMqChannel
{
    private readonly IChannel channel;

    public RabbitMqChannel(IChannel channel)
    {
        this.channel = channel ?? throw new ArgumentNullException(nameof(channel));
    }

    public Task DeclareExchangeAsync(
        string exchangeName,
        string exchangeType,
        CancellationToken cancellationToken = default)
    {
        return channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: exchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    public async Task DeclareConsumerTopologyAsync(
        RabbitMqTopology topology,
        RabbitMqOptions options,
        CancellationToken cancellationToken = default)
    {
        await DeclareExchangeAsync(
            options.ExchangeName!,
            options.ExchangeType!,
            cancellationToken).ConfigureAwait(false);

        await channel.ExchangeDeclareAsync(
            exchange: topology.DeadLetterExchangeName,
            type: "direct",
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        Dictionary<string, object?> queueArguments = new()
        {
            ["x-dead-letter-exchange"] = topology.DeadLetterExchangeName,
            ["x-dead-letter-routing-key"] = topology.DeadLetterRoutingKey
        };

        await channel.QueueDeclareAsync(
            queue: topology.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await channel.QueueDeclareAsync(
            queue: topology.DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await channel.QueueBindAsync(
            queue: topology.QueueName,
            exchange: options.ExchangeName!,
            routingKey: topology.RoutingKey,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await channel.QueueBindAsync(
            queue: topology.DeadLetterQueueName,
            exchange: topology.DeadLetterExchangeName,
            routingKey: topology.DeadLetterRoutingKey,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: options.PrefetchCount,
            global: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public Task PublishAsync(
        string exchangeName,
        string routingKey,
        string messageType,
        string messageId,
        string? correlationId,
        DateTimeOffset enqueuedAtUtc,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default)
    {
        BasicProperties properties = new()
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            MessageId = messageId,
            CorrelationId = correlationId,
            Type = messageType,
            Persistent = true,
            Timestamp = new AmqpTimestamp(enqueuedAtUtc.ToUnixTimeSeconds())
        };

        return channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken).AsTask();
    }

    public Task<string> StartConsumerAsync(
        string queueName,
        Func<RabbitMqDelivery, CancellationToken, Task> onReceived,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onReceived);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            RabbitMqDelivery delivery = new(
                args.DeliveryTag,
                args.BasicProperties.MessageId,
                args.BasicProperties.CorrelationId,
                args.Exchange,
                args.RoutingKey,
                args.Body.ToArray());

            await onReceived(delivery, CancellationToken.None).ConfigureAwait(false);
        };

        return channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }

    public Task CancelConsumerAsync(string consumerTag, CancellationToken cancellationToken = default)
    {
        return channel.BasicCancelAsync(consumerTag, cancellationToken: cancellationToken);
    }

    public Task AcknowledgeAsync(ulong deliveryTag, CancellationToken cancellationToken = default)
    {
        return channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: cancellationToken).AsTask();
    }

    public Task RejectAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default)
    {
        return channel.BasicRejectAsync(deliveryTag, requeue, cancellationToken).AsTask();
    }

    public Task NegativeAcknowledgeAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default)
    {
        return channel.BasicNackAsync(
            deliveryTag,
            multiple: false,
            requeue: requeue,
            cancellationToken: cancellationToken).AsTask();
    }

    public void Dispose()
    {
        channel.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return channel.DisposeAsync();
    }
}
