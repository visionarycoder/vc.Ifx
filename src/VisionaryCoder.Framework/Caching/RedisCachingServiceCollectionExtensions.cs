// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace VisionaryCoder.Framework.Caching;

/// <summary>
/// Extension methods for configuring Redis distributed caching services.
/// </summary>
public static class RedisCachingServiceCollectionExtensions
{
    /// <summary>
    /// Adds Redis distributed caching with connection string.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">Redis connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        // Add Redis connection
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(connectionString));

        // Add Microsoft's Redis distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });

        // Add extended distributed cache
        services.TryAddSingleton<IDistributedCacheExtended, RedisDistributedCache>();

        return services;
    }

    /// <summary>
    /// Adds Redis distributed caching with configuration options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for Redis options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        Action<ConfigurationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ConfigurationOptions();
        configure(options);

        // Add Redis connection
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(options));

        // Add Microsoft's Redis distributed cache
        services.AddStackExchangeRedisCache(cacheOptions =>
        {
            cacheOptions.ConfigurationOptions = options;
        });

        // Add extended distributed cache
        services.TryAddSingleton<IDistributedCacheExtended, RedisDistributedCache>();

        return services;
    }

    /// <summary>
    /// Adds Redis distributed caching for Azure Redis Cache with managed identity.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="hostName">Azure Redis Cache hostname (e.g., mycache.redis.cache.windows.net).</param>
    /// <param name="useSsl">Whether to use SSL connection. Default: true.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureRedisCache(
        this IServiceCollection services,
        string hostName,
        bool useSsl = true)
    {
        ArgumentNullException.ThrowIfNull(hostName);

        var options = new ConfigurationOptions
        {
            EndPoints = { hostName },
            Ssl = useSsl,
            AbortOnConnectFail = false,
            ConnectRetry = 3,
            ConnectTimeout = 5000,
            SyncTimeout = 5000
        };

        return services.AddRedisCache(o =>
        {
            o.EndPoints.Add(options.EndPoints[0]);
            o.Ssl = options.Ssl;
            o.AbortOnConnectFail = options.AbortOnConnectFail;
            o.ConnectRetry = options.ConnectRetry;
            o.ConnectTimeout = options.ConnectTimeout;
            o.SyncTimeout = options.SyncTimeout;
        });
    }
}
