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
/// Default implementation of correlation context using AsyncLocal.
/// </summary>
public sealed class DefaultCorrelationContext : ICorrelationContext
{
    private static readonly AsyncLocal<string?> correlationId = new();
    private static readonly AsyncLocal<Dictionary<string, string>?> data = new();
    
    /// <inheritdoc />
    public string? CorrelationId 
    { 
        get => correlationId.Value;
        set => correlationId.Value = value;
    }
    
    /// <inheritdoc />
    public Dictionary<string, string> Data 
    { 
        get => data.Value ??= new Dictionary<string, string>();
        set => data.Value = value;
    }
    
    public void SetCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
    }
}
