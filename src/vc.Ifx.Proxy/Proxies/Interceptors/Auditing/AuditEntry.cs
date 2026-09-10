namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Auditing;

/// <summary>
/// Represents a single audit entry recording a method invocation.
/// </summary>
/// <param name="CorrelationId">The correlation identifier for tracing related operations.</param>
/// <param name="Contract">The full name of the contract interface being invoked.</param>
/// <param name="Method">The name of the method that was invoked.</param>
/// <param name="Arguments">The redacted/safe arguments passed to the method.</param>
/// <param name="Succeeded">Indicates whether the method completed successfully.</param>
/// <param name="Error">Error message if the method failed; otherwise null.</param>
/// <param name="TimestampUtc">The UTC timestamp when the invocation was recorded.</param>
public sealed record AuditEntry(string? CorrelationId, string Contract, string Method, object? Arguments, bool Succeeded, string? Error, DateTimeOffset TimestampUtc);
