---
title: Chatty I/O Anti-Pattern Reference
doc_type: reference
status: active
last_updated: 2026-08-30
parent_skill: performance-antipatterns
---

# Chatty I/O Anti-Pattern

## Description

Continually sending many small network requests instead of fewer large requests, accumulating network latency.

## Detection Signals

- HTTP call or database query inside loop
- N+1 query pattern (1 query + N queries for related data)
- Individual item operations instead of bulk operations
- Separate calls for each property or field
- GraphQL queries without data loader pattern

## Why This Is Problematic

- Network latency multiplied by request count (10ms × 100 requests = 1 second)
- Database connection pool exhaustion
- Increased server load processing many small requests
- Poor performance over high-latency networks

## Remediation

**Batch operations:**

```csharp
// ❌ BEFORE: N+1 queries
var orders = await _context.Orders.ToListAsync();
foreach (var order in orders)
{
    order.Customer = await _context.Customers.FindAsync(order.CustomerId);
}

// ✅ AFTER: Single query with Include
var orders = await _context.Orders
    .Include(o => o.Customer)
    .ToListAsync();
```

**Bulk API operations:**

```csharp
// ❌ BEFORE: Individual HTTP calls
foreach (var item in items)
{
    await httpClient.PostAsJsonAsync("/api/items", item);
}

// ✅ AFTER: Bulk endpoint
await httpClient.PostAsJsonAsync("/api/items/bulk", items);
```

**Projection to reduce fields:**

```csharp
// ❌ BEFORE: Multiple queries for different fields
var name = await _context.Products.Where(p => p.Id == id).Select(p => p.Name).FirstAsync();
var price = await _context.Products.Where(p => p.Id == id).Select(p => p.Price).FirstAsync();

// ✅ AFTER: Single query with projection
var product = await _context.Products
    .Where(p => p.Id == id)
    .Select(p => new { p.Name, p.Price })
    .FirstAsync();
```

## Verification

Test: Count database queries or HTTP calls during operation.
Pass: Query/call count reduced by >80%. Total operation time decreases proportionally.

## Reference

[Chatty I/O Anti-Pattern - Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/antipatterns/chatty-io/)
