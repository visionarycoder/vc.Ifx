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

namespace Ifx.Proxy.Interceptor.Logging;
/// <summary>
/// Extension methods for adding logging interceptor services.
/// </summary>
public static class LoggingInterceptorExtensions
{
    /// <summary>
    /// Adds the logging interceptor to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the interceptor to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoggingInterceptor(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddLogging();
        services.AddSingleton<IProxyInterceptor, LoggingInterceptor>();
        return services;
    }
}
