# Domain-Specific Abstractions Migration Plan

**Date:** January 2025  
**Objective:** Move domain-specific abstractions from Framework.Abstractions to domain-specific Contracts projects  
**Principle:** Framework.Abstractions should contain ONLY truly cross-cutting concerns

---

## 🎯 Migration Strategy

### Core Principle: "Is this abstraction used by 3+ domains?"

- **YES** → Keep in Framework.Abstractions
- **NO** → Move to domain-specific Contracts project

---

## 📦 Current State Analysis

### Framework.Abstractions Contents (29 files)

| Category | Files | Domain Owner | Action |
|----------|-------|--------------|--------|
| **CQRS** | 6 files | Cross-cutting | ✅ KEEP |
| **Patterns** | 3 files | Cross-cutting | ✅ KEEP |
| **Primitives** | 2 files | Cross-cutting | ✅ KEEP |
| **Specifications** | 1 file | Cross-cutting | ✅ KEEP |
| **Events** | 2 files | Cross-cutting | ✅ KEEP |
| **ServiceResult** | 4 files | Cross-cutting | ✅ KEEP |
| **Messaging** | 4 files | Messaging domain | ➡️ MOVE |
| **Proxy** | 6 files | Resilience domain | ➡️ MOVE |
| **Proxy/Exceptions** | 3 files | Resilience domain | ➡️ MOVE |
| **Pipeline** | 3 files | Mixed ownership | ➡️ SPLIT |
| **Secrets** | 1 file | Security domain | ➡️ MOVE |
| **Options** | 1 file | Cross-cutting | ✅ KEEP |

---

## ✅ Files to KEEP in Framework.Abstractions (18 files)

### CQRS/ (6 files) - Command Query Responsibility Segregation
```
✅ ICommand.cs - Command marker interface
✅ IQuery.cs - Query marker interface
✅ ICommandHandler.cs - Command handler contract
✅ IQueryHandler.cs - Query handler contract
✅ IMediator.cs - Mediator pattern for CQRS
✅ IPipelineBehavior.cs - Pipeline behavior for cross-cutting concerns
```
**Reason:** Used by ALL domains for command/query operations

---

### Patterns/ (3 files) - Functional Programming Patterns
```
✅ Result.cs - Result monad for error handling
✅ Error.cs - Error representation
✅ ResultExtensions.cs - Result helper methods
```
**Reason:** Used across ALL domains for consistent error handling

---

### Primitives/ (2 files) - Domain Primitives
```
✅ EntityId.cs - Strongly-typed entity identifiers
✅ IEntityId.cs - Entity ID contract
```
**Reason:** Used by ALL domains for entity identification

---

### Specifications/ (1 file) - Specification Pattern
```
✅ Specification.cs - Specification pattern base
```
**Reason:** Used across multiple domains for query specifications

---

### Events/ (2 files) - Domain Events
```
✅ IDomainEvent.cs - Domain event marker
✅ IDomainEventHandler.cs - Domain event handler contract
```
**Reason:** Used across ALL domains for event-driven architecture

---

### ServiceResult (4 files) - Service Response Patterns
```
✅ ServiceResult.cs - Service operation result
✅ ServiceResultBase.cs - Base service result
✅ ServiceRequest.cs - Service request wrapper
✅ ServiceRequestOfType.cs - Generic service request
```
**Reason:** Used by ALL domains for consistent service responses

---

### Root (1 file) - Configuration
```
✅ Options.cs - Base configuration options
```
**Reason:** Used across ALL domains for configuration

---

## ➡️ Files to MOVE (11 files)

### Move to Framework.Messaging.Contracts (4 files)

#### Target: `src/VisionaryCoder.Framework.Messaging.Contracts/`
#### Namespace: `VisionaryCoder.Framework.Messaging.Contracts`

```
➡️ Messaging/IMessage.cs
➡️ Messaging/IMessagePublisher.cs
➡️ Messaging/IMessageConsumer.cs
➡️ Messaging/IMessageBus.cs
```

**Reason:** These abstractions are ONLY used by the Messaging domain (queues, buses, pub/sub)

**Impact:**
- Framework.Messaging references Messaging.Contracts
- Other packages rarely need messaging abstractions directly

---

