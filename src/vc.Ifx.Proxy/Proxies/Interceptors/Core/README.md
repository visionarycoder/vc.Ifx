---
title: Core Infrastructure — Interceptors Framework
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Foundational interfaces and types for the interceptor pipeline
tags:
  - core
  - infrastructure
  - contracts
audience: developer
source_paths:
  - Core/ITransientExceptionDetector.cs
  - Core/MethodContext.cs
  - Core/MethodArgument.cs
  - Core/InvocationItemNames.cs
  - Core/InvocationDelegate.cs
  - Core/DefaultTransientExceptionDetector.cs
---

# Core Infrastructure

Foundational interfaces and types that power the Boundary Interceptors framework.

## Files

| File | Purpose |
|---|---|
| `ITransientExceptionDetector.cs` | Contract for identifying transient exceptions safe to retry |
| `DefaultTransientExceptionDetector.cs` | Default implementation (TimeoutException, IOException) |
| `MethodContext.cs` | Execution context passed through the interceptor pipeline |
| `MethodArgument.cs` | Metadata about a single method parameter |
| `InvocationItemNames.cs` | Standard keys for the `context.Items` dictionary |
| `InvocationDelegate.cs` | Function delegate for pipeline chaining |

## Key Types

### MethodContext

Passed to every interceptor in the pipeline. Contains:

| Property | Purpose |
|---|---|
| `ContractType` | The interface being called |
| `MethodInfo` | Reflection info for the method |
| `Arguments` | Method arguments as array |
| `TargetObject` | The actual implementation instance |
| `Items` | Shared dictionary for inter-interceptor communication |
| `CancellationToken` | For async cancellation |

```csharp
public async ValueTask<object?> InvokeAsync(MethodContext context, InvocationDelegate next)
{
    // Read method info
    var methodName = context.MethodInfo.Name;
    var arguments = context.Arguments;
    
    // Store data for downstream interceptors
    context.Items["MyKey"] = "MyValue";
    
    // Call next interceptor
    var result = await next().ConfigureAwait(false);
    
    // Read data from upstream interceptors
    var correlationId = context.Items.TryGetValue(
        InvocationItemNames.CorrelationId, 
        out var cid
    ) ? cid : null;
    
    return result;
}
```

### IInvocationInterceptor

Every interceptor implements this interface:

```csharp
public interface IInvocationInterceptor
{
    ValueTask<object?> InvokeAsync(
        MethodContext context,
        InvocationDelegate next);
}
```

**Implementing a Custom Interceptor:**

```csharp
public sealed class LoggingInterceptor : IInvocationInterceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;
    
    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask<object?> InvokeAsync(
        MethodContext context,
        InvocationDelegate next)
    {
        _logger.LogInformation(
            "Calling {Method}",
            context.MethodInfo.Name
        );
        
        try
        {
            var result = await next().ConfigureAwait(false);
            _logger.LogInformation(
                "Method {Method} succeeded",
                context.MethodInfo.Name
            );
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Method {Method} failed",
                context.MethodInfo.Name
            );
            throw;
        }
    }
}
```

### InvocationItemNames

Standard keys for storing and retrieving data in `context.Items`:

```csharp
public static class InvocationItemNames
{
    public const string CorrelationId = "CorrelationId";
    public const string AuthenticationToken = "AuthenticationToken";
    public const string Principal = "Principal";
    public const string SafeArguments = "SafeArguments";
    // ... more keys
}
```

Use these keys to communicate between interceptors:

```csharp
// In JwtAuthenticationInterceptor
context.Items[InvocationItemNames.Principal] = principal;

// In AuthorizationInterceptor
if (context.Items.TryGetValue(InvocationItemNames.Principal, out var principal))
{
    // Use principal for policy checks
}
```

### ITransientExceptionDetector

Contract for identifying exceptions safe to retry:

```csharp
public interface ITransientExceptionDetector
{
    bool IsTransient(Exception exception);
}

// Default implementation
public sealed class DefaultTransientExceptionDetector : ITransientExceptionDetector
{
    public bool IsTransient(Exception exception)
        => exception is TimeoutException or IOException;
}
```

Used by:
- `RetryInterceptor` — Determines if exception warrants a retry
- `CircuitBreakerInterceptor` — Counts transient failures toward circuit breaker threshold

---

## Pipeline Execution Flow

```
1. Method called on proxy interface
2. BoundaryProxy captures metadata
3. Creates MethodContext with arguments
4. Builds interceptor chain
5. Interceptor 1: Runs logic, calls next()
   6. Interceptor 2: Runs logic, calls next()
      7. Interceptor 3: Runs logic, calls next()
         8. Target method executes
      9. Interceptor 3: Post-execution logic
   10. Interceptor 2: Post-execution logic
11. Interceptor 1: Post-execution logic, returns result
```

Each interceptor sees the full context and can:
- Inspect method name, arguments, types
- Store data for downstream interceptors
- Modify behavior before/after invocation
- Handle exceptions

---

## Common InvocationItemNames

| Key | Type | Set By | Used By |
|---|---|---|---|
| `CorrelationId` | string | CorrelationInterceptor | All observability interceptors |
| `AuthenticationToken` | string | (application code) | JwtAuthenticationInterceptor |
| `Principal` | ClaimsPrincipal | JwtAuthenticationInterceptor | AuthorizationInterceptor |
| `SafeArguments` | object[] | RedactionInterceptor | AuditInterceptor, logging |

---

## Design Principles

- **All types are sealed** — Prevent unintended inheritance
- **ValueTask for efficiency** — No allocation if synchronous
- **ConfigureAwait(false)** — Avoid context switching overhead
- **Thread-safe across invocations** — But not within a single invocation
- **Reflection used sparingly** — Only to detect return types

---

## Related

- [BoundaryProxy Infrastructure](../../README.md) — Dynamic proxy using DispatchProxy
- [Interceptor Registration](../USAGE.md#registration-order) — How to register interceptors
- [Custom Interceptors](../USAGE.md#custom-interceptor-implementation) — Implement your own

## See Also

- [README.md](../README.md) — Main interceptors overview
- [TOC.md](../TOC.md) — Complete table of contents

