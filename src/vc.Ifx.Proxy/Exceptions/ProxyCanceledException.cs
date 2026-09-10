namespace VisionaryCoder.Framework.Proxy.Exceptions;

/// <summary>
/// Represents an exception that occurs when a proxy operation is canceled.
/// </summary>
public class ProxyCanceledException : ProxyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyCanceledException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ProxyCanceledException(string message) : base(message) { }

    /// <param name="message"></param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ProxyCanceledException(string message, Exception innerException) : base(message, innerException) { }
}
