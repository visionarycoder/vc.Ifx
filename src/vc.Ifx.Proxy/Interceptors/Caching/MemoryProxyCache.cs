// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching;

/// <summary>
/// In-memory implementation of <see cref="IProxyCache"/> using <see cref="IMemoryCache"/>.
/// Provides fast, local caching with automatic expiration and memory management.
/// </summary>
public sealed class MemoryProxyCache(IMemoryCache cache, ILogger<MemoryProxyCache>? logger = null) : IProxyCache
{

    private readonly IMemoryCache cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<MemoryProxyCache> logger = logger ?? new NullLogger<MemoryProxyCache>();

    /// <summary>
    /// Gets a cached proxy response for the given key.
    /// </summary>
    /// <typeparam name="T">The type of the cached response data.</typeparam>
    /// <param name="key">The unique cache key to lookup.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The cached proxy response, or null if not found or expired.</returns>
    public Task<ProxyResponse<T>?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (cache.TryGetValue(key, out object? cachedValue) && cachedValue is ProxyResponse<T> typedResponse)
            {
                logger?.LogDebug("Cache hit for key: {CacheKey}", key);
                return Task.FromResult<ProxyResponse<T>?>(typedResponse);
            }

            logger?.LogDebug("Cache miss for key: {CacheKey}", key);
            return Task.FromResult<ProxyResponse<T>?>(null);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error retrieving cached item with key: {CacheKey}", key);
            return Task.FromResult<ProxyResponse<T>?>(null);
        }
    }

    /// <summary>
    /// Sets a proxy response in the cache with the given key and expiration.
    /// </summary>
    /// <typeparam name="T">The type of the response data to cache.</typeparam>
    /// <param name="key">The unique cache key for storage.</param>
    /// <param name="proxyResponse">The proxy response to cache.</param>
    /// <param name="expiration">The cache expiration duration.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous cache operation.</returns>
    public Task SetAsync<T>(string key, ProxyResponse<T> proxyResponse, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(proxyResponse);
        cancellationToken.ThrowIfCancellationRequested();

        if (expiration <= TimeSpan.Zero)
        {
            logger?.LogWarning("Attempted to cache item with non-positive expiration: {Expiration}. Skipping cache.", expiration);
            return Task.CompletedTask;
        }

        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                Priority = CacheItemPriority.Normal
            };

            // Add eviction callback for logging if logger is available
            if (logger != null)
            {
                options.RegisterPostEvictionCallback(OnEviction);
            }

            cache.Set(key, proxyResponse, options);
            logger?.LogDebug("Cached item with key: {CacheKey}, expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error caching item with key: {CacheKey}", key);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a cached item with the specified key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous removal operation.</returns>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            cache.Remove(key);
            logger?.LogDebug("Removed cached item with key: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error removing cached item with key: {CacheKey}", key);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears all cached items by disposing and recreating the cache if possible.
    /// Note: This implementation cannot clear IMemoryCache entirely, this is a limitation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous clear operation.</returns>
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger?.LogWarning("ClearAsync called on MemoryProxyCache. IMemoryCache does not support clearing all entries.");

        // IMemoryCache doesn't have a Clear method, so we can't implement this fully
        // This would require tracking all keys or using a different cache implementation
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key to check.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the key exists and is not expired; otherwise false.</returns>
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            bool exists = cache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error checking existence of cached item with key: {CacheKey}", key);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Callback invoked when cache items are evicted.
    /// </summary>
    /// <param name="key">The cache key that was evicted.</param>
    /// <param name="value">The evicted value.</param>
    /// <param name="reason">The reason for eviction.</param>
    /// <param name="state">Additional state information.</param>
    private void OnEviction(object key, object? value, EvictionReason reason, object? state)
    {
        logger?.LogDebug("Cache item evicted - Key: {CacheKey}, Reason: {EvictionReason}", key, reason);
    }
}
