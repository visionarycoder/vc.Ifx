namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

/// <summary>
/// Configuration options for Azure App Configuration service integration.
/// </summary>
public sealed record ConfigurationOptions
{
    /// <summary>
    /// The endpoint URI for the Azure App Configuration service.
    /// </summary>
    /// <example>https://your-config.azconfig.io</example>
    public Uri? Endpoint { get; init; }

    /// The label to use for environment-specific configuration (e.g., "Development", "Testing", "Staging", "Production").
    public string Label { get; init; } = "Production";

    /// The sentinel key used to trigger configuration refresh.
    public string SentinelKey { get; init; } = "App:Sentinel";

    /// The cache expiration time for configuration values.
    public TimeSpan CacheExpiration { get; init; } = TimeSpan.FromSeconds(30);

    /// Whether to use connection string authentication instead of managed identity.
    public bool UseConnectionString { get; init; } = false;

    /// The connection string for Azure App Configuration (when UseConnectionString is true).
    public string? ConnectionString { get; init; }
}
