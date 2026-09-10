// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching;

/// <summary>
/// Extension methods for adding comprehensive caching services to the dependency injection container.
/// Provides fluent configuration for caching interceptors, providers, and policies.
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Adds caching infrastructure with null object fallbacks (SOLID principle).
    /// Following SOLID principles: requires explicit intent for providers, uses Null Object pattern for fallbacks.
    /// </summary>
    /// <param name="services">The service collection to add caching services to.</param>
    /// <param name="configure">Optional configuration action for caching options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCaching(this IServiceCollection services, Action<CachingOptions>? configure = null)
    {
        AddInfrastructure(services);
        // Add memory cache for infrastructure
        services.AddMemoryCache();

        // Configure caching options
        if (configure != null)
        {
            services.Configure(configure);
        }

        // Register NULL OBJECT implementations as fallbacks (SOLID principle)
        // These will be used if no explicit providers are registered
        services.TryAddSingleton<ICacheKeyProvider, NullCacheKeyProvider>();
        services.TryAddSingleton<ICachePolicyProvider, NullCachePolicyProvider>();
        services.TryAddSingleton<IProxyCache, NullProxyCache>();

        // Register the caching interceptor
        services.TryAddSingleton<IOrderedProxyInterceptor>(provider => provider.GetRequiredService<CachingInterceptor>());

        return services;
    }

    /// <summary>
    /// Adds caching services by registering a single implementation type which can be either
    /// an IProxyCache, ICachePolicyProvider, or ICacheKeyProvider. This single generic overload
    /// allows concise registration in tests and consumers, such as registering
    /// <see cref="MemoryProxyCache"/> or <see cref="DefaultCachePolicyProvider"/>.
    /// </summary>
    public static IServiceCollection AddCaching<T>(this IServiceCollection services, Action<CachingOptions>? configure = null)
        where T : class
    {
        AddInfrastructure(services);
        services.AddMemoryCache();

        if (configure != null)
        {
            services.Configure(configure);
        }

        Type t = typeof(T);

        if (typeof(IProxyCache).IsAssignableFrom(t))
        {
            services.TryAddSingleton<ICacheKeyProvider, DefaultCacheKeyProvider>();
            services.TryAddSingleton<ICachePolicyProvider, DefaultCachePolicyProvider>();
            services.TryAddSingleton(typeof(IProxyCache), t);
        }
        else if (typeof(ICachePolicyProvider).IsAssignableFrom(t))
        {
            services.TryAddSingleton<ICacheKeyProvider, DefaultCacheKeyProvider>();
            services.TryAddSingleton(typeof(ICachePolicyProvider), t);
            services.TryAddSingleton<IProxyCache, MemoryProxyCache>();
        }
        else if (typeof(ICacheKeyProvider).IsAssignableFrom(t))
        {
            services.TryAddSingleton(typeof(ICacheKeyProvider), t);
            services.TryAddSingleton<ICachePolicyProvider, DefaultCachePolicyProvider>();
            services.TryAddSingleton<IProxyCache, MemoryProxyCache>();
        }
        else
        {
            throw new ArgumentException($"Type parameter {t.FullName} must implement IProxyCache, ICachePolicyProvider or ICacheKeyProvider.", nameof(T));
        }

        services.TryAddSingleton<IOrderedProxyInterceptor>(provider => provider.GetRequiredService<CachingInterceptor>());

        return services;
    }

    /// <summary>
    /// Adds caching with quick configuration for common scenarios.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="defaultDuration">The default cache duration.</param>
    /// <param name="maxCacheSize">Optional maximum cache size in entries.</param>
    /// <param name="enableEvictionLogging">Whether to enable eviction logging.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCaching(this IServiceCollection services, TimeSpan defaultDuration, int? maxCacheSize = null, bool enableEvictionLogging = false)
    {
        return services.AddCaching(options =>
        {
            options.DefaultDuration = defaultDuration;
            options.MaxCacheSize = maxCacheSize;
            options.EnableEvictionLogging = enableEvictionLogging;
        });
    }

    /// <summary>
    /// Legacy registration alias. Does not install a distributed backend; explicitly replace IProxyCache in the host.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration for caching options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDistributedCaching(this IServiceCollection services, Action<CachingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        return services.AddCaching(configure);
    }

    // SOLID Principle - Explicit Provider Registration Methods

    /// <summary>
    /// Explicitly registers a cache key provider implementation (SOLID principle).
    /// Replaces any existing provider with explicit intent, no automatic defaults.
    /// </summary>
    /// <typeparam name="T">The cache key provider implementation type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection ReplaceCacheKeyProvider<T>(this IServiceCollection services)
        where T : class, ICacheKeyProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        // Replace any existing implementation with explicit provider
        services.RemoveAll<ICacheKeyProvider>();
        services.AddSingleton<ICacheKeyProvider, T>();

        return services;
    }

    /// <summary>
    /// Explicitly registers a cache policy provider implementation (SOLID principle).
    /// Replaces any existing provider with explicit intent, no automatic defaults.
    /// </summary>
    /// <typeparam name="T">The cache policy provider implementation type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection ReplaceCachePolicyProvider<T>(this IServiceCollection services)
        where T : class, ICachePolicyProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        // Replace any existing implementation with explicit provider
        services.RemoveAll<ICachePolicyProvider>();
        services.AddSingleton<ICachePolicyProvider, T>();

        return services;
    }

    /// <summary>
    /// Explicitly registers a proxy cache implementation (SOLID principle).
    /// Replaces any existing cache with explicit intent, no automatic defaults.
    /// </summary>
    /// <typeparam name="T">The proxy cache implementation type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection ReplaceProxyCache<T>(this IServiceCollection services)
        where T : class, IProxyCache
    {
        ArgumentNullException.ThrowIfNull(services);

        // Replace any existing implementation with explicit cache
        services.RemoveAll<IProxyCache>();
        services.AddSingleton<IProxyCache, T>();

        return services;
    }

    /// <summary>
    /// Convenience method to register default caching providers explicitly.
    /// This method demonstrates how to replace null object providers with functional implementations.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection UseDefaultCachingProviders(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        AddInfrastructure(services);

        services.ReplaceCacheKeyProvider<DefaultCacheKeyProvider>();
        services.ReplaceCachePolicyProvider<DefaultCachePolicyProvider>();
        services.ReplaceProxyCache<MemoryProxyCache>();

        return services;
    }

    private static void AddInfrastructure(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddLogging();
        services.AddOptions<CachingOptions>();
        services.TryAddSingleton(provider => provider.GetRequiredService<IOptions<CachingOptions>>().Value);
        services.AddMemoryCache();
        services.TryAddSingleton(provider => new CachingInterceptor(
            provider.GetRequiredService<ILogger<CachingInterceptor>>(),
            provider.GetRequiredService<IProxyCache>(),
            provider.GetRequiredService<ICacheKeyProvider>(),
            provider.GetRequiredService<ICachePolicyProvider>()));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IProxyInterceptor, CachingInterceptor>(
            provider => provider.GetRequiredService<CachingInterceptor>()));
    }
}
