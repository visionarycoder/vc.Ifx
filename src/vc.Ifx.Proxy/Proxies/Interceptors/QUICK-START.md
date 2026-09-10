---
title: Quick Start - Boundary Interceptors
doc_type: guide
status: active
last_updated: 2026-08-19
summary: Get your first interceptor running in 5 minutes
tags:
  - interceptors
  - quick-start
  - tutorial
audience: developer
---

# Quick Start — Boundary Interceptors

Get your first interceptor running in 5 minutes.

## What You're Building

A payment service that automatically logs all calls, validates arguments, and tracks execution time.

## Step 1: Register Interceptors

```csharp
var services = new ServiceCollection();

// Register interceptors in pipeline order
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>(); // Must be first
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
services.AddScoped<IInvocationInterceptor, StatusInterceptor>();
services.AddScoped<IInvocationInterceptor, TimeoutInterceptor>();
services.AddScoped<IInvocationInterceptor, RetryInterceptor>();
services.AddScoped<IInvocationInterceptor, CircuitBreakerInterceptor>();
services.AddScoped<IInvocationInterceptor, IdempotencyInterceptor>();
services.AddScoped<IInvocationInterceptor, ExceptionInterceptor>();
services.AddScoped<IInvocationInterceptor, TelemetryInterceptor>();
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();
```

**Why order matters:** Interceptors execute top-to-bottom on the way in, bottom-to-top on the way out. Correlation should be first (all downstream interceptors use it). Validation should be early (fail fast before expensive operations).

## Step 2: Create a Service Interface

```csharp
public interface IPaymentService
{
    Task<bool> ProcessAsync(ProcessPaymentRequest request);
}

public class PaymentService : IPaymentService
{
    public async Task<bool> ProcessAsync(ProcessPaymentRequest request)
    {
        // Your actual logic
        await Task.Delay(100).ConfigureAwait(false);
        return true;
    }
}
```

## Step 3: Register with Interception

```csharp
services.AddScoped<IPaymentService>(provider =>
{
    var impl = new PaymentService();
    var interceptors = provider.GetServices<IInvocationInterceptor>();
    return BoundaryProxy<IPaymentService>.Create(impl, interceptors);
});
```

## Step 4: Use It

```csharp
var provider = services.BuildServiceProvider();
var paymentService = provider.GetRequiredService<IPaymentService>();

// Call it normally — interception is transparent
var result = await paymentService.ProcessAsync(
    new ProcessPaymentRequest { Amount = 100 }
).ConfigureAwait(false);

// What happened internally:
// 1. CorrelationInterceptor: Generated or extracted correlation ID
// 2. ValidationInterceptor: Validated the request
// 3. TelemetryInterceptor: Measured execution time and exported metrics
```

## Step 5: Add Configuration (Optional)

Control interceptor behavior per method using attributes:

```csharp
public interface IPaymentService
{
    [TimeoutAttribute(timeoutMilliseconds: 5000)]  // Timeout after 5 seconds
    [RetryAttribute(maxAttempts: 3, delayMilliseconds: 500)]  // Retry 3 times
    Task<bool> ProcessAsync(ProcessPaymentRequest request);
}
```

The `TimeoutInterceptor` and `RetryInterceptor` will automatically honor these attributes.

---

## Next Steps

- **Understand the full pipeline?** See [README.md](README.md) and [Execution Order section](README.md#execution-order)
- **Need detailed integration patterns?** Read [USAGE.md](USAGE.md)
- **Looking for a specific interceptor?** Browse [TOC.md](TOC.md) or visit a category README:
  - Security: [Authentication](Authentication/README.md), [Authorization](Authorization/README.md), [Redaction](Redaction/README.md)
  - Observability: [Correlation](Correlation/README.md), [Audit](Auditing/README.md), [Telemetry](Telemetry/README.md), [Status](Status/README.md)
  - Resilience: [Retry](Retry/README.md), [Timeout](Timeout/README.md), [CircuitBreaker](CircuitBreaker/README.md)
  - Data Integrity: [Validation](Validation/README.md), [Idempotency](Idempotency/README.md), [Exception](Exceptions/README.md)

---

## Common Mistakes

### ❌ Registering Interceptors Out of Order

```csharp
// WRONG: Validation runs after authorization
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
```

### ✅ Register in Execution Order

```csharp
// CORRECT: Validate early, authorize later
services.AddScoped<IInvocationInterceptor, AuthenticationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
```

### ❌ Forgetting GetServices()

```csharp
// WRONG: Empty interceptor list
var proxy = BoundaryProxy<IPaymentService>.Create(
    impl, 
    Array.Empty<IInvocationInterceptor>()
);
```

### ✅ Resolve from Dependency Injection

```csharp
// CORRECT: All registered interceptors injected
var interceptors = provider.GetServices<IInvocationInterceptor>();
var proxy = BoundaryProxy<IPaymentService>.Create(impl, interceptors);
```

### ❌ Not Using ConfigureAwait

```csharp
// WRONG: Can cause context switching overhead
var result = await next();
```

### ✅ Always Use ConfigureAwait(false)

```csharp
// CORRECT: No context switching overhead
var result = await next().ConfigureAwait(false);
```

---

**Next:** [USAGE.md](USAGE.md) for integration patterns or [README.md](README.md) for complete overview

