# Diagnostic Debt Analyzer

The vc.Ifx Roslyn analyzer package tracks compiler and code-analysis diagnostic debt in source code.

## Purpose

The analyzer finds references to:

- C# compiler diagnostics in the form `CS####`
- .NET code-analysis diagnostics in the form `CA####`

It does not replace the compiler or Microsoft code-analysis packages. Those tools still produce the actual `CS####` and `CA####` diagnostics. This analyzer makes diagnostic references and suppressions auditable.

## Diagnostics

| ID | Severity | Meaning |
| --- | --- | --- |
| `IFX1000` | Info | A comment references `CA####` or `CS####` without a disposition marker. |
| `IFX1001` | Warning | A `#pragma warning disable` suppression references `CA####` or `CS####` without justification. |

## Accepted Disposition Markers

Use one of these markers in the same comment or pragma line:

- `Justification:`
- `Tracked-by:`
- `Fix-by:`
- `Intentional:`

## Examples

Needs disposition:

```csharp
// CA1822
```

Accepted:

```csharp
// CA1822 Tracked-by: issue-123
```

Needs justification:

```csharp
#pragma warning disable CA1822
```

Accepted:

```csharp
#pragma warning disable CA1822 // Justification: required by serializer contract.
```

## Code Fixes

The code-fix package provides generic documentation fixes:

- `IFX1000` adds `Tracked-by: TODO - record the issue, owner, or removal plan.`
- `IFX1001` adds `Justification: TODO - explain why this diagnostic is intentionally suppressed.`

The fix is intentionally conservative. It creates a visible review point instead of suppressing or changing behavior silently.

## Packaging Guidance

Reference `vc.Ifx.Analyzers` as an analyzer package. Reference `vc.Ifx.CodeFixes` only in environments that load code-fix providers, such as IDE tooling or a future VSIX/tooling package.
