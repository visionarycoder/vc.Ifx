---
title: Auditing Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Record method invocations for compliance and audit trails
tags:
  - auditing
  - observability
  - compliance
audience: developer
source_paths:
  - Auditing/AuditInterceptor.cs
  - Auditing/IAuditSink.cs
  - Auditing/AuditEntry.cs
---

# Auditing Interceptor

Records method invocations for compliance, auditing, and forensics.

## Purpose

Capture detailed information about who called what, when, with what arguments, and what the result was. Essential for regulatory compliance (PCI, HIPAA, SOX, etc.).

## Files

| File | Purpose |
|---|---|
| `AuditInterceptor.cs` | Main interceptor implementation |
| `IAuditSink.cs` | Contract for audit storage (you implement this) |
| `AuditEntry.cs` | Audit record data model |

## How It Works

```csharp
public sealed class AuditInterceptor : IInvocationInterceptor
{
    // 1. Records method name, arguments, caller info
    // 2. Invokes target method
    // 3. Records result or exception
    // 4. Writes to IAuditSink (you implement where/how)
}
```

## Integration

### 1. Implement IAuditSink

```csharp
public class AuditDatabaseSink : IAuditSink
{
    private readonly IDbConnection _db;

    public async ValueTask WriteAsync(AuditEntry entry)
    {
        // Write to database, file, event log, or external service
        await _db.ExecuteAsync(
            "INSERT INTO AuditLog (Method, User, Args, Result, Timestamp) VALUES (@method, @user, @args, @result, @ts)",
            entry
        );
    }
}
```

### 2. Register Interceptor and Sink

```csharp
services.AddScoped<IAuditSink, AuditDatabaseSink>();
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();
```

### 3. Use It

```csharp
var service = provider.GetRequiredService<IMyService>();
// All calls are automatically audited
await service.DoSomethingAsync(data);
```

## Configuration

### Execution Order

Register **AuditInterceptor** after other interceptors:

```csharp
// Place it late in the pipeline so it observes complete flow
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();      // Early
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();       // Early
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();    // Early
// ... other interceptors ...
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();            // Late
```

### With Redaction

Combine with [RedactionInterceptor](../Redaction/README.md) to mask sensitive arguments:

```csharp
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();  // Early
// ... other interceptors ...
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();      // Late
```

The `AuditInterceptor` will read safe (redacted) arguments from `MethodContext.Items[InvocationItemNames.SafeArguments]`.

## AuditEntry Model

```csharp
public class AuditEntry
{
    public string? MethodName { get; set; }          // IMyService.DoSomething
    public string? ClassName { get; set; }            // IMyService
    public object[]? Arguments { get; set; }         // [request, context]
    public object? Result { get; set; }              // Execution result
    public Exception? Exception { get; set; }        // If it threw
    public DateTime TimestampUtc { get; set; }       // When it ran
    public long DurationMilliseconds { get; set; }   // How long it took
    public string? CorrelationId { get; set; }       // Link to trace
    public string? UserId { get; set; }              // Who called it
    public bool Success { get; set; }                // Pass/fail
}
```

## Performance Considerations

- **Async writes:** Implement `IAuditSink.WriteAsync()` to avoid blocking
- **Batching:** Buffer records and write in batches for high-throughput scenarios
- **Selective auditing:** Consider attributes to opt-in/opt-out per method
- **Redaction:** Use `RedactionInterceptor` to reduce data volume

## Related

- [Correlation Interceptor](../Correlation/README.md) — Track distributed call flow
- [Status Interceptor](../Status/README.md) — Track execution status
- [Telemetry Interceptor](../Telemetry/README.md) — Export metrics
- [Redaction Interceptor](../Redaction/README.md) — Mask sensitive data
- [Core Infrastructure](../Core/README.md) — MethodContext, InvocationItemNames

## See Also

- [README.md](../README.md#observability-4-interceptors) — Observability category overview
- [TOC.md](../TOC.md#auditing) — Full table of contents

