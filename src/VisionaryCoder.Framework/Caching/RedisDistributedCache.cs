// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace VisionaryCoder.Framework.Caching;

/// <summary>
/// Redis-based implementation of extended distributed cache.
/// </summary>
public sealed class RedisDistributedCache : IDistributedCacheExtended
{
    private readonly IDistributedCache distributedCache;
    private readonly IConnectionMultiplexer redis;
    private readonly ILogger<RedisDistributedCache> logger;
    private readonly JsonSerializerOptions jsonOptions;

    public RedisDistributedCache(
        IDistributedCache distributedCache,
        IConnectionMultiplexer redis,
        ILogger<RedisDistributedCache> logger)
    {
        this.distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        this.redis = redis ?? throw new ArgumentNullException(nameof(redis));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            return await distributedCache.GetAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting cache key {Key}", key);
            return null;
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        byte[]? data = await GetAsync(key, cancellationToken);
        
        if (data == null || data.Length == 0)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(data, jsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deserializing cache key {Key} to type {Type}", key, typeof(T).Name);
            return null;
        }
    }

    public async Task SetAsync(string key, byte[] value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }

            await distributedCache.SetAsync(key, value, options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting cache key {Key}", key);
            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            byte[] data = JsonSerializer.SerializeToUtf8Bytes(value, jsonOptions);
            await SetAsync(key, data, expiration, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error serializing and setting cache key {Key} for type {Type}", key, typeof(T).Name);
            throw;
        }
    }

    public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            await distributedCache.RefreshAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing cache key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache key {Key}", key);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            IDatabase db = redis.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking existence of cache key {Key}", key);
            return false;
        }
    }

    public async Task<Dictionary<string, byte[]?>> GetManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var result = new Dictionary<string, byte[]?>();

        try
        {
            IDatabase db = redis.GetDatabase();
            RedisKey[] redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            RedisValue[] values = await db.StringGetAsync(redisKeys);

            for (int i = 0; i < redisKeys.Length; i++)
            {
                result[redisKeys[i]!] = values[i].HasValue ? (byte[]?)values[i] : null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting multiple cache keys");
        }

        return result;
    }

    public async Task SetManyAsync(Dictionary<string, byte[]> values, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        try
        {
            IDatabase db = redis.GetDatabase();
            IBatch batch = db.CreateBatch();
            
            var tasks = new List<Task>();
            foreach (var kvp in values)
            {
                Task task = batch.StringSetAsync(kvp.Key, kvp.Value, expiration);
                tasks.Add(task);
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting multiple cache keys");
            throw;
        }
    }

    public async Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);

        try
        {
            IDatabase db = redis.GetDatabase();
            RedisKey[] redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            await db.KeyDeleteAsync(redisKeys);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing multiple cache keys");
            throw;
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            IDatabase db = redis.GetDatabase();
            return await db.StringIncrementAsync(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error incrementing cache key {Key}", key);
            throw;
        }
    }

    public async Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            IDatabase db = redis.GetDatabase();
            return await db.StringDecrementAsync(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error decrementing cache key {Key}", key);
            throw;
        }
    }
}
