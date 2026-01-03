# What's Next - Framework Package Split Roadmap

## ✅ Completed Packages (5 of 8)

### Phase 1 (Complete)
1. **Framework.Azure** - Cloud services integration
2. **Framework.EntityFrameworkCore** - ORM and data access
3. **Framework.Observability** - Logging, tracing, metrics
4. **Framework.Security** - Authentication, authorization, encryption

### Phase 2 (In Progress)
5. **Framework.Resilience** ✅ - Retry, circuit breakers, rate limiting

## ⏳ Remaining Phase 2 Packages (3 of 8)

### Priority 1: Framework.API
**Status:** Ready to implement (pattern established)  
**Effort:** ~30 minutes  
**Impact:** Medium - not all apps need API features

**Steps:**
```bash
# 1. Create project structure
# Copy Framework.Resilience.csproj as template
# Update PackageId, Description, Dependencies

# 2. Key dependencies to extract:
- Asp.Versioning.Mvc (2 packages)
- HotChocolate.* (3 packages - GraphQL)
- Swashbuckle.AspNetCore (Swagger)
- Microsoft.OpenApi

# 3. Files to move:
- src/VisionaryCoder.Framework/API/**

# 4. Add to solution
dotnet sln add src/Framework.API/Framework.API.csproj
dotnet sln add tests/Framework.API.Tests/Framework.API.Tests.csproj
```

**Command to Start:**
```
"Create VisionaryCoder.Framework.API package following the pattern from Framework.Resilience"
```

---

### Priority 2: Framework.Caching
**Status:** Ready to implement  
**Effort:** ~45 minutes  
**Impact:** High - Redis is heavyweight dependency

**Steps:**
```bash
# 1. Create project structure
# 2. Key dependencies:
- Microsoft.Extensions.Caching.StackExchangeRedis
- StackExchange.Redis
- Microsoft.AspNetCore.DataProtection.StackExchangeRedis
- OpenTelemetry.Instrumentation.StackExchangeRedis
- Polly.Caching.Memory
- Polly.Caching.MemoryCache

# 3. Files to move:
- src/VisionaryCoder.Framework/Caching/**
- src/VisionaryCoder.Framework/Proxy/Interceptors/Caching/**

# 4. Note: Some caching interceptors may reference Polly
# Consider if Framework.Caching should reference Framework.Resilience
```

**Command to Start:**
```
"Create VisionaryCoder.Framework.Caching package with Redis and distributed caching support"
```

---

### Priority 3: Framework.gRPC
**Status:** Ready to implement  
**Effort:** ~20 minutes  
**Impact:** Low - only needed for gRPC scenarios

**Steps:**
```bash
# 1. Create project structure
# 2. Key dependencies:
- Google.Protobuf
- Grpc.Net.Client
- Grpc.Net.ClientFactory
- Grpc.Tools (PrivateAssets)

# 3. Files to move:
- src/VisionaryCoder.Framework/Pipeline/Dispatch/GrpcRemoteDispatcher.cs
- src/VisionaryCoder.Framework/Pipeline/Dispatch/GenericGrpcClient.cs
- src/VisionaryCoder.Framework/Pipeline/Dispatch/Abstractions/GenericInvoker.proto

# 4. Update Framework.csproj:
- Remove <Protobuf Include> ItemGroup
```

**Command to Start:**
```
"Create VisionaryCoder.Framework.gRPC package for gRPC transport layer"
```

---

## 📋 Future Enhancements (Optional)

