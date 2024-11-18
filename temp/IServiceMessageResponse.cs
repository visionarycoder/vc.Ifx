using Eyefinity.Utilities.ErrorHandling;

namespace Eyefinity.Utilities.ServiceMessaging;

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
    /// concern in the errors collection..  Instead, use a Dependency-Injected ILogger interface to securely log technical
    /// details for support.
    /// </remarks>
    Error[] ResponseErrors { get; set; }

    /// <summary>
    /// String representation of the errors. Setting Errors will create UnexpectedError object and add to the ResponseErrors object
    /// </summary>
    /// <remarks>
    /// Version 2.* broke the contract for Errors by expecting an Array of Error objects. 
    /// Version 3.* changed it back to a string, but overrode the getter and setter to utilize the AddError and ResponseError properties.
    /// Marking as obsolete to force usage of AddError and ResponseErrors
    /// </remarks>
    string Errors { get; set; }

    /// <summary>
    /// Returns true if the message was not successful, in which case the ErrorResults property contains an appropriate
    /// error(s).  See the Errors property for details.  If the message succeeded, this property is false.
    /// </summary>
    bool HasErrors { get; }

}