// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.


// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching;

/// <summary>
/// Defines a contract for proxy caching operations that store and retrieve proxy responses.
/// Implementations should provide efficient, thread-safe caching with configurable expiration.
/// </summary>
public interface IProxyCache
{
    /// <summary>
    /// Gets a cached proxy response for the given key.
    /// </summary>
    /// <typeparam name="T">The type of the cached response data.</typeparam>
    /// <param name="key">The unique cache key to lookup.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The cached proxy response, or null if not found or expired.</returns>
    Task<ProxyResponse<T>?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a proxy response in the cache with the given key and expiration.
    /// </summary>
    /// <typeparam name="T">The type of the response data to cache.</typeparam>
    /// <param name="key">The unique cache key for storage.</param>
    /// <param name="proxyResponse">The proxy response to cache.</param>
    /// <param name="expiration">The cache expiration duration.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous cache operation.</returns>
    Task SetAsync<T>(string key, ProxyResponse<T> proxyResponse, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached item with the specified key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous removal operation.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cached items.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous clear operation.</returns>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key to check.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the key exists and is not expired; otherwise false.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}