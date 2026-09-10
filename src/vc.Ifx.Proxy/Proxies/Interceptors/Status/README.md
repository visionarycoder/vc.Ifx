---
title: Status Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Track execution status and report completion events
tags:
  - status
  - observability
  - metrics
audience: developer
source_paths:
  - Status/StatusInterceptor.cs
  - Status/IStatusSink.cs
  - Status/Status.cs
  - Status/StatusUpdated.cs
---

# Status Interceptor

Tracks method execution lifecycle and records completion status and duration.

## Purpose

Monitor method execution by recording:
- Execution start time
- Completion status (Success, Failure, Timeout)
- Total execution duration
- Exception details (if failed)

## How It Works

```
Method invoked
    ↓
StatusInterceptor records start time
    ↓
Execute method
    ↓
StatusInterceptor records status: [Success | Failure | Timeout]
    ↓
Calculate duration
    ↓
Store metadata in context.Items for downstream use
```

## Files

| File | Purpose |
|---|---|
| `StatusInterceptor.cs` | Records execution status and duration |
| `IStatusSink.cs` | Contract for status notifications |
| `Status.cs` | Status enumeration (Success, Failure, etc.) |
| `StatusUpdated.cs` | Event details |

## Integration

### 1. Register the Interceptor

```csharp
services.AddScoped<IInvocationInterceptor, StatusInterceptor>();
```

### 2. Implement Status Sink (Optional)

```csharp
public sealed class StatusSink : IStatusSink
{
    private readonly ILogger<StatusSink> _logger;
    
    public StatusSink(ILogger<StatusSink> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask OnStatusUpdatedAsync(StatusUpdated status)
    {
        _logger.LogInformation(
            "Method {Method} completed with {Status} in {Duration}ms",
            status.MethodName,
            status.Status,
            status.DurationMilliseconds
        );
        
        await Task.CompletedTask;
    }
}

// Register
services.AddScoped<IStatusSink, StatusSink>();
```

### 3. Use Status Data

```csharp
public sealed class MyInterceptor : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(MethodContext context, InvocationDelegate next)
    {
        var result = await next().ConfigureAwait(false);
        
        // Read status set by StatusInterceptor
        if (context.Items.TryGetValue("Status", out var status))
        {
            _logger.LogInformation("Execution status: {Status}", status);
        }
        
        return result;
    }
}
```

## Status Values

| Status | Meaning |
|---|---|
| `Success` | Method completed normally |
| `Failure` | Method threw exception |
| `Timeout` | Method exceeded time limit |

## Execution Order

Register `StatusInterceptor` **early to mid-pipeline** (to capture all downstream activity):

```csharp
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();     // 1st
services.AddScoped<IInvocationInterceptor, StatusInterceptor>();          // 2nd
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();      // 3rd
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();           // Later
```

## Related

- [Correlation Interceptor](../Correlation/README.md) — Track request flow (use with Status)
- [Telemetry Interceptor](../Telemetry/README.md) — Export status as metrics
- [Audit Interceptor](../Auditing/README.md) — Record status with audit trail
- [Core Infrastructure](../Core/README.md) — MethodContext, InvocationItemNames

## See Also

- [README.md](../README.md#observability-4-interceptors) — Observability category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#status) — Complete table of contents

The interceptor automatically tracks status for all methods:

```csharp
public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(PaymentRequest request);
    // Status automatically tracked
}
```

## Usage

### Reading Status

From any downstream interceptor or service:

```csharp
public class MyInterceptor : IInvocationInterceptor
{
    public async Task OnInvokeAsync(MethodContext context)
    {
        // After StatusInterceptor has run:
        var status = (ExecutionStatus)context.Items[InvocationItemNames.ExecutionStatus];
        var duration = (TimeSpan)context.Items[InvocationItemNames.ExecutionDuration];
        
        _logger.LogInformation(
            "Method {Method} completed with status {Status} in {Duration}ms",
            context.Method.Name,
            status,
            duration.TotalMilliseconds
        );
    }
}
```

## Status Values

```csharp
public enum ExecutionStatus
{
    NotStarted,   // Method not yet invoked
    Running,      // Method is executing
    Success,      // Method completed successfully
    Failure,      // Method threw exception
    Timeout,      // Method exceeded time limit
    Cancelled     // Method was cancelled
}
```

## Context Items Populated

| Key | Type | Description |
|---|---|---|
| `ExecutionStatus` | `ExecutionStatus` | Current execution status |
| `ExecutionDuration` | `TimeSpan` | Time elapsed since start |
| `ExecutionException` | `Exception` | Exception if status == Failure |
| `ExecutionStartTime` | `DateTime` | UTC start time |

## Logging Integration

StatusInterceptor can emit structured logs:

```csharp
_logger.LogInformation(
    "Method {Method} status {Status} duration {DurationMs}ms",
    context.Method.Name,
    status,
    context.Items[InvocationItemNames.ExecutionDuration]
);
```

## Execution Order

Status should run **relatively late** (8th position) after most processing:

```
1. Authentication
2. Authorization
3. Redaction
4. Validation
5. Correlation
6. Audit
7. Telemetry
8. → Status ← (runs here)
9. Retry
10. Timeout
11. CircuitBreaker
12. Idempotency
13. Exception handling
```

## Performance Considerations

StatusInterceptor has minimal overhead:
- Single stopwatch per invocation
- No allocations on success path
- Runs synchronously (doesn't await)

## Metrics Export

Export status metrics to telemetry systems:

```csharp
public class TelemetryStatusInterceptor : StatusInterceptor
{
    private readonly ITelemetryClient _telemetry;

    public override async Task OnInvokeAsync(MethodContext context)
    {
        await base.OnInvokeAsync(context);
        
        var status = (ExecutionStatus)context.Items[InvocationItemNames.ExecutionStatus];
        var duration = ((TimeSpan)context.Items[InvocationItemNames.ExecutionDuration]).TotalMilliseconds;
        
        _telemetry.TrackDependency(
            context.Method.Name,
            status == ExecutionStatus.Success ? true : false,
            duration
        );
    }
}
```

## Timeout Integration

Works with `TimeoutInterceptor` to distinguish timeouts from other failures:

```
Method running
    ↓
Timeout triggered
    ↓
TimeoutInterceptor sets status = Timeout
    ↓
StatusInterceptor records as timeout (not generic failure)
```

## Related

- [Telemetry/README.md](../Telemetry/README.md) — Export metrics to OpenTelemetry
- [Audit/README.md](../Auditing/README.md) — Record detailed audit trail
- [Timeout/README.md](../Timeout/README.md) — Enforce execution timeout

