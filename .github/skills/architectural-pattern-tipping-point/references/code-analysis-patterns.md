---
title: Code Analysis Patterns for Tipping Point Detection
doc_type: reference
status: active
last_updated: 2026-08-30
parent_skill: architectural-pattern-tipping-point
---

# Code Analysis Patterns for Tipping Point Detection

## Bounded Context Detection

Agent scans codebase for bounded context indicators to determine modular monolith readiness.

### Clear Bounded Contexts (Good for Modular Monolith)

```csharp
// Namespace boundaries align with business domains
namespace Wa.Wsdot.Fin.Idl.Accounting
{
    public class Journal { }
    public class GeneralLedger { }
}

namespace Wa.Wsdot.Fin.Idl.Payroll
{
    public class PayPeriod { }
    public class TimeEntry { }
}

// ✅ Minimal cross-references between namespaces
// ✅ Clear domain ownership
// ✅ Good modular monolith candidate
```

### Tangled Context (Refactor Before Splitting)

```csharp
// Everything in one namespace
namespace Wa.Wsdot.Fin.Idl.Services
{
    public class AccountingService { }
    public class PayrollService { }
    public class HRService { }
    public class ReportingService { }
    // All reference each other heavily
}

// ❌ Poor cohesion
// ❌ No clear boundaries
// ❌ Stay monolith and refactor first
```

## Deployment Boundary Analysis

Agent analyzes project dependencies to detect deployment domain readiness.

### Independent Deployment Domains (Ready for Split)

```
// Deployment Domain 1: Portal (UI + API)
Wa.Wsdot.Fin.Idl.Client.Portal.WebApi
  → Wa.Wsdot.Fin.Idl.Manager.Transport
  → Wa.Wsdot.Fin.Idl.Engine.Extracting
  → Wa.Wsdot.Fin.Idl.Access.Storage

// Deployment Domain 2: Scheduler (Background jobs)
Wa.Wsdot.Fin.Idl.Client.Scheduler.WebApp
  → Wa.Wsdot.Fin.Idl.Engine.Converting
  → Wa.Wsdot.Fin.Idl.Engine.Materializing
  → Wa.Wsdot.Fin.Idl.Access.Advantage

// ✅ No circular dependencies between domains
// ✅ Each domain has its own entry point
// ✅ Shared infrastructure abstracted through Access layer
// ✅ Ready for deployment domain pattern
```

### Tightly Coupled (Not Ready)

```
Portal.WebApi ←→ Scheduler.WebApp
  ↓           ↑
  → Shared state or synchronous calls between

// ❌ Circular dependency
// ❌ Not ready for independent deployment
// ❌ Fix coupling first
```

## Database Transaction Boundary Analysis

Agent identifies cross-context transactions that block service decomposition.

### Transaction Spans Contexts (Blocks Split)

```csharp
// ❌ Transaction spans contexts - prevents service split
public async Task ProcessPayrollJournal(PayrollBatch batch)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    // Payroll context
    var payPeriod = await _payrollRepo.CreatePayPeriod(batch);
    
    // Accounting context
    var journal = await _accountingRepo.PostJournal(batch.JournalEntries);
    
    await transaction.CommitAsync();
    // Both updates succeed or both roll back
}

// Problem: Distributed transaction will be needed if split
// Action: Redesign with saga pattern or keep together
```

### Saga Pattern Alternative (Split-Ready)

```csharp
// ✅ Eventual consistency with saga orchestration
public async Task ProcessPayrollJournal(PayrollBatch batch)
{
    // Payroll service: Create pay period
    var payPeriod = await _payrollService.CreatePayPeriod(batch);
    
    // Publish event (no transaction coupling)
    await _messageBus.PublishAsync(new PayPeriodCreatedEvent
    {
        PayPeriodId = payPeriod.Id,
        JournalEntries = batch.JournalEntries
    });
    
    // Accounting service handles event asynchronously
    // Compensating transaction if accounting fails
}

// ✅ Services can deploy independently
// ✅ No distributed transaction needed
// ✅ Eventual consistency acceptable
```

## Shared Kernel Detection

