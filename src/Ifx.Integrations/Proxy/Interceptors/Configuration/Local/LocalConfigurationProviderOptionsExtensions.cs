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
/// Local provider-specific validation extensions.
/// </summary>
public static class LocalConfigurationProviderOptionsExtensions
{
    public static void Validate(this LocalConfigurationProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.FilePath))
            throw new InvalidOperationException("FilePath cannot be null or empty.");

        if (options.AdditionalFiles is null || options.AdditionalFiles.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Additional file paths cannot be null or empty.");

        // Call shared validation
        ((ConfigurationProviderOptions)options).Validate();
    }
}
