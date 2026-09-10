---
title: Authorization Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Enforce access policies based on authenticated principal
tags:
  - authorization
  - security
  - policies
audience: developer
source_paths:
  - Authorization/AuthorizationInterceptor.cs
  - Authorization/IAuthorizationService.cs
  - Authorization/RequireAuthorizationAttribute.cs
---

# Authorization Interceptor

Enforces access policies after authentication establishes the principal.

## Purpose

Answer: **WHAT can you do?**

The `AuthorizationInterceptor` checks policies AFTER the authenticated principal is established. It ensures the authenticated user has the required permissions before the method executes.

**Important**: Authentication must run first. Without an authenticated principal, authorization always fails.

## Files

| File | Purpose |
|---|---|
| `AuthorizationInterceptor.cs` | Enforces authorization policies after authentication |
| `IAuthorizationService.cs` | Contract for policy evaluation |
| `RequireAuthorizationAttribute.cs` | Marks methods requiring specific policies |

## Integration

### 1. Register Both Interceptors in Correct Order

```csharp
// CORRECT ORDER:
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();    // 1st
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();         // 2nd
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();        // 3rd
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();     // Late

// WRONG - will always fail:
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();     // ✗ No principal yet
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();
```

### 2. Mark Methods for Authorization

```csharp
public interface IPaymentService
{
    [RequireAuthentication]                              // First: establish identity
    [RequireAuthorization("payment-processor")]          // Then: check policy
    Task<PaymentResult> ProcessAsync(PaymentRequest request);

    [RequireAuthentication]
    [RequireAuthorization("admin")]
    Task<IEnumerable<Payment>> GetAllPaymentsAsync();
}
```

### 3. Implement Authorization Service

Create a service that evaluates policies:

```csharp
public sealed class AuthorizationService : IAuthorizationService
{
    public bool IsAuthorized(ClaimsPrincipal principal, string policy)
    {
        return policy switch
        {
            "payment-processor" => principal.HasClaim("role", "processor") || 
                                   principal.HasClaim("role", "admin"),
            "admin" => principal.HasClaim("role", "admin"),
            _ => false
        };
    }
}
```

### 4. Register Service

```csharp
services.AddScoped<IAuthorizationService, AuthorizationService>();
```

## How It Works

1. **JwtAuthenticationInterceptor** runs first, validates JWT, sets `context.Items[InvocationItemNames.Principal]`
2. **RedactionInterceptor** (if registered) masks sensitive arguments
3. **ValidationInterceptor** (if registered) validates parameters
4. **AuthorizationInterceptor** runs:
   - Reads principal from context items
   - Reads policy from `[RequireAuthorization("policy-name")]` attribute
   - Calls `IAuthorizationService.IsAuthorized(principal, policy)`
   - Throws `UnauthorizedAccessException` if policy fails
5. Method executes (only if authorized)

## Execution Order

**Authorization must run AFTER Authentication**:

```csharp
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();    // 1st
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();        // 2nd
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();     // 3rd (or later)
```

## Related

- [Authentication Interceptor](../Authentication/README.md) — Establish principal (runs first)
- [Audit Interceptor](../Auditing/README.md) — Record authorized user actions
- [Security Support](../Security/README.md) — ICurrentPrincipalAccessor contract
- [Core Infrastructure](../Core/README.md) — InvocationItemNames, MethodContext

## See Also

- [README.md](../README.md#security-2-interceptors) — Security interceptor category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#authorization) — Complete table of contents
    Task DeleteUserAsync(string userId);
}
```

### Multiple Policies (User must have ANY of them)

```csharp
public interface IAdminService
{
    [RequireAuthentication]
    [RequireAuthorization("admin", "superadmin")]  // User needs "admin" OR "superadmin"
    Task ConfigureSystemAsync();
}
```

## Registration

```csharp
// Implement IAuthorizationService
services.AddScoped<IAuthorizationService, MyAuthorizationService>();

// Implement ICurrentPrincipalAccessor (how to access the authenticated principal)
services.AddScoped<ICurrentPrincipalAccessor, MyPrincipalAccessor>();

