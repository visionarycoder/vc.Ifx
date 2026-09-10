---
title: Redaction Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Mask sensitive method arguments before logging and auditing
tags:
  - redaction
  - security
  - data-protection
audience: developer
source_paths:
  - Redaction/RedactionInterceptor.cs
---

# Redaction Interceptor

Masks sensitive method arguments to prevent them from appearing in logs, audit trails, and monitoring systems.

## Purpose

Protect sensitive data (passwords, tokens, secrets) by automatically redacting arguments before logging and audit recording.

## How It Works

```
Method invoked with arguments
    (e.g., userId="user123", password="secret123")
    ↓
RedactionInterceptor inspects arguments
    ↓
Detects sensitive argument names (password, token, secret, etc.)
    ↓
Replaces sensitive values with [REDACTED]
    ↓
Passes redacted values to downstream interceptors
    (Audit, Logging, Telemetry see redacted values)
```

## Files

| File | Purpose |
|---|---|
| `RedactionInterceptor.cs` | Masks sensitive arguments |

## Integration

### 1. Register the Interceptor

```csharp
// Should run EARLY in the chain (after authentication if needed)
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();
```

### 2. Automatically Redacts These Parameters

Arguments whose names contain these keywords (case-insensitive) are automatically redacted:

| Keyword | Examples |
|---|---|
| `password` | Password, UserPassword, ClearPassword |
| `secret` | Secret, ApiSecret, ClientSecret |
| `token` | Token, AuthenticationToken, RefreshToken |
| `apikey` | ApiKey, APIKey, api_key |
| `authorization` | Authorization, AuthorizationHeader |
| `credential` | Credential, Credentials |

### 3. Usage Example

```csharp
public class ChangePasswordRequest
{
    public string UserId { get; set; }
    public string CurrentPassword { get; set; }  // ← Will be redacted
    public string NewPassword { get; set; }      // ← Will be redacted
}

public interface IUserService
{
    Task ChangePasswordAsync(ChangePasswordRequest request);
}

// When auditing or logging, redacted values appear as:
// { "UserId": "user123", "CurrentPassword": "[REDACTED]", "NewPassword": "[REDACTED]" }
```

## Execution Order

Register `RedactionInterceptor` **early in the chain** (after authentication):

```csharp
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();  // 1st
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();       // 2nd - prepare safe args
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();      // 3rd
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();           // Sees redacted values
```

## Sensitive Data Flow

1. **RedactionInterceptor runs first** — Redacts sensitive arguments
2. **AuditInterceptor sees redacted values** — Never logs passwords/tokens
3. **Logging/Telemetry see redacted values** — Safe for external systems
4. **Actual method gets original values** — Business logic works normally

## Related

- [Audit Interceptor](../Auditing/README.md) — Records audit trails (receives redacted args)
- [Authentication Interceptor](../Authentication/README.md) — Establish identity
- [Security Support](../Security/README.md) — ICurrentPrincipalAccessor contract
- [Core Infrastructure](../Core/README.md) — MethodContext, InvocationItemNames

## See Also

