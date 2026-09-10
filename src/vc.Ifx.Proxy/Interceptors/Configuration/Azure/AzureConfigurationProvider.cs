using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Azure;

/// <summary>
/// Provides Azure App Configuration-based configuration operations following Microsoft configuration patterns.
/// This service wraps Azure App Configuration with logging, error handling, caching, and async support.
/// Supports both connection string and managed identity authentication with automatic refresh capabilities.
/// </summary>
public sealed class AzureConfigurationProvider
    : ConfigurationProvider, IConfigurationProvider
{

    private new readonly AzureConfigurationProviderOptions options;

    public AzureConfigurationProvider(AzureConfigurationProviderOptions options, ILogger<AzureConfigurationProvider> logger)
        : base(options, logger)
    {

        ArgumentNullException.ThrowIfNull(options);
        this.options = options;
        this.options.Validate();

        configuration = BuildConfiguration();

        Logger.LogInformation("Azure App Configuration provider initialized for endpoint {Endpoint} with label {Label}", options.Endpoint?.ToString() ?? "[Connection String]", options.Label);

    }

    public override string ProviderName => "Azure";

    public bool IsAvailable
    {
        get
        {
            try
            {
                // Simple health check - try to get a test value
                _ = configuration["__health_check__"];
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Azure App Configuration health check failed");
                return false;
            }
        }
    }

    public override T GetValue<T>(string key, T defaultValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            string fullKey = GetFullKey(key);

            // Check cache first
            if (options.EnableCaching && TryGetFromCache(fullKey, out T cachedValue))
            {
                Logger.LogTrace("Configuration value retrieved from cache for key {Key}", key);
                return cachedValue;
            }

            // Get from Azure App Configuration
            string? stringValue = configuration[fullKey];
            if (string.IsNullOrEmpty(stringValue))
            {
                Logger.LogDebug("Configuration key {Key} not found, returning default value", key);
                return defaultValue;
            }

            T result = ConfigurationHelper.ConvertValue(stringValue, defaultValue);

            // Cache the result
            if (options.EnableCaching)
            {
                AddToCache(fullKey, result);
            }

            Logger.LogTrace("Configuration value retrieved for key {Key}", key);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get configuration value for key {Key}", key);
            return defaultValue;
        }
    }

    public override T GetSection<T>(string sectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        try
        {
            string fullSectionName = GetFullKey(sectionName);
            IConfigurationSection section = configuration.GetSection(fullSectionName);

            if (!section.Exists())
            {
                Logger.LogDebug("Configuration section {SectionName} not found, returning new instance", sectionName);
                return new T();
            }

            T? result = section.Get<T>() ?? new T();
            Logger.LogTrace("Configuration section retrieved for {SectionName}", sectionName);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get configuration section {SectionName}", sectionName);
            return new T();
        }
    }

    public override bool SetValue<T>(string key, T value)
    {
        Logger.LogWarning("SetValue operation not supported by Azure App Configuration provider. Use Azure portal or REST API to modify values.");
        throw new NotSupportedException("Azure App Configuration provider is read-only. Use Azure portal or REST API to modify configuration values.");
    }

    public override async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {

        if (!options.EnableRefresh)
        {
            Logger.LogDebug("Configuration refresh is disabled");
            return true;
        }

        try
        {
            if (await refreshSemaphore.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken))
            {
                try
                {
                    // Check if we need to refresh based on cache expiration
                    if (DateTimeOffset.UtcNow - lastRefresh < options.CacheExpiration)
                    {
                        Logger.LogTrace("Configuration refresh skipped - cache is still valid");
                        return true;
                    }

                    // Clear cache to force refresh
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

    private IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder();

        try
        {
            builder.AddAzureAppConfiguration(configOptions =>
            {
                // Use connection string or managed identity based on options
                if (options.UseConnectionString && !string.IsNullOrEmpty(options.ConnectionString))
                {
                    configOptions.Connect(options.ConnectionString);
                }
                else
                {
                    // Use DefaultAzureCredential for managed identity support
                    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        ExcludeInteractiveBrowserCredential = true // Better for production scenarios
                    });
                    configOptions.Connect(options.Endpoint!, credential);
                }

                // Select keys with optional prefix and specified label
                string keyFilter = string.IsNullOrEmpty(options.KeyPrefix) ? "*" : $"{options.KeyPrefix}*";
                configOptions.Select(keyFilter, options.Label);

                // Configure refresh if enabled
                if (options.EnableRefresh)
                {
                    configOptions.ConfigureRefresh(refresh =>
                    {
                        refresh.Register(options.SentinelKey, options.Label).SetRefreshInterval(options.CacheExpiration);
                    });
                }
            });
            return builder.Build();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to build Azure App Configuration");
            throw new InvalidOperationException("Failed to initialize Azure App Configuration provider", ex);
        }
    }

    private string GetFullKey(string key)
    {
        if (string.IsNullOrEmpty(options.KeyPrefix))
            return key;

        return $"{options.KeyPrefix}:{key}";
    }

    protected override void Dispose(bool disposing)
    {
        if (!isDisposed && disposing)
        {
            refreshSemaphore?.Dispose();
            ClearCache();
            isDisposed = true;
        }

        base.Dispose(disposing);
    }

}
