using Azure.Identity;
using Ifx;
using Ifx.Component;
using Ifx.Pipeline;
using Ifx.Proxy.Exceptions;
using Ifx.Querying;
using Ifx.Querying.Serialization;
using Ifx.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Threading.RateLimiting;

namespace Ifx.Proxy.Interceptor.Configuration.Azure;

/// <summary>
/// Configuration options for Azure App Configuration provider.
/// </summary>
public sealed class AzureConfigurationProviderOptions
    : ConfigurationProviderOptions
{
    /// <summary>
    /// The endpoint URI for the Azure App Configuration service.
    /// </summary>
    /// <example>https://your-config.azconfig.io</example>
    public Uri? Endpoint { get; init; }

    /// <summary>
    /// The label to use for environment-specific configuration (e.g., "Development", "Testing", "Staging", "Production").
    /// </summary>
    public string Label { get; init; } = "Production";

    /// <summary>
    /// The sentinel key used to trigger configuration refresh.
    /// </summary>
    public string SentinelKey { get; init; } = "App:Sentinel";

    /// <summary>
    /// Whether to use connection string authentication instead of managed identity.
    /// </summary>
    public bool UseConnectionString { get; init; } = false;

    /// <summary>
    /// The connection string for Azure App Configuration (when UseConnectionString is true).
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Whether to enable automatic refresh of configuration values.
    /// </summary>
    public bool EnableRefresh { get; init; } = true;
}
