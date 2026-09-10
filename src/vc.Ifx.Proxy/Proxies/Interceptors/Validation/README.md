---
title: Validation Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Enforce input parameter constraints before invocation
tags:
  - validation
  - data-integrity
  - constraints
audience: developer
source_paths:
  - Validation/ValidationInterceptor.cs
---

# Validation Interceptor

Validates method arguments against .NET Data Annotations before method execution.

## Purpose

Fail fast on invalid input by validating parameters early in the interceptor chain using standard .NET Data Annotations.

## Files

| File | Purpose |
|---|---|
| `ValidationInterceptor.cs` | Validates method arguments against data annotations |

## How It Works

```
Method invoked with arguments
    ↓
ValidationInterceptor inspects all arguments
    ↓
Checks for [Required], [Range], [EmailAddress], etc.
    ↓
If any constraint violated: throw ValidationException
    ↓
If all valid: continue to next interceptor
```

## Integration

### 1. Define Constraints on Data Classes

```csharp
public class PaymentRequest
{
    [Required]
    public string TransactionId { get; set; }

    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    [EmailAddress]
    public string RecipientEmail { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
}
```

### 2. Register the Interceptor

```csharp
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();
```

### 3. Call the Method

```csharp
public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(PaymentRequest request);
}

var request = new PaymentRequest
{
    TransactionId = "tx123",
    Amount = -50,  // ← Invalid! Range violation
    RecipientEmail = "invalid-email",  // ← Invalid! EmailAddress violation
    Description = ""  // ← Valid
};

try
{
    var result = await paymentService.ProcessAsync(request);
}
catch (ValidationException ex)
{
    // ex.Message includes all validation errors:
    // - Amount: must be between 0.01 and 1000000
    // - RecipientEmail: must be a valid email address
}
```

## Supported Annotations

| Annotation | Purpose |
|---|---|
| `[Required]` | Field must not be null/empty |
| `[Range(min, max)]` | Numeric value within bounds |
| `[StringLength(max)]` | String length constraint |
| `[EmailAddress]` | Valid email format |
| `[RegularExpression(pattern)]` | Regex match |
| `[MinLength(n)]` | Minimum length |
| `[MaxLength(n)]` | Maximum length |
| Custom validators | Implement `ValidationAttribute` |

## Execution Order

Register `ValidationInterceptor` **early in the chain** (after authentication if required):

```csharp
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();  // 1st
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();      // 2nd
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();   // 3rd
services.AddScoped<IInvocationInterceptor, AuditInterceptor>();
```

## Related

- [Core Infrastructure](../Core/README.md) — MethodContext, validation context
- [Exception Handling](../Exceptions/README.md) — Handle ValidationException

## See Also

- [README.md](../README.md#data-integrity-3-interceptors) — Data integrity category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#validation) — Complete table of contents
// - RecipientEmail: invalid format
```

## Validation Attributes

Supported data annotations:

| Attribute | Validation |
|-----------|-----------|
| `[Required]` | Field is not null/empty |
| `[Range(min, max)]` | Value is within bounds |
| `[StringLength(n)]` | String length is ≤ n |
| `[EmailAddress]` | Valid email format |
| `[Url]` | Valid URL format |
| `[RegularExpression(pattern)]` | Matches regex |
| `[MinLength(n)]` | Collection has ≥ n items |
| `[MaxLength(n)]` | Collection has ≤ n items |
| Custom `[ValidationAttribute]` | Your own validation logic |

See [Built-in Data Annotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations) for complete reference.

## Registration

```csharp
// Register early in pipeline (fail fast on invalid input)
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();  // Early!
services.AddScoped<IInvocationInterceptor, AuthorizationInterceptor>();
```

**Why early?** Don't waste resources (authorization, expensive operations) on invalid input.

## Custom Validation

Create custom validation attributes:

```csharp
public class ValidCurrencyAttribute : ValidationAttribute
{
    private static readonly string[] ValidCurrencies = { "USD", "EUR", "GBP" };

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is string currency && ValidCurrencies.Contains(currency))
            return ValidationResult.Success;

        return new ValidationResult($"Currency {value} not supported");
    }
}

public class PaymentRequest
{
    [ValidCurrency]
    public string Currency { get; set; }
}
```

## Error Reporting

Validation errors are collected and returned as:

```csharp
public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

try
{
    await paymentService.ProcessAsync(invalidRequest);
}
catch (ValidationException ex)
{
    foreach (var (field, errors) in ex.Errors)
    {
        foreach (var error in errors)
        {
            Console.WriteLine($"{field}: {error}");
        }
    }
}
```

## Performance Considerations

Validation uses reflection to inspect data annotations:
- First invocation for a type is slower (attribute discovery)
- Subsequent invocations are fast (metadata cached)
- For high-throughput scenarios, consider:
  - Validating at API boundary (ASP.NET Core `[ApiController]`)
  - Caching validation metadata
  - Using simpler validation for warm paths

## Disabling Validation

Skip validation for a specific method:

```csharp
public interface ILowLevelService
{
    [SkipValidation]  // This method bypasses validation interceptor
    Task<object> UnsafeCallAsync(IntPtr data);
}
```

Skip validation globally (for testing):

```csharp
services.AddScoped<IInvocationInterceptor, NoOpInterceptor>();  // Skip validation in tests
```

## Further Reading

- [Data Annotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
- [USAGE.md](../USAGE.md) — Overall registration patterns
- [QUICK-START.md](../QUICK-START.md) — Basic example