### Move to Framework.Resilience.Contracts (9 files)

#### Target: `src/VisionaryCoder.Framework.Resilience.Contracts/`
#### Namespace: `VisionaryCoder.Framework.Resilience.Contracts`

```
➡️ Proxy/IProxyInterceptor.cs
➡️ Proxy/IOrderedProxyInterceptor.cs
➡️ Proxy/ProxyContext.cs
➡️ Proxy/ProxyResponse.cs
➡️ Proxy/ProxyDelegate.cs
➡️ Proxy/IProxyPipeline.cs
➡️ Proxy/IProxyTransport.cs
➡️ Proxy/Exceptions/ProxyException.cs
➡️ Proxy/Exceptions/TransientProxyException.cs
➡️ Proxy/Exceptions/RetryableTransportException.cs
```

**Reason:** Proxy pattern is primarily used for resilience (retries, circuit breakers, interceptors)

**Current Usage:**
- Framework.Resilience (primary)
- Framework.Observability (logging/telemetry interceptors)
- Framework.Security (auth interceptors)

**Decision:** Proxy is a resilience concern. Other domains implement interceptors for their specific needs.

**Impact:**
- Framework.Observability references Resilience.Contracts (for interceptors)
- Framework.Security references Resilience.Contracts (for auth interceptors)
- Clear ownership: Resilience owns the proxy infrastructure

---

### Move to Framework.Resilience.Contracts (2 files)

#### Target: `src/VisionaryCoder.Framework.Resilience.Contracts/Pipeline/`
#### Namespace: `VisionaryCoder.Framework.Resilience.Contracts.Pipeline`

```
➡️ Pipeline/IRequest.cs - Request/response pipeline request marker
➡️ Pipeline/IInterceptor.cs - Pipeline interceptor pattern
```

**Reason:** These are used for the resilience pipeline (request/response interception for retries, etc.)

**Note:** Different from CQRS pipeline behaviors (IPipelineBehavior)

**Impact:**
- Framework.Resilience owns request/response pipeline
- Framework.Observability may use for tracing interceptors

---

### Move to Framework.Observability.Contracts (1 file)

#### Target: `src/VisionaryCoder.Framework.Observability.Contracts/Tracing/`
#### Namespace: `VisionaryCoder.Framework.Observability.Contracts.Tracing`

```
➡️ Pipeline/ISpan.cs - Distributed tracing span
```

**Reason:** ISpan is ONLY used for distributed tracing (OpenTelemetry)

**Impact:**
- Framework.Observability owns tracing abstractions
- Clear separation from pipeline abstractions

---

### Move to Framework.Security.Contracts (1 file)

#### Target: `src/VisionaryCoder.Framework.Security.Contracts/Secrets/`
#### Namespace: `VisionaryCoder.Framework.Security.Contracts.Secrets`

```
➡️ Secrets/ISecretProvider.cs - Secret management abstraction
```

**Reason:** Secret management is a security concern

**Impact:**
- Framework.Security owns secret management
- Other domains reference Security.Contracts if they need secrets

---

## 🏗️ New Project Structure

### Projects to Create

```
src/
├── VisionaryCoder.Framework.Abstractions/          ← Keep (18 files)
│   ├── CQRS/
│   ├── Patterns/
│   ├── Primitives/
│   ├── Specifications/
│   ├── Events/
│   └── ServiceResult patterns
│
├── VisionaryCoder.Framework.Messaging.Contracts/   ← NEW (4 files)
│   └── Messaging/
│
├── VisionaryCoder.Framework.Resilience.Contracts/  ← NEW (11 files)
│   ├── Proxy/
│   ├── Proxy/Exceptions/
│   └── Pipeline/
│
├── VisionaryCoder.Framework.Observability.Contracts/ ← NEW (1 file)
│   └── Tracing/
│
└── VisionaryCoder.Framework.Security.Contracts/    ← NEW (1 file)
    └── Secrets/
```

---

## 📋 Migration Steps

### Phase 1: Create Contracts Projects

#### Step 1.1: Create Framework.Messaging.Contracts
```bash
dotnet new classlib -n Framework.Messaging.Contracts -o src/VisionaryCoder.Framework.Messaging.Contracts
```

**Project References:**
- Framework.Abstractions (for Result, Error patterns if needed)

