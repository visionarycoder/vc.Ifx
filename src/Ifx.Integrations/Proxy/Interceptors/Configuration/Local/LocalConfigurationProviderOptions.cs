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

namespace Ifx.Proxy.Interceptor.Configuration.Local;

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
