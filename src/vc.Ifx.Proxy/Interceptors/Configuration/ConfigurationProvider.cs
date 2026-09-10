using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

public abstract class ConfigurationProvider
    : ServiceBase<ConfigurationProvider>, IConfigurationProvider
{

    protected internal IConfiguration configuration = null!;
    private protected readonly ConcurrentDictionary<string, (object? Value, DateTimeOffset CachedAt)> cache = new();
    private protected readonly SemaphoreSlim refreshSemaphore = new(1, 1);
    protected DateTimeOffset lastRefresh = DateTimeOffset.UtcNow;
    private protected bool isDisposed;
    private protected ConfigurationProviderOptions options;

    /// <inheritdoc/>
    public virtual string ProviderName => string.Empty;

    /// <inheritdoc/>
    protected ConfigurationProvider(ConfigurationProviderOptions options, ILogger<ConfigurationProvider> logger)
    : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.options.Validate();
    }

    /// <inheritdoc/>
    public abstract T GetValue<T>(string key, T defaultValue) where T : class, new();

    /// <inheritdoc/>
    public virtual async Task<T> GetValueAsync<T>(string key, T defaultValue = default!, CancellationToken cancellationToken = default) where T : class, new()
    {
        return cancellationToken.IsCancellationRequested
            ? throw new OperationCanceledException(cancellationToken)
            : await Task.FromResult(GetValue(key, defaultValue));
    }

    /// <inheritdoc/>
    public abstract bool SetValue<T>(string key, T value) where T : class, new();

    /// <inheritdoc/>
    public virtual async Task<bool> SetValueAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class, new()
    {
        return cancellationToken.IsCancellationRequested
            ? throw new OperationCanceledException(cancellationToken)
            : await Task.FromResult(SetValue(key, value));
    }

    /// <inheritdoc/>
    public abstract T GetSection<T>(string sectionName) where T : class, new();

    /// <inheritdoc/>
    public virtual async Task<T> GetSectionAsync<T>(string sectionName, CancellationToken cancellationToken = default) where T : class, new()
    {
        return cancellationToken.IsCancellationRequested
            ? throw new OperationCanceledException(cancellationToken)
            : await Task.FromResult(GetSection<T>(sectionName));
    }

    /// <inheritdoc/>
    public IDictionary<string, object?> GetAllValues()
    {
        try
        {
            var result = new Dictionary<string, object?>();
            string keyPrefix = options.KeyPrefix ?? string.Empty;
            bool hasPrefix = !string.IsNullOrEmpty(keyPrefix);
            int prefixLength = hasPrefix ? keyPrefix.Length : 0;

            foreach (KeyValuePair<string, string?> kvp in configuration.AsEnumerable())
            {
                if (string.IsNullOrEmpty(kvp.Key))
                    continue;

                if (hasPrefix && !kvp.Key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string cleanKey = hasPrefix
                    ? kvp.Key.Length > prefixLength ? kvp.Key[prefixLength..].TrimStart(':') : string.Empty
                    : kvp.Key;

                result[cleanKey] = kvp.Value;
            }

            Logger.LogTrace("Retrieved {Count} configuration values", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get all configuration values");
            return new Dictionary<string, object?>();
        }
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, object?>> GetAllValuesAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(GetAllValues());
    }

    /// <inheritdoc/>
    public virtual bool Refresh()
    {
        // Default refresh behavior simply clears cache and updates timestamp
        try
        {
            if (refreshSemaphore.Wait(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    ClearCache();
                    lastRefresh = DateTimeOffset.UtcNow;
                    Logger.LogDebug("Configuration refreshed successfully");
                    return true;
                }
                finally
                {
                    refreshSemaphore.Release();
                }
            }
            Logger.LogWarning("Configuration refresh timeout - another refresh operation is in progress");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to refresh configuration");
            return false;
        }
    }

    /// <inheritdoc/>
    public abstract Task<bool> RefreshAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Try get a cached value of type T using the provider's CacheExpiration option.
    /// </summary>
    protected bool TryGetFromCache<T>(string key, out T value)
    {
        value = default!;

        if (!cache.TryGetValue(key, out (object? Value, DateTimeOffset CachedAt) cached))
            return false;

        // Check if cache entry is expired
        if (DateTimeOffset.UtcNow - cached.CachedAt > options.CacheExpiration)
        {
            cache.TryRemove(key, out _);
            return false;
        }

        if (cached.Value is T typedValue)
        {
            value = typedValue;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Add or update cache entry with current timestamp.
    /// </summary>
    protected void AddToCache(string key, object? value)
    {
        cache[key] = (value, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Clear the shared cache.
    /// </summary>
    protected void ClearCache()
    {
        cache.Clear();
    }

}
