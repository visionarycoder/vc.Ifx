# vc.Ifx.Analyzers

Roslyn analyzers for vc.Ifx framework governance.

## Diagnostic Debt Analyzer

`DiagnosticDebtAnalyzer` finds diagnostic debt in C# source:

- `IFX1000` reports comment references to `CA####` or `CS####` diagnostics when the comment does not include a disposition marker.
- `IFX1001` reports `#pragma warning disable` suppressions for `CA####` or `CS####` diagnostics when the suppression does not include a justification.

Disposition markers are intentionally explicit:

- `Justification:`
- `Tracked-by:`
- `Fix-by:`
- `Intentional:`

## Why It Does Not Emit CA#### or CS####

Roslyn analyzers cannot enumerate every compiler or analyzer diagnostic produced elsewhere in the same compilation and then re-emit them generically. The compiler and installed analyzers remain responsible for producing `CS####` and `CA####` diagnostics.

This analyzer governs diagnostic debt around those IDs: comments, suppressions, and auditability.

## Examples

```csharp
// CA1822
```

Produces `IFX1000`.

```csharp
// CA1822 Tracked-by: issue-123
```

Accepted.

```csharp
#pragma warning disable CA1822
```

Produces `IFX1001`.

```csharp
#pragma warning disable CA1822 // Justification: required by serializer contract.
```

Accepted.
