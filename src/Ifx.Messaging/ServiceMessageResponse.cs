namespace Ifx.Messaging;

/// <summary>
/// Abstract base class for all response service messages.
/// </summary>
public abstract class ServiceMessageResponse : ServiceMessage, IServiceMessageResponse
{

    /// <summary>
    /// Contains a non-whitespace, human-readable, security-safe string appropriate for bubbling up to the client
    /// if the message was not successful.  If the message succeeded, this property is null.
    /// </summary>
    /// <remarks>
    /// Do not include exception detail, other code level detail, or other details which could cause a security
    /// concern in the error message.  Instead, use a Dependency-Injected ILogger interface to securely log technical
    /// details for support.
    /// </remarks>
    public ICollection<ErrorMessage> Errors { get; } = new List<ErrorMessage>();

    /// <summary>
    /// Returns true if any errors found in the collection.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;

}