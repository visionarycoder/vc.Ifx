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
/// Default audit sink that logs audit records.
/// </summary>
/// <param name="logger">The logger instance.</param>
public sealed class LoggingAuditSink(ILogger<LoggingAuditSink> logger) : IAuditSink
{
    public Task WriteAsync(AuditRecord auditRecord, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Audit: {OperationName} | Result: {Result} | Timestamp: {Timestamp} | CorrelationId: {CorrelationId}", auditRecord.OperationName, auditRecord.Result, auditRecord.Timestamp, auditRecord.CorrelationId);
        return Task.CompletedTask;
    }
}
