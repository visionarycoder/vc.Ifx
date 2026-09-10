using VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Local;

/// <summary>
/// Local provider-specific validation extensions.
/// </summary>
public static class LocalConfigurationProviderOptionsExtensions
{
    public static void Validate(this LocalConfigurationProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.FilePath))
            throw new InvalidOperationException("FilePath cannot be null or empty.");

        if (options.AdditionalFiles.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Additional file paths cannot be null or empty.");

        // Call shared validation
        ((ConfigurationProviderOptions)options).Validate();
    }
}