---

#### Step 1.2: Create Framework.Resilience.Contracts
```bash
dotnet new classlib -n Framework.Resilience.Contracts -o src/VisionaryCoder.Framework.Resilience.Contracts
```

**Project References:**
- Framework.Abstractions (for patterns)

---

#### Step 1.3: Create Framework.Observability.Contracts
```bash
dotnet new classlib -n Framework.Observability.Contracts -o src/VisionaryCoder.Framework.Observability.Contracts
```

**Project References:**
- Framework.Abstractions (minimal)

---

#### Step 1.4: Create Framework.Security.Contracts
```bash
dotnet new classlib -n Framework.Security.Contracts -o src/VisionaryCoder.Framework.Security.Contracts
```

**Project References:**
- Framework.Abstractions (minimal)

---

### Phase 2: Move Files

#### Step 2.1: Move Messaging Abstractions
```
Framework.Abstractions/Messaging/*.cs 
  → Framework.Messaging.Contracts/
```

**Update Namespaces:**
- OLD: `VisionaryCoder.Framework.Abstractions.Messaging`
- NEW: `VisionaryCoder.Framework.Messaging.Contracts`

---

#### Step 2.2: Move Resilience Abstractions
```
Framework.Abstractions/Proxy/*.cs 
  → Framework.Resilience.Contracts/Proxy/

Framework.Abstractions/Proxy/Exceptions/*.cs 
  → Framework.Resilience.Contracts/Proxy/Exceptions/

Framework.Abstractions/Pipeline/{IRequest,IInterceptor}.cs 
  → Framework.Resilience.Contracts/Pipeline/
```

**Update Namespaces:**
- OLD: `VisionaryCoder.Framework.Abstractions.Proxy`
- NEW: `VisionaryCoder.Framework.Resilience.Contracts.Proxy`

---

#### Step 2.3: Move Observability Abstractions
```
Framework.Abstractions/Pipeline/ISpan.cs 
  → Framework.Observability.Contracts/Tracing/
```

**Update Namespaces:**
- OLD: `VisionaryCoder.Framework.Abstractions.Pipeline`
- NEW: `VisionaryCoder.Framework.Observability.Contracts.Tracing`

---

#### Step 2.4: Move Security Abstractions
```
Framework.Abstractions/Secrets/*.cs 
  → Framework.Security.Contracts/Secrets/
```

**Update Namespaces:**
- OLD: `VisionaryCoder.Framework.Abstractions.Secrets`
- NEW: `VisionaryCoder.Framework.Security.Contracts.Secrets`

---

### Phase 3: Update Project References

#### Framework.Messaging
```xml
<ProjectReference Include="..\VisionaryCoder.Framework.Messaging.Contracts\Framework.Messaging.Contracts.csproj" />
```

#### Framework.Resilience
```xml
<ProjectReference Include="..\VisionaryCoder.Framework.Resilience.Contracts\Framework.Resilience.Contracts.csproj" />
```

#### Framework.Observability
```xml
<ProjectReference Include="..\VisionaryCoder.Framework.Observability.Contracts\Framework.Observability.Contracts.csproj" />
<ProjectReference Include="..\VisionaryCoder.Framework.Resilience.Contracts\Framework.Resilience.Contracts.csproj" />
```
(Needs Resilience.Contracts for proxy interceptors)

#### Framework.Security
```xml
<ProjectReference Include="..\VisionaryCoder.Framework.Security.Contracts\Framework.Security.Contracts.csproj" />
<ProjectReference Include="..\VisionaryCoder.Framework.Resilience.Contracts\Framework.Resilience.Contracts.csproj" />
```
(Needs Resilience.Contracts for proxy interceptors)

---

### Phase 4: Update Using Statements

#### Pattern to Find and Replace

**Messaging:**
```csharp
// OLD
using VisionaryCoder.Framework.Abstractions.Messaging;

// NEW
using VisionaryCoder.Framework.Messaging.Contracts;
```

**Proxy/Resilience:**
```csharp
// OLD
using VisionaryCoder.Framework.Abstractions.Proxy;
using VisionaryCoder.Framework.Abstractions.Proxy.Exceptions;

// NEW
using VisionaryCoder.Framework.Resilience.Contracts.Proxy;
using VisionaryCoder.Framework.Resilience.Contracts.Proxy.Exceptions;
```

