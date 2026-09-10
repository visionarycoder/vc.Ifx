namespace VisionaryCoder.Framework.Proxy.Exceptions;

/// <summary>
/// Exception thrown when a proxy operation times out.
/// </summary>
public class ProxyTimeoutException : ProxyException
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyTimeoutException"/> class.
    /// </summary>
    public ProxyTimeoutException() : base("The proxy operation timed out.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyTimeoutException"/> class with a specified timeout.
    /// </summary>
    /// <param name="timeout">The timeout that was exceeded.</param>
    public ProxyTimeoutException(TimeSpan timeout) : base($"The proxy operation timed out after {timeout}.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyTimeoutException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ProxyTimeoutException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyTimeoutException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ProxyTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }

}
