using System.Text.Json;
using vc.Ifx.Messaging;
using vc.Ifx.Messaging.ProviderSupport;

namespace vc.Ifx.Messaging.Kafka;

internal sealed class KafkaEnvelope<TMessage>
    where TMessage : IMessage
{
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);

    public Guid MessageId { get; init; }

    public Guid? CorrelationId { get; init; }

    public DateTimeOffset EnqueuedAtUtc { get; init; }

    public TMessage? Message { get; init; }

    public static byte[] Serialize(MessageEnvelope envelope) =>
        JsonSerializer.SerializeToUtf8Bytes(
            new KafkaEnvelope<TMessage>
            {
                MessageId = envelope.MessageId,
                CorrelationId = envelope.CorrelationId,
                EnqueuedAtUtc = envelope.EnqueuedAtUtc,
                Message = (TMessage)envelope.Message
            },
            serializerOptions);

    public static KafkaEnvelope<TMessage> Deserialize(byte[] value) =>
        JsonSerializer.Deserialize<KafkaEnvelope<TMessage>>(value, serializerOptions)
        ?? throw new InvalidDataException("The Kafka message envelope was empty.");
}
