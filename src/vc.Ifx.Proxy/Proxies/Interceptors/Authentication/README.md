---
title: Authentication Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Establish authenticated identity via JWT tokens
tags:
  - authentication
  - security
  - jwt
audience: developer
source_paths:
  - Authentication/JwtJwtAuthenticationInterceptor.cs
  - Authentication/IJwtTokenValidator.cs
  - Authentication/RequireAuthenticationAttribute.cs
---

# Authentication Interceptor

Validates JWT tokens and establishes authenticated ClaimsPrincipal for the request.

## Purpose

Answer: **WHO are you?**

The `JwtJwtAuthenticationInterceptor` validates JWT tokens before any method executes. It establishes a ClaimsPrincipal in `context.Items[InvocationItemNames.Principal]` that all downstream interceptors can access.

**Must run first in the interceptor chain** because all downstream security decisions (authorization, audit, redaction) depend on knowing the authenticated identity.

## Files

| File | Purpose |
|---|---|
| `JwtJwtAuthenticationInterceptor.cs` | Validates JWT tokens and establishes ClaimsPrincipal |
| `IJwtTokenValidator.cs` | Contract for JWT token validation |
| `RequireAuthenticationAttribute.cs` | Marks methods requiring authentication |

## Integration

### 1. Register the Interceptor

```csharp
// FIRST in the chain
services.AddScoped<IInvocationInterceptor, JwtJwtAuthenticationInterceptor>();
```

### 2. Mark Methods for Authentication

```csharp
public interface IUserService
{
    [RequireAuthentication]
    Task<UserInfo> GetProfileAsync();

    [RequireAuthentication("Bearer")]  // Explicit scheme
    Task UpdateProfileAsync(UserInfo profile);
}
```

### 3. Provide JWT Token

The interceptor looks for JWT tokens in this order:

1. **Context items** — `context.Items[InvocationItemNames.AuthenticationToken]`
2. **Method arguments** — Searches for `AuthenticationToken` or `Token` property on argument objects

Typical pattern with service messages:

```csharp
public class GetProfileRequest
{
    public string UserId { get; set; }
    public string AuthenticationToken { get; set; }  // ← Token extracted here
}

// Call it
var request = new GetProfileRequest 
{ 
    UserId = "user123", 
    AuthenticationToken = "eyJhbGc..."  // JWT token
};

var profile = await userService.GetProfileAsync(request);
```

### 4. Use Principal in Other Interceptors

Once authentication runs, downstream interceptors can access the principal:

```csharp
public sealed class AuthorizationInterceptor : IInvocationInterceptor
{
    public async ValueTask<object?> InvokeAsync(MethodContext context, InvocationDelegate next)
    {
        // JwtAuthenticationInterceptor already set this
        if (context.Items.TryGetValue(InvocationItemNames.Principal, out var principal))
        {
            var claimsPrincipal = (ClaimsPrincipal)principal;
            // Use for policy checks
        }
        
        return await next().ConfigureAwait(false);
    }
}
```

## Execution Order

Register **JwtAuthenticationInterceptor** **FIRST**:

```csharp
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();  // 1st
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();      // 2nd
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();   // 3rd
// ... other interceptors ...
```

## Related

- [Authorization Interceptor](../Authorization/README.md) — Enforce access policies (uses principal from Authentication)
- [Redaction Interceptor](../Redaction/README.md) — Mask sensitive data based on authenticated identity
- [Audit Interceptor](../Auditing/README.md) — Record authenticated user actions
- [Security Support](../Security/README.md) — ICurrentPrincipalAccessor contract
- [Core Infrastructure](../Core/README.md) — InvocationItemNames, MethodContext

## See Also

