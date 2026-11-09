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

namespace Ifx.Proxy.Interceptor.Correlation;
/// <summary>
/// Default correlation ID generator that creates GUIDs.
/// </summary>
public sealed class GuidCorrelationIdGenerator : ICorrelationIdGenerator
{
    /// <inheritdoc />
    public string GenerateId()
    {
        return Guid.NewGuid().ToString("D");
    }
}
