using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Ifx.Proxy.Interceptor.Caching.Providers;

/// <summary>Registers a Redis-backed distributed proxy cache.</summary>
public static class RedisProxyCacheExtensions
{
    /// <summary>Registers a Redis L2 cache with memory L1 and Pub/Sub invalidation.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionMultiplexer">The shared Redis connection used by the cache and invalidation channel.</param>
    /// <param name="configureRedis">Additional Redis cache configuration.</param>
    /// <param name="configureCache">Distributed proxy cache key configuration.</param>
    /// <param name="configureCaching">Proxy cache policy configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRedisProxyCaching(
        this IServiceCollection services,
        IConnectionMultiplexer connectionMultiplexer,
        Action<RedisCacheOptions>? configureRedis = null,
        Action<DistributedProxyCacheOptions>? configureCache = null,
        Action<CachingOptions>? configureCaching = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionMultiplexer);

        services.AddCaching(configureCaching);
        services.AddStackExchangeRedisCache(options =>
        {
            configureRedis?.Invoke(options);
            options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer);
        });
        services.AddMemoryCache();
        services.AddOptions<DistributedProxyCacheOptions>()
            .Configure(options => configureCache?.Invoke(options))
            .Validate(options => { options.Validate(); return true; })
            .ValidateOnStart();
        services.RemoveAll<IProxyCache>();
        services.AddSingleton<DistributedProxyCache>(provider => new DistributedProxyCache(
            provider.GetRequiredService<IDistributedCache>(),
            provider.GetRequiredService<IMemoryCache>(),
            provider.GetRequiredService<IOptions<DistributedProxyCacheOptions>>(),
            connectionMultiplexer,
            provider.GetRequiredService<ILogger<DistributedProxyCache>>()));
        services.AddSingleton<IProxyCache>(provider => provider.GetRequiredService<DistributedProxyCache>());
        services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<DistributedProxyCache>());
        return services;
    }
}
