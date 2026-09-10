namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Local;

/// <summary>
/// Configuration options for Local (file-based) App Configuration provider.
/// </summary>
public sealed class LocalConfigurationProviderOptions : ConfigurationProviderOptions
{
    /// <summary>
    /// The file path for the configuration file.
    /// </summary>
    public string FilePath { get; init; } = "appsettings.json";

    /// <summary>
    /// Whether to watch the file for changes and automatically reload.
    /// </summary>
    public bool ReloadOnChange { get; init; } = true;

    /// <summary>
    /// Whether the configuration file is optional.
    /// </summary>
    public bool Optional { get; init; } = false;

    /// <summary>
    /// Additional configuration files to include (e.g., environment-specific files).
    /// </summary>
    public IEnumerable<string> AdditionalFiles { get; init; } = [];

    /// <summary>
    /// The base path for configuration files.
    /// </summary>
    public string? BasePath { get; init; }

}
