namespace Util.Messaging.RabbitMQ;

internal sealed record RabbitMqDelivery(
    ulong DeliveryTag,
    string? MessageId,
    string? CorrelationId,
    string Exchange,
    string RoutingKey,
    ReadOnlyMemory<byte> Body);
