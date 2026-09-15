using System.Text.Json;
using Ifx.Messaging.Abstractions;

namespace Util.Messaging.RabbitMQ;

internal sealed class RabbitMqEnvelope<TMessage>
    where TMessage : IMessage
{
    public Guid MessageId { get; init; }

    public Guid? CorrelationId { get; init; }

    public DateTimeOffset EnqueuedAtUtc { get; init; }

    public TMessage? Message { get; init; }
}

internal static class RabbitMqEnvelopeSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static byte[] Serialize<TMessage>(MessageEnvelope envelope)
        where TMessage : IMessage
    {
        return JsonSerializer.SerializeToUtf8Bytes(
            new RabbitMqEnvelope<TMessage>
            {
                MessageId = envelope.MessageId,
                CorrelationId = envelope.CorrelationId,
                EnqueuedAtUtc = envelope.EnqueuedAtUtc,
                Message = (TMessage)envelope.Message
            },
            SerializerOptions);
    }

    public static RabbitMqEnvelope<TMessage>? Deserialize<TMessage>(ReadOnlyMemory<byte> body)
        where TMessage : IMessage
    {
        return JsonSerializer.Deserialize<RabbitMqEnvelope<TMessage>>(body.Span, SerializerOptions);
    }
}
