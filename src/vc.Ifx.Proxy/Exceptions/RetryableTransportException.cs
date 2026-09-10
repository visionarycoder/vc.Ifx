namespace VisionaryCoder.Framework.Proxy.Exceptions;

/// <summary>
/// Represents a transport exception that can be retried.
/// </summary>
public class RetryableTransportException : ProxyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryableTransportException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RetryableTransportException(string message) : base(message) { }

    /// <param name="message"></param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RetryableTransportException(string message, Exception innerException) : base(message, innerException) { }
}
