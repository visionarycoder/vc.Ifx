namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

/// <summary>
/// Validation extension methods for configuration provider options.
/// Base/common validation lives here.
/// </summary>
public static class ConfigurationProviderOptionsExtensions
{
    public static void Validate(this ConfigurationProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.CacheExpiration <= TimeSpan.Zero)
            throw new InvalidOperationException("CacheExpiration must be greater than zero.");
    }
}
