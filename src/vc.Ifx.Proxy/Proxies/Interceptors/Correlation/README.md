---
title: Correlation Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Track distributed operation flow across service boundaries with correlation IDs
tags:
  - correlation
  - observability
  - tracing
audience: developer
source_paths:
  - Correlation/CorrelationInterceptor.cs
---

# Correlation Interceptor

Establishes and tracks a correlation ID across the entire request lifecycle for distributed tracing.

## Purpose

Ensure every request gets a unique correlation ID that flows through all downstream calls, enabling tracing of complex operation chains across service boundaries.

## How It Works

```
Request arrives
    ↓
CorrelationInterceptor extracts or generates ID
    ↓
ID stored in context.Items[InvocationItemNames.CorrelationId]
    ↓
All downstream services can access it for logging/tracing
    ↓
Response includes correlation ID
```

## Integration

### Register the Interceptor

```csharp
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();
```

The interceptor automatically:
1. Checks for incoming correlation ID (from HTTP header, method parameter, or context)
2. Generates a new ID if none found
3. Stores ID in `context.Items[InvocationItemNames.CorrelationId]`
4. Makes it available to all downstream interceptors and services

### Access in Other Interceptors

```csharp
public sealed class MyInterceptor : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(MethodContext context, InvocationDelegate next)
    {
        // Read correlation ID set by CorrelationInterceptor
        if (context.Items.TryGetValue(InvocationItemNames.CorrelationId, out var correlationId))
        {
            _logger.LogInformation("CorrelationId: {CorrelationId}", correlationId);
        }
        
        return await next().ConfigureAwait(false);
    }
}
```

## Execution Order

Register **CorrelationInterceptor** **FIRST** in the pipeline:

```csharp
// FIRST - must run before other interceptors
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();

// Other interceptors depend on this
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
// ... more interceptors ...
```

## Related

- [Status Interceptor](../Status/README.md) — Track execution status
- [Telemetry Interceptor](../Telemetry/README.md) — Export traces with correlation IDs
- [Audit Interceptor](../Auditing/README.md) — Audit trail with correlation IDs
- [Core Infrastructure](../Core/README.md) — InvocationItemNames, MethodContext

## See Also

- [README.md](../README.md#observability-4-interceptors) — Observability category
- [Execution Order](../README.md#execution-order) — Full pipeline order
- [TOC.md](../TOC.md#correlation) — Complete table of contents

## Usage

### Reading the Correlation ID

From any interceptor or service:

```csharp
public class MyInterceptor : IInvocationInterceptor
{
    public async Task OnInvokeAsync(MethodContext context)
    {
        var correlationId = (string)context.Items[InvocationItemNames.CorrelationId];
        _logger.LogInformation("Processing request {CorrelationId}", correlationId);
    }
}
```

### Passing Correlation ID

Include in method parameters:

```csharp
public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(string correlationId, PaymentRequest request);
}
```

Or in HTTP headers:

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
```

## Correlation ID Generation

Default implementation uses `Guid.NewGuid()` format. Override to customize:

```csharp
public class CustomCorrelationInterceptor : CorrelationInterceptor
{
    protected override string GenerateCorrelationId()
    {
        return $"REQ-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(10000)}";
    }
}
```

## Configuration

Override extraction strategy:

```csharp
var correlationInterceptor = new CorrelationInterceptor
{
    CorrelationIdHeaderName = "X-Request-ID",  // Custom header name
    GenerateIfMissing = true                     // Auto-generate if not found
};
```

## Integration with Logging

Use structured logging to include correlation ID:

```csharp
_logger.LogInformation(
    "Operation {Operation} completed with correlation {CorrelationId}",
    "ProcessPayment",
    correlationId
);
```

## Execution Order

Correlation should run **early** (5th position) to make ID available to all downstream interceptors:

```
1. Authentication
2. Authorization
3. Redaction
4. Validation
5. → Correlation ← (runs here)
6. Audit
7. Telemetry
8. Status
9. Retry
10. Timeout
11. CircuitBreaker
12. Idempotency
13. Exception handling
```

## Related

- [Core/README.md](../Core/README.md) — InvocationItemNames constants
- [Telemetry](../Telemetry/README.md) — OpenTelemetry tracing
- [Audit](../Auditing/README.md) — Logging correlation with audit trail

