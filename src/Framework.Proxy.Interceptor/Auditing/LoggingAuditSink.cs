using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Interceptor.Auditing.Abstractions;

namespace VisionaryCoder.Framework.Proxy.Interceptor.Auditing;
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
