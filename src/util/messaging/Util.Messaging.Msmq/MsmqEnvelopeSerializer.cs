using System.Text.Json;
using Ifx.Messaging.Abstractions;

namespace Util.Messaging.Msmq;

internal static class MsmqEnvelopeSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static SerializedMsmqMessage Serialize(IMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Guid? correlationId = TryParseGuid(message.CorrelationId);
        MessageEnvelope envelope = MessageEnvelope.Create(message, correlationId);
        Type messageType = message.GetType();
        string messageTypeName = messageType.AssemblyQualifiedName!;

        SerializedEnvelope payload = new(
            envelope.MessageId,
            envelope.CorrelationId,
            envelope.EnqueuedAtUtc,
            messageTypeName,
            JsonSerializer.SerializeToElement(message, messageType, SerializerOptions));

        return new SerializedMsmqMessage(
            JsonSerializer.Serialize(payload, SerializerOptions),
            envelope.CorrelationId?.ToString("D"),
            messageType.FullName!);
    }

    public static MessageEnvelope Deserialize(string body)
    {
        string payloadBody = body.ValidateNotNullOrWhiteSpace(nameof(body));
        SerializedEnvelope payload = JsonSerializer.Deserialize<SerializedEnvelope>(payloadBody, SerializerOptions)
            ?? throw new InvalidOperationException("MSMQ message payload could not be deserialized.");

        Type messageType = Type.GetType(payload.MessageType, throwOnError: false)
            ?? throw new InvalidOperationException($"Unable to resolve message type '{payload.MessageType}'.");

        object? message = payload.Message.Deserialize(messageType, SerializerOptions);

        if (message is not IMessage typedMessage)
        {
            throw new InvalidOperationException($"Resolved message type '{payload.MessageType}' does not implement {nameof(IMessage)}.");
        }

        return new MessageEnvelope(payload.MessageId, payload.CorrelationId, payload.EnqueuedAtUtc, typedMessage);
    }

    private static Guid? TryParseGuid(string? value)
    {
        return Guid.TryParse(value, out Guid correlationId)
            ? correlationId
            : null;
    }

    private sealed record SerializedEnvelope(
        Guid MessageId,
        Guid? CorrelationId,
        DateTimeOffset EnqueuedAtUtc,
        string MessageType,
        JsonElement Message);
}

internal sealed record SerializedMsmqMessage(string Body, string? CorrelationId, string Label);

internal sealed record ReceivedMsmqMessage(string Body, string? CorrelationId, string Label);
