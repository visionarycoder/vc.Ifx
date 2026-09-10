---
title: Boundary Interception Patterns (Legacy)
doc_type: reference
status: superseded
last_updated: 2026-08-19
summary: Deprecated—use QUICK-START.md, USAGE.md, or README.md instead
tags:
  - interceptors
  - deprecated
  - legacy
audience: developer
---

# Boundary Interception with DispatchProxy

> ⚠️ **This document is superseded.** Use the newer documentation for accurate information:
> - **Quick Start** → [QUICK-START.md](./QUICK-START.md) (5 minutes)
> - **Integration Patterns** → [USAGE.md](./USAGE.md) (detailed reference)
> - **Overview** → [README.md](./README.md) (framework guide)

## Purpose

This pattern adds cross-cutting behavior to existing interface-based services with minimal changes to the application.

The application continues to call its existing interfaces. `DispatchProxy` inserts an in-memory interception layer between the caller and the real implementation.

Typical uses include:

- Correlation tracking
- Argument redaction
- Telemetry
- Audit logging
- Call status tracking
- Validation
- Timeouts
- Retries
- Circuit breakers
- Authorization
- Idempotency
- Exception normalization

The primary developer-visible change is in dependency injection registration.

---

## 1. Runtime Flow

```text
Caller
  ↓
BoundaryInterceptor<TContract>
  ↓
CorrelationInterceptor
  ↓
ArgumentRedactionInterceptor
  ↓
TelemetryInterceptor
  ↓
AuditInterceptor
  ↓
StatusInterceptor
  ↓
ValidationInterceptor
  ↓
TimeoutInterceptor
  ↓
RetryInterceptor
  ↓
CircuitBreakerInterceptor
  ↓
AuthorizationInterceptor
  ↓
IdempotencyInterceptor
  ↓
ExceptionInterceptor
  ↓
Actual Implementation
```

Interceptors execute in DI registration order on the way into the call.

Post-call behavior executes in reverse order as the call returns through the pipeline.

---

## 2. Core Invocation Types

### InvocationArgument

```csharp
public sealed record InvocationArgument(string Name, Type ParameterType, object? Value);
```

### InvocationDelegate

```csharp
public delegate ValueTask<object?> InvocationDelegate();
```

### IInvocationInterceptor

```csharp
public interface IInvocationInterceptor
{
    ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next);
}
```

### InvocationItemNames

```csharp
public static class InvocationItemNames
{
    public const string CorrelationId = nameof(CorrelationId);
    public const string SafeArguments = nameof(SafeArguments);
}
```

### InvocationContext

```csharp
using System.Reflection;

public sealed class InvocationContext
{
    public required Type ContractType { get; init; }

    public required MethodInfo Method { get; init; }

    public required object Target { get; init; }

    public required IReadOnlyList<InvocationArgument> Arguments { get; init; }

    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    public TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute
        => Method.GetCustomAttribute<TAttribute>(true) ?? ContractType.GetCustomAttribute<TAttribute>(true);

    public bool HasAttribute<TAttribute>() where TAttribute : Attribute
        => GetAttribute<TAttribute>() is not null;
}
```

---

## 3. BoundaryInterceptor

`BoundaryInterceptor<TContract>` is the central proxy.

It:

1. Receives calls made through an interface.
2. Captures method metadata and arguments.
3. Builds an interceptor pipeline.
4. Invokes the real implementation.
5. Handles synchronous and asynchronous return values.

