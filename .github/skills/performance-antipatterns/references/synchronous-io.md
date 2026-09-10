---
title: Synchronous I/O Anti-Pattern Reference
doc_type: reference
status: active
last_updated: 2026-08-30
parent_skill: performance-antipatterns
---

# Synchronous I/O Anti-Pattern

## Description

Blocking the calling thread while I/O completes, preventing the thread from handling other requests.

## Detection Signals

- `.Result` or `.Wait()` on `Task` in async method
- `Task.Run(() => { ... }).Result` wrapping async operation
- Synchronous file I/O (`File.ReadAllText`, `File.WriteAllText`)
- Synchronous HTTP calls (`HttpClient.Send` instead of `SendAsync`)
- Synchronous database calls (ADO.NET without `Async` suffix)

## Why This Is Problematic

- Thread pool threads blocked during I/O
- Thread starvation under load
- Request queue grows, timeouts increase
- Cannot scale horizontally to handle more concurrent requests

## Remediation

**Use async/await throughout:**

```csharp
// ❌ BEFORE: Blocks thread during I/O
public ActionResult<Customer> GetCustomer(int id)
{
    var customer = _context.Customers.Find(id);
    var discount = _httpClient.GetStringAsync($"/discounts/{id}").Result;
    return Ok(customer);
}

// ✅ AFTER: Async throughout
public async Task<ActionResult<Customer>> GetCustomer(int id, CancellationToken cancellationToken)
{
    var customer = await _context.Customers.FindAsync(id, cancellationToken);
    var discount = await _httpClient.GetStringAsync($"/discounts/{id}", cancellationToken);
    return Ok(customer);
}
```

**File I/O:**

```csharp
// ❌ BEFORE
var content = File.ReadAllText(path);

// ✅ AFTER
var content = await File.ReadAllTextAsync(path, cancellationToken);
```

## Verification

Test: Load test with 100+ concurrent requests.
Pass: Request latency remains stable. Thread pool thread count stays below total thread count / 2.

## Reference

[Synchronous I/O Anti-Pattern - Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/antipatterns/synchronous-io/)
