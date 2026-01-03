// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Auditing;
/// <summary>
/// Auditing interceptor that emits audit records for proxy operations.
/// Order: 300 (executes last in the pipeline).
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="auditSinks">The audit sinks.</param>
public sealed class AuditingInterceptor(ILogger<AuditingInterceptor> logger, IEnumerable<IAuditSink> auditSinks) : IOrderedProxyInterceptor
{
    private readonly ILogger<AuditingInterceptor> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IEnumerable<IAuditSink> auditSinks = auditSinks ?? throw new ArgumentNullException(nameof(auditSinks));
    /// <inheritdoc />
    public int Order => 300;
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        string requestType = context.Request?.GetType().Name ?? "Unknown";
        string correlationId = context.Items.TryGetValue("CorrelationId", out object? corrId) ?
            corrId?.ToString() ?? Guid.NewGuid().ToString("D") :
            Guid.NewGuid().ToString("D");

        DateTime startTime = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            ProxyResponse<T> result = await next(context, cancellationToken);

            stopwatch.Stop();
            // Create audit record
            var auditRecord = new AuditRecord
            {
                CorrelationId = correlationId,
                MethodName = $"Proxy.{requestType}",
                Timestamp = startTime,
                Success = result.IsSuccess,
                ErrorMessage = result.IsSuccess ? null : "Operation failed",
                Duration = stopwatch.Elapsed,
                CompletedAt = DateTime.UtcNow
            };
            // Emit to all audit sinks
            await EmitAuditRecord(auditRecord, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            // Create audit record for exception
            var auditRecord = new AuditRecord
            {
                CorrelationId = correlationId,
                MethodName = $"Proxy.{requestType}",
                Timestamp = startTime,
                Success = false,
                ErrorMessage = ex.Message,
                Duration = stopwatch.Elapsed,
                CompletedAt = DateTime.UtcNow,
                ExceptionType = ex.GetType().Name
            };
            // Emit to all audit sinks (best effort, don't let audit failure affect the operation)
            try
            {
                await EmitAuditRecord(auditRecord, cancellationToken);
            }
            catch (Exception auditEx)
            {
                logger.LogWarning(auditEx, "Failed to emit audit record for failed operation");
            }
            throw;
        }
    }
    private async Task EmitAuditRecord(AuditRecord auditRecord, CancellationToken cancellationToken = default)
    {
        foreach (IAuditSink sink in auditSinks)
        {
            try
            {
                await sink.WriteAsync(auditRecord, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to emit audit record to sink {SinkType}", sink.GetType().Name);
            }
        }
    }
    private static Dictionary<string, object?> CreateMetadata(
        ProxyContext context,
        object? result = null,
        Exception? exception = null)
    {
        var metadata = new Dictionary<string, object?>
        {
            ["ResultType"] = context.ResultType?.Name ?? "Unknown"
        };
        // Add context items (excluding sensitive data)
        foreach (KeyValuePair<string, object?> item in context.Items.Where(kvp => !IsSensitiveKey(kvp.Key)))
        {
            metadata[$"Context.{item.Key}"] = item.Value;
        }
        if (exception != null)
        {
            metadata["Exception.Type"] = exception.GetType().Name;
            metadata["Exception.StackTrace"] = exception.StackTrace;
        }
        return metadata;
    }
    private static bool IsSensitiveKey(string key)
    {
        string[] sensitiveKeys = ["Authorization", "Password", "Secret", "Token", "Key"];
        return sensitiveKeys.Any(sensitive =>
            key.Contains(sensitive, StringComparison.OrdinalIgnoreCase));
    }
}