```csharp
using System.Reflection;
using System.Runtime.ExceptionServices;

public sealed class BoundaryInterceptor<TContract> : DispatchProxy where TContract : class
{
    private TContract target = null!;

    private IReadOnlyList<IInvocationInterceptor> interceptors = [];

    public static TContract Create(TContract target, IEnumerable<IInvocationInterceptor> interceptors)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(interceptors);

        var proxy = Create<TContract, BoundaryInterceptor<TContract>>();
        var interceptor = (BoundaryInterceptor<TContract>)(object)proxy;

        interceptor.target = target;
        interceptor.interceptors = interceptors.ToArray();

        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        args ??= [];

        var parameters = targetMethod.GetParameters();

        var arguments = parameters
            .Select((parameter, index) => new InvocationArgument(parameter.Name ?? $"arg{index}", parameter.ParameterType, args[index]))
            .ToArray();

        var context = new InvocationContext
        {
            ContractType = typeof(TContract),
            Method = targetMethod,
            Target = target,
            Arguments = arguments
        };

        InvocationDelegate pipeline = () => InvokeTargetAsync(targetMethod, args);

        for (var index = interceptors.Count - 1; index >= 0; index--)
        {
            var current = interceptors[index];
            var next = pipeline;

            pipeline = () => current.InvokeAsync(context, next);
        }

        return AdaptReturnValue(targetMethod.ReturnType, pipeline);
    }

    private async ValueTask<object?> InvokeTargetAsync(MethodInfo method, object?[] args)
    {
        object? result;

        try
        {
            result = method.Invoke(target, args);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }

        return await AwaitResultAsync(method.ReturnType, result).ConfigureAwait(false);
    }

    private static object? AdaptReturnValue(Type returnType, InvocationDelegate pipeline)
    {
        if (returnType == typeof(void))
        {
            pipeline().AsTask().GetAwaiter().GetResult();
            return null;
        }

        if (returnType == typeof(Task))
            return ExecuteTaskAsync(pipeline);

        if (returnType == typeof(ValueTask))
            return new ValueTask(ExecuteTaskAsync(pipeline));

        if (returnType.IsGenericType)
        {
            var genericType = returnType.GetGenericTypeDefinition();
            var resultType = returnType.GetGenericArguments()[0];

            if (genericType == typeof(Task<>))
            {
                return typeof(BoundaryInterceptor<TContract>)
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                    .Single(method => method.Name == nameof(ExecuteTaskAsync) && method.IsGenericMethodDefinition)
                    .MakeGenericMethod(resultType)
                    .Invoke(null, [pipeline]);
            }

            if (genericType == typeof(ValueTask<>))
            {
                return typeof(BoundaryInterceptor<TContract>)
                    .GetMethod(nameof(ExecuteValueTask), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(resultType)
                    .Invoke(null, [pipeline]);
            }
        }

        return pipeline().AsTask().GetAwaiter().GetResult();
    }

    private static async ValueTask<object?> AwaitResultAsync(Type returnType, object? result)
    {
        if (returnType == typeof(void))
            return null;

        if (returnType == typeof(Task))
        {
            await ((Task)result!).ConfigureAwait(false);
            return null;
        }

        if (returnType == typeof(ValueTask))
        {
            await ((ValueTask)result!).ConfigureAwait(false);
            return null;
        }

        if (returnType.IsGenericType)
        {
            var genericType = returnType.GetGenericTypeDefinition();

            if (genericType == typeof(Task<>))
            {
                var task = (Task)result!;

                await task.ConfigureAwait(false);

                return task.GetType().GetProperty("Result")!.GetValue(task);
            }

            if (genericType == typeof(ValueTask<>))
            {
                var asTaskMethod = returnType.GetMethod("AsTask")!;
                var task = (Task)asTaskMethod.Invoke(result, null)!;

                await task.ConfigureAwait(false);

                return task.GetType().GetProperty("Result")!.GetValue(task);
            }
        }

        return result;
    }

    private static async Task ExecuteTaskAsync(InvocationDelegate pipeline)
    {
        await pipeline().ConfigureAwait(false);
    }

    private static async Task<TResult?> ExecuteTaskAsync<TResult>(InvocationDelegate pipeline)
    {
        var result = await pipeline().ConfigureAwait(false);

        return result is null
            ? default
            : (TResult)result;
    }

    private static ValueTask<TResult?> ExecuteValueTask<TResult>(InvocationDelegate pipeline)
        => new(ExecuteTaskAsync<TResult>(pipeline));
}
```

---

## 4. DI Registration Extension

```csharp
using Microsoft.Extensions.DependencyInjection;

public static class BoundaryInterceptionServiceCollectionExtensions
{
    public static IServiceCollection AddBoundaryInterceptedScoped<TContract, TImplementation>(this IServiceCollection services)
        where TContract : class
        where TImplementation : class, TContract
    {
        services.AddScoped<TImplementation>();

        services.AddScoped<TContract>(serviceProvider =>
        {
            var target = serviceProvider.GetRequiredService<TImplementation>();
            var interceptors = serviceProvider.GetServices<IInvocationInterceptor>();

            return BoundaryInterceptor<TContract>.Create(target, interceptors);
        });

        return services;
    }
}
```

