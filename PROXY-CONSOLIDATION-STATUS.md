# Proxy Consolidation Status

## ✅ Completed Tasks

### 1. Removed Duplicate Folders
- ✅ Deleted `src/Framework.Security/Proxy` folder
- ✅ Deleted `src/Framework.Proxy/Interceptors/Authentication` folder
- ✅ Deleted `src/Framework.Proxy/Interceptors/Authorization` folder  
- ✅ Deleted `src/Framework.Proxy/Interceptors/Identity` folder
- ✅ Moved observability interceptors from `src/Framework.Observability/Proxy` to `src/Framework.Proxy/Interceptors`

### 2. Consolidated Structure
Final structure in `src/Framework.Proxy/Interceptors`:
- ✅ `Auditing/` - Audit logging interceptors
- ✅ `Logging/` - Logging interceptors (moved from Observability)
- ✅ `Telemetry/` - Telemetry interceptors (moved from Observability)
- ✅ `Resilience/` - Resilience and circuit breaker interceptors
- ✅ `Retries/` - Retry logic interceptors
- ✅ `Security/` - All security, authentication, and authorization interceptors consolidated here
  - `Security/Providers/` - Context and token providers
  - `Security/Web/` - JWT and web-based authentication interceptors

### 3. Project Configuration
- ✅ Updated `Framework.Proxy.csproj` with proper references to `Framework.Proxy.Abstractions`
- ✅ Added Polly NuGet package to `Directory.Packages.props`
- ✅ Updated `Framework.Observability.csproj` to reference `Framework.Proxy.Abstractions`
- ✅ Removed missing `Framework.Resilience` project from solution file

## ⚠️ Remaining Issues

### 1. Missing Using Statements
**Problem**: ~69 files in `src/Framework.Proxy/Interceptors/**/*.cs` need to add:
```csharp
using VisionaryCoder.Framework.Abstractions.Proxy;
```

**Files Affected**:
- All interceptor implementation files that use `IProxyInterceptor`, `IOrderedProxyInterceptor`, `ProxyContext`, `ProxyDelegate<T>`, or `ProxyResponse<T>`

**Solution**: Run this PowerShell script:
```powershell
$files = Get-ChildItem "src\Framework.Proxy\Interceptors" -Recurse -Filter "*.cs"
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match "IProxyInterceptor|IOrderedProxyInterceptor|ProxyContext|ProxyDelegate|ProxyResponse") {
        if ($content -notmatch "using VisionaryCoder\.Framework\.Abstractions\.Proxy;") {
            # Find the last using statement
            if ($content -match "(?s)(.*using [^;]+;)") {
                $newContent = $content -replace "(using [^;]+;)(\r?\n\r?\nnamespace)", "`$1`nusing VisionaryCoder.Framework.Abstractions.Proxy;`$2"
                Set-Content -Path $file.FullName -Value $newContent -NoNewline
                Write-Host "Updated: $($file.Name)" -ForegroundColor Green
            }
        }
    }
}
```

### 2. Missing HttpContext Dependencies
**Problem**: Some provider files need `Microsoft.AspNetCore.Http` package reference

**Files Affected**:
- `src/Framework.Proxy/Interceptors/Security/Providers/DefaultTenantContextProvider.cs`
- `src/Framework.Proxy/Interceptors/Security/Providers/DefaultUserContextProvider.cs`

**Solution**: Add to `Framework.Proxy.csproj`:
```xml
<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" />
```

### 3. Test File Updates
**Problem**: Test files may reference old namespaces

**Solution**: Run tests and update namespace references as needed. Most tests already use correct namespaces:
- `VisionaryCoder.Framework.Proxy.Interceptors.Logging`
- `VisionaryCoder.Framework.Proxy.Interceptors.Telemetry`
- `VisionaryCoder.Framework.Proxy.Interceptors.Security`
- `VisionaryCoder.Framework.Proxy.Interceptors.Auditing`

## 📋 Quick Fix Steps

1. **Add missing using statements** (run the PowerShell script above)
2. **Add ASP.NET Core HTTP package** to `Framework.Proxy.csproj`
3. **Build solution**: `dotnet build`
4. **Run tests**: `dotnet test`
5. **Fix any remaining test failures** by updating namespace references

## 📁 Final Folder Structure

```
src/
├── Framework.Proxy.Abstractions/
│   ├── IProxyInterceptor.cs
│   ├── IOrderedProxyInterceptor.cs
│   ├── ProxyContext.cs
│   ├── ProxyDelegate.cs
│   ├── ProxyResponse.cs
│   └── Exceptions/
│
└── Framework.Proxy/
    └── Interceptors/
        ├── Auditing/
        │   ├── AuditingInterceptor.cs
        │   ├── IAuditSink.cs
        │   └── ...
        ├── Logging/
        │   ├── LoggingInterceptor.cs
        │   ├── TimingInterceptor.cs
        │   └── ...
        ├── Telemetry/
        │   ├── TelemetryInterceptor.cs
        │   ├── ITelemetryInterceptor.cs
        │   └── ...
        ├── Resilience/
        │   ├── ResilienceInterceptor.cs
        │   ├── RateLimitingInterceptor.cs
        │   └── ...
        ├── Retries/
        │   ├── RetryInterceptor.cs
        │   ├── CircuitBreakerInterceptor.cs
        │   └── ...
        └── Security/
            ├── SecurityInterceptor.cs
            ├── AuthorizationResult.cs
            ├── TenantContext.cs
            ├── UserContext.cs
            ├── Providers/
            │   ├── DefaultTenantContextProvider.cs
            │   ├── DefaultUserContextProvider.cs
            │   ├── DefaultTokenProvider.cs
            │   └── ...
            └── Web/
                ├── JwtAuthenticationInterceptor.cs
                ├── JwtBearerInterceptor.cs
                ├── KeyVaultJwtInterceptor.cs
                └── ...
```

## 🎯 Benefits of This Consolidation

1. **Single Source of Truth**: All proxy-related code is in `Framework.Proxy` and `Framework.Proxy.Abstractions`
2. **No Duplicates**: Removed duplicate classes like `AuthorizationResult`, `TenantContext`, `UserContext`
3. **Clear Separation**: Abstractions are separate from implementations
4. **Better Organization**: Related interceptors are grouped by function (Security, Logging, Telemetry, etc.)
5. **Easier Maintenance**: Developers know exactly where to find and modify proxy code
