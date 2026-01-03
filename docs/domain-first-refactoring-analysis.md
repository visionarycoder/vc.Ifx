# Domain-First Refactoring Analysis

## Current Structure (Technology-First)

### Framework.Azure
**Files:** 16 files  
**Technology Focus:** Azure SDK

**Domain Mapping:**
```
Storage/Azure/Blob/ (2 files)
  → Domain: STORAGE
  → New Location: Framework.Storage/Azure/Blob/

Data/Azure/Table/ (3 files)
  → Domain: DATA ACCESS
  → New Location: Framework.DataAccess/Azure/Table/

Messaging/Azure/ (4 files)
  → Domain: MESSAGING
  → New Location: Framework.Messaging/Azure/

Secrets/Azure/KeyVault/ (4 files)
  → Domain: CONFIGURATION
  → New Location: Framework.Configuration/Azure/KeyVault/

Proxy/Interceptors/Configuration/Azure/ (3 files)
  → Domain: CONFIGURATION
  → New Location: Framework.Configuration/Azure/AppConfiguration/
```

---

### Framework.EntityFrameworkCore
**Files:** 19 files  
**Technology Focus:** EF Core

**Domain Mapping:**
```
Primitives/Data/EFCore/ (2 files)
  → Domain: DATA ACCESS
  → New Location: Framework.DataAccess/EntityFrameworkCore/Primitives/

Filtering/EFCore/ (2 files)
  → Domain: DATA ACCESS
  → New Location: Framework.DataAccess/EntityFrameworkCore/Filtering/

Querying/ (15 files)
  → Domain: DATA ACCESS
  → New Location: Framework.DataAccess/EntityFrameworkCore/Querying/
```

---

### Framework.Security
**Files:** 55 files  
**Technology Focus:** Auth/Authz/Encryption

**Domain Mapping:**
```
Security/ (3 files - password hashing)
  → Domain: IDENTITY
  → New Location: Framework.Identity/Authentication/PasswordHashing/

Proxy/Interceptors/Security/ (23 files)
  → Domain: IDENTITY
  → New Location: Framework.Identity/Interceptors/

Proxy/Interceptors/Authentication/ (18 files)
  → Domain: IDENTITY
  → New Location: Framework.Identity/Authentication/

Proxy/Interceptors/Authorization/ (5 files)
  → Domain: IDENTITY
  → New Location: Framework.Identity/Authorization/

Proxy/Interceptors/Auditing/ (6 files)
  → Domain: IDENTITY
  → New Location: Framework.Identity/Auditing/
```

---

### Framework.Observability
**Files:** 23 files  
**Cross-Cutting:** Infrastructure concern

**Decision:** KEEP AS-IS (infrastructure layer)
```
Logging/ (11 files) → KEEP
Pipeline/Observability/ (4 files) → KEEP
Proxy/Interceptors/Logging/ (5 files) → KEEP
Proxy/Interceptors/Telemetry/ (3 files) → KEEP
```

---

### Framework.Resilience
**Files:** 12 files  
**Cross-Cutting:** Infrastructure concern

**Decision:** KEEP AS-IS (infrastructure layer)
```
Proxy/Interceptors/Resilience/ (4 files) → KEEP
Proxy/Interceptors/Retries/ (7 files) → KEEP
Pipeline/Interceptors/ResilienceInterceptor (1 file) → KEEP
```

---

## New Domain-Based Structure

### Framework.Messaging (NEW)
**Domain:** Asynchronous communication  
**Files:** 4 from Framework.Azure

```
Framework.Messaging/
├── Abstractions/
│   ├── IMessageBus.cs (from Framework.Abstractions)
│   ├── IMessagePublisher.cs (from Framework.Abstractions)
│   ├── IMessageConsumer.cs (from Framework.Abstractions)
│   └── IMessage.cs (from Framework.Abstractions)
├── Azure/
│   ├── ServiceBusMessaging.cs
│   ├── Queue/
│   │   ├── AzureQueueStorageProvider.cs
│   │   ├── AzureQueueStorageOptions.cs
│   │   └── IQueueStorageProvider.cs
│   └── EventGrid/ (future)
└── InMemory/ (future)

Dependencies:
  - Azure.Messaging.ServiceBus
  - Azure.Messaging.EventGrid
  - Azure.Storage.Queues
```

---