Existing registration:

```csharp
services.AddScoped<IFileHandlingEngine, FileHandlingEngine>();
```

Intercepted registration:

```csharp
services.AddBoundaryInterceptedScoped<IFileHandlingEngine, FileHandlingEngine>();
```

---

## 5. CorrelationInterceptor

```csharp
using System.Diagnostics;

public sealed class CorrelationInterceptor : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        var correlationId = FindServiceMessage(context.Arguments)?.CorrelationId
            ?? Activity.Current?.TraceId.ToString()
            ?? Guid.NewGuid().ToString("N");

        context.Items[InvocationItemNames.CorrelationId] = correlationId;

        return await next().ConfigureAwait(false);
    }

    private static IServiceMessage? FindServiceMessage(IEnumerable<InvocationArgument> arguments)
    {
        foreach (var argument in arguments)
        {
            if (argument.Value is IServiceMessage serviceMessage)
                return serviceMessage;

            if (argument.Value is null)
                continue;

            foreach (var property in argument.Value.GetType().GetProperties())
            {
                if (property.GetIndexParameters().Length > 0)
                    continue;

                if (property.GetValue(argument.Value) is IServiceMessage nestedMessage)
                    return nestedMessage;
            }
        }

        return null;
    }
}
```

Example application contract:

```csharp
public interface IServiceMessage
{
    string? CorrelationId { get; }
}
```

---

## 6. ArgumentRedactionInterceptor

```csharp
public sealed class ArgumentRedactionInterceptor : IInvocationInterceptor
{
    private static readonly string[] SensitiveNames =
    [
        "password",
        "secret",
        "token",
        "apikey",
        "authorization",
        "credential"
    ];

    public ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        var safeArguments = context.Arguments.ToDictionary(
            argument => argument.Name,
            argument => ShouldRedact(argument.Name) ? "***REDACTED***" : argument.Value);

        context.Items[InvocationItemNames.SafeArguments] = safeArguments;

        return next();
    }

    private static bool ShouldRedact(string name)
        => SensitiveNames.Any(value => name.Contains(value, StringComparison.OrdinalIgnoreCase));
}
```

---

## 7. TelemetryInterceptor

```csharp
using System.Diagnostics;
using Microsoft.Extensions.Logging;

public sealed class TelemetryInterceptor(ILogger<TelemetryInterceptor> logger) : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await next().ConfigureAwait(false);

            logger.LogDebug("{Contract}.{Method} completed in {ElapsedMilliseconds} ms", context.ContractType.Name, context.Method.Name, stopwatch.Elapsed.TotalMilliseconds);

            return result;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "{Contract}.{Method} failed after {ElapsedMilliseconds} ms", context.ContractType.Name, context.Method.Name, stopwatch.Elapsed.TotalMilliseconds);

            throw;
        }
    }
}
```

---

## 8. AuditInterceptor

### Supporting types

```csharp
public sealed record AuditEntry(
    string? CorrelationId,
    string Contract,
    string Method,
    object? Arguments,
    bool Succeeded,
    string? Error,
    DateTimeOffset TimestampUtc);

public interface IAuditSink
{
    ValueTask WriteAsync(AuditEntry entry);
}
```

### Interceptor

```csharp
public sealed class AuditInterceptor(IAuditSink auditSink) : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        context.Items.TryGetValue(InvocationItemNames.CorrelationId, out var correlationValue);
        context.Items.TryGetValue(InvocationItemNames.SafeArguments, out var arguments);

        var correlationId = correlationValue?.ToString();

        try
        {
            var result = await next().ConfigureAwait(false);

            await auditSink.WriteAsync(new AuditEntry(correlationId, context.ContractType.FullName ?? context.ContractType.Name, context.Method.Name, arguments, true, null, DateTimeOffset.UtcNow));

            return result;
        }
        catch (Exception exception)
        {
            await auditSink.WriteAsync(new AuditEntry(correlationId, context.ContractType.FullName ?? context.ContractType.Name, context.Method.Name, arguments, false, exception.Message, DateTimeOffset.UtcNow));

            throw;
        }
    }
}
```

