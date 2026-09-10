using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Retries;

/// <summary>
/// Represents a transport exception that cannot be retried.
/// </summary>
public class NonRetryableTransportException : ProxyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NonRetryableTransportException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NonRetryableTransportException(string message) : base(message) { }

    /// <param name="message"></param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public NonRetryableTransportException(string message, Exception innerException) : base(message, innerException) { }
}
