# VisionaryCoder.Framework.Security

Comprehensive security features for VisionaryCoder Framework including authentication, authorization, encryption, and audit logging.

## Features

### Authentication
- JWT token generation and validation
- OAuth 2.0 / OpenID Connect support
- Multi-factor authentication (MFA)
- Refresh token handling
- API key authentication

### Authorization
- Role-Based Access Control (RBAC)
- Attribute-Based Access Control (ABAC)
- Policy-based authorization
- Custom authorization policies
- Resource-based authorization

### Password Security
- BCrypt password hashing
- Configurable work factor
- Salt generation
- Password validation
- Secure password reset flows

### Data Protection
- ASP.NET Core Data Protection API
- Key management and rotation
- Encryption at rest
- Azure Key Vault integration
- Redis-backed key storage

### Security Interceptors
- Authentication interceptors for proxy calls
- Authorization enforcement
- Audit logging for security events
- Rate limiting per user/role
- Security context propagation

## Installation

```bash
dotnet add package VisionaryCoder.Framework.Security
```

## Quick Start

### Password Hashing

```csharp
services.AddPasswordHashing(options =>
{
    options.WorkFactor = 12; // BCrypt work factor (4-31)
});

public class UserService
{
    private readonly IPasswordHasher passwordHasher;
    
    public UserService(IPasswordHasher passwordHasher)
    {
        this.passwordHasher = passwordHasher;
    }
    
    public async Task<User> CreateUserAsync(string email, string password)
    {
        var hashedPassword = passwordHasher.HashPassword(password);
        
        var user = new User 
        { 
            Email = email,
            PasswordHash = hashedPassword 
        };
        
        await repository.AddAsync(user);
        return user;
    }
    
    public bool ValidatePassword(User user, string password)
    {
        return passwordHasher.VerifyPassword(password, user.PasswordHash);
    }
}
```

### JWT Authentication

```csharp
services.AddJwtAuthentication(options =>
{
    options.SecretKey = configuration["Jwt:SecretKey"];
    options.Issuer = "https://myapp.com";
    options.Audience = "https://myapp.com";
    options.TokenLifetime = TimeSpan.FromHours(1);
    options.RefreshTokenLifetime = TimeSpan.FromDays(7);
});

public class AuthService
{
    private readonly ITokenProvider tokenProvider;
    
    public async Task<TokenResult> AuthenticateAsync(string email, string password)
    {
        // Validate credentials
        var user = await ValidateUserAsync(email, password);
        
        // Generate tokens
        var tokenRequest = new TokenRequest
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = user.Roles,
            Claims = new Dictionary<string, string>
            {
                ["tenant_id"] = user.TenantId.ToString()
            }
        };
        
        return await tokenProvider.GenerateTokenAsync(tokenRequest);
    }
}
```

### Role-Based Authorization

```csharp
// Configure authorization policies
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("ManagerOrAdmin", policy => 
        policy.RequireRole("Manager", "Admin"));
    
    options.AddPolicy("ActiveUser", policy =>
        policy.RequireClaim("status", "Active"));
});

// Use in controllers
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        // Only admins can access this
    }
}
```

### Custom Authorization Policy

```csharp
public class ResourceOwnerPolicy : IAuthorizationPolicy
{
    public async Task<AuthorizationResult> AuthorizeAsync(ProxyContext context)
    {
        var userId = context.UserContext.UserId;
        var resourceOwnerId = context.Request.GetHeader("Resource-Owner-Id");
        
        if (userId == resourceOwnerId || context.UserContext.HasRole("Admin"))
        {
            return AuthorizationResult.Success();
        }
        
        return AuthorizationResult.Failure("User is not the resource owner");
    }
}

// Register policy
services.AddProxyAuthorization(options =>
{
    options.AddPolicy<ResourceOwnerPolicy>("ResourceOwner");
});
```

### Data Protection

```csharp
services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(connectionString, "keys", "data-protection-keys")
    .ProtectKeysWithAzureKeyVault(keyIdentifier, credentials);

public class SecureDataService
{
    private readonly IDataProtector protector;
    
    public SecureDataService(IDataProtectionProvider provider)
    {
        protector = provider.CreateProtector("MyApp.SecureData");
    }
    
    public string EncryptSensitiveData(string plainText)
    {
        return protector.Protect(plainText);
    }
    
    public string DecryptSensitiveData(string cipherText)
    {
        return protector.Unprotect(cipherText);
    }
}
```

### Security Interceptors

```csharp
// Configure security interceptors
services.AddProxySecurity(options =>
{
    options.RequireAuthentication = true;
    options.EnforceAuthorization = true;
    options.AuditSecurityEvents = true;
    options.PropagateSecurityContext = true;
});

// JWT Bearer interceptor
services.AddProxyInterceptor<JwtBearerInterceptor>(order: 1);

// Authorization interceptor
services.AddProxyInterceptor<SecurityInterceptor>(order: 2);

// Audit interceptor
services.AddProxyInterceptor<AuditingInterceptor>(order: 100);
```