---

## 9. StatusInterceptor

### Supporting types

```csharp
public enum InvocationStatus
{
    Started,
    Completed,
    Failed
}

public sealed record InvocationStatusUpdate(
    string? CorrelationId,
    string Contract,
    string Method,
    InvocationStatus Status,
    string? Error,
    DateTimeOffset TimestampUtc);

public interface IInvocationStatusSink
{
    ValueTask PublishAsync(InvocationStatusUpdate update);
}
```

### Interceptor

```csharp
public sealed class StatusInterceptor(IInvocationStatusSink statusSink) : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        context.Items.TryGetValue(InvocationItemNames.CorrelationId, out var correlationValue);

        var correlationId = correlationValue?.ToString();

        await statusSink.PublishAsync(CreateUpdate(context, correlationId, InvocationStatus.Started));

        try
        {
            var result = await next().ConfigureAwait(false);

            await statusSink.PublishAsync(CreateUpdate(context, correlationId, InvocationStatus.Completed));

            return result;
        }
        catch (Exception exception)
        {
            await statusSink.PublishAsync(CreateUpdate(context, correlationId, InvocationStatus.Failed, exception.Message));

            throw;
        }
    }

    private static InvocationStatusUpdate CreateUpdate(InvocationContext context, string? correlationId, InvocationStatus status, string? error = null)
        => new(correlationId, context.ContractType.FullName ?? context.ContractType.Name, context.Method.Name, status, error, DateTimeOffset.UtcNow);
}
```

---

## 10. ValidationInterceptor

```csharp
using System.ComponentModel.DataAnnotations;

public sealed class ValidationInterceptor : IInvocationInterceptor
{
    public ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument.Value is null)
                continue;

            Validator.ValidateObject(argument.Value, new ValidationContext(argument.Value), validateAllProperties: true);
        }

        return next();
    }
}
```

---

## 11. TimeoutInterceptor

### Attribute

```csharp
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class InvocationTimeoutAttribute(int milliseconds) : Attribute
{
    public TimeSpan Timeout { get; } = TimeSpan.FromMilliseconds(milliseconds);
}
```

### Interceptor

```csharp
public sealed class TimeoutInterceptor : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        var attribute = context.GetAttribute<InvocationTimeoutAttribute>();

        if (attribute is null)
            return await next().ConfigureAwait(false);

        try
        {
            return await next().AsTask().WaitAsync(attribute.Timeout).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw new TimeoutException($"{context.ContractType.Name}.{context.Method.Name} exceeded {attribute.Timeout}.");
        }
    }
}
```

Example:

```csharp
[InvocationTimeout(30000)]
Task ProcessAsync(CancellationToken cancellationToken);
```

---

## 12. RetryInterceptor

### Attribute

```csharp
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class RetryAttribute(int maxAttempts = 3, int delayMilliseconds = 200) : Attribute
{
    public int MaxAttempts { get; } = maxAttempts;

    public TimeSpan Delay { get; } = TimeSpan.FromMilliseconds(delayMilliseconds);
}
```

## Transient exception detector

```csharp
public interface ITransientExceptionDetector
{
    bool IsTransient(Exception exception);
}

public sealed class DefaultTransientExceptionDetector : ITransientExceptionDetector
{
    public bool IsTransient(Exception exception)
        => exception is TimeoutException
            or IOException;
}
```

### Interceptor

```csharp
public sealed class RetryInterceptor(ITransientExceptionDetector detector) : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        var attribute = context.GetAttribute<RetryAttribute>();

        if (attribute is null)
            return await next().ConfigureAwait(false);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (Exception exception) when (attempt < attribute.MaxAttempts && detector.IsTransient(exception))
            {
                await Task.Delay(attribute.Delay).ConfigureAwait(false);
            }
        }
    }
}
```

---

## 13. CircuitBreakerInterceptor

### Attribute

```csharp
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class CircuitBreakerAttribute(int failureThreshold = 3, int breakSeconds = 30) : Attribute
{
    public int FailureThreshold { get; } = failureThreshold;

    public TimeSpan BreakDuration { get; } = TimeSpan.FromSeconds(breakSeconds);
}
```

### Exception

