namespace Util.Messaging.RabbitMQ;

internal interface IRabbitMqChannel : IDisposable, IAsyncDisposable
{
    Task DeclareExchangeAsync(
        string exchangeName,
        string exchangeType,
        CancellationToken cancellationToken = default);

    Task DeclareConsumerTopologyAsync(
        RabbitMqTopology topology,
        RabbitMqOptions options,
        CancellationToken cancellationToken = default);

    Task PublishAsync(
        string exchangeName,
        string routingKey,
        string messageType,
        string messageId,
        string? correlationId,
        DateTimeOffset enqueuedAtUtc,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default);

    Task<string> StartConsumerAsync(
        string queueName,
        Func<RabbitMqDelivery, CancellationToken, Task> onReceived,
        CancellationToken cancellationToken = default);

    Task CancelConsumerAsync(string consumerTag, CancellationToken cancellationToken = default);

    Task AcknowledgeAsync(ulong deliveryTag, CancellationToken cancellationToken = default);

    Task RejectAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default);

    Task NegativeAcknowledgeAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default);
}