## Configuration

### appsettings.json

```json
{
  "Security": {
    "PasswordHashing": {
      "WorkFactor": 12
    },
    "Jwt": {
      "SecretKey": "your-secret-key-here-minimum-32-characters",
      "Issuer": "https://myapp.com",
      "Audience": "https://myapp.com",
      "TokenLifetimeMinutes": 60,
      "RefreshTokenLifetimeDays": 7,
      "ValidateIssuerSigningKey": true,
      "ValidateIssuer": true,
      "ValidateAudience": true,
      "ValidateLifetime": true
    },
    "Authentication": {
      "DefaultScheme": "Bearer",
      "AllowAnonymous": false
    },
    "Authorization": {
      "DefaultPolicy": "Authenticated"
    },
    "Auditing": {
      "Enabled": true,
      "AuditSuccessfulOperations": false,
      "AuditFailedOperations": true,
      "IncludeSensitiveData": false
    }
  }
}
```

## Security Best Practices

### Authentication
- **Never store passwords in plain text** - always use BCrypt or similar
- **Use HTTPS only** - never transmit credentials over HTTP
- **Implement token refresh** - keep access tokens short-lived
- **Validate token signatures** - verify JWT signatures on every request
- **Use secure random generators** - for tokens, salts, and keys

### Authorization
- **Principle of least privilege** - grant minimum necessary permissions
- **Default deny** - require explicit authorization for all operations
- **Separate authentication from authorization** - verify identity first, then permissions
- **Use policy-based authorization** - centralize authorization logic
- **Audit authorization failures** - log all denied access attempts

### Password Security
- **Enforce strong passwords** - minimum length, complexity requirements
- **Use high work factors** - BCrypt work factor 12+ (adjust for your security needs)
- **Implement rate limiting** - prevent brute force attacks
- **Never log passwords** - even hashed passwords shouldn't be logged
- **Secure password reset** - use time-limited tokens, verify email

### Data Protection
- **Encrypt sensitive data at rest** - use Data Protection API
- **Rotate keys regularly** - implement key rotation policies
- **Use purpose strings** - isolate encryption contexts
- **Store keys securely** - Azure Key Vault or secure key storage
- **Never hardcode keys** - use configuration or key vaults

### API Security
- **Use API keys for service-to-service** - separate from user authentication
- **Implement rate limiting** - prevent abuse and DoS
- **Validate all inputs** - prevent injection attacks
- **Use CORS properly** - restrict origins in production
- **Enable HSTS** - force HTTPS connections

## Audit Logging

```csharp
public class AuditingService
{
    private readonly IAuditSink auditSink;
    
    public async Task LogSecurityEventAsync(string operation, string userId, bool success)
    {
        var auditRecord = new AuditRecord
        {
            Timestamp = DateTime.UtcNow,
            Operation = operation,
            UserId = userId,
            Success = success,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers["User-Agent"]
        };
        
        await auditSink.WriteAsync(auditRecord);
    }
}
```

## Multi-Tenancy Support

```csharp
public class TenantContext
{
    public string TenantId { get; set; }
    public string TenantName { get; set; }
}

public class TenantContextProvider : ITenantContextProvider
{
    public TenantContext GetTenantContext(ProxyContext context)
    {
        var tenantId = context.Request.GetHeader("X-Tenant-Id");
        return new TenantContext { TenantId = tenantId };
    }
}
```

## Common Scenarios

### Secure API Endpoint

```csharp
[Authorize(Roles = "Admin")]
[RateLimit(RequestsPerMinute = 100)]
public class SecureController : ControllerBase
{
    [HttpPost("sensitive-operation")]
    [RequireAuditing]
    public async Task<IActionResult> SensitiveOperation([FromBody] Request request)
    {
        // Validate authorization
        // Perform operation
        // Audit result
    }
}
```

### External Service Authentication

```csharp
services.AddProxyInterceptor<ApiKeyAuthenticationInterceptor>(options =>
{
    options.ApiKeyHeader = "X-API-Key";
    options.ApiKeyValue = configuration["ExternalService:ApiKey"];
});
```

## Dependencies

This package depends on:
- `VisionaryCoder.Framework` - Base types
- BCrypt.Net-Next for password hashing
- System.IdentityModel.Tokens.Jwt for JWT handling
- ASP.NET Core Data Protection

## Version Compatibility

| Framework.Security | .NET Version | BCrypt | JWT |
|-------------------|--------------|--------|-----|
| 1.0.0             | .NET 10 LTS  | 4.0.3  | 8.14.0 |

## License

MIT License - see LICENSE file for details

## Support

- GitHub Issues: https://github.com/visionarycoder/Framework/issues
- Documentation: https://github.com/visionarycoder/Framework/wiki
