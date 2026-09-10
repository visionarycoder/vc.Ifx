---
title: Security Support Infrastructure
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Helper contracts for security-related interceptor functionality
tags:
  - security
  - authentication
  - authorization
audience: developer
source_paths:
  - Security/ICurrentPrincipalAccessor.cs
---

# Security Support Infrastructure

Shared contracts supporting security-related interceptors (Authentication, Authorization, Redaction).

## Purpose

Provide common types and abstractions for security operations across the interceptor framework.

## Files

| File | Purpose |
|---|---|
| `ICurrentPrincipalAccessor.cs` | Contract for accessing the authenticated principal |

## ICurrentPrincipalAccessor

Provides access to the `ClaimsPrincipal` established by `JwtAuthenticationInterceptor`.

```csharp
public interface ICurrentPrincipalAccessor
{
    ClaimsPrincipal Principal { get; }
}
```

Used by:
- `JwtAuthenticationInterceptor` — Establishes principal from JWT token
- `AuthorizationInterceptor` — Evaluates policies against principal claims
- `RedactionInterceptor` — Identity-based redaction rules
- Custom application code — Access claims and roles

## Implementation Example

```csharp
public sealed class CurrentPrincipalAccessor : ICurrentPrincipalAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentPrincipalAccessor(IHttpContextAccessor httpContextAccessor)
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

## Accessing Principal in Custom Code

```csharp
public sealed class MyBusinessService
{
    private readonly ICurrentPrincipalAccessor _principalAccessor;

    public MyBusinessService(ICurrentPrincipalAccessor principalAccessor)
    {
        _principalAccessor = principalAccessor;
    }

    public async Task<IEnumerable<Order>> GetMyOrdersAsync()
    {
        var principal = _principalAccessor.Principal;
        var userId = principal.FindFirst("sub")?.Value;  // Subject claim
        
        return await _db.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }
}
```

## Registration

```csharp
services.AddHttpContextAccessor();  // Prerequisite for web context
services.AddScoped<ICurrentPrincipalAccessor, CurrentPrincipalAccessor>();

// Security interceptors
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();
```

## Common Claims

Standard claims in JWT tokens and ClaimsPrincipal:

| Claim | Meaning |
|---|---|
| `sub` | Subject (User ID) |
| `name` | User display name |
| `email` | User email address |
| `roles` | Comma-separated roles |
| `given_name` | First name |
| `family_name` | Last name |
| Custom claims | Application-specific claims |

## Related

- [Authentication Interceptor](../Authentication/README.md) — Establishes principal from JWT
- [Authorization Interceptor](../Authorization/README.md) — Evaluates policies using principal
- [Redaction Interceptor](../Redaction/README.md) — May redact based on identity
- [Core Infrastructure](../Core/README.md) — MethodContext stores principal

## See Also

- [README.md](../README.md#security-2-interceptors) — Security category overview
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#security-support) — Complete table of contents

