using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

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
        else if (!options.Endpoint.IsAbsoluteUri || options.Endpoint.Scheme != Uri.UriSchemeHttps ||
            options.Endpoint.UserInfo.Length != 0 || options.Endpoint.Query.Length != 0 || options.Endpoint.Fragment.Length != 0)
        {
            throw new InvalidOperationException("Endpoint must be an absolute HTTPS URI without credentials, query or fragment.");
        }

        if (string.IsNullOrWhiteSpace(options.Label))
            throw new InvalidOperationException("Label cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(options.SentinelKey))
            throw new InvalidOperationException("SentinelKey cannot be null or empty.");

        // Call shared validation
        ((ConfigurationProviderOptions)options).Validate();
    }

    /// <summary>Applies validated connection, selector and sentinel settings without loading remote configuration.</summary>
    public static AzureAppConfigurationOptions ApplyTo(this AzureConfigurationProviderOptions options, AzureAppConfigurationOptions clientOptions)
    {
        options.Validate();
        ArgumentNullException.ThrowIfNull(clientOptions);
        if (options.UseConnectionString)
            clientOptions.Connect(options.ConnectionString);
        else
            clientOptions.Connect(options.Endpoint!, new DefaultAzureCredential(new DefaultAzureCredentialOptions { ExcludeInteractiveBrowserCredential = true }));
        string prefix = options.KeyPrefix?.TrimEnd(':') ?? string.Empty;
        clientOptions.Select(string.IsNullOrEmpty(prefix) ? "*" : $"{prefix}:*", options.Label);
        if (options.EnableRefresh)
            clientOptions.ConfigureRefresh(refresh => refresh.Register(options.SentinelKey, options.Label, refreshAll: true).SetRefreshInterval(options.CacheExpiration));
        return clientOptions;
    }
}
