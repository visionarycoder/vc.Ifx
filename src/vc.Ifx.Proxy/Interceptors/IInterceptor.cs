namespace VisionaryCoder.Framework.Proxy.Interceptors;

/// <summary>
/// Base interface for all proxy interceptors.
/// </summary>
public interface IInterceptor
{
    /// <summary>
    /// Gets the order in which this interceptor should be executed.
    /// Lower numbers execute first.
    /// </summary>
    int Order { get; }
}