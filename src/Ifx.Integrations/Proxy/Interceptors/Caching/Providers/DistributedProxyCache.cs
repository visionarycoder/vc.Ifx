using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ifx.Proxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Ifx.Proxy.Interceptor.Caching.Providers;

/// <summary>Stores proxy responses in an in-process L1 and Redis-backed L2 cache.</summary>
public sealed class DistributedProxyCache : IProxyCache, IHostedService
{
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IDistributedCache cache;
    private readonly IMemoryCache memoryCache;
    private readonly IProxyCacheInvalidationChannel invalidationChannel;
    private readonly ILogger<DistributedProxyCache> logger;
    private readonly string cacheNamespace;
    private readonly string instanceId = Guid.NewGuid().ToString("N");
    private readonly object sync = new();
    private readonly Queue<CacheInvalidation> pendingInvalidations = new();
    private long generation;
    private long invalidationVersion;
    private bool initialized;

    /// <summary>Creates a two-tier cache using the supplied Redis connection multiplexer.</summary>
    public DistributedProxyCache(
        IDistributedCache cache,
        IMemoryCache memoryCache,
        IOptions<DistributedProxyCacheOptions> options,
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<DistributedProxyCache>? logger = null)
        : this(
            cache,
            memoryCache,
            options,
            new RedisProxyCacheInvalidationChannel(
                (connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer))).GetSubscriber(),
                connectionMultiplexer.GetDatabase(),
                GetCacheNamespace(GetOptions(options)) + ":invalidation"),
            logger)
    {
    }

    internal DistributedProxyCache(
        IDistributedCache cache,
        IMemoryCache memoryCache,
        IOptions<DistributedProxyCacheOptions> options,
        IProxyCacheInvalidationChannel invalidationChannel,
        ILogger<DistributedProxyCache>? logger = null)
    {
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        ArgumentNullException.ThrowIfNull(options);
        this.invalidationChannel = invalidationChannel ?? throw new ArgumentNullException(nameof(invalidationChannel));
        this.logger = logger ?? NullLogger<DistributedProxyCache>.Instance;
        cacheNamespace = GetCacheNamespace(GetOptions(options));
    }

    /// <inheritdoc />
    public async Task<ProxyResponse<T>?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        string keyHash = GetKeyHash(key);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            long currentGeneration;
            long currentVersion;
            string cacheKey;
            byte[]? payload;
            lock (sync)
            {
                if (!initialized)
                {
                    throw new InvalidOperationException("The distributed proxy cache has not started.");
                }

                currentGeneration = generation;
                currentVersion = invalidationVersion;
                cacheKey = GetCacheKey(currentGeneration, keyHash);
                payload = memoryCache.Get<byte[]>(cacheKey);
            }

            if (payload is null)
            {
                payload = await cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
                if (payload is null)
                {
                    return null;
                }
            }

            CacheEntry<T> entry = Deserialize<T>(payload);
            if (entry.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            {
                memoryCache.Remove(cacheKey);
                return null;
            }

            lock (sync)
            {
                if (currentGeneration != generation || currentVersion != invalidationVersion)
                {
                    continue;
                }

                memoryCache.Set(cacheKey, payload, new MemoryCacheEntryOptions
                {
                    Size = 1,
                    AbsoluteExpiration = entry.ExpiresAtUtc
                });
                return entry.Response;
            }
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key,
        ProxyResponse<T> proxyResponse,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(proxyResponse);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiration, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();

        long currentGeneration;
        long currentVersion;
        lock (sync)
        {
            if (!initialized)
            {
                throw new InvalidOperationException("The distributed proxy cache has not started.");
            }

            currentGeneration = generation;
            currentVersion = invalidationVersion;
        }

        string keyHash = GetKeyHash(key);
        string cacheKey = GetCacheKey(currentGeneration, keyHash);
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.Add(expiration);
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(new CacheEntry<T>(expiresAt, proxyResponse), serializerOptions);
        var entryOptions = new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAt };
        await cache.SetAsync(cacheKey, payload, entryOptions, cancellationToken).ConfigureAwait(false);
        lock (sync)
        {
            if (currentGeneration == generation && currentVersion == invalidationVersion)
            {
                memoryCache.Set(cacheKey, payload, new MemoryCacheEntryOptions
                {
                    Size = 1,
                    AbsoluteExpiration = expiresAt
                });
            }
        }
        await PublishInvalidationAsync(new CacheInvalidation(instanceId, currentGeneration, keyHash, IsClear: false), CancellationToken.None)
            .ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        long currentGeneration = GetCurrentGeneration();
        string keyHash = GetKeyHash(key);
        await cache.RemoveAsync(GetCacheKey(currentGeneration, keyHash), cancellationToken).ConfigureAwait(false);
        lock (sync)
        {
            invalidationVersion++;
            memoryCache.Remove(GetCacheKey(currentGeneration, keyHash));
        }
        await PublishInvalidationAsync(new CacheInvalidation(instanceId, currentGeneration, keyHash, IsClear: false), CancellationToken.None)
            .ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = GetCurrentGeneration();
        long nextGeneration = await invalidationChannel.AdvanceGenerationAsync(cancellationToken).ConfigureAwait(false);
        SetGeneration(nextGeneration);
        await PublishInvalidationAsync(new CacheInvalidation(instanceId, nextGeneration, KeyHash: null, IsClear: true), CancellationToken.None)
            .ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        await GetAsync<JsonElement>(key, cancellationToken).ConfigureAwait(false) is not null;

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await invalidationChannel.SubscribeAsync(HandleInvalidation, cancellationToken).ConfigureAwait(false);
        long current = await invalidationChannel.GetGenerationAsync(cancellationToken).ConfigureAwait(false);

        lock (sync)
        {
            generation = Math.Max(generation, current);
            invalidationVersion++;
            initialized = true;
            while (pendingInvalidations.TryDequeue(out CacheInvalidation? invalidation))
            {
                ApplyInvalidation(invalidation);
            }
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) =>
        invalidationChannel.UnsubscribeAsync(HandleInvalidation, cancellationToken);

    private static DistributedProxyCacheOptions GetOptions(IOptions<DistributedProxyCacheOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Value.Validate();
    }

    private long GetCurrentGeneration()
    {
        lock (sync)
        {
            if (!initialized)
            {
                throw new InvalidOperationException("The distributed proxy cache has not started.");
            }

            return generation;
        }
    }

    private void HandleInvalidation(string payload)
    {
        CacheInvalidation invalidation;
        try
        {
            invalidation = JsonSerializer.Deserialize<CacheInvalidation>(payload, serializerOptions)
                ?? throw new JsonException("The invalidation message was empty.");
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Ignoring an invalid Redis proxy-cache invalidation message.");
            return;
        }

        if (invalidation.InstanceId == instanceId)
        {
            return;
        }

        lock (sync)
        {
            if (!initialized)
            {
                pendingInvalidations.Enqueue(invalidation);
                return;
            }

            ApplyInvalidation(invalidation);
        }
    }

    private void ApplyInvalidation(CacheInvalidation invalidation)
    {
        invalidationVersion++;
        if (invalidation.IsClear && invalidation.KeyHash is null)
        {
            generation = Math.Max(generation, invalidation.Generation);
            return;
        }

        if (invalidation.KeyHash is not null)
        {
            memoryCache.Remove(GetCacheKey(invalidation.Generation, invalidation.KeyHash));
        }
    }

    private Task PublishInvalidationAsync(CacheInvalidation invalidation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string payload = JsonSerializer.Serialize(invalidation, serializerOptions);
        return invalidationChannel.PublishAsync(payload, cancellationToken);
    }

    private CacheEntry<T> Deserialize<T>(byte[] payload) =>
        JsonSerializer.Deserialize<CacheEntry<T>>(payload, serializerOptions)
        ?? throw new InvalidDataException("The distributed cache contained an empty proxy response.");

    private string GetCacheKey(long currentGeneration, string keyHash) =>
        $"{cacheNamespace}:{currentGeneration}:{keyHash}";

    private static string GetKeyHash(string key) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));

    private static string GetCacheNamespace(DistributedProxyCacheOptions options) =>
        $"ifx:proxy-cache:{options.KeyPrefix}";

    private void SetGeneration(long value)
    {
        lock (sync)
        {
            generation = Math.Max(generation, value);
        }
    }

    private sealed record CacheEntry<T>(DateTimeOffset ExpiresAtUtc, ProxyResponse<T> Response);

    private sealed record CacheInvalidation(string InstanceId, long Generation, string? KeyHash, bool IsClear);
}
