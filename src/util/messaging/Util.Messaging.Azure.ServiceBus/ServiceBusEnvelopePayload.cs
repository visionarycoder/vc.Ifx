using System.Text.Json;
using Ifx.Messaging.Abstractions;

namespace Util.Messaging.Azure.ServiceBus;

internal sealed class ServiceBusEnvelopePayload
{
    public Guid MessageId { get; init; }

    public Guid? CorrelationId { get; init; }

    public DateTimeOffset EnqueuedAtUtc { get; init; }

    public string? MessageType { get; init; }

    public JsonElement Message { get; init; }

    public static ServiceBusEnvelopePayload Create(MessageEnvelope envelope, Type messageType, JsonSerializerOptions serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(messageType);
        ArgumentNullException.ThrowIfNull(serializerOptions);

        return new ServiceBusEnvelopePayload
        {
            MessageId = envelope.MessageId,
            CorrelationId = envelope.CorrelationId,
            EnqueuedAtUtc = envelope.EnqueuedAtUtc,
            MessageType = messageType.AssemblyQualifiedName ?? messageType.FullName ?? messageType.Name,
            Message = JsonSerializer.SerializeToElement((object)envelope.Message, messageType, serializerOptions)
        };
    }
}
