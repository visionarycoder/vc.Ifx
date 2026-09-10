using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Local;

/// <summary>
/// Provides local file-based configuration operations following Microsoft configuration patterns.
/// This service wraps local JSON configuration files with logging, error handling, caching, and async support.
/// Supports file watching for automatic reloading and multiple configuration file sources.
/// </summary>
public sealed class LocalConfigurationProvider
    : ConfigurationProvider, IConfigurationProvider
{

    private new readonly LocalConfigurationProviderOptions options;
    private readonly FileSystemWatcher? fileWatcher;

    public LocalConfigurationProvider(LocalConfigurationProviderOptions options, ILogger<LocalConfigurationProvider> logger)
        : base(options, logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.options.Validate();
        configuration = BuildConfiguration();

        // Set up file watcher if reload on change is enabled
        if (options.ReloadOnChange)
        {
            fileWatcher = SetupFileWatcher();
        }
        Logger.LogInformation("Local App Configuration provider initialized for file {FilePath} with {AdditionalFileCount} additional files", options.FilePath, options.AdditionalFiles.Count());
    }

    public override string ProviderName => "Local";

    public bool IsAvailable
    {
        get
        {
            try
            {
                bool mainFileExists = File.Exists(GetFullPath(options.FilePath));
                return options.Optional || mainFileExists;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Local configuration health check failed");
                return false;
            }
        }
    }

    public override bool Refresh()
    {
        try
        {
            if (refreshSemaphore.Wait(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    // Clear cache to force refresh
                    if (options.EnableCaching)
                    {
                        ClearCache();
                    }

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

    public override async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (await refreshSemaphore.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken))
            {
                try
                {
                    // Clear cache to force refresh
                    if (options.EnableCaching)
                    {
                        ClearCache();
                    }

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

    public override T GetValue<T>(string key, T defaultValue)
    {
        try
        {
            string fullKey = GetFullKey(key);

            if (options.EnableCaching && TryGetFromCache(fullKey, out T cachedValue))
            {
                Logger.LogTrace("Cache hit for key '{Key}'", key);
                return cachedValue;
            }

            string? stringValue = configuration[fullKey];
            if (string.IsNullOrEmpty(stringValue))
            {
                Logger.LogWarning("Configuration key '{Key}' not found. Returning default value.", key);
                return defaultValue;
            }

            T value = ConfigurationHelper.ConvertValue(stringValue, defaultValue);
            if (options.EnableCaching)
            {
                AddToCache(fullKey, value);
                Logger.LogTrace("Cached value for key '{Key}'", key);
            }
            return value;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving configuration value for key '{Key}'. Returning default value.", key);
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

            T result = section.Get<T>() ?? new T();
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
        Logger.LogWarning("SetValue operation not supported by Local App Configuration provider. Modify the configuration files directly.");
        throw new NotSupportedException("Local App Configuration provider is read-only. Modify the configuration files directly.");
    }

    public bool UpdateSection<T>(string sectionName, T value)
    {
        Logger.LogWarning("UpdateSection operation not supported by Local App Configuration provider. Modify the configuration files directly.");
        throw new NotSupportedException("Local App Configuration provider is read-only. Modify the configuration files directly.");
    }

    protected override void Dispose(bool disposing)
    {
        if (!isDisposed && disposing)
        {
            fileWatcher?.Dispose();
            refreshSemaphore?.Dispose();
            ClearCache();
            isDisposed = true;
        }
        base.Dispose(disposing);
    }

    private string GetFullKey(string key)
    {
        if (string.IsNullOrEmpty(options.KeyPrefix))
            return key;

        return $"{options.KeyPrefix}:{key}";
    }

    private string GetFullPath(string filePath)
    {
        if (Path.IsPathRooted(filePath))
            return filePath;

        if (!string.IsNullOrEmpty(options.BasePath))
            return Path.Combine(options.BasePath, filePath);

        return Path.Combine(Directory.GetCurrentDirectory(), filePath);
    }

    private void OnConfigurationFileChanged(object sender, FileSystemEventArgs e)
    {
        Logger.LogDebug("Configuration file {FilePath} changed, clearing cache", e.FullPath);

        // Clear cache when file changes
        if (options.EnableCaching)
        {
            ClearCache();
        }

        lastRefresh = DateTimeOffset.UtcNow;
    }

    private IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder();

        // Set base path if provided
        if (!string.IsNullOrEmpty(options.BasePath))
        {
            builder.SetBasePath(options.BasePath);
        }

        try
        {
            // Add main configuration file
            builder.AddJsonFile(options.FilePath, optional: options.Optional, reloadOnChange: options.ReloadOnChange);

            // Add additional configuration files
            foreach (string additionalFile in options.AdditionalFiles)
            {
                builder.AddJsonFile(additionalFile, optional: true, reloadOnChange: options.ReloadOnChange);
            }

            // Add environment variables as fallback
            builder.AddEnvironmentVariables();

            return builder.Build();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to build local configuration");
            throw new InvalidOperationException("Failed to initialize Local App Configuration provider", ex);
        }
    }

    private FileSystemWatcher? SetupFileWatcher()
    {
        try
        {
            string filePath = GetFullPath(options.FilePath);
            string? directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                return null;

            if (!Directory.Exists(directory))
            {
                Logger.LogWarning("Directory {Directory} does not exist, file watcher will not be set up", directory);
                return null;
            }

            var watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnConfigurationFileChanged;

            Logger.LogDebug("File watcher set up for {FilePath}", filePath);
            return watcher;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to set up file watcher for {FilePath}", options.FilePath);
            return null;
        }
    }

}