```csharp
public sealed class CircuitBreakerOpenException(string message) : Exception(message);
```

### State

```csharp
internal sealed class CircuitBreakerState
{
    public object SyncRoot { get; } = new();

    public int FailureCount { get; set; }

    public DateTimeOffset? OpenUntilUtc { get; set; }
}
```

### Interceptor

```csharp
using System.Collections.Concurrent;

public sealed class CircuitBreakerInterceptor(ITransientExceptionDetector detector) : IInvocationInterceptor
{
    private readonly ConcurrentDictionary<string, CircuitBreakerState> states = new(StringComparer.Ordinal);

    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        var attribute = context.GetAttribute<CircuitBreakerAttribute>();

        if (attribute is null)
            return await next().ConfigureAwait(false);

        var key = $"{context.ContractType.FullName}.{context.Method.Name}";
        var state = states.GetOrAdd(key, _ => new CircuitBreakerState());

        lock (state.SyncRoot)
        {
            if (state.OpenUntilUtc > DateTimeOffset.UtcNow)
                throw new CircuitBreakerOpenException($"Circuit breaker is open for {key}.");

            if (state.OpenUntilUtc is not null)
            {
                state.OpenUntilUtc = null;
                state.FailureCount = 0;
            }
        }

        try
        {
            var result = await next().ConfigureAwait(false);

            lock (state.SyncRoot)
            {
                state.FailureCount = 0;
                state.OpenUntilUtc = null;
            }

            return result;
        }
        catch (Exception exception) when (detector.IsTransient(exception))
        {
            lock (state.SyncRoot)
            {
                state.FailureCount++;

                if (state.FailureCount >= attribute.FailureThreshold)
                    state.OpenUntilUtc = DateTimeOffset.UtcNow + attribute.BreakDuration;
            }

            throw;
        }
    }
}
```

---

## 14. AuthorizationInterceptor

### Attribute

```csharp
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class RequireAuthorizationAttribute(string policy) : Attribute
{
    public string Policy { get; } = policy;
}
```

## Supporting contracts

```csharp
using System.Security.Claims;

public interface IInvocationAuthorizationService
{
    ValueTask<bool> AuthorizeAsync(ClaimsPrincipal principal, string policy);
}

public interface ICurrentPrincipalAccessor
{
    ClaimsPrincipal Principal { get; }
}
```

### Interceptor

```csharp
public sealed class AuthorizationInterceptor(ICurrentPrincipalAccessor principalAccessor, IInvocationAuthorizationService authorizationService) : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        var attribute = context.GetAttribute<RequireAuthorizationAttribute>();

        if (attribute is null)
            return await next().ConfigureAwait(false);

        var authorized = await authorizationService.AuthorizeAsync(principalAccessor.Principal, attribute.Policy).ConfigureAwait(false);

        if (!authorized)
            throw new UnauthorizedAccessException($"Authorization policy '{attribute.Policy}' failed.");

        return await next().ConfigureAwait(false);
    }
}
```

---

## 15. IdempotencyInterceptor

### Attribute

```csharp
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class IdempotentAttribute : Attribute;
```

## Supporting contracts

```csharp
public interface IIdempotencyKeyResolver
{
    string GetKey(InvocationContext context);
}

public interface IIdempotencyStore
{
    ValueTask<bool> ExistsAsync(string key);

    ValueTask<object?> GetAsync(string key);

    ValueTask SaveAsync(string key, object? result);
}
```

### Interceptor

```csharp
public sealed class IdempotencyInterceptor(IIdempotencyKeyResolver keyResolver, IIdempotencyStore store) : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        if (!context.HasAttribute<IdempotentAttribute>())
            return await next().ConfigureAwait(false);

        var key = keyResolver.GetKey(context);

        if (await store.ExistsAsync(key).ConfigureAwait(false))
            return await store.GetAsync(key).ConfigureAwait(false);

        var result = await next().ConfigureAwait(false);

        await store.SaveAsync(key, result).ConfigureAwait(false);

        return result;
    }
}
```

---

## 16. ExceptionInterceptor

### Supporting contract

```csharp
public interface IInvocationExceptionHandler
{
    Exception Handle(InvocationContext context, Exception exception);
}
```

### Interceptor