Agent identifies shared code that complicates deployment independence.

### Shared Kernel (Deployment Coupling Risk)

```csharp
// Common utilities used by all modules
namespace Wa.Wsdot.Fin.Idl.Util
{
    public static class DateHelper { }
    public static class ValidationHelper { }
    public static class CurrencyHelper { }
}

// All modules reference Util directly
// Change to Util requires all modules to redeploy

// Risk Assessment:
// - If Util changes rarely → Low risk for deployment domains
// - If Util changes frequently → Extract to NuGet package first
```

### NuGet Package Extraction (Deployment Independence)

```csharp
// Extract stable shared code to NuGet package
// Wa.Wsdot.Fin.Idl.SharedKernel (NuGet)
namespace Wa.Wsdot.Fin.Idl.SharedKernel
{
    public static class DateHelper { }
    public static class CurrencyHelper { }
}

// Each deployment domain versions independently
// Portal v1.2.0 uses SharedKernel v2.1.0
// Scheduler v1.5.0 uses SharedKernel v2.1.0
// Admin v1.0.0 uses SharedKernel v2.0.0 (older, still works)

// ✅ No forced coordinated deployment
```

## Coupling Metrics Analysis

Agent uses static analysis to measure coupling and detect high-risk areas.

### Afferent/Efferent Coupling

```csharp
// High Afferent Coupling (Ca) = Many incoming dependencies
// This is STABLE - many depend on it, hard to change
public class SharedConfiguration
{
    // 15 classes depend on this
    // Ca = 15, stable
}

// High Efferent Coupling (Ce) = Many outgoing dependencies
// This is UNSTABLE - depends on many, easy to change
public class ReportGenerator
{
    // Uses 20 different services
    // Ce = 20, unstable
}

// Instability = Ce / (Ce + Ca)
// I = 0 (stable, hard to change)
// I = 1 (unstable, easy to change)

// Problem Pattern:
// Core domain with I > 0.5 = too unstable
// Infrastructure with I < 0.3 = too rigid

// ✅ Good: Core domain I < 0.3 (stable)
// ✅ Good: Infrastructure I > 0.5 (flexible)
```

## Module Size Analysis

Agent measures module size to detect extraction candidates.

### Module Size Thresholds

```
Module Analysis:
  Accounting: 45K LOC, 120 classes
    ✅ Reasonable size, keep together

  Payroll: 85K LOC, 250 classes
    ⚠️  Large; analyze for sub-contexts
      - TimeTracking: 35K LOC
      - PayCalculation: 30K LOC
      - Benefits: 20K LOC
    → Candidate for splitting into 3 modules

  Shared: 120K LOC, 300 classes
    ❌ Too large; likely contains multiple concerns
    → Urgent refactoring needed before any split
```

## API Contract Stability Analysis

Agent tracks API changes to determine service boundary stability.

### Stable Boundary (Safe to Extract)

```csharp
// Public API unchanged for 6+ months
public interface IAccountingService
{
    Task<Journal> PostJournalAsync(JournalRequest request);
    Task<IEnumerable<Journal>> GetJournalsAsync(DateRange range);
}

// ✅ Stable contract = good service candidate
// ✅ Versioning strategy in place
```

### Volatile Boundary (Not Ready)

```csharp
// Interface changed 5 times in last 3 months
public interface IReportingService
{
    // Week 1: Added parameter
    Task<Report> GenerateAsync(ReportType type, DateRange range);
    
    // Week 4: Changed return type
    Task<ReportResult> GenerateAsync(ReportType type, DateRange range);
    
    // Week 8: Added another parameter
    Task<ReportResult> GenerateAsync(ReportType type, DateRange range, string format);
}

// ❌ Unstable boundary
// ❌ Not ready for service extraction
// ❌ Let boundary stabilize first
```

## Verification

Agent verifies code analysis completeness:

- [ ] Bounded context boundaries identified
- [ ] Deployment domain candidates analyzed
- [ ] Transaction boundaries documented
- [ ] Shared kernel coupling measured
- [ ] Coupling metrics calculated
- [ ] Module sizes assessed
- [ ] API stability tracked
