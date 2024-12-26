namespace Ifx.Messaging;

/// <summary>
/// Abstract base of all service messages, both requests and responses.  Contains common properties to track message correlation
/// and timing throughout the system.
/// </summary>
public abstract class ServiceMessage : IServiceMessage
{

	/// <summary>
	/// Unique Id of the message.  Every request and response has a different Id.  
	/// </summary>
	public Guid MessageId { get; set; }

	/// <summary>
	/// Correlates all messages that help implement a Manager request back to the originating Manager request.
	/// See ServiceMessageFactory.CreateFrom() for details, and the remarks on that method for usage guidelines.
	/// </summary>
	public Guid CorrelationId { get; set; }

	/// <summary>
	/// UTC timestamp of when the message was created.
	/// </summary>
	public DateTime TimestampUtc { get; set; }

}