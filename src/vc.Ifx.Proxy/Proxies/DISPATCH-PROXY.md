---
title: DispatchProxy - Understanding the Pattern
doc_type: guide
status: active
last_updated: 2026-08-19
---

# Understanding DispatchProxy and Dynamic Proxies

This document explains the underlying pattern. For practical usage, see [QUICK-START.md](Interceptors/QUICK-START.md).

## What is DispatchProxy?

[DispatchProxy](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.dispatchproxy) is a .NET runtime feature that creates dynamic proxies — objects that intercept method calls on an interface and redirect them to custom logic.

**Key points:**

- Part of `System.Reflection` namespace
- Creates proxies at runtime (no compile-time code generation)
- Works with any interface
- Minimal performance overhead
- Thread-safe across different proxy instances

### Official Microsoft Documentation

- [DispatchProxy API Reference](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.dispatchproxy)
- [CreateProxy Method](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.dispatchproxy.create)
- [Reflection in .NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/reflection/reflection)

---

## How BoundaryProxy Uses DispatchProxy

### The Problem

You have:
```csharp
public interface IPaymentService
{
    Task<bool> ProcessAsync(PaymentRequest request);
}

public class PaymentService : IPaymentService
{
    public async Task<bool> ProcessAsync(PaymentRequest request)
    {
        // Your business logic
    }
}
```

You want to add behavior without modifying `PaymentService`:

- Log all calls
- Validate inputs
- Track execution time
- Retry on failure
- etc.

### The Solution: DispatchProxy

Create a proxy that sits between the caller and the real implementation:

```csharp
Caller
  ↓
[BoundaryInterceptor<IPaymentService>]  ← DispatchProxy
  ↓
[CorrelationInterceptor]
  ↓
[ValidationInterceptor]
  ↓
[TelemetryInterceptor]
  ↓
PaymentService (actual implementation)
```

When the caller invokes `ProcessAsync()`:

1. The proxy's `Invoke()` method is called by the .NET runtime
2. Metadata about the method is captured (name, parameters, return type)
3. The interceptor pipeline is built
4. Each interceptor runs in sequence
5. The actual method is invoked
6. The result flows back through interceptors in reverse order

---

## Core Pattern: The Interceptor Pipeline

### What is InvocationContext?

```csharp
public sealed class InvocationContext
{
    public required Type ContractType { get; init; }      // IPaymentService
    public required MethodInfo Method { get; init; }      // ProcessAsync method info
    public required object Target { get; init; }          // PaymentService instance
    public required IReadOnlyList<InvocationArgument> Arguments { get; init; }  // [request]
    public IDictionary<string, object?> Items { get; }   // Shared data between interceptors
}
```

This object is passed through the entire interceptor pipeline. It contains:
- **Method metadata** — What method was called?
- **Arguments** — What data was passed in?
- **Items dictionary** — A shared store for interceptor communication

### What is IInvocationInterceptor?

```csharp
public interface IInvocationInterceptor
{
    ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next);
}
```

Every interceptor implements this. The pattern:

```csharp
public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
{
    // 1. PRE-INVOCATION: Examine the call
    var correlationId = context.Items[InvocationItemNames.CorrelationId];
    logger.LogInformation("Calling {Method} with correlation {Id}", 
        context.Method.Name, correlationId);

    try
    {
        // 2. INVOKE NEXT: Call the next interceptor (or target method)
        var result = await next().ConfigureAwait(false);

        // 3. POST-INVOCATION: React to success
        logger.LogInformation("Method succeeded");

        return result;
    }
    catch (Exception exception)
    {
        // 4. EXCEPTION HANDLING: Handle errors
        logger.LogError(exception, "Method failed");
        throw;
    }
}
```

### What is InvocationDelegate?

```csharp
public delegate ValueTask<object?> InvocationDelegate();
```

