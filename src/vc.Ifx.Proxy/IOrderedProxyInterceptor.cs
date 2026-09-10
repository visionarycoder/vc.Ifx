namespace VisionaryCoder.Framework.Proxy;

/// Defines a contract for ordered proxy interceptors.
public interface IOrderedProxyInterceptor : IProxyInterceptor
{
    /// <summary>
    /// Gets the order in which this interceptor should be executed.
    /// Lower values execute first.
    /// </summary>
    int Order { get; }
}