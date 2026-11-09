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

namespace Ifx.Proxy.Interceptor.Resilience;

/// <summary>
/// Rate limiter configuration for tracking request counts and time windows.
/// </summary>
public sealed class RateLimiterConfig
{
    /// <summary>
    /// Gets or sets the maximum number of requests allowed in the time window.
    /// </summary>
    public int MaxRequests { get; set; } = 100;
    /// Gets or sets the time window for rate limiting.
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(1);
}
