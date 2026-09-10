---
title: Analyzer Remediation Patterns
doc_type: reference
status: active
last_updated: 2026-08-31
summary: Before-and-after remediation patterns for the five MSTest analyzers covered by test-modernization-controller.
target_audience: ai
related_docs:
  - ..\SKILL.md
tags:
  - mstest
  - analyzers
  - remediation
---
# Analyzer Remediation Patterns

This reference preserves the analyzer-fix patterns now managed by `test-modernization-controller` and extends them for controller-driven modernization work.

## Remediation Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent captures baseline warnings for the five supported analyzer IDs. | Run the scoped build filter. | Warning list exists. |
| 2 | Agent selects the smallest local edit that resolves the warning. | Review the planned change. | One warning family maps to one local fix. |
| 3 | Agent validates the touched project after the edit. | Build and run tests for scope. | Warning is removed and tests still pass. |
| 4 | Agent records any residue that requires a wider migration pass. | Review report notes. | Residual work is explicit. |

## MSTEST0017

| Item | Guidance |
|---|---|
| Defect | Outdated MSTest usage |
| Agent action | Apply the analyzer-approved MSTest pattern at the smallest scope |
| Guardrail | Do not suppress the warning |

### Before

```csharp
Assert.AreEqual(result.TotalPay, 125m);
```

### After

```csharp
Assert.AreEqual(expected: 125m, actual: result.TotalPay);
```

### Validation

- Run `dotnet build <target> /nologo`.
- Confirm zero `MSTEST0017` warnings remain in scope.

## MSTEST0042

| Item | Guidance |
|---|---|
| Defect | Non-compliant modern usage |
| Agent action | Refactor to the current MSTest pattern expected by the analyzer |
| Guardrail | Keep the expected and actual intent readable |

### Before

```csharp
[DataTestMethod]
[DataRow(7, 8, 15)]
[DataRow(7, 8, 15)]
public void Add_WithRows_ReturnsSum(int left, int right, int expected)
{
    Assert.AreEqual(expected, left + right);
}
```

### After

```csharp
[TestMethod]
[DataRow(7, 8, 15)]
[DataRow(3, 4, 7)]
public void Add_WithRows_ReturnsSum(int left, int right, int expected)
{
    Assert.AreEqual(expected: expected, actual: left + right);
}
```

### Validation

- Run `dotnet build <target> /nologo`.
- Confirm zero `MSTEST0042` warnings remain in scope.

## MSTEST0044

| Item | Guidance |
|---|---|
| Defect | Non-compliant test structure |
| Agent action | Replace the flagged construct with the compliant MSTest form |
| Guardrail | Keep test execution deterministic |

### Before

```csharp
[DataTestMethod]
[DataRow("night", 125)]
public void CalculateTotals_WithShift_ReturnsExpectedAmount(string shift, decimal expected)
{
    var result = CalculateTotals(shift);
    Assert.AreEqual(expected, result.TotalPay);
}
```

### After

```csharp
[TestMethod]
[DataRow("night", 125)]
public void CalculateTotals_WithShift_ReturnsExpectedAmount(string shift, decimal expected)
{
    var result = CalculateTotals(shift);
    Assert.AreEqual(expected: expected, actual: result.TotalPay);
}
```

### Validation

- Run `dotnet build <target> /nologo`.
- Confirm zero `MSTEST0044` warnings remain in scope.

## MSTEST0052

| Item | Guidance |
|---|---|
| Defect | Static `TestContext` storage |
| Agent action | Keep `TestContext` instance-scoped and pass it to helpers |
| Guardrail | Do not replace one global mutable state pattern with another |

### Before

```csharp
private static TestContext testContext = null!;

public TestContext TestContext
{
    get => testContext;
    set => testContext = value;
}
```

### After

```csharp
public TestContext TestContext { get; set; } = null!;

private void WriteDiagnostics()
{
    TestContext.WriteLine("diagnostics");
}
```

### Validation

- Run `rg "static\\s+TestContext" Tests AzureAPI src`.
- Confirm zero matches remain in the touched scope.

## MSTEST0065

| Item | Guidance |
|---|---|
| Defect | Obsolete `[DataTestMethod]` usage |
| Agent action | Replace `[DataTestMethod]` with `[TestMethod]` and keep scenario intent explicit |
| Guardrail | Keep scenario coverage equal or larger |

### Before

```csharp
[DataTestMethod]
[DataRow("weekend", 125)]
[DataRow("weekday", 100)]
public void CalculateTotals_WithShift_ReturnsExpectedAmount(string shift, decimal expected)
{
    var result = CalculateTotals(shift);
    Assert.AreEqual(expected, result.TotalPay);
}
```

### After

```csharp
[TestMethod]
[DataRow("weekend", 125)]
[DataRow("weekday", 100)]
public void CalculateTotals_WithShift_ReturnsExpectedAmount(string shift, decimal expected)
{
    var result = CalculateTotals(shift);
    Assert.AreEqual(expected: expected, actual: result.TotalPay);
}
```

### Validation

- Run `rg "\\[DataTestMethod\\]" Tests AzureAPI src`.
- Confirm zero matches remain in the touched scope.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Warning suppression replaces real remediation | Apply the code change that the analyzer expects |
| One edit addresses several concerns at once | Split the work by warning family when evidence becomes unclear |
| Scenario names become vague during data-test cleanup | Keep explicit scenario intent in method names and data rows |
| Shared helper fixes skip local validation | Build and test the touched project after each helper change |

## Testing Strategies

| Strategy | Use |
|---|---|
| Targeted project build | Use after each analyzer family fix |
| Touched-project test run | Use after source edits |
| Whole-scope test run | Use after modernization or multi-project helper edits |
| Residual warning scan | Use before closing the workflow |
