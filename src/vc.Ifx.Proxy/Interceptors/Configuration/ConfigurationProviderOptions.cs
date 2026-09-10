using System;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

/// <summary>
/// Base configuration options shared by provider options.
/// </summary>
public abstract class ConfigurationProviderOptions
{
    /// <summary>
    /// The prefix to filter configuration keys (optional).
    /// </summary>
    public string? KeyPrefix { get; init; }

    /// <summary>
    /// Whether to enable caching of configuration values.
    /// </summary>
    public bool EnableCaching { get; init; } = true;

    /// <summary>
    /// The cache expiration time for configuration values.
    /// Providers should validate this value via validation extensions.
    /// </summary>
    public TimeSpan CacheExpiration { get; init; } = TimeSpan.FromMinutes(5);
}
