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

namespace Ifx.Proxy.Interceptor.Retries;

/// <summary>
/// Represents a transport exception that cannot be retried.
/// </summary>
public class NonRetryableTransportException : ProxyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NonRetryableTransportException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NonRetryableTransportException(string message) : base(message) { }

    /// <param name="message"></param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public NonRetryableTransportException(string message, Exception innerException) : base(message, innerException) { }
}
