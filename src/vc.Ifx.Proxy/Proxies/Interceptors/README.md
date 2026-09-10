---
title: Boundary Interceptors Framework
doc_type: guide
status: active
last_updated: 2026-08-19
summary: Proxy-based interception pipeline for method invocation cross-cutting concerns
tags:
  - interceptors
  - boundary-proxy
  - framework
audience: developer
---

# Boundary Interceptors Framework

## TL;DR

Use **DispatchProxy** to add cross-cutting behavior (logging, validation, retries, authorization) to interface-based services without modifying their implementations. Interceptors form a composable pipeline that runs before, around, and after method invocations.

**One-liner:** "Proxy-based interception for method invocation cross-cutting concerns."

### Common Use Cases

- **Observability:** Correlation tracking, telemetry, auditing
- **Resilience:** Retries, timeouts, circuit breakers
- **Security:** Authentication, authorization, argument redaction, validation
- **Data Integrity:** Exception handling, idempotency caching

### Key Concept

Interceptors wrap the actual implementation in a pipeline. Each interceptor can examine and modify behavior before calling `next()`:

```
Request → [Authentication] → [Authorization] → [Validation] → [Target] → Response
```

---

## Quick Start

**New to this framework?** Start here: [QUICK-START.md](QUICK-START.md) (5 minutes)

**Want detailed integration patterns?** See: [USAGE.md](USAGE.md)

---

## Interceptors by Category

### Security (3 interceptors)

| Interceptor | Purpose | Documentation |
|---|---|---|
| Authentication | Establish identity via JWT tokens | [Authentication/README.md](Authentication/README.md) |
| Authorization | Enforce access policies | [Authorization/README.md](Authorization/README.md) |
| Redaction | Mask sensitive method arguments | [Redaction/README.md](Redaction/README.md) |

### Observability (4 interceptors)

| Interceptor | Purpose | Documentation |
|---|---|---|
| Correlation | Track distributed operation flow | [Correlation/README.md](Correlation/README.md) |
| Audit | Record operation execution history | [Auditing/README.md](Auditing/README.md) |
| Telemetry | Export OpenTelemetry metrics | [Telemetry/README.md](Telemetry/README.md) |
| Status | Track execution status and completion | [Status/README.md](Status/README.md) |

### Resilience (3 interceptors)

| Interceptor | Purpose | Documentation |
|---|---|---|
| Retry | Exponential backoff for transient failures | [Retry/README.md](Retry/README.md) |
| Timeout | Enforce operation time limits | [Timeout/README.md](Timeout/README.md) |
| CircuitBreaker | Prevent cascading failures | [CircuitBreaker/README.md](CircuitBreaker/README.md) |

### Data Integrity (3 interceptors)

| Interceptor | Purpose | Documentation |
|---|---|---|
| Validation | Enforce input constraints | [Validation/README.md](Validation/README.md) |
| Idempotency | Cache results for repeated operations | [Idempotency/README.md](Idempotency/README.md) |
| Exception | Transform and wrap exceptions | [Exceptions/README.md](Exceptions/README.md) |

### Infrastructure

- [Core Contracts](Core/README.md) — Base types and shared infrastructure
- [Security Support](Security/README.md) — Principal accessor contract

---

## 30-Second Example

```csharp
// 1. Register interceptors in execution order
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();
services.AddScoped<IInvocationInterceptor, TelemetryInterceptor>();
services.AddScoped<IInvocationInterceptor, StatusInterceptor>();
services.AddScoped<IInvocationInterceptor, RetryInterceptor>();
services.AddScoped<IInvocationInterceptor, TimeoutInterceptor>();
services.AddScoped<IInvocationInterceptor, CircuitBreakerInterceptor>();
services.AddScoped<IInvocationInterceptor, IdempotencyInterceptor>();
services.AddScoped<IInvocationInterceptor, ExceptionInterceptor>();

// 2. Register your service with interception
services.AddScoped<IPaymentService>(provider =>
{
    var impl = new PaymentService();
    var interceptors = provider.GetServices<IInvocationInterceptor>();
    return BoundaryProxy<IPaymentService>.Create(impl, interceptors);
});

// 3. Use it (interception is transparent)
var payment = serviceProvider.GetRequiredService<IPaymentService>();
await payment.ProcessAsync(request);  // All interceptors run in order
```

---

## Supported Return Types

- `void` (synchronous)
- `Task` (async, no result)
- `ValueTask` (async struct-based, no result)
- `Task<T>` (async with result)
- `ValueTask<T>` (async struct-based with result)

---

## Execution Order

Interceptors execute in registration order. A recommended order for common scenarios:

1. **Authentication** — Establish identity
2. **Authorization** — Check permissions
3. **Redaction** — Prepare safe arguments for logging
4. **Validation** — Enforce input constraints
5. **Correlation** — Start trace context
6. **Audit** — Record operation
7. **Telemetry** — Export metrics
8. **Status** — Track execution state
9. **Retry** — Prepare retry logic
10. **Timeout** — Set time boundaries
11. **CircuitBreaker** — Prevent cascading failures
12. **Idempotency** — Check for cached results
13. **Exception** — Transform exceptions

---

## Thread Safety

- **BoundaryProxy** is thread-safe for concurrent invocations
- **CircuitBreakerInterceptor** uses `ConcurrentDictionary` for thread-safe state
- **MethodContext.Items** dictionary is not thread-safe; don't share contexts between threads

---

## Limitations

- Interceptors cannot modify method arguments (read-only)
- Generic methods are supported through reflection
- Internal method calls bypass the proxy
- Static methods cannot be intercepted

---

## See Also

- [BoundaryProxy Infrastructure](../README.md) — Dynamic proxy architecture
- [Core Contracts](Core/README.md) — IInvocationInterceptor, MethodContext types
- [Usage Patterns](USAGE.md) — Integration and registration examples


