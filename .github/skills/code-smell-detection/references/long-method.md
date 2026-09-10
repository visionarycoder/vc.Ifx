---
title: Long Method Smell Reference
doc_type: reference
status: active
last_updated: 2026-08-30
parent_skill: code-smell-detection
---

# Long Method Smell

## Description

Method exceeds reasonable line count or cyclomatic complexity, making it hard to understand and maintain.

## Detection Signals

- Method >50 lines
- Cyclomatic complexity >10
- Multiple levels of nested loops or conditionals
- Comments explaining what blocks do (blocks need extraction)
- Excessive local variables

## Why This Is Problematic

- Difficult to understand what method does
- Hard to test all code paths
- Changes risky due to complex control flow
- Violates Single Responsibility Principle

## Remediation

**Extract Method (Compose Method pattern):**

```csharp
// ❌ BEFORE: Long method with multiple responsibilities
public async Task<Invoice> ProcessOrder(Order order)
{
    // Validate order (15 lines)
    if (order == null) throw new ArgumentNullException(nameof(order));
    if (!order.Items.Any()) throw new InvalidOperationException("No items");
    // ... more validation
    
    // Calculate totals (20 lines)
    var subtotal = 0m;
    foreach (var item in order.Items)
    {
        subtotal += item.Price * item.Quantity;
    }
    var tax = subtotal * GetTaxRate(order.ShippingAddress.State);
    var shipping = CalculateShipping(order.ShippingAddress, order.Items);
    var total = subtotal + tax + shipping;
    
    // Create invoice (15 lines)
    var invoice = new Invoice
    {
        OrderId = order.Id,
        CustomerId = order.CustomerId,
        // ... many properties
    };
    
    await _context.Invoices.AddAsync(invoice);
    await _context.SaveChangesAsync();
    
    return invoice;
}

// ✅ AFTER: Extracted into smaller focused methods
public async Task<Invoice> ProcessOrder(Order order)
{
    ValidateOrder(order);
    var totals = CalculateTotals(order);
    var invoice = CreateInvoice(order, totals);
    await SaveInvoice(invoice);
    return invoice;
}

private void ValidateOrder(Order order)
{
    if (order == null) throw new ArgumentNullException(nameof(order));
    if (!order.Items.Any()) throw new InvalidOperationException("No items");
    // ... validation logic
}

private OrderTotals CalculateTotals(Order order)
{
    var subtotal = order.Items.Sum(i => i.Price * i.Quantity);
    var tax = subtotal * GetTaxRate(order.ShippingAddress.State);
    var shipping = CalculateShipping(order.ShippingAddress, order.Items);
    return new OrderTotals(subtotal, tax, shipping);
}

private Invoice CreateInvoice(Order order, OrderTotals totals)
{
    return new Invoice
    {
        OrderId = order.Id,
        CustomerId = order.CustomerId,
        Subtotal = totals.Subtotal,
        Tax = totals.Tax,
        Shipping = totals.Shipping,
        Total = totals.Total
    };
}

private async Task SaveInvoice(Invoice invoice)
{
    await _context.Invoices.AddAsync(invoice);
    await _context.SaveChangesAsync();
}
```

## Single Level of Abstraction Principle

Each method operates at one level of abstraction:

```csharp
// ❌ BEFORE: Mixed abstraction levels
public void ProcessPayment(Payment payment)
{
    // High-level business logic
    ValidatePayment(payment);
    
    // Low-level HTTP details (wrong abstraction level)
    using var httpClient = new HttpClient();
    var request = new HttpRequestMessage(HttpMethod.Post, "https://payment-gateway.com/charge");
    request.Headers.Add("Authorization", $"Bearer {_apiKey}");
    var response = await httpClient.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<PaymentResult>(content);
    
    // High-level again
    UpdatePaymentStatus(payment, result);
}

// ✅ AFTER: Consistent abstraction level
public async Task ProcessPayment(Payment payment)
{
    ValidatePayment(payment);
    var result = await _paymentGateway.ChargeAsync(payment);
    UpdatePaymentStatus(payment, result);
}
```

## Verification

Test: Measure cyclomatic complexity and line count before/after.
Pass: Method <30 lines. Complexity <5. All tests pass.

## Reference

[Long Method - Refactoring Guru](https://refactoring.guru/smells/long-method)
