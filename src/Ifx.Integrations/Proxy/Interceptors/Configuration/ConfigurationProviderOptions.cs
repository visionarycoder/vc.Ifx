using Ifx;
using Ifx.Component;
using Ifx.Pipeline;
using Ifx.Proxy.Exceptions;
using Ifx.Querying;
using Ifx.Querying.Serialization;
using Ifx.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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

namespace Ifx.Proxy.Interceptor.Configuration;

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
