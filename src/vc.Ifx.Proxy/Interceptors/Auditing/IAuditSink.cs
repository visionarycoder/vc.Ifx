namespace VisionaryCoder.Framework.Proxy.Interceptors.Auditing;

/// <summary>
/// Defines a contract for audit sinks.
/// </summary>
public interface IAuditSink
{
    /// <summary>
    /// Writes an audit record.
    /// </summary>
    /// <param name="auditRecord">The audit record to write.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteAsync(AuditRecord auditRecord, CancellationToken cancellationToken = default);
}
