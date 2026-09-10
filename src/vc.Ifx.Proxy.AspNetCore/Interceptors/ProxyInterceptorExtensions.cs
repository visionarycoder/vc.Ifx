// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using VisionaryCoder.Framework.Proxy.Interceptors.Auditing;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;
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
            .AddRetryInterceptor()
            .AddAuditingInterceptor();
    }
    /// Adds the security interceptor (order -200).
    public static IServiceCollection AddSecurityInterceptor(this IServiceCollection services)
    {
        services.TryAddTransient<IOrderedProxyInterceptor, SecurityInterceptor>();
        return services;
    }
    /// Adds security enrichers and authorization policies.
    public static IServiceCollection AddSecurityEnricher<TEnricher>(this IServiceCollection services)
        where TEnricher : class, IProxySecurityEnricher
    {
        services.TryAddTransient<IProxySecurityEnricher, TEnricher>();
        return services;
    }
    /// Adds an authorization policy.
    public static IServiceCollection AddAuthorizationPolicy<TPolicy>(this IServiceCollection services)
        where TPolicy : class, IProxyAuthorizationPolicy
    {
        services.TryAddTransient<IProxyAuthorizationPolicy, TPolicy>();
        return services;
    }
    /// Adds JWT Bearer enricher with a token provider.
    public static IServiceCollection AddJwtBearerEnricher(
        this IServiceCollection services,
        Func<IServiceProvider, Task<string?>> tokenProvider)
    {
        services.TryAddTransient<IProxySecurityEnricher>(provider =>
        {
            ILogger<JwtBearerEnricher> logger = provider.GetRequiredService<ILogger<JwtBearerEnricher>>();
            return new JwtBearerEnricher(logger, () => tokenProvider(provider));
        });
        return services;
    }
    /// Adds the telemetry interceptor (order -50).
    public static IServiceCollection AddTelemetryInterceptor(this IServiceCollection services)
    {
        services.TryAddSingleton(new ActivitySource("VisionaryCoder.Framework.Proxy"));
        services.TryAddTransient<IOrderedProxyInterceptor, TelemetryInterceptor>();
        return services;
    }
    /// Adds the correlation interceptor (order 0).
    public static IServiceCollection AddCorrelationInterceptor(this IServiceCollection services)
    {
        services.TryAddSingleton<ICorrelationContext, DefaultCorrelationContext>();
        services.TryAddSingleton<ICorrelationIdGenerator, GuidCorrelationIdGenerator>();
        services.TryAddTransient<IOrderedProxyInterceptor, CorrelationInterceptor>();
        return services;
    }
    /// Adds the logging interceptor (order 100).
    public static IServiceCollection AddLoggingInterceptor(this IServiceCollection services)
    {
        services.TryAddTransient<IOrderedProxyInterceptor, LoggingInterceptor>();
        return services;
    }
    /// Adds the caching interceptor (order 150).
    public static IServiceCollection AddCachingInterceptor(this IServiceCollection services)
    {
        services.TryAddTransient<IOrderedProxyInterceptor, CachingInterceptor>();
        return services;
    }
    /// Adds a proxy cache implementation.
    public static IServiceCollection AddProxyCache<TCache>(this IServiceCollection services)
        where TCache : class, IProxyCache
    {
        services.TryAddSingleton<IProxyCache, TCache>();
        return services;
    }
    /// Adds the resilience interceptor (order 180).
    public static IServiceCollection AddResilienceInterceptor(this IServiceCollection services)
    {
        services.TryAddTransient<IOrderedProxyInterceptor, ResilienceInterceptor>();
        return services;
    }
    /// Adds the retry interceptor (order 200).
    public static IServiceCollection AddRetryInterceptor(this IServiceCollection services)
    {
        services.TryAddTransient<IOrderedProxyInterceptor, RetryInterceptor>();
        return services;
    }
    /// Adds the auditing interceptor (order 300).
    public static IServiceCollection AddAuditingInterceptor(this IServiceCollection services)
    {
        services.TryAddTransient<IAuditSink, LoggingAuditSink>();
        services.TryAddTransient<IOrderedProxyInterceptor, AuditingInterceptor>();
        return services;
    }
    /// Adds an audit sink.
    public static IServiceCollection AddAuditSink<TSink>(this IServiceCollection services)
        where TSink : class, IAuditSink
    {
        services.TryAddTransient<IAuditSink, TSink>();
        return services;
    }
}
