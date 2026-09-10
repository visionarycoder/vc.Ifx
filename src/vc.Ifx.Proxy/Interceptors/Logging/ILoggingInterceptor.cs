namespace VisionaryCoder.Framework.Proxy.Interceptors.Logging;

/// Interface for logging interceptors.
public interface ILoggingInterceptor : IInterceptor
{
    /// <summary>
    /// Intercepts method calls for logging.
    /// </summary>
    /// <param name="methodName">The name of the method being called.</param>
    /// <param name="parameters">The method parameters.</param>
    /// <param name="next">The next operation in the pipeline.</param>
    /// <returns>The result of the operation.</returns>
    Task<T> InterceptAsync<T>(string methodName, object[] parameters, Func<Task<T>> next);
}
