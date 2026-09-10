---
title: CircuitBreaker Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Prevent cascading failures by stopping calls to failing services
tags:
  - circuit-breaker
  - resilience
  - fault-tolerance
audience: developer
source_paths:
  - CircuitBreaker/CircuitBreakerInterceptor.cs
  - CircuitBreaker/CircuitBreakerAttribute.cs
  - CircuitBreaker/CircuitBreakerState.cs
  - CircuitBreaker/CircuitBreakerOpenException.cs
---

# CircuitBreaker Interceptor

Prevents cascading failures by stopping calls to services that are failing.

## Purpose

Fail fast and protect upstream services when a downstream dependency is unhealthy, using the circuit breaker pattern.

## How It Works

```
Circuit CLOSED (healthy)
    ↓
Calls proceed normally
    ↓ [transient failures accumulate]
Threshold exceeded
    ↓
Circuit OPEN (tripped)
    ↓
New calls fail immediately with CircuitBreakerOpenException
    ↓ [after timeout]
Circuit HALF-OPEN (testing)
    ↓
Next call attempts
    ↓ [success → CLOSED, failure → OPEN]
Back to CLOSED or OPEN
```

## Files

| File | Purpose |
|---|---|
| `CircuitBreakerInterceptor.cs` | Monitors failures and trips circuit |
| `CircuitBreakerAttribute.cs` | Configures thresholds and timeout |
| `CircuitBreakerState.cs` | Tracks circuit state |
| `CircuitBreakerOpenException.cs` | Thrown when circuit is open |

## Integration

### 1. Register the Interceptor

```csharp
services.AddScoped<IInvocationInterceptor, CircuitBreakerInterceptor>();
```

### 2. Mark Methods with Circuit Breaker

```csharp
public interface IDownstreamService
{
    [CircuitBreaker(
        FailureThreshold = 5,              // Trip after 5 failures
        SuccessThreshold = 2,              // Close after 2 successes
        TimeoutMs = 30000                  // Try again after 30s
    )]
    Task<Response> CallAsync(Request request);
}
```

### 3. Handle Circuit Open

```csharp
try
{
    var result = await downstreamService.CallAsync(request);
}
catch (CircuitBreakerOpenException ex)
{
    _logger.LogWarning("Circuit is open, service unavailable");
    // Provide fallback response or graceful degradation
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
}
```

## States

| State | Behavior | Transitions |
|---|---|---|
| **CLOSED** | Requests pass through | → OPEN if threshold exceeded |
| **OPEN** | All requests fail immediately | → HALF-OPEN after timeout |
| **HALF-OPEN** | Next request tests recovery | → CLOSED if succeeds, OPEN if fails |

## Configuration

| Option | Purpose |
|---|---|
| `FailureThreshold` | Number of failures to trip circuit |
| `SuccessThreshold` | Number of successes to close circuit |
| `TimeoutMs` | Time before trying again (OPEN → HALF-OPEN) |

## Transient vs. Permanent Failures

Uses `ITransientExceptionDetector` to determine which exceptions count toward failure threshold:

**Counted (Transient):**
- `TimeoutException`
- `IOException`
- HTTP 5xx errors

**Not Counted (Permanent):**
- Validation errors
- Authentication failures
- 4xx errors

## Execution Order

Register `CircuitBreakerInterceptor` **late in the chain**:

```csharp
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
services.AddScoped<IInvocationInterceptor, TimeoutInterceptor>();
services.AddScoped<IInvocationInterceptor, RetryInterceptor>();
services.AddScoped<IInvocationInterceptor, CircuitBreakerInterceptor>();  // Last
```

## Typical Pattern

Combine with Retry and Timeout:

```csharp
[Timeout(Milliseconds = 5000)]
[Retry(MaxRetries = 3, InitialDelayMs = 100)]
[CircuitBreaker(FailureThreshold = 5, TimeoutMs = 30000)]
Task<Response> CallExternalServiceAsync(Request request);
```

Execution order:
1. Timeout enforces max duration
2. Retry attempts on transient failure
3. CircuitBreaker trips if retries exhausted

## Related

- [Retry Interceptor](../Retry/README.md) — Retry transient failures (pair with circuit breaker)
- [Timeout Interceptor](../Timeout/README.md) — Enforce time limits
- [Core Infrastructure](../Core/README.md) — ITransientExceptionDetector