```csharp
public sealed class ExceptionInterceptor(IInvocationExceptionHandler exceptionHandler) : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw exceptionHandler.Handle(context, exception);
        }
    }
}
```

---

## 17. DI Registration Order

Register interceptors in the order they should execute:

```csharp
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();
services.AddScoped<IInvocationInterceptor, ArgumentRedactionInterceptor>();
services.AddScoped<IInvocationInterceptor, TelemetryInterceptor>();
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();
services.AddScoped<IInvocationInterceptor, StatusInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
services.AddScoped<IInvocationInterceptor, TimeoutInterceptor>();
services.AddScoped<IInvocationInterceptor, RetryInterceptor>();
services.AddSingleton<IInvocationInterceptor, CircuitBreakerInterceptor>();
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
services.AddScoped<IInvocationInterceptor, IdempotencyInterceptor>();
services.AddScoped<IInvocationInterceptor, ExceptionInterceptor>();
```

Register interceptor dependencies:

```csharp
services.AddSingleton<ITransientExceptionDetector, DefaultTransientExceptionDetector>();

services.AddScoped<IAuditSink, AuditSink>();
services.AddScoped<IInvocationStatusSink, InvocationStatusSink>();
services.AddScoped<ICurrentPrincipalAccessor, CurrentPrincipalAccessor>();
services.AddScoped<IInvocationAuthorizationService, InvocationAuthorizationService>();
services.AddScoped<IIdempotencyKeyResolver, IdempotencyKeyResolver>();
services.AddScoped<IIdempotencyStore, IdempotencyStore>();
services.AddScoped<IInvocationExceptionHandler, InvocationExceptionHandler>();
```

Register the proxied service:

```csharp
services.AddBoundaryInterceptedScoped<IFileHandlingEngine, FileHandlingEngine>();
```

---

## 18. Example Contract

```csharp
public interface IFileHandlingEngine
{
    [InvocationTimeout(30000)]
    [Retry(maxAttempts: 3, delayMilliseconds: 500)]
    [CircuitBreaker(failureThreshold: 3, breakSeconds: 30)]
    Task CopyAsync(IServiceMessage message, string sourceFile, string destinationFile, CancellationToken cancellationToken);
}
```

---

## 19. Example Implementation

The implementation contains no interception-specific code.

```csharp
public sealed class FileHandlingEngine : IFileHandlingEngine
{
    public async Task CopyAsync(IServiceMessage message, string sourceFile, string destinationFile, CancellationToken cancellationToken)
    {
        await File.CopyAsync(sourceFile, destinationFile, cancellationToken);
    }
}
```

---

## 20. Example Caller

The caller contains no interception-specific code.

```csharp
public sealed class PortalManager(IFileHandlingEngine fileHandlingEngine)
{
    public Task CopyAsync(IServiceMessage message, string sourceFile, string destinationFile, CancellationToken cancellationToken)
        => fileHandlingEngine.CopyAsync(message, sourceFile, destinationFile, cancellationToken);
}
```

---

## 21. Important Constraints

`DispatchProxy` only observes calls that pass through the proxy.

It does not automatically intercept:

- Objects created directly with `new`.
- Static methods.
- Internal calls from one method to another on the same object.
- Calls made through a concrete implementation reference instead of the proxied interface.
- Services resolved outside the DI registration that creates the proxy.

Use interfaces and DI consistently at public architectural boundaries.

---

## 22. Recommended Use

Universal interceptors:

```text
Correlation
Argument Redaction
Telemetry
Exception Handling
```

Opt-in interceptors:

```text
Audit
Status
Validation
Timeout
Retry
Circuit Breaker
Authorization
Idempotency
```

Attributes or configuration can control opt-in behaviors without modifying callers.

---

## 23. Minimal Adoption

Before:

```csharp
services.AddScoped<IFileHandlingEngine, FileHandlingEngine>();
```

After:

```csharp
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();
services.AddScoped<IInvocationInterceptor, ArgumentRedactionInterceptor>();
services.AddScoped<IInvocationInterceptor, TelemetryInterceptor>();
services.AddScoped<IInvocationInterceptor, ExceptionInterceptor>();

services.AddBoundaryInterceptedScoped<IFileHandlingEngine, FileHandlingEngine>();
```

Existing callers and business implementations remain unchanged.
