using Ifx.Messaging.Abstractions;

namespace Util.Messaging;

/// <summary>
/// Wraps a message with transport-oriented metadata that providers can stamp at enqueue time.
/// </summary>
public sealed class MessageEnvelope
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageEnvelope"/> class.
    /// </summary>
    /// <param name="messageId">The envelope identifier assigned by the caller or transport.</param>
    /// <param name="correlationId">The correlation identifier for related messages.</param>
    /// <param name="enqueuedAtUtc">The UTC timestamp when the message was enqueued.</param>
    /// <param name="message">The wrapped message payload.</param>
    public MessageEnvelope(Guid messageId, Guid? correlationId, DateTimeOffset enqueuedAtUtc, IMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        MessageId = messageId;
        CorrelationId = correlationId;
        EnqueuedAtUtc = enqueuedAtUtc;
        Message = message;
    }

    /// <summary>
    /// Gets the unique identifier for this envelope.
    /// </summary>
    public Guid MessageId { get; }

    /// <summary>
    /// Gets the correlation identifier that links related messages.
    /// </summary>
    public Guid? CorrelationId { get; }

    /// <summary>
    /// Gets the UTC timestamp when the envelope was created for enqueueing.
    /// </summary>
    public DateTimeOffset EnqueuedAtUtc { get; }

    /// <summary>
    /// Gets the wrapped message payload.
    /// </summary>
    public IMessage Message { get; }

    /// <summary>
    /// Creates a new envelope with a generated identifier and the current UTC enqueue timestamp.
    /// </summary>
    /// <param name="message">The wrapped message payload.</param>
    /// <param name="correlationId">The optional correlation identifier for related messages.</param>
    /// <returns>A stamped envelope that wraps the supplied message.</returns>
    public static MessageEnvelope Create(IMessage message, Guid? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new MessageEnvelope(
            Guid.NewGuid(),
            correlationId,
            DateTimeOffset.UtcNow,
            message);
    }
}
