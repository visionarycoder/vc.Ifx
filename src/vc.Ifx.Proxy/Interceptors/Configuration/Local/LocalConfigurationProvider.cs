using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Local;

/// <summary>Read-only JSON configuration with owned reload notifications and explicit refresh.</summary>
public sealed class LocalConfigurationProvider : ConfigurationProvider, IConfigurationProvider
{
    private readonly LocalConfigurationProviderOptions localOptions;
    private readonly IConfigurationRoot root;
    private readonly IDisposable? reloadRegistration;
    private readonly string fullPath;

    /// <summary>Loads the main JSON file, optional overlays, then environment overrides.</summary>
    public LocalConfigurationProvider(LocalConfigurationProviderOptions options, ILogger<LocalConfigurationProvider> logger)
        : base(options, logger)
    {
        options.Validate();
        localOptions = options;
        string basePath = Path.GetFullPath(options.BasePath ?? Directory.GetCurrentDirectory());
        fullPath = Path.GetFullPath(options.FilePath, basePath);
        var builder = new ConfigurationBuilder().SetBasePath(basePath)
            .AddJsonFile(options.FilePath, options.Optional, options.ReloadOnChange);
        foreach (string file in options.AdditionalFiles)
            builder.AddJsonFile(file, optional: true, reloadOnChange: options.ReloadOnChange);
        root = builder.AddEnvironmentVariables().Build();
        configuration = root;
        if (options.ReloadOnChange)
            reloadRegistration = ChangeToken.OnChange(root.GetReloadToken, ClearCache);
    }

    /// <inheritdoc />
    public override string ProviderName => "Local";

    /// <summary>Indicates whether this provider's required main file exists.</summary>
    public bool IsAvailable => !isDisposed && (localOptions.Optional || File.Exists(fullPath));

    /// <inheritdoc />
    public override bool Refresh()
    {
        ThrowIfDisposed();
        refreshSemaphore.Wait();
        try { return Reload(); }
        finally { refreshSemaphore.Release(); }
    }

    /// <inheritdoc />
    public override async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await refreshSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try { return Reload(); }
        finally { refreshSemaphore.Release(); }
    }

    private bool Reload()
    {
        try
        {
            root.Reload();
            ClearCache();
            lastRefresh = DateTimeOffset.UtcNow;
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Local configuration reload failed.");
            return false;
        }
    }

    /// <inheritdoc />
    public override T GetValue<T>(string key, T defaultValue)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        string fullKey = GetFullKey(key);
        if (localOptions.EnableCaching && TryGetFromCache(fullKey, out T cached))
            return cached;
        string? text = configuration[fullKey];
        if (text is null)
            return defaultValue;
        T value = ConfigurationHelper.ConvertValue(text, defaultValue);
        if (localOptions.EnableCaching)
            AddToCache(fullKey, value);
        return value;
    }

    /// <inheritdoc />
    public override T GetSection<T>(string sectionName)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        return configuration.GetSection(GetFullKey(sectionName)).Get<T>() ?? new T();
    }

    /// <inheritdoc />
    public override bool SetValue<T>(string key, T value)
        => throw new NotSupportedException("Local configuration is read-only. Modify its source files.");

    /// <summary>Rejects writes because this provider is read-only.</summary>
    public bool UpdateSection<T>(string sectionName, T value)
        => throw new NotSupportedException("Local configuration is read-only. Modify its source files.");

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // This sealed provider has no finalizer; only managed disposal is exposed.
        if (!isDisposed)
        {
            reloadRegistration?.Dispose();
            ((IDisposable)root).Dispose();
            refreshSemaphore.Dispose();
            ClearCache();
            isDisposed = true;
        }
        base.Dispose(disposing);
    }

    private string GetFullKey(string key) => string.IsNullOrEmpty(localOptions.KeyPrefix)
        ? key
        : $"{localOptions.KeyPrefix.TrimEnd(':')}:{key}";
}