- [README.md](../README.md#security-2-interceptors) — Security category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#redaction) — Complete table of contents
public interface IUserService
{
    [RequireAuthentication]
    Task CreateUserAsync(string userId, string password);  // Password auto-redacted

    [RequireAuthentication]
    Task ResetPasswordAsync(string userId, string newPassword);  // Auto-redacted
}
```

Logs show:

```
[INFO] Method invoked: IUserService.CreateUserAsync
  Arguments: { userId: "user123", password: "***REDACTED***" }
```

## Registration

**Must run EARLY, before logging and audit interceptors:**

```csharp
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();    // 1st - identity
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();         // 2nd - hide secrets
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();        // 3rd
// ... other interceptors ...
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();             // Late
services.AddScoped<IInvocationInterceptor, LoggingInterceptor>();           // Late
```

**Why so early?** Secrets must be hidden before downstream interceptors (audit, logging, telemetry) see the arguments.

## How It Works

The interceptor:

1. Iterates through all method arguments
2. Checks each argument name against sensitive keywords
3. Creates a dictionary of "safe arguments" with redacted values
4. Stores safe arguments in `context.Items[InvocationItemNames.SafeArguments]`

Downstream interceptors retrieve safe arguments:

```csharp
context.Items.TryGetValue(InvocationItemNames.SafeArguments, out var safeArgsObj);
var safeArguments = (Dictionary<string, object?>)safeArgsObj;
// Use safeArguments instead of context.Arguments for logging
```

## Flow Diagram

```
Method called with arguments: { userId, password, secret }
                              ↓
           RedactionInterceptor processes arguments
                              ↓
         Scan for sensitive names: password ✓, secret ✓
                              ↓
        Create safe dict: { userId: "user123", password: "***REDACTED***", secret: "***REDACTED***" }
                              ↓
        Store in context.Items[SafeArguments]
                              ↓
          [Next interceptor reads safe arguments]
                              ↓
    Audit/Logging show redacted values in records
```

## Performance Considerations

- **Reflection overhead** — Scans argument names (negligible, ~microseconds per argument)
- **Dictionary creation** — Creates new dictionary per invocation (minimal allocation)

**Optimization tips:**
- Runs only once per method invocation (not per parameter)
- Consider extending to check argument types if reflection overhead becomes measurable

## Sensitive Keywords

Current redaction keywords:

```csharp
private static readonly string[] SensitiveNames =
[
    "password",
    "secret",
    "token",
    "apikey",
    "authorization",
    "credential"
];
```

To add more keywords, extend the `SensitiveNames` array in `RedactionInterceptor.cs`.

## ICurrentPrincipalAccessor

Provides access to the authenticated principal established by `JwtAuthenticationInterceptor`:

```csharp
public interface ICurrentPrincipalAccessor
{
    ClaimsPrincipal Principal { get; }
}
```

Used by `AuthorizationInterceptor` to evaluate policies against the authenticated user.

## Common Patterns

### Pattern 1: Automatic Redaction

```csharp
public interface IAuthService
{
    Task AuthenticateAsync(string username, string password);  // Auto-redacted
    Task ResetPasswordAsync(string email, string newPassword);  // Auto-redacted
}
```

No attributes needed — just use standard naming.

### Pattern 2: API Keys and Tokens

```csharp
public interface IExternalService
{
    Task CallExternalAsync(string apikey, string data);  // apikey auto-redacted
    Task AuthenticatedCallAsync(string authorizationToken, string data);  // Auto-redacted
}
```

### Pattern 3: Credentials

```csharp
public interface IDatabaseService
{
    Task ConnectAsync(string connectionString, string credential);  // credential auto-redacted
}
```

## Error Handling

The interceptor does not throw exceptions. If an argument name matches a sensitive keyword, its value is redacted. Non-matching arguments are preserved.

## Integration with Other Interceptors

**Audit Interceptor** uses safe arguments:

```csharp
context.Items.TryGetValue(InvocationItemNames.SafeArguments, out var safe);
var auditEntry = new AuditEntry(correlationId, method, safe, /* ... */);  // Uses safe arguments
```

**Logging Interceptor** uses safe arguments:

```csharp
logger.LogInfo("Method: {Method}, Args: {@SafeArgs}", methodName, 
    context.Items[InvocationItemNames.SafeArguments]);
```

## Registration Order (Critical)

```csharp
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();       // 1st
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();    // 2nd
services.AddScoped<IInvocationInterceptor, RedactionInterceptor>();         // 3rd (EARLY!)
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();        // 4th
// ... other interceptors ...
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();     // Late
```

## Further Reading

- [PII Protection Best Practices](https://owasp.org/www-community/attacks/Sensitive_Data_Exposure)
- [GDPR Data Protection](https://gdpr-info.eu/)
- [Auditing](../Auditing/README.md) — Uses redacted data for compliance
- [Authentication](../Authentication/README.md) — Complementary security layer
- [USAGE.md](../USAGE.md) — Integration patterns


