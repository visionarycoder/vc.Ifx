---
title: Integration Patterns for Boundary Interceptors
doc_type: guide
status: active
last_updated: 2026-08-19
summary: Complete reference for integrating interceptors into applications
tags:
  - integration
  - usage-patterns
  - dependency-injection
audience: developer
---

# Integration Patterns

Complete reference for integrating Boundary Interceptors into your application.

## Recommended Registration Order

```csharp
var services = new ServiceCollection();

// 1. OBSERVABILITY: Establish correlation & baseline logging
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();

// 2. SECURITY: Establish authenticated identity (must run early)
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();

// 3. REDACTION: Mask sensitive data (before validation/audit logging)
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();

// 4. VALIDATION: Fail fast on bad input
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();

// 5. SECURITY: Check authorization (after authentication & validation)
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();

// 6. OBSERVABILITY: Track execution status
services.AddScoped<IInvocationInterceptor, StatusInterceptor>();

// 7. RESILIENCE: Implement timeout
services.AddScoped<IInvocationInterceptor, TimeoutInterceptor>();

// 8. RESILIENCE: Implement retry logic
services.AddScoped<IInvocationInterceptor, RetryInterceptor>();

// 9. RESILIENCE: Prevent cascading failures
services.AddScoped<IInvocationInterceptor, CircuitBreakerInterceptor>();

// 10. OPTIMIZATION: Cache idempotent results
services.AddScoped<IInvocationInterceptor, IdempotencyInterceptor>();

// 11. ERROR HANDLING: Transform exceptions at the boundary
services.AddScoped<IInvocationInterceptor, ExceptionInterceptor>();

// 12. OBSERVABILITY: Record telemetry
services.AddScoped<IInvocationInterceptor, TelemetryInterceptor>();

// 13. AUDITING: Record all activity for compliance
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();
```

**Why this order matters:**
- Correlation first → all downstream use the same ID
- Authentication early → all downstream security depends on identity
- Redaction before logging → sensitive data never in logs
- Validation early → fail cheap before expensive operations
- Authorization after validation → don't check perms if data is invalid
- Timeout/Retry before business logic → enforces limits
- Audit/Telemetry last → observe the complete flow

