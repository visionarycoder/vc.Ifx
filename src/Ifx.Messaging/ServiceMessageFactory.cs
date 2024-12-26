// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CA1000
namespace Ifx.Messaging;

/// <summary>
/// Factory for creating service message requests and responses.  Only sets base properties.
/// Message-specific properties must be set subsequently.
/// </summary>
/// <typeparam name="T">The derived, concrete message type to be created by this factory.  Should be derived
/// from the ServiceMessageRequest or ServiceMessageResponse abstract base class.</typeparam>
public static class ServiceMessageFactory<T> where T : IServiceMessage, new()
{

	/// <summary>
	/// Creates a new message with an Empty CorrelationId.
	/// </summary>
	/// <returns>A new message, with unique MessageId, and CorrelationId set to Guid.Empty.</returns>
	public static T Create()
	{
		return Create(Guid.Empty);
	}

	/// <summary>
	/// Creates a new message with a caller-supplied CorrelationId.  If the CorrelationID is Guid.Empty, use the MessageId.
	/// </summary>
	/// <param name="correlationId">Id to correlate related messages.</param>
	/// <returns>A new message, with unique MessageId.  The CorrelationId set to the caller-supplied value if the value is not Guid.Empty otherwise the MessageId is used.</returns>
	public static T Create(Guid correlationId)
	{
		var messageId = Guid.NewGuid();
		var instance = new T
		{
			MessageId = messageId,
			CorrelationId = correlationId == Guid.Empty ? messageId : correlationId,
			TimestampUtc = DateTime.UtcNow,
		};
		return instance;
	}

	/// <summary>
	/// Creates a new message that correlates to the supplied caller message.  If the caller has no CorrelationId,
	/// the new message sets its CorrelationId to the MessageId of the caller.  Otherwise, it is set to the same
	/// CorrelationId as that of the caller.
	/// </summary>
	/// <param name="caller">The calling message to which the new message will be correlated.</param>
	/// <returns>A new message, with unique MessageId, and CorrelationId set to correlate with the caller.</returns>
	/// <remarks>
	/// Services should use this method to create the response that correlates to a request.
	/// Services that call other services should also use this method to create requests to those other services.
	/// In both cases, the service should pass in the message that is currently being handled.
	/// This allows all messages through the system to be traced back to an originating Manager message, no matter
	/// how many services are involved.
	/// </remarks>
	public static T CreateFrom(IServiceMessage caller)
	{
		var message = Create(caller.CorrelationId);
		return message;
	}

}