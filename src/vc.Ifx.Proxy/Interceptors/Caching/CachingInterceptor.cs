// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching;

/// <summary>
/// Interceptor that provides intelligent caching for proxy operations to improve performance.
/// Uses configurable cache policies and providers for flexible caching strategies.
/// </summary>
public sealed class CachingInterceptor : IOrderedProxyInterceptor
{
    private sealed class DelegateCacheKeyProvider(Func<ProxyContext, string> keyGenerator) : ICacheKeyProvider
    {
        public string? GenerateKey(ProxyContext context) => keyGenerator(context);
    }

    /// <inheritdoc />
    public int Order => 50;

    private readonly ILogger<CachingInterceptor> logger;
    private readonly IProxyCache proxyCache;
    private readonly ICacheKeyProvider keyProvider;
    private readonly ICachePolicyProvider policyProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingInterceptor"/> class.
    /// </summary>
    public CachingInterceptor(
        ILogger<CachingInterceptor> logger,
        IProxyCache proxyCache,
        ICacheKeyProvider keyProvider,
        ICachePolicyProvider policyProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.proxyCache = proxyCache ?? throw new ArgumentNullException(nameof(proxyCache));
        this.keyProvider = keyProvider ?? throw new ArgumentNullException(nameof(keyProvider));
        this.policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
    }

    /// <summary>
    /// Backward-compatible constructor that adapts IMemoryCache and options into provider-based caching services.
    /// </summary>
    public CachingInterceptor(
        ILogger<CachingInterceptor> logger,
        IMemoryCache cache,
        CachingOptions options)
        : this(
            logger,
            new MemoryProxyCache(cache ?? throw new ArgumentNullException(nameof(cache))),
            CreateKeyProvider(options),
            new DefaultCachePolicyProvider(options))
    {
    }

    /// <summary>
    /// Invokes the caching interceptor logic for proxy operations.
    /// Attempts cache retrieval before execution and caches successful responses.
    /// </summary>
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        cancellationToken.ThrowIfCancellationRequested();
        string operationName = context.OperationName ?? "Unknown";
        string correlationId = context.CorrelationId ?? "None";

        if (IsCachingDisabled(context))
        {
            logger.LogDebug("Caching disabled for operation '{OperationName}'. Correlation ID: '{CorrelationId}'",
                operationName, correlationId);
            return await next(context, cancellationToken);
        }

        CachePolicy cachePolicy = policyProvider.GetPolicy(context);
        if (!cachePolicy.IsCachingEnabled || !policyProvider.ShouldCache(context))
        {
            logger.LogDebug("Caching policy disabled for operation '{OperationName}'. Correlation ID: '{CorrelationId}'",
                operationName, correlationId);
            return await next(context, cancellationToken);
        }

        string? cacheKey = keyProvider.GenerateKey(context);
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            logger.LogDebug("Cache key generation bypassed caching for operation '{OperationName}'. Correlation ID: '{CorrelationId}'",
                operationName, correlationId);
            return await next(context, cancellationToken);
        }

        ProxyResponse<T>? cachedResponse = await proxyCache.GetAsync<T>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            logger.LogDebug("Cache hit for operation '{OperationName}' with key '{CacheKey}'. Correlation ID: '{CorrelationId}'",
                operationName, cacheKey, correlationId);
            context.Metadata["CacheHit"] = true;
            context.Metadata["CacheKey"] = cacheKey;
            return cachedResponse;
        }

        logger.LogDebug("Cache miss for operation '{OperationName}' with key '{CacheKey}'. Correlation ID: '{CorrelationId}'",
            operationName, cacheKey, correlationId);

        ProxyResponse<T> response = await next(context, cancellationToken);
        if (ShouldCacheResponse(response, cachePolicy))
        {
            TimeSpan expiration = GetExpiration(context, cachePolicy);
            if (expiration > TimeSpan.Zero)
            {
                await proxyCache.SetAsync(cacheKey, response, expiration, cancellationToken);
                logger.LogDebug("Cached successful response for operation '{OperationName}' with key '{CacheKey}' for {Duration}. Correlation ID: '{CorrelationId}'",
                    operationName, cacheKey, expiration, correlationId);
            }
        }

        context.Metadata["CacheHit"] = false;
        context.Metadata["CacheKey"] = cacheKey;
        return response;
    }

    private static ICacheKeyProvider CreateKeyProvider(CachingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.KeyGenerator is null
            ? new DefaultCacheKeyProvider()
            : new DelegateCacheKeyProvider(options.KeyGenerator);
    }

    private static TimeSpan GetExpiration(ProxyContext context, CachePolicy cachePolicy)
    {
        if (context.Metadata.TryGetValue("CacheDurationSeconds", out object? durationObj) &&
            durationObj is int seconds &&
            seconds > 0)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        return cachePolicy.Duration;
    }

    private static bool IsCachingDisabled(ProxyContext context)
    {
        if (context.Metadata.TryGetValue("DisableCache", out object? disableCache) &&
            disableCache is bool and true)
        {
            return true;
        }

        return (context.Headers ?? [])
            .Where(header => header.Key.Equals("Cache-Control", StringComparison.OrdinalIgnoreCase))
            .SelectMany(header => (header.Value ?? string.Empty).Split(','))
            .Select(directive => directive.Split('=', 2)[0].Trim())
            .Any(directive => directive.Equals("no-cache", StringComparison.OrdinalIgnoreCase) ||
                directive.Equals("no-store", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ShouldCacheResponse<T>(ProxyResponse<T> response, CachePolicy policy)
    {
        if (!response.IsSuccess)
        {
            return false;
        }

        return response.Data == null || policy.ShouldCache(response.Data);
    }
}
