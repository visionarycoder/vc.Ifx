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

namespace Ifx.Proxy.Interceptor.Auditing;

/// <summary>
/// Configuration options for the auditing interceptor.
/// </summary>
public class AuditingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to include request headers in audit records.
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;
    /// Gets or sets a value indicating whether to include error details in audit records.
    public bool IncludeErrorDetails { get; set; } = true;
    /// Gets or sets a value indicating whether to include response data size in audit records.
    public bool IncludeResponseData { get; set; } = false;
}
