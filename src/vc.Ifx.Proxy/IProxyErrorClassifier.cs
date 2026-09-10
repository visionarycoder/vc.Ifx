namespace VisionaryCoder.Framework.Proxy;

/// <summary>
/// Defines a contract for proxy error classifiers.
/// </summary>
public interface IProxyErrorClassifier
{
    /// <summary>
    /// Determines whether an exception should be retried.
    /// </summary>
    /// <param name="exception">The exception to classify.</param>
    /// <returns>True if the exception should be retried; otherwise, false.</returns>
    bool ShouldRetry(Exception exception);
    /// Determines whether an exception is transient.
    /// <returns>True if the exception is transient; otherwise, false.</returns>
    bool IsTransient(Exception exception);
}
