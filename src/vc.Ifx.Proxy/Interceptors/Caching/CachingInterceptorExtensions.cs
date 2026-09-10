using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching;
/// <summary>
/// Extension methods for adding caching interceptor services.
/// </summary>
public static class CachingInterceptorExtensions
{
    /// <summary>
    /// Adds the caching interceptor to the service collection with default options.
    /// </summary>
    /// <param name="services">The service collection to add the interceptor to.</param>
    /// <param name="configure">Optional configuration action for caching options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCachingInterceptor(
        this IServiceCollection services,
        Action<CachingOptions>? configure = null)
    {
        services.AddMemoryCache();

        if (configure != null)
        {
            services.Configure(configure);
        }
        services.AddSingleton<IProxyInterceptor>(provider =>
        {
            ILogger<CachingInterceptor> logger = provider.GetRequiredService<ILogger<CachingInterceptor>>();
            IMemoryCache cache = provider.GetRequiredService<IMemoryCache>();
            CachingOptions options = provider.GetService<IOptions<CachingOptions>>()?.Value ?? new CachingOptions();

            return new CachingInterceptor(
                logger,
                cache,
                options);
        });
        return services;
    }
    /// <summary>
    /// Adds the caching interceptor with specific configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="defaultCacheDuration">The default cache duration.</param>
    /// <param name="keyGenerator">Optional custom cache key generator.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCachingInterceptor(
        this IServiceCollection services,
        TimeSpan defaultCacheDuration,
        Func<ProxyContext, string>? keyGenerator = null)
    {
        return services.AddCachingInterceptor(options =>
        {
            options.DefaultDuration = defaultCacheDuration;
            options.KeyGenerator = keyGenerator;
        });
    }
}
