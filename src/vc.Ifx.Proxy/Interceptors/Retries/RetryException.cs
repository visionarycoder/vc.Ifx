using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Retries;

/// Exception thrown when retry operations fail.
public class RetryException : ProxyException
{
    /// <summary>
    /// Gets the number of retry attempts made.
    /// </summary>
    public int AttemptCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryException"/> class.
    /// </summary>
    /// <param name="attemptCount">The number of retry attempts made.</param>
    public RetryException(int attemptCount)
        : base($"Operation failed after {attemptCount} retry attempts")
    {
        AttemptCount = attemptCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="attemptCount">The number of retry attempts made.</param>
    public RetryException(string message, int attemptCount)
        : base(message)
    {
        AttemptCount = attemptCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="attemptCount">The number of retry attempts made.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RetryException(string message, int attemptCount, Exception innerException)
        : base(message, innerException)
    {
        AttemptCount = attemptCount;
    }
}