### Framework.DataAccess (NEW)
**Domain:** Structured data persistence  
**Files:** 19 from Framework.EntityFrameworkCore + 3 from Framework.Azure

```
Framework.DataAccess/
├── Abstractions/
│   ├── IRepository.cs (to be created)
│   └── IUnitOfWork.cs (to be created)
├── EntityFrameworkCore/
│   ├── Primitives/
│   │   ├── EntityIdValueConverter.cs
│   │   └── EntityIdModelBuilderExtensions.cs
│   ├── Filtering/
│   │   ├── EfFilterExecutionStrategy.cs
│   │   └── EfFilterExpressionBuilder.cs
│   └── Querying/
│       ├── QueryFilter.cs
│       ├── QueryFilterExtensions.cs
│       └── Serialization/ (5 files)
├── Azure/
│   └── Table/
│       ├── AzureTableStorageProvider.cs
│       ├── AzureTableStorageOptions.cs
│       └── ITableStorageProvider.cs
└── Dapper/ (future)

Dependencies:
  - Microsoft.EntityFrameworkCore (all packages)
  - Azure.Data.Tables
  - EFCore.NamingConventions
```

---

### Framework.Storage (NEW)
**Domain:** File and blob storage  
**Files:** 2 from Framework.Azure + existing storage files

```
Framework.Storage/
├── Abstractions/
│   └── IStorageProvider.cs (from Framework)
├── Azure/
│   └── Blob/
│       ├── AzureBlobStorageProvider.cs
│       └── AzureBlobStorageOptions.cs
├── Local/
│   ├── LocalStorageProvider.cs (from Framework)
│   └── LocalStorageOptions.cs (from Framework)
└── Ftp/
    ├── FtpStorageProvider.cs (from Framework)
    └── FtpStorageOptions.cs (from Framework)

Dependencies:
  - Azure.Storage.Blobs
  - FluentFTP
```

---

### Framework.Identity (RENAME from Framework.Security)
**Domain:** Authentication, authorization, and user identity  
**Files:** 55 from Framework.Security

```
Framework.Identity/
├── Abstractions/
│   ├── IAuthenticationService.cs (to be created)
│   └── IAuthorizationPolicy.cs (exists)
├── Authentication/
│   ├── PasswordHashing/
│   │   ├── IPasswordHasher.cs
│   │   ├── PasswordHasher.cs
│   │   └── PasswordHashingServiceCollectionExtensions.cs
│   ├── JWT/
│   │   ├── ITokenProvider.cs
│   │   ├── TokenRequest.cs
│   │   ├── TokenResult.cs
│   │   └── JwtOptions.cs
│   ├── Providers/
│   │   ├── DefaultUserContextProvider.cs
│   │   ├── DefaultTenantContextProvider.cs
│   │   └── DefaultTokenProvider.cs
│   └── Interceptors/
│       ├── JwtAuthenticationInterceptor.cs
│       ├── KeyVaultJwtInterceptor.cs
│       └── AuthenticationExtensions.cs
├── Authorization/
│   ├── Policies/
│   │   ├── IAuthorizationPolicy.cs
│   │   ├── RoleBasedAuthorizationPolicy.cs
│   │   └── NullAuthorizationPolicy.cs
│   ├── Results/
│   │   └── AuthorizationResult.cs
│   └── AuthorizationExtensions.cs
├── Security/
│   ├── SecurityInterceptor.cs
│   ├── UserContext.cs
│   ├── TenantContext.cs
│   └── Enrichers/
│       ├── UserContextEnricher.cs
│       ├── TenantContextEnricher.cs
│       └── JwtBearerEnricher.cs
└── Auditing/
    ├── AuditingInterceptor.cs
    ├── AuditRecord.cs
    ├── IAuditSink.cs
    └── LoggingAuditSink.cs

Dependencies:
  - BCrypt.Net-Next
  - System.IdentityModel.Tokens.Jwt
  - Microsoft.AspNetCore.DataProtection (all packages)
```

---

### Framework.Configuration (NEW)
**Domain:** Application settings and secrets management  
**Files:** 7 from Framework.Azure + existing secrets

