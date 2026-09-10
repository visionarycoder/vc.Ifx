namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Auditing;

/// <summary>
/// Defines a contract for storing audit records of method invocations.
/// Implementations persist audit entries for compliance, troubleshooting, and forensic analysis.
/// </summary>
public interface IAuditSink
{
    /// <summary>
    /// Writes an audit entry to the audit store.
    /// </summary>
    /// <param name="entry">The audit entry to write.</param>
    ValueTask WriteAsync(AuditEntry entry);
}