## See Also

- [README.md](../README.md#resilience-3-interceptors) — Resilience category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#resilience) — Complete table of contents

### TimeoutInterceptor

Cancels long-running operations:

```csharp
public interface IReportService
{
    [InvocationTimeout(5000)]  // 5 seconds max
    Task<Report> GenerateAsync(ReportRequest request);
}
```

If execution exceeds 5 seconds:
```
[WARN] Method timeout exceeded (5000ms)
[WARN] Cancelling operation
[ERROR] Throwing TimeoutException
```

**Use for:** External API calls, database queries, expensive computations

**Default:** No timeout (methods run to completion)

### CircuitBreakerInterceptor

Prevents cascading failures by stopping calls when downstream is unhealthy:

```csharp
public interface IPaymentGateway
{
    [CircuitBreaker(failureThreshold: 5, breakDurationSeconds: 30)]
    Task<PaymentResult> ChargeAsync(PaymentRequest request);
}
```

States:
```
CLOSED (normal):         Pass calls through, count failures
OPEN (broken):           Reject all calls immediately (fail fast)
HALF_OPEN (recovering):  Allow one test call, watch for success
```

Sequence:
```
Calls 1-4: Fail (count = 4)
Call 5: Fail (count = 5, threshold reached)
Circuit opens → OPEN state for 30 seconds
Calls 6-10: Rejected immediately (no network call)
After 30 seconds: Enter HALF_OPEN state
Test call: Succeeds → Return to CLOSED
Test call: Fails → Stay OPEN for another 30 seconds
```

**Use for:** External service dependencies (payment gateways, databases, APIs)

## Stacking Resilience Patterns

Combine them for robust handling:

```csharp
public interface IRobustPaymentService
{
    [Retry(maxAttempts: 3, delayMilliseconds: 500)]
    [TimeoutPerAttempt(10000)]
    [CircuitBreaker(failureThreshold: 5, breakDurationSeconds: 60)]
    Task<PaymentResult> ChargeAsync(PaymentRequest request);
}
```

Execution flow:
```
CircuitBreaker checks: Is circuit closed? → YES, proceed
Retry loop (max 3 attempts):
  Attempt 1:
    Timeout enforcer: Set 10-second deadline
    Network call starts
    Succeeds within 10 seconds → Return result
```

vs. Failure scenario:
```
Attempt 1: Fails (500 error)
Wait 500ms (retry delay)
Attempt 2: Fails (500 error)
Wait 500ms backoff
Attempt 3: Fails (timeout, exceeds 10 seconds)
All attempts failed → Throw exception
CircuitBreaker increments failure count (now 5)
Circuit opens → Future calls fail immediately
```

## Registration Order

```csharp
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
// ...
services.AddScoped<IInvocationInterceptor, RetryInterceptor>();          // Mid-late
services.AddScoped<IInvocationInterceptor, TimeoutInterceptor>();        // Mid-late
services.AddScoped<IInvocationInterceptor, CircuitBreakerInterceptor>();  // Late
```

**Why this order?**
- Run validation first (don't retry bad input)
- Run timeouts/circuit-breakers late (they protect outbound calls)

## Configuration Examples

### Aggressive Retry
```csharp
[Retry(maxAttempts: 5, delayMilliseconds: 1000)]
```
Good for: Flaky networks, unstable services

### Short Timeout
```csharp
[InvocationTimeout(2000)]
```
Good for: User-facing API calls (don't make users wait)

### Protective Circuit Breaker
```csharp
[CircuitBreaker(failureThreshold: 3, breakDurationSeconds: 120)]
```
Good for: Expensive services, high-traffic scenarios

## Performance Impact

- **Retry** — Adds latency on failure (time × retry attempts)
- **Timeout** — Minimal overhead (background timer)
- **CircuitBreaker** — State check only (~microseconds)

For high-traffic systems:
- Use conservative retry counts (3-5, not 10+)
- Set tight timeouts (prevent pile-up)
- Monitor circuit breaker trips (indicates downstream issues)

## Further Reading

- [Retry Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly)
- [Circuit Breaker Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-circuit-breaker-pattern)
- [Timeout Best Practices](https://github.com/grpc/proposal/blob/master/A6-client-retries.md)
- [USAGE.md](../USAGE.md) — Overall patterns


