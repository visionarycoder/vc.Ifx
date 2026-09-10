---
title: Boundary Proxies
doc_type: guide
status: active
last_updated: 2026-08-19
---

# Proxies

Dynamic proxy generation for method interception using DispatchProxy.

## What It Does

The Proxies folder provides the **runtime proxy infrastructure** that enables the Interceptors framework. It uses .NET's `DispatchProxy` to dynamically create proxy objects that wrap target implementations and apply a pipeline of interceptors.

```
User calls interface method
    ↓
DispatchProxy.Invoke intercepts the call
    ↓
Build interceptor pipeline
    ↓
Execute each interceptor in sequence
    ↓
Call target implementation
    ↓
Return result through pipeline
```

## Core Files

### BoundaryProxy<TContract>

The core dynamic proxy factory. Implements `DispatchProxy` to intercept method calls.

```csharp
public sealed class BoundaryProxy<TContract> : DispatchProxy
    where TContract : class
{
    public static TContract Create(
        TContract target,
        IEnumerable<IInvocationInterceptor> interceptors);
}
```

**Usage:**

```csharp
var target = new PaymentService();
var interceptors = new[] 
{ 
    new JwtAuthenticationInterceptor(),
    new AuditInterceptor()
};

var proxy = BoundaryProxy<IPaymentService>.Create(target, interceptors);
await proxy.ProcessAsync(request);  // Interceptors run automatically
```

**Supported Return Types:**
- `void` (synchronous)
- `Task` (async, no result)
- `ValueTask` (async struct-based, no result)
- `Task<T>` (async with result)
- `ValueTask<T>` (async struct-based with result)

### BoundaryProxyServiceCollectionExtensions

Extension methods for registering intercepted services in dependency injection.

```csharp
public static IServiceCollection AddBoundaryInterceptedScoped<TContract, TImplementation>(
    this IServiceCollection services)
    where TContract : class
    where TImplementation : class, TContract
```

**Usage:**

```csharp
var services = new ServiceCollection();

// Register interceptors first
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();

// Register service with automatic proxy wrapping
services.AddBoundaryInterceptedScoped<IPaymentService, PaymentService>();

var serviceProvider = services.BuildServiceProvider();
var payment = serviceProvider.GetRequiredService<IPaymentService>();

await payment.ProcessAsync(request);  // All interceptors applied
```

## How It Works

1. **Proxy Creation** — `BoundaryProxy<T>.Create()` uses `DispatchProxy.Create()` to generate a runtime proxy class
2. **Interception** — Proxy's `Invoke()` method is called for every method invocation
3. **Pipeline Building** — All registered interceptors are sequenced in order
4. **Pipeline Execution** — Each interceptor gets a chance to examine/modify before calling next
5. **Result Adaptation** — Return value is adapted to expected type (void, Task, ValueTask, etc.)
6. **Return to Caller** — Result flows back through interceptor pipeline

## Interceptor Pipeline

Interceptors execute in **registration order**:

```csharp
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();   // 1st
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();    // 2nd
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();       // 3rd
// ... etc
```

Request flow:
```
Authentication → Authorization → Validation → ... → Target → Response
```

## Context Sharing

All interceptors in a pipeline share the same `MethodContext.Items` dictionary for cross-interceptor communication:

```csharp
// JwtAuthenticationInterceptor sets:
context.Items[InvocationItemNames.Principal] = principal;

// AuthorizationInterceptor reads:
var principal = (ClaimsPrincipal)context.Items[InvocationItemNames.Principal];

// AuditInterceptor reads:
var principal = (ClaimsPrincipal)context.Items[InvocationItemNames.Principal];
```

## Limitations

- Cannot intercept static methods
- Cannot modify method parameters (read-only)
- Internal method calls bypass the proxy
- Proxy only works with interface-based services

## Thread Safety

`BoundaryProxy<T>` is thread-safe:
- Each method invocation gets its own `MethodContext` instance
- No shared mutable state between invocations
- Safe for concurrent use in web applications

## Performance Characteristics

- **Proxy creation** — ~1ms per factory call (one-time cost)
- **Proxy invocation** — ~0.1ms overhead per method call (reflection cost)
- **Memory** — Minimal allocation; reuses `MethodContext` types
- **GC pressure** — Low; interceptor pipeline uses arrays (no allocations on happy path)

## Integration Patterns

### Pattern 1: Manual Proxy Creation

```csharp
var service = new MyService();
var interceptors = GetInterceptors();  // From DI container
var proxy = BoundaryProxy<IMyService>.Create(service, interceptors);
```

### Pattern 2: DI Registration

```csharp
services.AddBoundaryInterceptedScoped<IMyService, MyService>();
```

### Pattern 3: Factory with Configuration

```csharp
services.AddScoped<IMyService>(provider =>
{
    var impl = new MyService(/* dependencies */);
    var interceptors = provider.GetServices<IInvocationInterceptor>();
    var filtered = interceptors.Where(i => i.Applies(typeof(IMyService)));
    return BoundaryProxy<IMyService>.Create(impl, filtered);
});
```

## Troubleshooting

### "No parameterless constructor"

DispatchProxy requires the proxied interface to have concrete implementation. Ensure:
- Target class is concrete (not abstract)
- All interface methods are implemented
- Constructor matches implementation

### "Method not being intercepted"

Verify:
- Service is retrieved through proxy (not direct implementation)
- Interceptors are registered before service registration
- Method is on the interface (not internal/private)
- Interface is not static

### "Interceptor not seeing context items"

Ensure interceptors execute in correct order:
```csharp
// Producer must run before consumer
services.AddScoped<IInvocationInterceptor, ProducerInterceptor>();    // Sets context items
services.AddScoped<IInvocationInterceptor, ConsumerInterceptor>();    // Reads context items
```

## Related Folders

- [Interceptors/](Interceptors/README.md) — All interceptor implementations (13 total)
- [Interceptors/Core/](Interceptors/Core/README.md) — Base contracts and MethodContext
- [Interceptors/Authentication/](Interceptors/Authentication/README.md) — JWT validation
- [Interceptors/Authorization/](Interceptors/Authorization/README.md) — Policy enforcement

## References

- Microsoft Docs: [DispatchProxy Class](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.dispatchproxy)
- MSDN: [Dynamic Proxies in C#](https://docs.microsoft.com/en-us/archive/blogs/rmbyers/managed-reflection-emit-rationale-and-source-code)
- Pattern: Decorator/Chain of Responsibility hybrid with runtime proxy generation

