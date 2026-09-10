using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VisionaryCoder.Framework.Proxy;

/// <summary>
/// Extension methods for configuring proxy pipeline services.
/// </summary>
public static class ProxyExtensions
{

    /// <param name="services">The service collection.</param>
    extension(IServiceCollection services)
    {

        /// <summary>
        /// Adds the default proxy pipeline.
        /// </summary>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection AddProxyPipeline()
    {
        // Register core pipeline components
        services.TryAddSingleton<IProxyPipeline, DefaultProxyPipeline>();

        // Register memory cache if not already registered
        services.TryAddSingleton<IMemoryCache, MemoryCache>();
        return services;

    }

    /// <summary>
    /// Adds a custom proxy transport implementation.
    /// </summary>
    /// <typeparam name="TTransport">The transport implementation type.</typeparam>
    /// <returns>The service collection for chaining.</returns>
    public IServiceCollection AddProxyTransport<TTransport>()
        where TTransport : class, IProxyTransport
    {
        services.TryAddSingleton<IProxyTransport, TTransport>();
        return services;
    }

    /// <summary>
    /// Adds a custom interceptor to the proxy pipeline.
    /// </summary>
    /// <typeparam name="TInterceptor">The interceptor implementation type.</typeparam>
    /// <param name="lifetime">The service lifetime for the interceptor.</param>
    /// <returns>The service collection for chaining.</returns>
    public IServiceCollection AddProxyInterceptor<TInterceptor>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TInterceptor : class, IProxyInterceptor
    {
        services.TryAdd(ServiceDescriptor.Describe(typeof(TInterceptor), typeof(TInterceptor), lifetime));
        services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IProxyInterceptor), typeof(TInterceptor), lifetime));
        return services;
    }
}
}