### Framework.BackgroundJobs
**Dependencies:** Quartz (2 packages)  
**Files:** Outbox/**, background job scheduling  
**Priority:** Low - can stay in Framework for now

### Framework.Storage
**Dependencies:** FluentFTP  
**Files:** Storage/** (except Azure - already moved)  
**Priority:** Low - minimal footprint

### Framework Meta-Package
**Concept:** Single package that references all sub-packages  
**Benefit:** Users can `dotnet add package VisionaryCoder.Framework` to get everything  
**Alternative:** Keep current Framework as the "full" package

---

## 🏗️ Architectural Refactoring (Future)

### Move Base Types to Framework.Core
**Goal:** Remove circular dependency where new packages reference Framework

**Types to Move:**
- `ServiceBase<T>` - Base service class
- `IStorageProvider` - Storage abstraction
- `ISecretProvider` - Secrets abstraction
- `IMessagePublisher` - Messaging abstraction
- `IMessageConsumer` - Messaging abstraction
- `IMessageBus` - Messaging abstraction
- `Options` - Base options class

**Benefits:**
- New packages reference Core instead of Framework
- Framework becomes true meta-package
- Cleaner dependency graph
- Users can use Core + specific packages without Framework

**Impact:** Breaking change (requires version bump to 2.0.0)

---

## 🎯 Quick Win Actions (Do These First)

### 1. Complete Phase 2 (30-90 minutes)
Create API, Caching, and gRPC packages following established pattern.

### 2. Update Framework.csproj (15 minutes)
Remove dependencies that are now in sub-packages:
- Remove all Polly.* packages → Framework.Resilience
- Remove API packages → Framework.API  
- Remove Redis packages → Framework.Caching
- Remove gRPC packages → Framework.gRPC

### 3. Test All Packages (30 minutes)
- Run full solution build
- Run all tests
- Generate code coverage report
- Verify package generation

### 4. Update Documentation (15 minutes)
- Update main README with new package structure
- Update package-split-summary.md
- Create migration guide for users

---

## 📊 Progress Tracking

### Package Count
- ✅ Completed: 5 packages
- ⏳ In Progress: 3 packages  
- 📋 Future: 2+ packages
- **Total:** 10+ packages (from 1 monolith)

### Files Migrated
- ✅ Phase 1: 113 files
- ✅ Phase 2 (so far): 12 files
- ⏳ Remaining: ~17 files
- **Total:** ~142 files to migrate

### Dependencies Reduced
- **Before:** 1 package with 70+ dependencies
- **After Phase 2:** 8 focused packages
- **Per-package average:** 5-15 dependencies
- **User savings:** 50-60 dependencies for typical apps

---

## 🚀 Recommended Execution Order

### Today (if time permits):
1. ✅ Framework.Resilience (DONE)
2. Create Framework.API
3. Create Framework.Caching
4. Create Framework.gRPC
5. Update Framework.csproj
6. Build and test everything
7. Update documentation

### Tomorrow:
1. Review package structure
2. Consider base type refactoring
3. Create migration guide
4. Update NuGet package metadata
5. Prepare for v2.0.0 release

### Next Week:
1. Publish packages to NuGet (if ready)
2. Update samples and documentation
3. Create blog post about architecture
4. Plan version 3.0.0 features

---

## 💡 Key Decisions Needed

### 1. Framework Package Strategy
**Option A:** Keep Framework as "everything" package  
**Option B:** Make Framework a meta-package (empty, just references)  
**Option C:** Deprecate Framework, users pick packages

**Recommendation:** Option A for now, Option B in v3.0.0

### 2. Version Numbering
**Current:** All packages at 1.0.0  
**Option A:** Keep synchronized versions  
**Option B:** Independent versioning per package

**Recommendation:** Option A initially, Option B after stability

### 3. Breaking Changes Timeline
**Now:** Non-breaking additions only  
**v2.0.0:** Move base types to Core  
**v3.0.0:** Framework becomes meta-package

---

## 📝 Commands for Quick Copy-Paste

```bash
# Create Framework.API
dotnet new classlib -n VisionaryCoder.Framework.API -o src/VisionaryCoder.Framework.API
dotnet new mstest -n VisionaryCoder.Framework.API.Tests -o tests/VisionaryCoder.Framework.API.Tests

# Create Framework.Caching
dotnet new classlib -n VisionaryCoder.Framework.Caching -o src/VisionaryCoder.Framework.Caching
dotnet new mstest -n VisionaryCoder.Framework.Caching.Tests -o tests/VisionaryCoder.Framework.Caching.Tests

# Create Framework.gRPC
dotnet new classlib -n VisionaryCoder.Framework.gRPC -o src/VisionaryCoder.Framework.gRPC
dotnet new mstest -n VisionaryCoder.Framework.gRPC.Tests -o tests/VisionaryCoder.Framework.gRPC.Tests

# Add all to solution
dotnet sln add src/**/Framework.*.csproj
dotnet sln add tests/**/Framework.*.Tests.csproj

# Build everything
dotnet build

# Run all tests
dotnet test
```

---

## 🎉 Success Criteria

Phase 2 is complete when:
- ✅ All 8 packages created
- ✅ All packages build successfully
- ✅ All tests pass
- ✅ Documentation updated
- ✅ Solution file includes all projects
- ✅ Framework.csproj cleaned up (dependencies removed)

---

**Ready to Continue?** Use the commands above or say:
> "Continue with Framework.API package creation"

---

*Last Updated: 2025-01-15*  
*Status: 5 of 8 packages complete, 3 remaining in Phase 2*