```
Framework.Configuration/
├── Abstractions/
│   ├── IConfigurationProvider.cs (exists)
│   └── ISecretProvider.cs (from Framework)
├── Azure/
│   ├── AppConfiguration/
│   │   ├── AzureConfigurationProvider.cs
│   │   ├── AzureConfigurationProviderOptions.cs
│   │   └── AzureConfigurationProviderOptionsExtensions.cs
│   └── KeyVault/
│       ├── KeyVaultSecretProvider.cs
│       ├── KeyVaultOptions.cs
│       ├── KeyVaultExtensions.cs
│       └── SecretOptions.cs
├── Local/
│   ├── LocalConfigurationProvider.cs (from Framework)
│   ├── LocalSecretProvider.cs (from Framework)
│   └── LocalConfigurationProviderOptions.cs (from Framework)
└── Environment/ (future)

Dependencies:
  - Azure.Security.KeyVault.Secrets
  - Azure.Extensions.AspNetCore.Configuration.Secrets
  - Microsoft.Extensions.Configuration.AzureAppConfiguration
```

---

## Cross-Cutting Packages (Keep As-Is)

### Framework.Observability
**Type:** Infrastructure  
**Keep:** Yes  
**Reason:** Monitoring is cross-cutting concern, not domain capability

### Framework.Resilience
**Type:** Infrastructure  
**Keep:** Yes  
**Reason:** Fault tolerance is cross-cutting concern, not domain capability

---

## Migration Summary

### Packages Removed
- ❌ Framework.Azure (split across 4 domains)
- ❌ Framework.EntityFrameworkCore (moved to DataAccess)
- ❌ Framework.Security (renamed to Identity)

### Packages Created
- ✅ Framework.Messaging (new)
- ✅ Framework.DataAccess (new)
- ✅ Framework.Storage (new)
- ✅ Framework.Identity (renamed from Security)
- ✅ Framework.Configuration (new)

### Packages Unchanged
- ✅ Framework.Abstractions
- ✅ Framework.Core
- ✅ Framework.Patterns
- ✅ Framework.Observability
- ✅ Framework.Resilience
- ✅ Framework (main package)

---

## File Movement Matrix

| Source Package | Source Files | Destination Package | Destination Path |
|----------------|--------------|---------------------|------------------|
| Framework.Azure | Storage/Azure/Blob/* | Framework.Storage | Azure/Blob/* |
| Framework.Azure | Data/Azure/Table/* | Framework.DataAccess | Azure/Table/* |
| Framework.Azure | Messaging/Azure/* | Framework.Messaging | Azure/* |
| Framework.Azure | Secrets/Azure/KeyVault/* | Framework.Configuration | Azure/KeyVault/* |
| Framework.Azure | Proxy/Interceptors/Configuration/Azure/* | Framework.Configuration | Azure/AppConfiguration/* |
| Framework.EntityFrameworkCore | All files | Framework.DataAccess | EntityFrameworkCore/* |
| Framework.Security | All files | Framework.Identity | (reorganized structure) |

---

## Dependency Changes

### Before (Technology-First)
```xml
<!-- User wants messaging -->
<PackageReference Include="VisionaryCoder.Framework.Azure" />
<!-- Gets ALL Azure services (Blob, Table, ServiceBus, KeyVault, etc.) -->
```

### After (Domain-First)
```xml
<!-- User wants messaging -->
<PackageReference Include="VisionaryCoder.Framework.Messaging" />
<!-- Gets ONLY messaging abstractions + Azure ServiceBus/Queue implementations -->
```

---

## Namespace Changes

### Before
```csharp
using VisionaryCoder.Framework.Azure.Messaging;
using VisionaryCoder.Framework.Azure.Storage.Blob;
using VisionaryCoder.Framework.Security.Authentication;
```

### After
```csharp
using VisionaryCoder.Framework.Messaging.Azure;
using VisionaryCoder.Framework.Storage.Azure.Blob;
using VisionaryCoder.Framework.Identity.Authentication;
```

---

## Benefits

1. **Domain Clarity:** Package names reflect business capabilities
2. **Technology Agnostic:** Can swap Azure → AWS without package rename
3. **Granular Dependencies:** Install only the domains you need
4. **Team Organization:** Teams align with domains, not platforms
5. **Future-Proof:** Add new tech (Kafka, Dapper) without new top-level packages

---

## Migration Steps (High-Level)

1. Create 5 new domain packages
2. Copy files from old packages to new domains
3. Update namespaces in all files
4. Update project references
5. Remove old technology packages
6. Update solution file
7. Build and test
8. Update documentation

---

**Next:** Begin creating new domain package structures
