namespace VisionaryCoder.Framework.Proxy.Exceptions;

/// <summary>
/// Exception thrown when a proxy operation fails due to a transient error.
/// </summary>
public class TransientProxyException : ProxyException
{

    /// <summary>
    /// Initializes a new instance of the <see cref="TransientProxyException"/> class.
    /// </summary>
    public TransientProxyException() : base("A transient proxy error occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransientProxyException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TransientProxyException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransientProxyException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TransientProxyException(string message, Exception innerException) : base(message, innerException)
    {
    }

}
