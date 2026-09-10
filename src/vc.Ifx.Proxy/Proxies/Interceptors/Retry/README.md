---
title: Retry Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Exponential backoff retry logic for transient failures
tags:
  - retry
  - resilience
  - exponential-backoff
audience: developer
source_paths:
  - Retries/RetryInterceptor.cs
  - Retries/RetryAttribute.cs
---

# Retry Interceptor

Automatically retries transient failures using exponential backoff.

## Purpose

Improve resilience by automatically retrying operations that fail with transient exceptions (timeout, network errors).

## How It Works

```
Method invoked
    ↓
Attempt 1 (immediate)
    ↓ [transient failure]
Wait 100ms (exponential backoff)
    ↓
Attempt 2
    ↓ [transient failure]
Wait 200ms
    ↓
Attempt 3
    ↓ [success or permanent failure]
Return result or throw
```

## Files

| File | Purpose |
|---|---|
| `RetryInterceptor.cs` | Retries with exponential backoff |
| `RetryAttribute.cs` | Configures max retries and backoff |

## Integration

### 1. Register the Interceptor

```csharp
services.AddScoped<IInvocationInterceptor, RetryInterceptor>();
```

### 2. Mark Methods for Retry

```csharp
public interface IExternalApiService
{
    [Retry(MaxRetries = 3, InitialDelayMs = 100)]
    Task<Response> CallExternalServiceAsync(string id);

    [Retry(MaxRetries = 5)]
    Task<Data> FetchDataAsync();
}
```

### 3. Use the Service

```csharp
try
{
    // If fails with transient exception, will retry automatically
    var result = await apiService.CallExternalServiceAsync("123");
}
catch (HttpRequestException ex)
{
    // After all retries exhausted
    _logger.LogError(ex, "Failed after retries");
}
```

## Transient vs. Permanent Failures

**Retried (Transient):**
- `TimeoutException`
- `IOException`
- HTTP 5xx errors
- Network connectivity issues

**Not Retried (Permanent):**
- `ValidationException`
- Authentication/Authorization failures
- HTTP 4xx errors (except 408, 429)
- Business logic failures

Uses `ITransientExceptionDetector` to determine which exceptions are safe to retry.

## Configuration

| Option | Purpose |
|---|---|
| `MaxRetries` | Maximum number of retry attempts |
| `InitialDelayMs` | Initial wait time in milliseconds |
| Backoff multiplier | Increases delay each retry (e.g., 100ms, 200ms, 400ms) |

## Execution Order

Register `RetryInterceptor` **late in the chain** (after timeout):

```csharp
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
services.AddScoped<IInvocationInterceptor, TimeoutInterceptor>();
services.AddScoped<IInvocationInterceptor, RetryInterceptor>();  // Late, after timeout
```

## Related

- [Timeout Interceptor](../Timeout/README.md) — Enforce time limits (pair with retry)
- [CircuitBreaker Interceptor](../CircuitBreaker/README.md) — Prevent cascading failures
- [Core Infrastructure](../Core/README.md) — ITransientExceptionDetector

## See Also

- [README.md](../README.md#resilience-3-interceptors) — Resilience category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#retries) — Complete table of contents
    // Automatically retried up to 3 times on transient failure
    return await _externalApi.GetAsync(id);
}
```

## Configuration

### Attribute-Based

```csharp
[RetryAttribute(
    MaxRetries = 3,
    InitialDelayMs = 100,
    MaxDelayMs = 5000,
    BackoffMultiplier = 2.0
)]
public async Task<Result> DoWorkAsync() { }
```

### Global Configuration

```csharp
var retryInterceptor = new RetryInterceptor
{
    DefaultMaxRetries = 3,
    DefaultInitialDelayMs = 100,
    DefaultMaxDelayMs = 5000,
    DefaultBackoffMultiplier = 2.0
};

services.AddScoped<IInvocationInterceptor>(_ => retryInterceptor);
```

## Transient Exceptions

By default, retries on:
- `TimeoutException`
- `HttpRequestException` (with transient status codes)
- `IOException`
- Custom exception handling can be implemented

```csharp
public class CustomRetryInterceptor : RetryInterceptor
{
    protected override bool IsTransient(Exception ex)
    {
        return ex is TimeoutException
            || ex is HttpRequestException
            || ex is MyCustomTransientException
            || base.IsTransient(ex);
    }
}
```

## Exponential Backoff Formula

```
delay = min(
    initialDelayMs * (backoffMultiplier ^ attemptNumber),
    maxDelayMs
)
```

Example with defaults:
- Attempt 1: 0ms (immediate)
- Attempt 2: 100ms
- Attempt 3: 200ms
- Attempt 4: 400ms (if max retries ≥ 4)

## Jitter

Add randomization to prevent thundering herd:

```csharp
public class JitteredRetryInterceptor : RetryInterceptor
{
    private static readonly Random _random = new Random();

    protected override async Task DelayAsync(int delayMs, MethodContext context)
    {
        var jitter = _random.Next(0, (int)(delayMs * 0.1));
        await Task.Delay(delayMs + jitter);
    }
}
```

## Execution Order

Retry should run **late** (9th position) to apply after all earlier validations:

```
1. Authentication
2. Authorization
3. Redaction
4. Validation
5. Correlation
6. Audit
7. Telemetry
8. Status
9. → Retry ← (runs here)
10. Timeout
11. CircuitBreaker
12. Idempotency
13. Exception handling
```

## Circuit Breaker Integration

Works with `CircuitBreakerInterceptor` to prevent cascading failures:

```
Retry fails N times
    ↓
Reports to CircuitBreaker
    ↓
CircuitBreaker may open to prevent further retries
    ↓
Downstream calls fail fast (no retry attempts)
```

## Logging

Retry events are logged for debugging:

```csharp
_logger.LogWarning(
    "Retrying method {Method} after {DelayMs}ms (attempt {Attempt}/{Max})",
    method.Name,
    delayMs,
    attempt,
    maxRetries
);
```

## Limitations

- Cannot retry void methods (no result to cache between attempts)
- Stateful operations may have side effects on retry
- Use `[Idempotent]` to mark safe-to-retry methods

## Related

- [CircuitBreaker/README.md](../CircuitBreaker/README.md) — Circuit breaker integration
- [Timeout/README.md](../Timeout/README.md) — Timeout enforcement during retries
- [Idempotency/README.md](../Idempotency/README.md) — Result caching for repeated calls

