using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Azure;

/// <summary>Read-only Azure configuration with explicit root reload and cache invalidation.</summary>
public sealed class AzureConfigurationProvider : ConfigurationProvider, IConfigurationProvider
{
    private readonly AzureConfigurationProviderOptions azureOptions;
    private readonly IConfigurationRoot root;
    private readonly bool ownsConfiguration;
    private readonly IDisposable reloadRegistration;

    public AzureConfigurationProvider(AzureConfigurationProviderOptions options, ILogger<AzureConfigurationProvider> logger)
        : this(options, logger, builder => builder.Build()) { }

    /// <summary>Builds an owned root through a host-supplied factory, allowing startup customization and offline tests.</summary>
    public AzureConfigurationProvider(AzureConfigurationProviderOptions options, ILogger<AzureConfigurationProvider> logger,
        Func<IConfigurationBuilder, IConfigurationRoot> configurationFactory)
        : this(options, logger, BuildConfiguration(options, configurationFactory), ownsConfiguration: true) { }

    /// <summary>Uses an existing configuration root; ownership remains with the caller unless explicitly transferred.</summary>
    public AzureConfigurationProvider(AzureConfigurationProviderOptions options, ILogger<AzureConfigurationProvider> logger,
        IConfigurationRoot configuration, bool ownsConfiguration = false) : base(options, logger)
    {
        options.Validate();
        root = configuration ?? throw new ArgumentNullException(nameof(configuration));
        azureOptions = options;
        this.configuration = root;
        this.ownsConfiguration = ownsConfiguration;
        reloadRegistration = ChangeToken.OnChange(root.GetReloadToken, ClearCache);
    }

    public override string ProviderName => "Azure";

    /// <summary>Checks local snapshot readability, not remote connectivity.</summary>
    public bool IsAvailable
    {
        get
        {
            if (isDisposed) return false;
            try { root.GetSection(string.Empty); return true; }
            catch (Exception) { return false; }
        }
    }

    public override T GetValue<T>(string key, T defaultValue)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        string fullKey = GetFullKey(key);
        if (azureOptions.EnableCaching && TryGetFromCache(fullKey, out T cached))
            return cached;
        string? text = configuration[fullKey];
        if (text is null) return defaultValue;
        T value = ConfigurationHelper.ConvertValue(text, defaultValue);
        if (azureOptions.EnableCaching) AddToCache(fullKey, value);
        return value;
    }

    public override T GetSection<T>(string sectionName)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        return configuration.GetSection(GetFullKey(sectionName)).Get<T>() ?? new T();
    }

    public override bool SetValue<T>(string key, T value)
        => throw new NotSupportedException("Azure configuration is read-only. Modify its source.");

    public override bool Refresh()
    {
        ThrowIfDisposed();
        if (!azureOptions.EnableRefresh) return true;
        refreshSemaphore.Wait();
        try { return Reload(); }
        finally { refreshSemaphore.Release(); }
    }

    public override async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        if (!azureOptions.EnableRefresh) return true;
        await refreshSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool result = Reload();
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
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
            Logger.LogError(exception, "Azure configuration reload failed.");
            return false;
        }
    }

    private static IConfigurationRoot BuildConfiguration(AzureConfigurationProviderOptions options, Func<IConfigurationBuilder, IConfigurationRoot> configurationFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configurationFactory);
        options.Validate();
        try { return configurationFactory(new ConfigurationBuilder().AddAzureAppConfiguration(sdk => options.ApplyTo(sdk))); }
        catch (Exception exception) { throw new InvalidOperationException("Failed to initialize Azure App Configuration provider", exception); }
    }

    private string GetFullKey(string key) => string.IsNullOrEmpty(azureOptions.KeyPrefix)
        ? key : $"{azureOptions.KeyPrefix.TrimEnd(':')}:{key}";

    protected override void Dispose(bool disposing)
    {
        // This sealed provider has no finalizer; only managed disposal is exposed.
        if (!isDisposed)
        {
            reloadRegistration.Dispose();
            if (ownsConfiguration) ((IDisposable)root).Dispose();
            refreshSemaphore.Dispose();
            ClearCache();
            isDisposed = true;
        }
        base.Dispose(disposing);
    }
}