**Pipeline:**
```csharp
// OLD
using VisionaryCoder.Framework.Abstractions.Pipeline;

// NEW
using VisionaryCoder.Framework.Resilience.Contracts.Pipeline; // For IRequest, IInterceptor
using VisionaryCoder.Framework.Observability.Contracts.Tracing; // For ISpan
```

**Secrets:**
```csharp
// OLD
using VisionaryCoder.Framework.Abstractions.Secrets;

// NEW
using VisionaryCoder.Framework.Security.Contracts.Secrets;
```

---

## 📊 Impact Analysis

### Dependency Changes

#### Before Migration
```
All Domains → Framework.Abstractions (everything)
```

#### After Migration
```
All Domains → Framework.Abstractions (CQRS, Patterns, Primitives, Events)
Messaging → Messaging.Contracts
Resilience → Resilience.Contracts
Observability → Observability.Contracts + Resilience.Contracts
Security → Security.Contracts + Resilience.Contracts
```

---

### Benefits

1. **✅ Clear Domain Ownership**
   - Each domain owns its contracts
   - No confusion about where abstractions belong

2. **✅ Reduced Coupling**
   - Domains only reference contracts they actually use
   - Framework.Abstractions stays lean and stable

3. **✅ Better Versioning**
   - Domain contracts can version independently
   - Breaking changes in Proxy don't affect Messaging

4. **✅ Improved Discoverability**
   - Developers know where to find domain-specific contracts
   - Framework.Abstractions contains only universal patterns

5. **✅ Follows VBD Principles**
   - Contracts organized by volatility and domain
   - High-volatility domains (Resilience, Security) have their own contracts

---

### Risks & Mitigation

#### Risk 1: Increased Project Count
**Impact:** 4 new projects  
**Mitigation:** Clear naming (*.Contracts) makes purpose obvious

#### Risk 2: Circular Dependencies
**Impact:** Observability and Security need Resilience.Contracts  
**Mitigation:** Contracts projects have no implementation dependencies

#### Risk 3: Migration Effort
**Impact:** ~50 files need namespace updates  
**Mitigation:** Systematic find/replace, build after each phase

---

## ✅ Success Criteria

1. **Zero Build Errors** after migration
2. **All Tests Pass** with no regressions
3. **Framework.Abstractions** contains only cross-cutting concerns (18 files)
4. **Each domain** has clear contract ownership
5. **No circular dependencies** between Contracts projects
6. **Documentation** updated to reflect new structure

---

## 📈 Estimated Effort

| Phase | Tasks | Estimated Time |
|-------|-------|----------------|
| **Phase 1** | Create 4 new projects | 30 minutes |
| **Phase 2** | Move 17 files | 45 minutes |
| **Phase 3** | Update project references | 30 minutes |
| **Phase 4** | Update using statements | 60 minutes |
| **Validation** | Build and test | 30 minutes |
| **TOTAL** | | **3 hours** |

---

## 🎯 Recommended Approach

### Option A: Big Bang Migration (Recommended)
- Migrate all domains in one session
- Easier to track dependencies
- Single comprehensive test cycle
- **Time:** 3 hours

### Option B: Incremental Migration
- Migrate one domain at a time
- Lower risk but more context switching
- Multiple build/test cycles
- **Time:** 4-5 hours (overhead)

**Recommendation:** Option A (Big Bang) - cleaner and faster with clear rollback point

---

## 📝 Post-Migration Checklist

- [ ] All 4 Contracts projects created and added to solution
- [ ] All 17 files moved to correct Contracts projects
- [ ] All namespaces updated in moved files
- [ ] All project references updated in domain packages
- [ ] All using statements updated across solution
- [ ] Solution builds with zero errors
- [ ] All existing tests pass
- [ ] Documentation updated (README files, architecture docs)
- [ ] Git commit with clear message
- [ ] Team notified of namespace changes

---

**Migration Plan Created:** January 2025  
**Status:** Ready for execution  
**Risk Level:** 🟢 LOW (clear dependencies, no data impact)  
**Next Action:** Execute Phase 1 - Create Contracts projects

