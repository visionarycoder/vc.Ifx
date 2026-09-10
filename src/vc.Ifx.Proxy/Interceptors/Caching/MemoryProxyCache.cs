using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching;

/// <summary>Best-effort, instance-isolated response caching over a caller-owned memory cache.</summary>
public sealed class MemoryProxyCache(IMemoryCache cache, ILogger<MemoryProxyCache>? logger = null) : IProxyCache
{
    private readonly IMemoryCache cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<MemoryProxyCache> logger = logger ?? NullLogger<MemoryProxyCache>.Instance;
    private long generation;

    /// <inheritdoc />
    public Task<ProxyResponse<T>?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return Task.FromResult(cache.TryGetValue(Key(key), out object? value) && value is ProxyResponse<T> response
                ? Copy(response)
                : null);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Response cache read failed.");
            return Task.FromResult<ProxyResponse<T>?>(null);
        }
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, ProxyResponse<T> proxyResponse, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(proxyResponse);
        cancellationToken.ThrowIfCancellationRequested();
        if (expiration <= TimeSpan.Zero)
            return Task.CompletedTask;
        try
        {
            cache.Set(Key(key), Copy(proxyResponse), new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                Size = 1
            });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Response cache write failed.");
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            cache.Remove(Key(key));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Response cache removal failed.");
        }
        return Task.CompletedTask;
    }

    /// <summary>Atomically invalidates this instance's entries without clearing other users of the shared cache.</summary>
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Old generations are unreachable immediately and are reclaimed by normal expiration.
        Interlocked.Increment(ref generation);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return Task.FromResult(cache.TryGetValue(Key(key), out _));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Response cache existence check failed.");
            return Task.FromResult(false);
        }
    }

    private object Key(string key) => (this, Volatile.Read(ref generation), key);

    private static ProxyResponse<T> Copy<T>(ProxyResponse<T> response) => new()
    {
        Data = response.Data,
        IsSuccess = response.IsSuccess,
        ErrorMessage = response.ErrorMessage,
        StatusCode = response.StatusCode
    };
}
