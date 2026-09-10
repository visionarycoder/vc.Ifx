---
title: Timeout Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Enforce operation time limits with graceful cancellation
tags:
  - timeout
  - resilience
  - cancellation
audience: developer
source_paths:
  - Timeout/TimeoutInterceptor.cs
  - Timeout/TimeoutAttribute.cs
---

# Timeout Interceptor

Enforces maximum execution duration on method invocations with automatic cancellation.

## Purpose

Cancel methods that exceed a specified time limit to prevent hanging operations and resource exhaustion.

## How It Works

```
Method invoked with 5000ms timeout
    ↓
Start timer
    ↓
Method executing...
    ↓ [after 4000ms, still running]
Still within timeout window
    ↓
Method executing...
    ↓ [after 5001ms, still running]
TIMEOUT! Cancel operation
    ↓
Throw OperationCanceledException
```

## Files

| File | Purpose |
|---|---|
| `TimeoutInterceptor.cs` | Enforces timeout with cancellation |
| `TimeoutAttribute.cs` | Specifies timeout in milliseconds |

## Integration

### 1. Register the Interceptor

```csharp
services.AddScoped<IInvocationInterceptor, TimeoutInterceptor>();
```

### 2. Mark Methods with Timeout

```csharp
public interface IExternalApiService
{
    [Timeout(Milliseconds = 5000)]
    Task<Result> CallExternalApiAsync(string id);

    [Timeout(Milliseconds = 30000)]  // 30 seconds
    Task<IEnumerable<Data>> FetchLargeDatasetAsync();
}
```

### 3. Handle Timeout Exceptions

```csharp
try
{
    var result = await apiService.CallExternalApiAsync("123");
}
catch (OperationCanceledException ex)
{
    _logger.LogWarning(ex, "Request timed out");
    // Handle timeout gracefully
}
```

## Configuration

### Per-Method Timeout

```csharp
[Timeout(Milliseconds = 5000)]
Task<Response> CallServiceAsync();
```

### Default Timeout (if no attribute)

If `TimeoutInterceptor` is registered but no `[Timeout]` attribute is present, the interceptor passes through without enforcing a timeout.

## Execution Order

Register `TimeoutInterceptor` **late in the chain** (after validation, authentication, authorization):

```csharp
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
services.AddScoped<IInvocationInterceptor, TimeoutInterceptor>();  // Late
services.AddScoped<IInvocationInterceptor, RetryInterceptor>();    // After timeout
```

## Related

- [Retry Interceptor](../Retry/README.md) — Retry transient failures (register after timeout)
- [CircuitBreaker Interceptor](../CircuitBreaker/README.md) — Prevent cascading failures
- [Core Infrastructure](../Core/README.md) — MethodContext, InvocationItemNames

## See Also

- [README.md](../README.md#resilience-3-interceptors) — Resilience category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#timeout) — Complete table of contents

## Configuration

### Attribute-Based

```csharp
[TimeoutAttribute(Milliseconds = 5000)]
public async Task<ApiResponse> FetchDataAsync() { }
```

### Global Configuration

```csharp
var timeoutInterceptor = new TimeoutInterceptor
{
    DefaultTimeoutMs = 5000  // 5 seconds
};

services.AddScoped<IInvocationInterceptor>(_ => timeoutInterceptor);
```

### Per-Method Override

```csharp
[TimeoutAttribute(Milliseconds = 10000)]  // Override global
public async Task<Result> SlowOperationAsync() { }
```

## Implementation

Uses `CancellationToken` for clean cancellation:

```csharp
public class TimeoutInterceptor : IInvocationInterceptor
{
    public async Task OnInvokeAsync(MethodContext context)
    {
        var timeoutMs = GetTimeout(context);
        using var cts = new CancellationTokenSource(timeoutMs);
        
        try
        {
            // Inject cancellation token into method
            InjectCancellationToken(context, cts.Token);
            await next();
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Method {context.Method.Name} exceeded {timeoutMs}ms timeout"
            );
        }
    }
}
```

## Cancellation Token Injection

TimeoutInterceptor injects cancellation token into methods that accept it:

```csharp
// Method with CancellationToken parameter
public async Task<Result> FetchAsync(string id, CancellationToken ct)
{
    return await _client.GetAsync(id, ct);  // Respects timeout
}

// Method without CancellationToken
public async Task<Result> LegacyFetchAsync(string id)
{
    // Timeout still enforced but no token to cancel internal operations
    return await _legacyApi.GetAsync(id);
}
```

## Timeout Detection

Distinguish timeout from other exceptions:

```csharp
public async Task InvokeAsync(MethodContext context)
{
    try
    {
        await method();
    }
    catch (TimeoutException ex)
    {
        _logger.LogError("Operation timed out: {Message}", ex.Message);
    }
    catch (OperationCanceledException ex)
    {
        _logger.LogError("Operation cancelled: {Message}", ex.Message);
    }
}
```

## Execution Order

Timeout should run **relatively late** (10th position) after validation but before circuit breaker:

```
1. Authentication
2. Authorization
3. Redaction
4. Validation
5. Correlation
6. Audit
7. Telemetry
8. Status
9. Retry
10. → Timeout ← (runs here)
11. CircuitBreaker
12. Idempotency
13. Exception handling
```

## Retry Integration

Works with `RetryInterceptor` to timeout individual attempts:

```csharp
[TimeoutAttribute(Milliseconds = 2000)]    // Per-attempt timeout
[RetryAttribute(MaxRetries = 3)]           // Retry if times out
public async Task<Result> FetchAsync(string id, CancellationToken ct)
{
    return await _api.GetAsync(id, ct);
}
```

Each attempt gets its own 2-second timeout window.

## Timeout Values

Common timeout durations:

| Operation | Typical Timeout |
|-----------|---|
| Database query | 5-10 seconds |
| HTTP API call | 10-30 seconds |
| File I/O | 30-60 seconds |
| External service | 30-60 seconds |
| Batch operation | 5-10 minutes |

```csharp
// Examples
[TimeoutAttribute(Milliseconds = 5000)]     // 5 seconds
[TimeoutAttribute(Milliseconds = 30000)]    // 30 seconds
[TimeoutAttribute(Milliseconds = 300000)]   // 5 minutes
```

## Graceful Degradation

Catch timeout and provide fallback:

```csharp
[TimeoutAttribute(Milliseconds = 5000)]
public async Task<Result> FetchWithFallbackAsync(string id)
{
    try
    {
        return await _api.GetAsync(id);
    }
    catch (TimeoutException)
    {
        _logger.LogWarning("API timeout, using cached value for {Id}", id);
        return await _cache.GetAsync(id);
    }
}
```

## Thread Safety

TimeoutInterceptor is thread-safe:
- Each invocation gets its own `CancellationTokenSource`
- No shared mutable state
- Safe for concurrent invocations

## Limitations

- Does not cancel synchronous code (only async)
- Requires method to support `CancellationToken` for clean cancellation
- Network timeouts are separate from method timeouts
- Does not interrupt blocked I/O operations

## Related

- [Retry/README.md](../Retry/README.md) — Retry with per-attempt timeout
- [Status/README.md](../Status/README.md) — Track timeout events
- [CircuitBreaker/README.md](../CircuitBreaker/README.md) — Circuit breaker cascade prevention

