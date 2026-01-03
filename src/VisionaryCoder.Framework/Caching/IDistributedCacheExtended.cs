// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Caching;

/// <summary>
/// Extended distributed cache interface with additional Redis-specific operations.
/// </summary>
public interface IDistributedCacheExtended
{
    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a typed value from the cache using JSON deserialization.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in the cache with optional expiration.
    /// </summary>
    Task SetAsync(string key, byte[] value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a typed value in the cache using JSON serialization.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Refreshes the expiration time for a cached item.
    /// </summary>
    Task RefreshAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple values from the cache in a single operation.
    /// </summary>
    Task<Dictionary<string, byte[]?>> GetManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets multiple values in the cache in a single operation.
    /// </summary>
    Task SetManyAsync(Dictionary<string, byte[]> values, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple values from the cache in a single operation.
    /// </summary>
    Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments a numeric value in the cache atomically.
    /// </summary>
    Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decrements a numeric value in the cache atomically.
    /// </summary>
    Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);
}
