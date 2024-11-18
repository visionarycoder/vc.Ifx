using Eyefinity.Utilities.ErrorHandling;
using System;
using System.Linq;

namespace Eyefinity.Utilities.ServiceMessaging;

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
    public Error[] ResponseErrors { get; set; } = Array.Empty<Error>();

    /// <summary>
    /// String representation of the errors. Setting Errors will create UnexpectedError object and add to the ResponseErrors object
    /// </summary>
    /// <remarks>
    /// Version 2.* broke the contract for Errors by expecting an Array of Error objects. 
    /// Version 3.* changed it back to a string, but overrode the getter and setter to utilize the AddError and ResponseError properties.
    /// Marking as obsolete to force usage of AddError and ResponseErrors
    /// </remarks>
    [Obsolete("Use ResponseErrors or the AddError Method")]
    public string Errors
    {
        get
        {
            return string.Join(", ", this.ResponseErrors.Select(n => n.Description.ToString()));
        }
        set
        {
            ResponseErrors = string.IsNullOrWhiteSpace(value)
                ? Array.Empty<Error>()
                : new []{new ErrorBuilder(UnexpectedError.Error).SetDescription(value).Build()};
        }
    }

    /// <summary>
    /// Returns true if any errors found in the collection.
    /// </summary>
    public bool HasErrors => ResponseErrors != null && ResponseErrors.Any(i => i != null);
}