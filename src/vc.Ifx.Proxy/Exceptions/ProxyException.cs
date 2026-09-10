namespace VisionaryCoder.Framework.Proxy.Exceptions;

/// <summary>
/// Base exception for proxy operations.
/// </summary>
[Serializable]
public class ProxyException : Exception
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyException"/> class.
    /// </summary>
    public ProxyException() { }

    /// Initializes a new instance of the <see cref="ProxyException"/> class with a specified error message.
    /// <param name="message">The message that describes the error.</param>
    public ProxyException(string message) : base(message) { }

    /// Initializes a new instance of the <see cref="ProxyException"/> class with a specified error message and inner exception.
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ProxyException(string message, Exception innerException) : base(message, innerException) { }

}
