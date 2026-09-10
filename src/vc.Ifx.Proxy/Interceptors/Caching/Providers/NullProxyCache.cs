// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

/// <summary>
/// Null Object implementation of <see cref="IProxyCache"/> that provides safe fallback behavior.
/// Does not cache anything and always returns cache misses when no explicit cache is registered.
/// Follows SOLID principles by ensuring safe operation without implicit defaults.
/// </summary>
public sealed class NullProxyCache : IProxyCache
{
    /// <summary>
    /// Always returns null to indicate cache miss.
    /// This ensures safe fallback behavior when no explicit cache provider is configured.
    /// </summary>
    /// <typeparam name="T">The type of the cached response data.</typeparam>
    /// <param name="key">The cache key (ignored in null implementation).</param>
    /// <param name="cancellationToken">Cancellation token (ignored in null implementation).</param>
    /// <returns>Always returns null to indicate cache miss.</returns>
    public Task<ProxyResponse<T>?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ProxyResponse<T>?>(null);
    }

    /// <summary>
    /// Does nothing since this is a null implementation.
    /// This ensures safe fallback behavior when no explicit cache provider is configured.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="key">The cache key (ignored in null implementation).</param>
    /// <param name="proxyResponse">The proxy response (ignored in null implementation).</param>
    /// <param name="expiration">The expiration time (ignored in null implementation).</param>
    /// <param name="cancellationToken">Cancellation token (ignored in null implementation).</param>
    /// <returns>Completed task indicating no-op operation.</returns>
    public Task SetAsync<T>(string key, ProxyResponse<T> proxyResponse, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        // Do nothing - this is a null cache
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing since this is a null implementation.
    /// This ensures safe fallback behavior when no explicit cache provider is configured.
    /// </summary>
    /// <param name="key">The cache key (ignored in null implementation).</param>
    /// <param name="cancellationToken">Cancellation token (ignored in null implementation).</param>
    /// <returns>Completed task indicating no-op operation.</returns>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // Do nothing - this is a null cache
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing since this is a null implementation.
    /// This ensures safe fallback behavior when no explicit cache provider is configured.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token (ignored in null implementation).</param>
    /// <returns>Completed task indicating no-op operation.</returns>
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        // Do nothing - this is a null cache
        return Task.CompletedTask;
    }

    /// <summary>
    /// Always returns false since nothing is ever cached.
    /// This ensures safe fallback behavior when no explicit cache provider is configured.
    /// </summary>
    /// <param name="key">The cache key (ignored in null implementation).</param>
    /// <param name="cancellationToken">Cancellation token (ignored in null implementation).</param>
    /// <returns>Always returns false to indicate no items exist.</returns>
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}