- [README.md](../README.md#security-2-interceptors) — Security interceptor category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#authentication) — Complete table of contents

```csharp
public interface IPaymentService
{
    [RequireAuthentication]
    Task<PaymentResult> ProcessAsync(PaymentRequest request);
}

public class PaymentRequest : IServiceMessage
{
    public string AuthenticationToken { get; set; }  // Interceptor extracts this
    public PaymentDetails Details { get; set; }
}
```

## Registration

**Must be FIRST in the interceptor chain:**

```csharp
// Implement IJwtTokenValidator with your token validation logic
services.AddScoped<IJwtTokenValidator>(sp => 
    new MyJwtValidator(
        signingKey: configuration["Auth:SigningKey"],
        issuer: configuration["Auth:Issuer"],
        audience: configuration["Auth:Audience"]));

// Register interceptor first
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();

// Then other interceptors
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();  // AFTER auth
```

## Implementing IJwtTokenValidator

Example implementation:

```csharp
public class MyJwtValidator : IJwtTokenValidator
{
    private readonly string _signingKey;
    private readonly string _issuer;
    private readonly string _audience;

    public MyJwtValidator(string signingKey, string issuer, string audience)
    {
        _signingKey = signingKey;
        _issuer = issuer;
        _audience = audience;
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_signingKey);

        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (SecurityTokenException ex)
        {
            throw new SecurityException("Token validation failed.", ex);
        }
    }
}
```

## Context Storage

After successful authentication, the interceptor stores:

```csharp
context.Items[InvocationItemNames.Principal] = claimsPrincipal;
```

Downstream interceptors can retrieve it:

```csharp
context.Items.TryGetValue(InvocationItemNames.Principal, out var principalObj);
var principal = (ClaimsPrincipal)principalObj;
var userName = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

## Error Handling

Possible exceptions:

| Condition | Exception | Message |
|-----------|-----------|---------|
| No `RequireAuthentication` attribute | Proceeds to next interceptor | N/A |
| Token not found | `UnauthorizedAccessException` | "No authentication token provided." |
| Token invalid/expired | `UnauthorizedAccessException` | "Token validation failed." |
| Validator throws non-auth exception | `UnauthorizedAccessException` | "Token validation failed." (wraps inner exception) |

## Performance Considerations

- **Token parsing** — Usually 1-5ms depending on token size and claims
- **Signature validation** — Microseconds to a few ms (validate signing key once, cache it)
- **Claims extraction** — Negligible

**Optimization tips:**
- Pre-load and cache JWT signing keys
- Consider caching validated tokens (if token rotation is acceptable)
- Use symmetric keys (HS256) instead of asymmetric (RS256) for speed if security permits

## Common Patterns

### Pattern: Service Message with Token

```csharp
public class ServiceRequest : IServiceMessage
{
    public string AuthenticationToken { get; set; }
    public string CorrelationId { get; set; }
    // ... business data
}

public interface IService
{
    [RequireAuthentication]
    Task<Result> ExecuteAsync(ServiceRequest request);
}
```

### Pattern: Token Passed via Context Items

```csharp
// In calling code, before invoking proxy method:
context.Items[InvocationItemNames.AuthenticationToken] = jwtToken;

// JwtAuthenticationInterceptor finds it and validates
```

## Flow Diagram

```
Client Request (with JWT token)
        ↓
[RequireAuthentication] attribute detected
        ↓
Extract token from arguments/context
        ↓
IJwtTokenValidator.ValidateToken(token)
        ↓
Token valid? 
  ✓ YES → Extract ClaimsPrincipal, store in context.Items[Principal]
  ✗ NO  → throw UnauthorizedAccessException
        ↓
[Continue to next interceptor]
        ↓
AuthorizationInterceptor (uses Principal from context)
        ↓
Target method executes (authenticated)
```

## Further Reading

- [JWT Best Practices](https://tools.ietf.org/html/rfc7519)
- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication)
- [IdentityModel (JWT handling)](https://github.com/IdentityModel/IdentityModel)
- [Authorization](../Authorization/README.md) — Next step after authentication
- [USAGE.md](../USAGE.md) — Integration patterns

