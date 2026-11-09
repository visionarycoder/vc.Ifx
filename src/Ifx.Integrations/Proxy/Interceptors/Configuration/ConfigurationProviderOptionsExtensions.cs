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
