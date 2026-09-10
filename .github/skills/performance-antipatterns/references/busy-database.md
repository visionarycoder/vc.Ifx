---
title: Busy Database Anti-Pattern Reference
doc_type: reference
status: active
last_updated: 2026-08-30
parent_skill: performance-antipatterns
---

# Busy Database Anti-Pattern

## Description

Offloading processing to a data store when the application tier handles the work more efficiently.

## Detection Signals

- Stored procedures contain complex business logic
- Database triggers perform calculations or transformations
- Database CPU usage disproportionately high compared to application tier
- Complex T-SQL logic duplicates application code

## Why This Is Problematic

- Database CPU becomes bottleneck
- Difficult to scale (vertical scaling only)
- Business logic split between application and database
- Testing and debugging complexity increases
- Database licensing costs increase with CPU requirements

## Remediation

**Move logic to application tier:**

```csharp
// ❌ BEFORE: Database does calculation
var result = await context.Customers
    .FromSqlRaw("EXEC CalculateCustomerDiscount @CustomerId", customerId)
    .FirstOrDefaultAsync();

// ✅ AFTER: Application does calculation
var customer = await context.Customers
    .Where(c => c.Id == customerId)
    .Include(c => c.Orders)
    .FirstOrDefaultAsync();

var discount = discountEngine.Calculate(customer);
```

**Exception:** Batch data transformations (ETL) belong in database or dedicated processing tier.

## Verification

Test: Run load test measuring database CPU before and after.
Pass: Database CPU usage decreases by >30%. Application tier CPU increases proportionally.

## Reference

[Busy Database Anti-Pattern - Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/antipatterns/busy-database/)
