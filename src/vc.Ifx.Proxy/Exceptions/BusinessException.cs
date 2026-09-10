namespace VisionaryCoder.Framework.Proxy.Exceptions;

/// <summary>
/// Represents a business logic exception.
/// </summary>
public class BusinessException : ProxyException
{
    /// <param name="message">The message that describes the error.</param>
    public BusinessException(string message) : base(message) { }

    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BusinessException(string message, Exception innerException) : base(message, innerException) { }
}
