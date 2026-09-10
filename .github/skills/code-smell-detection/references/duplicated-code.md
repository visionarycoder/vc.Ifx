---
title: Duplicated Code Smell Reference
doc_type: reference
status: active
last_updated: 2026-08-30
parent_skill: code-smell-detection
---

# Duplicated Code Smell

## Description

Identical or very similar code exists in multiple locations.

## Detection Signals

- Code blocks >10 lines repeated in 2+ locations
- Same logic with minor variable name differences
- Copy-paste programming evidence
- Similar methods across classes

## Why This Is Problematic

- Changes require updates in multiple places
- Inconsistency risk when one location updated but not others
- Increased maintenance burden
- Harder to reason about system behavior

## Remediation

**Extract Method:**

```csharp
// ❌ BEFORE: Duplicated validation logic
public void ProcessOrder(Order order)
{
    if (order == null) throw new ArgumentNullException(nameof(order));
    if (order.Items.Count == 0) throw new InvalidOperationException("Order has no items");
    if (order.Total <= 0) throw new InvalidOperationException("Order total invalid");
    // process order
}

public void CancelOrder(Order order)
{
    if (order == null) throw new ArgumentNullException(nameof(order));
    if (order.Items.Count == 0) throw new InvalidOperationException("Order has no items");
    if (order.Total <= 0) throw new InvalidOperationException("Order total invalid");
    // cancel order
}

// ✅ AFTER: Extract Method
private void ValidateOrder(Order order)
{
    if (order == null) throw new ArgumentNullException(nameof(order));
    if (order.Items.Count == 0) throw new InvalidOperationException("Order has no items");
    if (order.Total <= 0) throw new InvalidOperationException("Order total invalid");
}

public void ProcessOrder(Order order)
{
    ValidateOrder(order);
    // process order
}

public void CancelOrder(Order order)
{
    ValidateOrder(order);
    // cancel order
}
```

**Extract Class for duplicated behavior:**

```csharp
// ❌ BEFORE: Tax calculation duplicated
public class OrderService
{
    public decimal CalculateTotal(Order order)
    {
        var subtotal = order.Items.Sum(i => i.Price * i.Quantity);
        var tax = subtotal * 0.08m; // duplicated
        return subtotal + tax;
    }
}

public class InvoiceService
{
    public decimal CalculateTotalDue(Invoice invoice)
    {
        var subtotal = invoice.LineItems.Sum(i => i.Amount);
        var tax = subtotal * 0.08m; // duplicated
        return subtotal + tax;
    }
}

// ✅ AFTER: Extract TaxCalculator
public class TaxCalculator
{
    private const decimal TaxRate = 0.08m;
    
    public decimal CalculateTax(decimal subtotal) => subtotal * TaxRate;
}

public class OrderService
{
    private readonly TaxCalculator _taxCalculator;
    
    public decimal CalculateTotal(Order order)
    {
        var subtotal = order.Items.Sum(i => i.Price * i.Quantity);
        var tax = _taxCalculator.CalculateTax(subtotal);
        return subtotal + tax;
    }
}
```

## Verification

Test: Count occurrences of duplicated logic pattern before and after.
Pass: Duplication reduced to 1 occurrence. All tests pass.

## Reference

[Duplicated Code - Refactoring Guru](https://refactoring.guru/smells/duplicated-code)
