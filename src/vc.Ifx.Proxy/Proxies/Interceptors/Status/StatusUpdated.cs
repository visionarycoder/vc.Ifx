namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Status;

/// <summary>
/// Represents a status update for a method invocation.
/// </summary>
/// <param name="CorrelationId">The correlation identifier for tracing related operations.</param>
/// <param name="Contract">The full name of the contract interface being invoked.</param>
/// <param name="Method">The name of the method being invoked.</param>
/// <param name="Status">The current status of the invocation.</param>
/// <param name="Error">Error message if the invocation failed; otherwise null.</param>
/// <param name="TimestampUtc">The UTC timestamp when the status was recorded.</param>
public sealed record StatusUpdated(string? CorrelationId, string Contract, string Method, Status Status, string? Error, DateTimeOffset TimestampUtc);
