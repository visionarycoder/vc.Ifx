namespace Ifx.Messaging;

/// <summary>
/// Common interfaces for all response service messages.
/// </summary>
public interface IServiceMessageResponse : IServiceMessage
{

	/// <summary>
	/// Collection of errors returned from the called component.
	/// </summary>
	/// <remarks>
	/// Do not include exception detail, other code level detail, or other details which could cause a security
	/// concern in the errors collection.  Instead, use a Dependency-Injected ILogger interface to securely log technical
	/// details for support.
	/// </remarks>
	ICollection<ErrorMessage> Errors { get; }
	
	/// <summary>
	/// Returns true if the message was not successful, in which case the ErrorResults property contains an appropriate
	/// error(s).  See the Errors property for details.  If the message succeeded, this property is false.
	/// </summary>
	bool HasErrors { get; }

}