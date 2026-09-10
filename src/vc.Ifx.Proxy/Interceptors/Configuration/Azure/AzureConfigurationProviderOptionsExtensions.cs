using VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Azure;

/// <summary>
/// Azure provider-specific validation extensions.
/// </summary>
public static class AzureConfigurationProviderOptionsExtensions
{
    public static void Validate(this AzureConfigurationProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.UseConnectionString)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new InvalidOperationException("ConnectionString must be provided when UseConnectionString is true.");
        }
        else if (options.Endpoint is null)
        {
            throw new InvalidOperationException("Endpoint must be provided when not using connection string authentication.");
        }

        if (string.IsNullOrWhiteSpace(options.Label))
            throw new InvalidOperationException("Label cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(options.SentinelKey))
            throw new InvalidOperationException("SentinelKey cannot be null or empty.");

        // Call shared validation
        ((ConfigurationProviderOptions)options).Validate();
    }
}