It's a simple function that invokes the next handler in the pipeline. When you call `await next()`, you're calling the next interceptor (or the target method if you're last in the chain).

---

## Execution Order

Interceptors execute **in registration order** on the way in, and **in reverse order** on the way out:

```
Registration Order:
1. CorrelationInterceptor
2. ValidationInterceptor  
3. TelemetryInterceptor

Execution Flow:
→ Correlation (start)
  → Validation (start)
    → Telemetry (start)
      → Target Method
    ← Telemetry (end)
  ← Validation (end)
← Correlation (end)
```

This means:
- **Outer interceptors** (registered first) run pre-logic before inner ones
- **Inner interceptors** (registered last) run closer to the actual method
- **Post-logic** (after `await next()`) runs in reverse order

**Example: Correlation ID must be established first**
```csharp
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();   // Register FIRST
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();    // Then validation
services.AddScoped<IInvocationInterceptor, TelemetryInterceptor>();     // Then telemetry
```

Why? Because downstream interceptors need the correlation ID to be available in `context.Items`.

---

## Return Type Handling

DispatchProxy methods return `object?`, but your interface might return:
- `void` (synchronous)
- `Task` (async, no result)
- `ValueTask` (async struct-based)
- `Task<T>` (async with result)
- `ValueTask<T>` (async struct-based with result)

`BoundaryInterceptor` handles this through reflection:

1. Detect the target method's return type
2. Build an appropriate async wrapper
3. Execute the interceptor pipeline
4. Adapt the result back to the original return type

This happens transparently — your interceptors always work with `ValueTask<object?>`, and the proxy handles the conversion.

---

## Inter-Interceptor Communication: The Items Dictionary

Interceptors can't directly reference each other. Instead, they use `context.Items`:

```csharp
// CorrelationInterceptor stores the correlation ID
var correlationId = Guid.NewGuid().ToString();
context.Items[InvocationItemNames.CorrelationId] = correlationId;
var result = await next().ConfigureAwait(false);

// Downstream interceptor retrieves it
if (context.Items.TryGetValue(InvocationItemNames.CorrelationId, out var id))
{
    logger.LogInformation("Correlation ID: {Id}", id);
}
```

**Important:** The `Items` dictionary is **NOT thread-safe** within a single `InvocationContext` instance, but it **IS safe** across different proxy instances. Each method invocation gets its own context.

---

## Why Use DispatchProxy Over Other Approaches?

| Approach | Pros | Cons |
|----------|------|------|
| **DispatchProxy** | No code generation, simple API, flexible | Runtime overhead, reflection |
| **Source Generators** | Compile-time, zero runtime cost | Complex to implement, must know interfaces at build time |
| **Decorator Pattern** | Explicit, easy to understand | Requires a wrapper class per interface, verbose |
| **Aspect-Oriented Programming (PostSharp)** | Powerful, automatic | Expensive, external dependency, steep learning curve |

DispatchProxy is ideal for frameworks because it's:
- **Simple** — Just implement `IInvocationInterceptor`
- **Flexible** — Works with any interface
- **Composable** — Multiple interceptors in one pipeline
- **No lock-in** — Standard .NET, no special tools needed

---

## Key Concepts from .NET Reflection

This implementation relies on several .NET Reflection features:

### MethodInfo
[MethodInfo](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.methodinfo) represents a method's metadata:
- Name, parameters, return type
- Attributes and custom annotations
- How to invoke it via reflection

### ParameterInfo
[ParameterInfo](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.parameterinfo) represents a single parameter:
- Name, type, default values
- Attributes (e.g., `[Required]`)

### Getting Attributes from Methods
```csharp
// Get a timeout attribute if present
var timeoutAttr = method.GetCustomAttribute<InvocationTimeoutAttribute>(inherit: true);

// GetCustomAttribute checks both the method and interface for the attribute
// (inherit: true includes inherited interfaces)
```

### Invoking Methods at Runtime
```csharp
// Invoke a method by reflection
var result = targetMethod.Invoke(target, args);

// This is how BoundaryInterceptor calls the actual implementation
```

### Handling TargetInvocationException
```csharp
try
{
    result = targetMethod.Invoke(target, args);
}
catch (TargetInvocationException tie) when (tie.InnerException is not null)
{
    // Unwrap the actual exception for cleaner stack traces
    ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
}
```

See: [TargetInvocationException](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.targetinvocationexception)

---

## Async Patterns and ValueTask

This framework uses `ValueTask<object?>` throughout for efficiency:

```csharp
public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
{
    var result = await next().ConfigureAwait(false);  // ValueTask<object?>
    return result;
}
```

**Why ValueTask?**
- If the operation completes synchronously, no allocation
- If it's async, allocated once and used multiple times
- Better for frameworks that expect high throughput

[ValueTask vs Task](https://learn.microsoft.com/en-us/archive/msdn-magazine/2015-07/async-await-best-practices-in-asynchronous-programming#important-async-return-types)

**ConfigureAwait(false)?**
- Avoids unnecessary context switching
- Critical in libraries (IFX is a library)
- See: [ConfigureAwait in Library Code](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html)

---

## Performance Considerations

1. **Reflection overhead** — Method lookups and invocations via reflection have overhead
   - Mitigated by: Caching, proper use of `ConfigureAwait(false)`, avoiding boxing

2. **Allocation** — Each invocation allocates `InvocationContext` and `InvocationArgument[]`
   - Mitigated by: Using `ValueTask` instead of `Task`, array pooling (if needed)

3. **Serialization in interceptors** — Some interceptors (Audit, Telemetry) may serialize arguments
   - Mitigated by: Optional redaction, batch writes

For most applications, the framework's overhead is negligible compared to actual I/O (database, network).

---

## Further Reading

- [DispatchProxy on Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.dispatchproxy)
- [Reflection in .NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/reflection/reflection)
- [Async/Await Best Practices](https://learn.microsoft.com/en-us/archive/msdn-magazine/2015-07/async-await-best-practices-in-asynchronous-programming)
- [Proxy Pattern on Refactoring.Guru](https://refactoring.guru/design-patterns/proxy)
- [Decorator Pattern on Refactoring.Guru](https://refactoring.guru/design-patterns/decorator)