// Register interceptor (AFTER authentication)
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
```

## Implementing IAuthorizationService

The service evaluates whether the authenticated principal satisfies the policy:

```csharp
public class MyAuthorizationService : IAuthorizationService
{
    public async ValueTask<bool> AuthorizeAsync(ClaimsPrincipal principal, string policy)
    {
        // Example: Role-based authorization
        if (policy == "admin")
            return principal.IsInRole("Administrator");

        if (policy == "payment-processor")
            return principal.IsInRole("PaymentProcessor") || principal.IsInRole("Admin");

        // Example: Claim-based authorization
        if (policy == "merchant-access")
            return principal.HasClaim("merchant_id", c => !string.IsNullOrEmpty(c));

        // Policy not found
        throw new ArgumentException($"Unknown policy: {policy}");
    }
}
```

## Implementing ICurrentPrincipalAccessor

Provides access to the authenticated principal:

```csharp
public class MyPrincipalAccessor : ICurrentPrincipalAccessor
{
    public ClaimsPrincipal Principal
    {
        get => /* retrieve from context, thread-local storage, or HTTP context */;
        // set optional
    }
}

// Common implementation for HTTP contexts:
public class HttpContextPrincipalAccessor : ICurrentPrincipalAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextPrincipalAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal Principal
    {
        get => _httpContextAccessor.HttpContext?.User 
            ?? throw new InvalidOperationException("No HTTP context available");
    }
}
```

## Common Authorization Patterns

### Pattern 1: Role-Based Access Control (RBAC)

```csharp
[RequireAuthentication]
[RequireAuthorization("admin")]
Task DeleteUserAsync(string userId);

// In AuthorizationService:
if (policy == "admin")
    return principal.IsInRole("Administrator");
```

### Pattern 2: Claim-Based Access

```csharp
[RequireAuthentication]
[RequireAuthorization("merchant-access")]
Task<MerchantInfo> GetMerchantAsync();

// In AuthorizationService:
if (policy == "merchant-access")
    return principal.HasClaim("merchant_id", c => !string.IsNullOrEmpty(c));
```

### Pattern 3: Resource-Based Authorization

```csharp
[RequireAuthentication]
[RequireAuthorization("owner")]  // User must be owner of the resource
Task UpdateProfileAsync(UserProfile profile);

// In AuthorizationService:
if (policy == "owner")
{
    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var resourceOwnerId = /* extract from context */;
    return userId == resourceOwnerId;
}
```

### Pattern 4: Multiple Policies (OR Logic)

```csharp
[RequireAuthentication]
[RequireAuthorization("admin", "superadmin", "auditor")]
Task GenerateReportAsync(ReportRequest request);

// In AuthorizationService:
if (policy == "admin") return principal.IsInRole("Administrator");
if (policy == "superadmin") return principal.IsInRole("SuperAdministrator");
if (policy == "auditor") return principal.HasClaim("role", "Auditor");

return false;
```

## Error Handling

When authorization fails:

```csharp
throw new UnauthorizedAccessException(
    $"Authorization policy '{policy}' failed for user '{principal.FindFirst(ClaimTypes.NameIdentifier)?.Value}'.");
```

This exception propagates to the caller, indicating insufficient permissions.

## Flow Diagram

```
[Authentication Complete] → Principal established in context
                             ↓
                    [RequireAuthorization] attribute detected
                             ↓
                    Retrieve policy name (e.g., "admin")
                             ↓
                    Call IAuthorizationService.AuthorizeAsync(principal, policy)
                             ↓
                    Policy check:
                      ✓ User has policy → Continue to method
                      ✗ User lacks policy → Throw UnauthorizedAccessException
                             ↓
                    Target method executes (authorized)
```

## Performance Considerations

- **Policy evaluation** — Usually microseconds (cached role/claim lookups)
- **Principal access** — Depends on storage (in-memory: microseconds, database: milliseconds)

**Optimization tips:**
- Cache authorization policy results if they don't change frequently
- Pre-load principal claims instead of lazy-loading them
- Consider separating heavy policies into background workers

## Registration Order (Critical)

```csharp
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();       // 1st - context
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();    // 2nd - identity
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();         // 3rd - data protection
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();        // 4th - input validation
// ... other interceptors (logging, retry, timeout, etc.) ...
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();     // Last - permissions check
```

**Why late?** Don't check permissions until input is valid and identity is established.

## Further Reading

- [Authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization)
- [Claims-Based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims)
- [Role-Based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)
- [Policy-Based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- [Authentication](../Authentication/README.md) — Prerequisite
- [USAGE.md](../USAGE.md) — Integration patterns

