# vc.Ifx.Analyzers

Roslyn analyzers for vc.Ifx framework governance.

## Diagnostic Debt Analyzer

`DiagnosticDebtAnalyzer` finds diagnostic debt in C# source:

- `IFX1000` reports CA/CS/IFX diagnostic references in ordinary and XML documentation comments without a meaningful disposition.
- `IFX1001` reports explicit pragma suppressions and semantically resolved `SuppressMessage` / `UnconditionalSuppressMessage` attributes without meaningful justification.

Disposition markers are intentionally explicit:

- `Justification:`
- `Tracked-by:`
- `Fix-by:`
- `Intentional:`

Comments accept any listed marker with a nonempty value. Pragmas require `Justification:` in a trailing `//` comment; attributes require their constant `Justification` property. Empty values and values starting with TODO, TBD, or FIXME remain unresolved. Consequently, the existing code fix's TODO placeholder deliberately leaves debt visible until completed. Suppression pragmas inspect the disabled code list only, normalize numeric compiler codes (168 becomes CS0168), and ignore restore/inactive directives. Strings and unsupported IDs are not scanned.

Both analyzers skip generated source, respect standard diagnostic severity settings, support concurrent execution and cancellation, and perform no external I/O.

## Control-Flow Nesting

`MethodNestingAnalyzer` emits `IFX1100` when an ordinary method exceeds control-flow nesting depth 4 by default. The [diagnostic catalog](https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#ifx1100) defines the exact `ifx-control-nesting-v1` metric: nested if, switch, loops, and try statements increase depth; braces and nested function bodies do not. Else-if adds an if level. Constructors, accessors, and expression-bodied methods are outside this version's scope.

Diagnostics expose `MetricName`, `MetricVersion`, `MeasuredValue`, and `Threshold` string properties for reporting. The analyzer writes no report files and does not measure coverage. Cyclomatic complexity remains upstream CA1502; IFX1100 measures a distinct source nesting policy.

```editorconfig
[*.cs]
dotnet_code_quality.IFX1000.analyze_comments = true
dotnet_code_quality.IFX1001.analyze_suppressions = true
dotnet_code_quality.IFX1100.max_nesting_depth = 4
```

The boolean options default to true. The depth limit accepts invariant integers 0 through 64; invalid values fall back to 4. Use `dotnet_diagnostic.IFX1100.severity = none` to disable the rule. Full policy and upstream remediation ownership are documented in the catalog.

## Registered Scope and Validation

The exported analyzers are `DiagnosticDebtAnalyzer`, `MethodNestingAnalyzer`, and `Ifx001Signature`. Legacy VBD/SEC/CQ/framework helper rules retain their opt-in Initialize APIs; this update tests and repairs them without automatically activating upstream-overlapping rules. Tests lock the exact export set. Debt descriptor release metadata lives in the defining `vc.Ifx.Roslyn` assembly; analyzer-local metadata lives here. Targets remain netstandard2.0 with stable C# 14.

Legacy fixes include exact Task identities in proxy signatures, generic member/type usage normalization, source-order dependency traversal, compilation-reference support, correct nested namespace class counts, sibling-independent nesting, complete SQL concatenation operands, raw SQL interpolation detection and corrected security remediation text. Cancellation checks cover callback entry and growing semantic/dependency/report loops. The [catalog](https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy) documents each retained policy and its limitations; syntax-level security and immutability rules are not whole-program safety proofs.

Tests use Roslyn compilations with explicit references and per-tree options. They validate locations, messages, severity, generated-file exclusion, option fallbacks, stable concurrent results and cancellation before/during execution. Run through the repository wrapper to serialize compilation and coverage:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -Filter FullyQualifiedName~Roslyn.Analyzers
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Analyzers -Filter 'FullyQualifiedName~Roslyn.Analyzers|FullyQualifiedName~Roslyn.DiagnosticDebtAnalyzerTests|FullyQualifiedName~Roslyn.SharedContracts'
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project src/vc.Ifx.Analyzers/vc.Ifx.Analyzers.csproj -WarningsAsErrors
```

The strict report includes the entire analyzer assembly, including all retained legacy helpers and Ifx001Signature. No blanket rule removal or new coverage exclusion is used to reach the threshold.

Verification on 2026-09-09: 506 tests passed, including 46 original debt/shared-contract regression cases. Whole analyzer module coverage: 100% lines (2024/2024), 100% branches (1090/1090), 100% methods. Strict evidence: `TestResults/coverage/vc.Ifx.Analyzers/4803bd0e0a104e059b89cc2595043c1c/vc.Ifx.UnitTests/coverage.opencover.xml`. Serialized analyzer build passed with 0 warnings and 0 errors. Implementation ready; awaiting global integration. Orchestrator owns the final solution build, full suite, global coverage and packaging integration; this is not a full-solution result.

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
