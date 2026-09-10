// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using VisionaryCoder.Framework.Proxy.Interceptors.Auditing;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;
using VisionaryCoder.Framework.Proxy.Interceptors.Correlation;
using VisionaryCoder.Framework.Proxy.Interceptors.Logging;
using VisionaryCoder.Framework.Proxy.Interceptors.Resilience;
using VisionaryCoder.Framework.Proxy.Interceptors.Retries;
using VisionaryCoder.Framework.Proxy.Interceptors.Security;
using VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;
using VisionaryCoder.Framework.Proxy.Interceptors.Telemetry;

namespace VisionaryCoder.Framework.Proxy.Interceptors;
/// <summary>
/// Extension methods for configuring proxy interceptors in the dependency injection container.
/// </summary>
public static class ProxyInterceptorExtensions
{
    /// <summary>
    /// Adds all proxy interceptors with their default configurations and proper ordering.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration action for proxy options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddProxyInterceptors(
        this IServiceCollection services,
        Action<ProxyOptions>? configureOptions = null)
    {
        Infrastructure(services);
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
            services.Configure<ProxyOptions>(options =>
            {
                // Set default values
                options.Timeout = TimeSpan.FromSeconds(30);
                options.CircuitBreakerFailures = 3;
                options.CircuitBreakerDuration = TimeSpan.FromMinutes(1);
                options.MaxRetries = 3;
                options.RetryDelay = TimeSpan.FromMilliseconds(500);
            });
        return services
            .AddSecurityInterceptor()
            .AddTelemetryInterceptor()
            .AddCorrelationInterceptor()
            .AddLoggingInterceptor()
            .AddCachingInterceptor()
            .AddResilienceInterceptor()
            .AddAuditingInterceptor();
    }
    /// Adds the security interceptor (order -200).
    public static IServiceCollection AddSecurityInterceptor(this IServiceCollection services)
    {
        Infrastructure(services);
        AddOrdered<SecurityInterceptor>(services);
        return services;
    }
    /// Adds security enrichers and authorization policies.
    public static IServiceCollection AddSecurityEnricher<TEnricher>(this IServiceCollection services)
        where TEnricher : class, IProxySecurityEnricher
    {
        Infrastructure(services);
        services.TryAddEnumerable(ServiceDescriptor.Transient<IProxySecurityEnricher, TEnricher>());
        return services;
    }
    /// Adds an authorization policy.
    public static IServiceCollection AddAuthorizationPolicy<TPolicy>(this IServiceCollection services)
        where TPolicy : class, IProxyAuthorizationPolicy
    {
        Infrastructure(services);
        services.TryAddEnumerable(ServiceDescriptor.Transient<IProxyAuthorizationPolicy, TPolicy>());
        return services;
    }
    /// Adds JWT Bearer enricher with a token provider.
    public static IServiceCollection AddJwtBearerEnricher(
        this IServiceCollection services,
        Func<IServiceProvider, Task<string?>> tokenProvider)
    {
        Infrastructure(services);
        ArgumentNullException.ThrowIfNull(tokenProvider);
        if (services.Any(descriptor => descriptor.ServiceType == typeof(JwtBearerEnricher)))
            return services;
        services.TryAddScoped<JwtBearerEnricher>(provider =>
        {
            ILogger<JwtBearerEnricher> logger = provider.GetRequiredService<ILogger<JwtBearerEnricher>>();
            return new JwtBearerEnricher(logger, () => tokenProvider(provider));
        });
        services.AddScoped<IProxySecurityEnricher>(provider => provider.GetRequiredService<JwtBearerEnricher>());
        return services;
    }
    /// Adds the telemetry interceptor with its core-defined order.
    public static IServiceCollection AddTelemetryInterceptor(this IServiceCollection services)
    {
        services.TryAddSingleton(provider => new ActivitySource("VisionaryCoder.Framework.Proxy"));
        Infrastructure(services);
        AddOrdered<TelemetryInterceptor>(services);
        return services;
    }
    /// Adds the correlation interceptor with its core-defined order.
    public static IServiceCollection AddCorrelationInterceptor(this IServiceCollection services)
    {
        services.TryAddSingleton<ICorrelationContext, DefaultCorrelationContext>();
        services.TryAddSingleton<ICorrelationIdGenerator, GuidCorrelationIdGenerator>();
        Infrastructure(services);
        AddOrdered<CorrelationInterceptor>(services);
        return services;
    }
    /// Adds the logging interceptor with its core-defined order.
    public static IServiceCollection AddLoggingInterceptor(this IServiceCollection services)
    {
        Infrastructure(services);
        AddOrdered<LoggingInterceptor>(services);
        return services;
    }
    /// Adds the caching interceptor with its core-defined order.
    public static IServiceCollection AddCachingInterceptor(this IServiceCollection services)
    {
        Infrastructure(services);
        services.TryAddSingleton<IProxyCache, NullProxyCache>();
        services.TryAddSingleton<ICacheKeyProvider, NullCacheKeyProvider>();
        services.TryAddSingleton<ICachePolicyProvider, NullCachePolicyProvider>();
        services.TryAddTransient(provider => new CachingInterceptor(
            provider.GetRequiredService<ILogger<CachingInterceptor>>(), provider.GetRequiredService<IProxyCache>(),
            provider.GetRequiredService<ICacheKeyProvider>(), provider.GetRequiredService<ICachePolicyProvider>()));
        AddOrdered<CachingInterceptor>(services);
        return services;
    }
    /// Adds a proxy cache implementation.
    public static IServiceCollection AddProxyCache<TCache>(this IServiceCollection services)
        where TCache : class, IProxyCache
    {
        Infrastructure(services);
        services.TryAddSingleton<IProxyCache, TCache>();
        return services;
    }
    /// Adds the resilience interceptor with its core-defined order.
    public static IServiceCollection AddResilienceInterceptor(this IServiceCollection services)
    {
        Infrastructure(services);
        AddOrdered<ResilienceInterceptor>(services);
        return services;
    }
    /// Adds the retry interceptor with its core-defined order.
    public static IServiceCollection AddRetryInterceptor(this IServiceCollection services)
    {
        Infrastructure(services);
        AddOrdered<RetryInterceptor>(services);
        return services;
    }
    /// Adds the auditing interceptor with its core-defined order.
    public static IServiceCollection AddAuditingInterceptor(this IServiceCollection services)
    {
        services.TryAddTransient<IAuditSink, LoggingAuditSink>();
        Infrastructure(services);
        AddOrdered<AuditingInterceptor>(services);
        return services;
    }
    /// Adds an audit sink.
    public static IServiceCollection AddAuditSink<TSink>(this IServiceCollection services)
        where TSink : class, IAuditSink
    {
        Infrastructure(services);
        services.TryAddEnumerable(ServiceDescriptor.Transient<IAuditSink, TSink>());
        return services;
    }
    private static void AddOrdered<T>(IServiceCollection services) where T : class, IOrderedProxyInterceptor
    {
        services.TryAddTransient<T>();
        Func<IServiceProvider, T> factory = provider => provider.GetRequiredService<T>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IOrderedProxyInterceptor>(factory));
        services.TryAddEnumerable(ServiceDescriptor.Transient<IProxyInterceptor>(factory));
    }

    private static void Infrastructure(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddLogging();
        services.AddOptions<ProxyOptions>();
    }
}
