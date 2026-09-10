---
title: Exception Handling Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Transform and wrap exceptions for consistent error handling
tags:
  - exception-handling
  - error-transformation
  - data-integrity
audience: developer
source_paths:
  - Exceptions/ExceptionInterceptor.cs
---

# Exception Handling Interceptor

Transforms and wraps exceptions at the service boundary for consistent error handling.

## Purpose

Catch upstream exceptions (database, network, infrastructure) and transform them into consistent domain exceptions for callers.

## How It Works

```
Upstream exception thrown
    (e.g., SqlException, HttpRequestException)
    ↓
ExceptionInterceptor catches it
    ↓
Inspects exception type
    ↓
Transforms to domain exception
    (e.g., ServiceUnavailableException)
    ↓
Consumer receives consistent exception type
```

## Files

| File | Purpose |
|---|---|
| `ExceptionInterceptor.cs` | Catches and transforms exceptions |

## Integration

### 1. Register the Interceptor

```csharp
services.AddScoped<IInvocationInterceptor, ExceptionInterceptor>();
```

### 2. Calling Methods (Exception Handling)

```csharp
public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(PaymentRequest request);
}

try
{
    // Actual implementation may throw SqlException, 
    // but ExceptionInterceptor transforms it
    var result = await paymentService.ProcessAsync(request);
}
catch (ServiceUnavailableException ex)
{
    // Consistent domain exception
    _logger.LogError(ex, "Service temporarily unavailable");
}
catch (InvalidOperationException ex)
{
    // Business logic error
    _logger.LogWarning(ex, "Invalid operation");
}
```

## Exception Transformation

The interceptor provides a consistent interface by transforming low-level exceptions:

| Upstream Exception | Transformed To |
|---|---|
| `SqlException` | `ServiceUnavailableException` |
| `HttpRequestException` | `ServiceUnavailableException` |
| `TimeoutException` | `ServiceUnavailableException` |
| `IOException` | `ServiceUnavailableException` |
| `InvalidOperationException` | `InvalidOperationException` (pass through) |
| `ArgumentException` | `ArgumentException` (pass through) |

## Execution Order

`ExceptionInterceptor` can run anywhere, but typically **early to mid-pipeline**:

```csharp
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();      // 1st
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();   // 2nd
services.AddScoped<IInvocationInterceptor, ExceptionInterceptor>();        // 3rd
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();    // 4th
```

## Related

- [Core Infrastructure](../Core/README.md) — MethodContext, ITransientExceptionDetector
- [Timeout Interceptor](../Timeout/README.md) — Works with exception handling
- [Retry Interceptor](../Retry/README.md) — Catches transient exceptions

## See Also

- [README.md](../README.md#data-integrity-3-interceptors) — Data integrity category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#exceptions) — Complete table of contents
}
```

### InvocationTimeoutException

Thrown when operation exceeds configured timeout:

```csharp
try
{
    await service.SlowOperationAsync();
}
catch (InvocationTimeoutException ex)
{
    Console.WriteLine($"Operation timed out: {ex.Message}");
}
```

### ServiceUnavailableException

Thrown when downstream service is unavailable:

```csharp
try
{
    await gateway.ProcessAsync(request);
}
catch (ServiceUnavailableException ex)
{
    // Service is down, maybe retry later
    logger.LogWarning("Payment gateway unavailable: {Message}", ex.Message);
}
```

### Validation Exceptions

See [Validation](../Validation/README.md) folder.

## Registration

```csharp
// Register LAST in pipeline (catch all exceptions)
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();
// ... other interceptors ...
services.AddScoped<IInvocationInterceptor, ExceptionInterceptor>();  // Last
```

**Why last?** Catch exceptions thrown by all upstream interceptors and the target method.

## Transformation Logic

The interceptor checks the exception type and transforms:

```csharp
try
{
    return await next().ConfigureAwait(false);
}
catch (OperationCanceledException)
{
    // Timeout scenario
    throw new InvocationTimeoutException(...);
}
catch (HttpRequestException hre) when (hre.InnerException is TimeoutException)
{
    // Network timeout
    throw new InvocationTimeoutException(...);
}
catch (HttpRequestException hre) when (hre.StatusCode == 503)
{
    // Service unavailable
    throw new ServiceUnavailableException(...);
}
catch (InvalidOperationException)
{
    // Let it through (not a service boundary issue)
    throw;
}
```

## Exception Translation Map

| Original Exception | Translated To | When |
|---|---|---|
| `OperationCanceledException` | `InvocationTimeoutException` | Timeout exceeded |
| `HttpRequestException (503)` | `ServiceUnavailableException` | Remote service down |
| `HttpRequestException (5xx)` | `ServiceUnavailableException` | Remote server error |
| `HttpRequestException (timeout)` | `InvocationTimeoutException` | Network timeout |
| `ArgumentException` | `ArgumentException` | Invalid argument (pass through) |
| `UnauthorizedAccessException` | `UnauthorizedAccessException` | Permission denied (pass through) |
| Custom domain exceptions | (same) | Already in correct form (pass through) |

## Best Practices

### ✅ DO

- Re-throw domain exceptions (they're already appropriate)
- Add correlation ID to transformed exceptions for tracing
- Log original exception with full details server-side
- Return user-friendly message in transformed exception

### ❌ DON'T

- Swallow exceptions (let them propagate)
- Expose stack traces to clients
- Expose internal details (connection strings, paths) to clients
- Transform domain exceptions to generic ones

## Example: API Endpoint

```csharp
[HttpPost("payments")]
public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
{
    try
    {
        var result = await paymentService.ProcessAsync(request);
        return Ok(result);
    }
    catch (InvocationTimeoutException ex)
    {
        logger.LogError(ex, "Payment timeout");
        return StatusCode(504, new { error = "Service timeout" });
    }
    catch (ServiceUnavailableException ex)
    {
        logger.LogError(ex, "Payment gateway unavailable");
        return StatusCode(503, new { error = "Service temporarily unavailable" });
    }
    catch (ValidationException ex)
    {
        logger.LogWarning("Payment validation failed: {@Errors}", ex.Errors);
        return BadRequest(new { errors = ex.Errors });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected payment error");
        return StatusCode(500, new { error = "Internal server error" });
    }
}
```

## Correlation and Debugging

Exceptions include correlation ID for tracing:

```csharp
catch (InvocationException ex)
{
    var correlationId = ex.Data["CorrelationId"];
    logger.LogError("Call failed [correlation: {Id}]: {Error}", 
        correlationId, ex.Message);
}
```

Server logs:
```
[ERROR] Call failed [correlation: 550e8400-e29b-41d4-a716-446655440000]: timeout
```

Client can include correlation ID in support requests for investigation.

## Further Reading

- [Exception Best Practices in .NET](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)
- [Problem Details (RFC 9457)](https://tools.ietf.org/html/rfc9457)
- [Auditing](../Auditing/README.md) — Error logging and compliance
- [USAGE.md](../USAGE.md) — Overall error handling patterns