See [README.md](./README.md#execution-order) for detailed rationale.

---

## Registering Services with Interception

### Pattern 1: Simple Service

```csharp
services.AddScoped<IPaymentService>(provider =>
{
    var impl = new PaymentService(
        provider.GetRequiredService<IPaymentGateway>());
    var interceptors = provider.GetServices<IInvocationInterceptor>();
    return BoundaryProxy<IPaymentService>.Create(impl, interceptors);
});
```

### Pattern 2: With Decorators or Other Patterns

```csharp
services.AddScoped<IPaymentService>(provider =>
{
    // Build your inner decorators first
    var gateway = provider.GetRequiredService<IPaymentGateway>();
    var cached = new CachedPaymentGateway(gateway);
    
    // Create the actual implementation with dependencies
    var impl = new PaymentService(cached);
    
    // Wrap with interceptors
    var interceptors = provider.GetServices<IInvocationInterceptor>();
    return BoundaryProxy<IPaymentService>.Create(impl, interceptors);
});
```

### Pattern 3: Multiple Implementations

```csharp
// Register based on configuration
services.AddScoped<IPaymentService>(provider =>
{
    IPaymentService impl = provider.GetRequiredService<IConfiguration>()["UseStripe"] == "true"
        ? new StripePaymentService(provider.GetRequiredService<StripeClient>())
        : new PaypalPaymentService(provider.GetRequiredService<PaypalClient>());
    
    var interceptors = provider.GetServices<IInvocationInterceptor>();
    return BoundaryProxy<IPaymentService>.Create(impl, interceptors);
});
```

---

## Attribute-Based Configuration

### Controlling Behavior Per Method

```csharp
public interface IPaymentService
{
    /// Single method with specific config
    [RequireAuthentication]
    [RequireAuthorization("payment-processor")]
    [Timeout(Milliseconds = 10000)]
    [Retry(MaxRetries = 3, InitialDelayMs = 500)]
    Task<PaymentResult> ProcessAsync(PaymentRequest request);

    /// Another method with different config
    [Timeout(Milliseconds = 5000)]
    [Idempotent(DurationSeconds = 300)]
    Task<RefundResult> RefundAsync(RefundRequest request);
}
```

### Common Attributes

| Attribute | Interceptor | Purpose |
|---|---|---|
| `[RequireAuthentication]` | AuthenticationInterceptor | Require JWT token validation |
| `[RequireAuthorization("policy")]` | AuthorizationInterceptor | Check policy before proceeding |
| `[Timeout(Milliseconds = ms)]` | TimeoutInterceptor | Fail if execution exceeds ms |
| `[Retry(MaxRetries = n, InitialDelayMs = ms)]` | RetryInterceptor | Retry up to N times with delay |
| `[Idempotent(DurationSeconds = sec)]` | IdempotencyInterceptor | Cache result for duplicate calls |
| `[CircuitBreaker(FailureThreshold = n, TimeoutMs = ms)]` | CircuitBreakerInterceptor | Open circuit after N failures |

---

## Custom Interceptor Implementation

### Basic Pattern

```csharp
public sealed class MyCustomInterceptor : IInvocationInterceptor
{
    private readonly ILogger<MyCustomInterceptor> _logger;

    public MyCustomInterceptor(ILogger<MyCustomInterceptor> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(
        MethodContext context, 
        InvocationDelegate next)
    {
        // PRE-INVOCATION
        _logger.LogInformation("Method: {MethodName}", context.MethodInfo.Name);

        try
        {
            // INVOKE NEXT
            var result = await next().ConfigureAwait(false);

            // POST-INVOCATION
            _logger.LogInformation("Method succeeded");
            return result;
        }
        catch (Exception exception)
        {
            // EXCEPTION HANDLING
            _logger.LogError(exception, "Method failed");
            throw;
        }
    }
}
```

### Pattern: Conditional Behavior Based on Attributes

```csharp
public sealed class ConditionalInterceptor : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(
        MethodContext context, 
        InvocationDelegate next)
    {
        // Check if this method has a specific attribute
        var timeoutAttr = context.MethodInfo.GetCustomAttribute<TimeoutAttribute>();
        if (timeoutAttr is not null)
        {
            // Apply custom logic based on the attribute
            using var cts = new CancellationTokenSource(timeout.MillisecondsTimeout);
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw new InvocationTimeoutException($"Method {context.Method.Name} timed out");
            }
        }

        // No attribute, proceed normally
        return await next().ConfigureAwait(false);
    }
}
```

### Pattern: Sharing Data via Context.Items

```csharp
public sealed class ProducerInterceptor : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(
        InvocationContext context, 
        InvocationDelegate next)
    {
        // Compute something
        var computedValue = Guid.NewGuid().ToString();
        
        // Store in context for downstream interceptors
        context.Items["MyComputedValue"] = computedValue;
        
        return await next().ConfigureAwait(false);
    }
}

public sealed class ConsumerInterceptor : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(
        InvocationContext context, 
        InvocationDelegate next)
    {
        // Retrieve computed value from upstream interceptor
        if (context.Items.TryGetValue("MyComputedValue", out var value))
        {
            logger.LogInformation("Computed value: {Value}", value);
        }
        
        return await next().ConfigureAwait(false);
    }
}
```

---

## Working with InvocationContext

### Inspecting Method Metadata

```csharp
public async ValueTask<object?> InvokeAsync(
    InvocationContext context, 
    InvocationDelegate next)
{
    // Method name
    var methodName = context.Method.Name;

    // Return type
    var returnType = context.Method.ReturnType;

    // Parameters
    var parameters = context.Method.GetParameters();
    foreach (var param in parameters)
    {
        logger.LogInformation("Param: {Name} of type {Type}", 
            param.Name, param.ParameterType);
    }

    // Interface type
    var interfaceType = context.ContractType;

    return await next().ConfigureAwait(false);
}
```

### Working with Arguments

```csharp
public async ValueTask<object?> InvokeAsync(
    InvocationContext context, 
    InvocationDelegate next)
{
    // Arguments are already parsed
    foreach (var arg in context.Arguments)
    {
        logger.LogInformation(
            "Arg {Name} ({Type}): {@Value}", 
            arg.Name, 
            arg.ParameterType, 
            arg.Value);
    }

    return await next().ConfigureAwait(false);
}
```

### Modifying Context Before Invocation

```csharp
public async ValueTask<object?> InvokeAsync(
    InvocationContext context, 
    InvocationDelegate next)
{
    // Modify argument values before target invocation
    var firstArg = context.Arguments[0];
    if (firstArg.Value is MyRequest request)
    {
        // Modify the request
        request.Timestamp = DateTimeOffset.UtcNow;
        
        // This affects what the target method receives
    }

    return await next().ConfigureAwait(false);
}
```

---

## Exception Handling in Interceptors

### Pattern 1: Catch and Log, Re-throw

```csharp
public async ValueTask<object?> InvokeAsync(
    InvocationContext context, 
    InvocationDelegate next)
{
    try
    {
        return await next().ConfigureAwait(false);
    }
    catch (InvalidOperationException ioe)
    {
        logger.LogError(ioe, "Invalid operation in {Method}", 
            context.Method.Name);
        throw;  // Re-throw to let other interceptors handle it
    }
}
```

### Pattern 2: Transform Exceptions

```csharp
public async ValueTask<object?> InvokeAsync(
    InvocationContext context, 
    InvocationDelegate next)
{
    try
    {
        return await next().ConfigureAwait(false);
    }
    catch (HttpRequestException hre)
    {
        // Transform external exception to domain exception
        throw new ServiceUnavailableException(
            $"Service {context.Method.Name} is unavailable", 
            hre);
    }
}
```

### Pattern 3: Swallow (Use Carefully!)

```csharp
public async ValueTask<object?> InvokeAsync(
    InvocationContext context, 
    InvocationDelegate next)
{
    try
    {
        return await next().ConfigureAwait(false);
    }
    catch (OperationCanceledException oce)
    {
        logger.LogWarning(oce, "Operation cancelled");
        // Return a default value instead of throwing
        return GetDefaultResult(context.Method.ReturnType);
    }
}
```

---

## Testing Interceptors

### Unit Test Pattern

```csharp
[TestClass]
public class MyInterceptorTests
{
    [TestMethod]
    public async Task InvokeAsync_LogsMethodName()
    {
        // Arrange
        var logger = new Mock<ILogger<MyInterceptor>>();
        var interceptor = new MyInterceptor(logger.Object);
        
        var context = new InvocationContext
        {
            ContractType = typeof(ITestService),
            Method = typeof(ITestService).GetMethod("TestMethod")!,
            Target = new TestService(),
            Arguments = Array.Empty<InvocationArgument>()
        };
        
        ValueTask<object?> next() => new(default(object?));

        // Act
        await interceptor.InvokeAsync(context, next);

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMethod")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
```

---

## Performance Best Practices

### ✅ DO

- Use `ConfigureAwait(false)` in all async operations
- Minimize allocations in pre-invocation logic
- Cache reflection results if calling frequently
- Use `ValueTask` for interceptors

### ❌ DON'T

- Block on async code (don't use `.Result` or `.Wait()`)
- Perform expensive I/O in interceptors unless necessary
- Allocate large objects per invocation
- Log serialized arguments without redaction

---

## Selectively Disabling Interceptors

### Pattern: Environment-Based Configuration

```csharp
services.AddScoped<IInvocationInterceptor>(provider =>
{
    var env = provider.GetRequiredService<IHostEnvironment>();
    
    if (env.IsProduction())
    {
        return new AuditInterceptor(...);  // Always audit in production
    }
    
    return new NoOpInterceptor();  // Skip audit in development
});
```

### Pattern: NoOp Interceptor

```csharp
public sealed class NoOpInterceptor : IInvocationInterceptor
{
    public ValueTask<object?> InvokeAsync(
        InvocationContext context, 
        InvocationDelegate next)
        => next();
}
```

---

## Next Steps

- See [Proxies/README.md](../README.md) for the underlying DispatchProxy pattern
- Browse category READMEs for specific interceptor usage
- Check category folders for implementation details

