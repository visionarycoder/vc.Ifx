// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Logging;

namespace VisionaryCoder.Framework.Logging;

/// <summary>
/// Extension methods for adding comprehensive logging services to the dependency injection container.
/// Provides fluent configuration for logging interceptors with various options and behaviors.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds logging infrastructure with null object fallbacks (SOLID principle).
    /// Following SOLID principles: requires explicit intent for interceptors, uses Null Object pattern for fallbacks.
    /// </summary>
    /// <param name="services">The service collection to add logging services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLogging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        // Register NULL OBJECT implementation as fallback (SOLID principle)
        // This will be used if no explicit logging interceptors are registered
        services.TryAddSingleton<IOrderedProxyInterceptor, NullLoggingInterceptor>();

        return services;
    }

    /// <summary>
    /// Adds only the logging interceptor without timing measurements.
    /// Useful when timing data is not needed or handled elsewhere.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoggingInterceptor(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOrderedProxyInterceptor, LoggingInterceptor>());
        return services;
    }

    /// <summary>
    /// Adds only the timing interceptor without general logging.
    /// Useful for performance monitoring scenarios where only timing data is needed.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="slowThresholdMs">Threshold in milliseconds for slow operation warnings.</param>
    /// <param name="criticalThresholdMs">Threshold in milliseconds for critical operation errors.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTimingInterceptor(this IServiceCollection services, long slowThresholdMs = 1000, long criticalThresholdMs = 5000)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOrderedProxyInterceptor, TimingInterceptor>(provider =>
        {
            ILogger<TimingInterceptor> logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TimingInterceptor>>();
            return new TimingInterceptor(logger)
            {
                SlowOperationThresholdMs = slowThresholdMs,
                CriticalOperationThresholdMs = criticalThresholdMs
            };
        }));

        return services;
    }

    /// <summary>
    /// Adds null logging interceptors for scenarios where logging should be disabled.
    /// Provides minimal overhead while maintaining the interceptor contract.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNullLogging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IOrderedProxyInterceptor, NullLoggingInterceptor>();
        return services;
    }

    /// <summary>
    /// Adds custom logging interceptor with specific configuration.
    /// </summary>
    /// <typeparam name="TLoggingInterceptor">The type of logging interceptor to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLogging<TLoggingInterceptor>(this IServiceCollection services)
        where TLoggingInterceptor : class, IOrderedProxyInterceptor
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOrderedProxyInterceptor, TLoggingInterceptor>());
        return services;
    }

    /// <summary>
    /// Adds comprehensive logging with custom timing thresholds and additional interceptor types.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure logging behavior.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLogging(this IServiceCollection services, Action<LoggingOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        var options = new LoggingOptions();
        configureOptions(options);

        if (options.EnableStandardLogging)
        {
            services.AddLoggingInterceptor();
        }

        if (options.EnableTiming)
        {
            services.AddTimingInterceptor(options.SlowOperationThresholdMs, options.CriticalOperationThresholdMs);
        }

        return services;
    }

    // SOLID Principle - Explicit Interceptor Registration Methods

    /// <summary>
    /// Explicitly appends a logging interceptor. Existing registrations are retained.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection UseLoggingInterceptor(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Add logging interceptor (multiple interceptors can coexist)
        services.AddSingleton<IOrderedProxyInterceptor, LoggingInterceptor>();

        return services;
    }

    /// <summary>
    /// Explicitly appends a timing interceptor. Existing registrations are retained.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="slowThresholdMs">Threshold for slow operation warnings.</param>
    /// <param name="criticalThresholdMs">Threshold for critical operation errors.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection UseTimingInterceptor(
        this IServiceCollection services,
        long slowThresholdMs = 1000,
        long criticalThresholdMs = 5000)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Add timing interceptor with explicit configuration
        services.AddSingleton<IOrderedProxyInterceptor>(provider =>
        {
            ILogger<TimingInterceptor> logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TimingInterceptor>>();
            return new TimingInterceptor(logger)
            {
                SlowOperationThresholdMs = slowThresholdMs,
                CriticalOperationThresholdMs = criticalThresholdMs
            };
        });

        return services;
    }

    /// <summary>
    /// Convenience method to register default logging interceptors explicitly.
    /// Existing interceptors, including null implementations, remain registered.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection UseDefaultLoggingInterceptors(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.UseLoggingInterceptor();
        services.UseTimingInterceptor();

        return services;
    }
}
