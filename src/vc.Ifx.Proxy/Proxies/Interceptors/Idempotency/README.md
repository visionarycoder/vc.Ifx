---
title: Idempotency Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Cache and reuse results for repeated operations
tags:
  - idempotency
  - caching
  - deduplication
audience: developer
source_paths:
  - Idempotency/IdempotencyInterceptor.cs
  - Idempotency/IIdempotencyKeyResolver.cs
  - Idempotency/IIdempotencyStore.cs
  - Idempotency/IdempotentAttribute.cs
---

# Idempotency Interceptor

Caches and reuses results for operations with identical inputs.

## Purpose

Improve performance and reduce duplicate work by caching results for repeated method calls with the same arguments.

## How It Works

```
First call with arguments
    ↓
IdempotencyInterceptor caches result
    ↓
Duplicate call (same arguments)
    ↓
Returns cached result immediately (no method execution)
    ↓
Different arguments
    ↓
Executes method and caches new result
```

## Files

| File | Purpose |
|---|---|
| `IdempotencyInterceptor.cs` | Caches results for repeated calls |
| `IIdempotencyKeyResolver.cs` | Generates cache keys from arguments |
| `IIdempotencyStore.cs` | Contract for result storage |
| `IdempotentAttribute.cs` | Marks methods as cacheable |

## Integration

### 1. Register the Interceptor

```csharp
services.AddScoped<IInvocationInterceptor, IdempotencyInterceptor>();
```

### 2. Mark Methods as Idempotent

```csharp
public interface IUserService
{
    [Idempotent(DurationSeconds = 300)]  // Cache for 5 minutes
    Task<UserProfile> GetProfileAsync(string userId);

    [Idempotent(DurationSeconds = 3600)]  // Cache for 1 hour
    Task<IEnumerable<Role>> GetRolesAsync(string userId);
}
```

### 3. Call the Method

```csharp
// First call
var profile1 = await userService.GetProfileAsync("user123");
// Executes method, returns result, caches it

// Duplicate call (same argument)
var profile2 = await userService.GetProfileAsync("user123");
// Returns cached result immediately (same object reference)

// Different argument
var profile3 = await userService.GetProfileAsync("user456");
// Executes method (different arguments), caches separately
```

## Configuration

| Option | Purpose |
|---|---|
| `DurationSeconds` | How long to cache the result |

## Idempotency Key Generation

The interceptor generates a cache key from:
- Method name
- Argument values
- Argument types

Calls with identical arguments get the same cached result.

## Use Cases

**Good for:**
- Read-only operations (GetUser, GetSettings, GetRoles)
- Lookup functions that rarely change
- External API calls with expensive rate limits

**Not for:**
- Write operations (Create, Update, Delete)
- Time-sensitive data
- Operations with side effects

## Execution Order

Register `IdempotencyInterceptor` **early to mid-pipeline** (before business logic):

```csharp
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
services.AddScoped<IInvocationInterceptor, IdempotencyInterceptor>();  // Before business logic
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();
```

## Related

- [Core Infrastructure](../Core/README.md) — MethodContext, cache key generation
- [Status Interceptor](../Status/README.md) — Track cached call status

## See Also

- [README.md](../README.md#data-integrity-3-interceptors) — Data integrity category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#idempotency) — Complete table of contents
// Executes method (different cache key)
```

## When to Use

### ✅ Good Use Cases

- **Read-only queries** — `GetUser()`, `GetProductInfo()`, etc.
- **Expensive computations** — Reports, aggregations
- **Rarely-changing data** — Configuration, reference data
- **Batch operations** — Avoid duplicate work

### ❌ Bad Use Cases

- **State-changing operations** — `CreateOrder()`, `UpdateProfile()`
- **Time-sensitive operations** — Current stock price, system status
- **Operations with side effects** — Email sending, payment processing

## Configuration

### Basic Usage

```csharp
public interface IProductService
{
    [Idempotent]
    Task<Product> GetAsync(int productId);
}
```

Cache duration:
```csharp
[Idempotent(durationSeconds: 3600)]  // Cache for 1 hour
```

### Selective Idempotency

Only cache for specific arguments:

```csharp
public interface IOrderService
{
    // Idempotent: same order ID always returns same status
    [Idempotent(cacheKeyPrefix: "order-status")]
    Task<OrderStatus> GetStatusAsync(string orderId);
    
    // NOT idempotent: creates new every time
    Task<Order> CreateAsync(OrderRequest request);
}
```

## Cache Key Generation

The interceptor generates cache keys from:
1. Method name
2. All method arguments (serialized)

Example:
```
Method: GetProfileAsync(userId: "user123")
Cache Key: "GetProfileAsync::user123"

Method: GetAsync(productId: 42)
Cache Key: "GetAsync::42"

Method: SearchAsync(term: "laptop", limit: 10)
Cache Key: "SearchAsync::laptop::10"
```

## Implementation Details

Caching is **scoped to a single proxy instance**:

```csharp
var userService = BoundaryInterceptor<IUserService>.Create(impl, interceptors);

// Cache is per-instance
var profile1 = await userService.GetProfileAsync("user123");  // Cache miss, execute
var profile2 = await userService.GetProfileAsync("user123");  // Cache hit, return cached
```

If you have multiple proxy instances, they have independent caches:

```csharp
var service1 = BoundaryInterceptor<IUserService>.Create(impl1, interceptors);
var service2 = BoundaryInterceptor<IUserService>.Create(impl2, interceptors);

var p1 = await service1.GetProfileAsync("user123");  // Caches in service1
var p2 = await service2.GetProfileAsync("user123");  // Separate cache in service2
```

## Invalidation

There's no automatic invalidation. Manual clear (if supported):

```csharp
// Pseudo-code: specific interceptor implementation may vary
if (interceptor is IdempotencyInterceptor idempotent)
{
    idempotent.ClearCache();  // Clear all cache entries
}
```

Or use attributes to exclude methods:

```csharp
public interface IUserService
{
    [Idempotent]
    Task<UserProfile> GetProfileAsync(string userId);  // Cached

    Task UpdateProfileAsync(string userId, UserProfile updated);  // Not cached (no attribute)
}
```

## Registration

```csharp
// Register idempotency interceptor
services.AddScoped<IInvocationInterceptor, IdempotencyInterceptor>();
```

Register **late** in the pipeline (after validation, auth, resilience):

```csharp
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
services.AddScoped<IInvocationInterceptor, RetryInterceptor>();
services.AddScoped<IInvocationInterceptor, IdempotencyInterceptor>();  // Late
```

**Why?** Cache the final result after all processing, not intermediate states.

## Performance

- **Cache hit** — Sub-microsecond dictionary lookup
- **Cache miss** — Executes normally, then caches result
- **Memory usage** — Depends on cache size and TTL

For high-volume scenarios:
- Use appropriate TTL (not forever)
- Monitor cache size
- Consider distributed caching for multi-instance deployments

## Thread Safety

The cache is **thread-safe** within a single invocation context but **NOT thread-safe** across concurrent calls to the same method with different arguments on the same instance.

For thread-safe distributed caching, integrate with:
- Redis
- Memcached
- Azure Cache for Redis

## Further Reading

- [Cache-Aside Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside)
- [Idempotency and Deduplication](https://stripe.com/blog/idempotency)
- [Resilience](../CircuitBreaker/README.md) — Works well with retry patterns
- [USAGE.md](../USAGE.md) — Overall integration patterns

