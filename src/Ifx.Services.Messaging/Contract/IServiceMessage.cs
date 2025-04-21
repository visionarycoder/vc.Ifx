namespace vc.Ifx.Services.Messaging.Contract;

/// <summary>
/// Common interface for all service messages, both requests and responses. Contains common properties to track message correlation
/// and timing throughout the system.
/// </summary>
public interface IServiceMessage : IEquatable<IServiceMessage>
{
    /// <summary>
    /// Unique Id of the message. Every request and response has a different Id.
    /// </summary>
    Guid MessageId { get; set; }

    /// <summary>
    /// Correlates all messages that help implement a Manager request back to the originating Manager request.
    /// See ServiceMessageFactory.CreateFrom() for details, and the remarks on that method for usage guidelines.
    /// </summary>
    Guid CorrelationId { get; set; }

    /// <summary>
    /// UTC timestamp of when the message was created.
    /// </summary>
    DateTime TimestampUtc { get; set; }

    /// <summary>
    /// The type of the message (e.g., Request, Response, Fault).
    /// </summary>
    string MessageType { get; set; }

    /// <summary>
    /// The source system that generated the message.
    /// </summary>
    string SourceSystem { get; set; }
}
