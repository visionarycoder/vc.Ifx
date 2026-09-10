# Code fix support matrix

Owner: CodeFixes contract; Gate 3 reconciliation: Nash; Updated: 2026-09-10

Implemented bounded contract. Local implementation and catalog handoff are verified;
repository-wide final acceptance remains In-flight with Orchestrator.
This package consumes diagnostics; it never
re-emits CA/CS diagnostics. Shared IFX1000/1001 IDs, descriptors and DiagnosticId
property remain unchanged. The catalog and this matrix were reconciled together
under the explicit Gate 3 documentation handoff.

## Supported remediation design

| Diagnostic | Emitter | Provider | Fix All | Bounds / behavior risk | Test file |
| --- | --- | --- | --- | --- | --- |
| IFX1000 | vc.Ifx.Analyzers | DiagnosticDebtCodeFixProvider | Document/project/solution, deduplicated edits | Add an explicit TODO disposition scaffold inside an ordinary line/block comment; XML documentation requires manual editing. Never fabricate an issue, owner or decision; TODO remains diagnosed. Preserve comment/code boundaries, encoding and newlines. Low runtime risk. | DiagnosticDebtCodeFixTests.cs, DebtSafetyTests.cs |
| IFX1001 | vc.Ifx.Analyzers | DiagnosticDebtCodeFixProvider | Document/project/solution, deduplicated edits | Add TODO justification to the existing pragma or resolved SuppressMessage/UnconditionalSuppressMessage attribute. Do not introduce suppression, expand its scope, or claim it is justified. Low runtime risk. | DiagnosticDebtCodeFixTests.cs |
| IFX005 | vc.Ifx.Analyzers, opt-in legacy rule | Ifx005ControllerAttributeOrderCodeFixProvider | No | Single reviewed reorder of separate lists containing only framework Authorize, AllowAnonymous, Route, ApiController attributes; never remove authorization/scope metadata. No action across directives, documentation trivia, ambiguous lists or unresolved/lookalike attributes. Attribute metadata order changes, so this remains opt-in and single-diagnostic. | ActiveProviderTests.cs |
| CA1062 | Microsoft.CodeAnalysis.NetAnalyzers | Ca1062CodeFixProvider | No | Explicit opt-in null guard for the sole resolved reference parameter in an ordinary block-bodied public method. Reject async/iterator/ref/value/error/nullable-optional ambiguity and pre-existing guards; require framework ThrowIfNull API. Preserve statements and evaluation order. Null inputs now throw ArgumentNullException earlier; callers must review. | ActiveProviderTests.cs, CompilerEnvironmentTests.cs |

The CA1062 selection is based on the upstream [rule/fix inventory](https://raw.githubusercontent.com/dotnet/roslyn-analyzers/main/src/NetAnalyzers/Microsoft.CodeAnalysis.NetAnalyzers.md#ca1062-validate-arguments-of-public-methods),
which lists no code fix. Recheck upstream support before future release expansion.
The local integration test loads the actual analyzer from the SDK pinned in
`global.json`, verifies its CA1062 location, compiles the action result, and proves
the upstream diagnostic disappears. No surrogate CA1062 emitter is used for this check.
No CS fix is promised without a verified remediation gap and safe bounded design.

## Legacy audit decisions

Retired provider types remain constructible for compatibility but are not exported
or advertised as remediation. Their registration returns no actions, Fix All is
absent, and cancellation is honored. Regression tests enumerate every type and
ensure source remains unchanged. These withdrawals are deliberate safety changes,
not assertions that the old transformations were correct.

| ID | Emitter | Existing provider | Disposition / risk in former action | Test file |
| --- | --- | --- | --- | --- |
| CA1051 | NetAnalyzers | Ca1051CodeFixProvider | Retire field-to-property conversion: ref uses, layout, reflection and binary API shape can change. No blanket suppression. | LegacyProviderTests.cs |
| CA1303 | NetAnalyzers | Ca1303CodeFixProvider | Retire file suppression; application must select localization resources. | LegacyProviderTests.cs |
| CA1413 | Legacy FxCop | Ca1413CodeFixProvider | Retire file suppression; COM layout requires application review. | LegacyProviderTests.cs |
| CA1704 | Legacy FxCop | Ca1704CodeFixProvider | Retire file suppression; naming/dictionary policy is not inferred. | LegacyProviderTests.cs |
| CA1707 | NetAnalyzers | Ca1707CodeFixProvider | Retire file suppression; rename needs reference/API compatibility review. | LegacyProviderTests.cs |
| CA1801 | Deprecated NetAnalyzers/FxCop | Ca1801CodeFixProvider | Retire discard insertion; prefer upstream IDE0060 and explicit API review. | LegacyProviderTests.cs |
| CA1806 | NetAnalyzers | Ca1806CodeFixProvider | Retire discard insertion; masking an ignored HRESULT/result is not remediation. | LegacyProviderTests.cs |
| CA1812 | NetAnalyzers | Ca1812CodeFixProvider | Retire file suppression; reflection/DI activation requires application knowledge. | LegacyProviderTests.cs |
| CA1819 | NetAnalyzers | Ca1819CodeFixProvider | Retire file suppression; collection ownership/API decisions are not inferred. | LegacyProviderTests.cs |
| CA1822 | NetAnalyzers | Ca1822CodeFixProvider | Retire duplicate static conversion; use upstream fix, which can update references. | LegacyProviderTests.cs |
| CA1823 | NetAnalyzers | Ca1823CodeFixProvider | Retire duplicate field removal; old implementation lost initializer side effects. | LegacyProviderTests.cs |
| CA1824 | NetAnalyzers | Ca1824CodeFixProvider | Retire file suppression; actual resource language must be supplied by the application. | LegacyProviderTests.cs |
| CA1825 | NetAnalyzers | Ca1825CodeFixProvider | Retire duplicate Array.Empty fix; old syntax-only implementation accepted invalid array shapes. | LegacyProviderTests.cs |
| CA2207 | NetAnalyzers | Ca2207CodeFixProvider | Retire mismatched enum rewrite. Actual rule concerns struct static initialization; do not rename it to CA1008 and duplicate another upstream fix. | LegacyProviderTests.cs |
| MSTEST0017 | MSTest.Analyzers | MSTest0017CodeFixProvider | Retire duplicate swap. Old syntax-only matching accepted unrelated Assert types and could change argument evaluation order. | LegacyProviderTests.cs |

Upstream evidence: [CA1825 fixer source](https://source.dot.net/Microsoft.CodeAnalysis.NetAnalyzers/Microsoft.NetCore.Analyzers/Runtime/AvoidZeroLengthArrayAllocations.Fixer.cs.html),
[NetAnalyzers fix inventory](https://raw.githubusercontent.com/dotnet/roslyn-analyzers/main/src/NetAnalyzers/Microsoft.CodeAnalysis.NetAnalyzers.md),
[CA2207 rule](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2207),
and [MSTEST0017 built-in fix](https://learn.microsoft.com/dotnet/core/testing/mstest-analyzers/mstest0017).

## Deferred upstream areas

Unused/misordered usings, disposal, cancellation propagation, file-scoped namespaces
and analyzer release tracking remain with existing Roslyn/NetAnalyzers tooling.
XML documentation requires meaningful API content; no fabricated CS1591 prose.
Nullable fixes must not introduce null-forgiving operators or change public
annotations merely to silence warnings. Additional providers require a documented
upstream gap and separate semantic/cancellation/Fix All tests before export.

## Verification contract

Tests use an AdhocWorkspace and real C# compilation. Supported fixes must preserve
unrelated text/trivia and compile before/after in valid fixtures. Diagnostic-debt
tests run the stable analyzer before/after; placeholders must remain outstanding,
and repeated application must not append duplicate scaffolds. Fix All tests include
multiple IDs on one line, multiple documents/projects, equivalence keys and
cancellation. Unsupported/stale/foreign locations produce no action. Final coverage
must measure the entire vc.Ifx.CodeFixes DLL at 100% lines and branches, not only
the active providers. Global solution/package gates remain separately owned.

## Accepted Selection and Catalog Handoff

Orchestrator accepted this bounded selection on 2026-09-10. The reconciled
[diagnostic catalog](diagnostic-catalog.md) advertises only IFX1000/1001
scaffolds with document/project/solution Fix All, bounded IFX005 reorder with no
Fix All, and CA1062 reviewed single guard with no Fix All. The 15 legacy rows above
are retired/unexported, not automatic remediation. CA2207 is a withdrawn
mismatch, not an enum fix; never relabel it as CA1008. No new CA/CS diagnostics
are emitted. This matrix is authoritative for limitations. The plan's starter list
does not require every listed CA/CS area; the deferred areas above are accepted
scope decisions, not permission to restore unsafe providers.

## Package Composition

CodeFixes has no Analyzers project reference and does not bundle its DLL. Only the
private vc.Ifx.Roslyn helper accompanies the code-fix assembly. Install
vc.Ifx.Analyzers separately for IFX diagnostics; CA1062 remains host-owned.
The unit-test harness references Analyzers independently and is not evidence of a
transitive package dependency. No provider source or action changed in this fix.

Orchestrator accepted the package-owner evidence on 2026-09-10: 48 host checks,
19 validator checks, six compiler probes and two fresh-cache NuGet consumers passed
in run `b096e65d29104d539734b3e9247c8502`. CodeFixes-only composition does not
implicitly emit IFX; combined installation emits IFX1000 once. This is delegated
package evidence, not a new run by Gate 3 or proof of every IDE host.

## Local Verification Results

2026-09-09, frozen local handoff:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.CodeFixes -TestSourceScope Roslyn/CodeFixes -TestPackage vc.Ifx.CodeFixes -WarningsAsErrors
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project src/vc.Ifx.CodeFixes/vc.Ifx.CodeFixes.csproj -Configuration Release -WarningsAsErrors
```

- Strict gate passed: 96 tests, zero failures/skips, 194/194 lines and 176/176
  branches, full `vc.Ifx.CodeFixes` module at 100%; no new coverage exclusions.
- Coverage evidence: `TestResults/coverage/vc.Ifx.CodeFixes/519d738e3ea74577b777bfa81c31bf07/summary.json`
  and `vc.Ifx.UnitTests/coverage.opencover.xml`, `vc.Ifx.UnitTests/tests.trx` beneath it.
- Release package project build passed with zero warnings and zero errors.
  CS8602 nullable flow is corrected; no warning was disabled.
- Both commands use the shared mutex and `-m:1 -p:BuildInParallel=false`.
- CA1062 is exercised against SDK 10.0.401's actual NetAnalyzers implementation.
  C# 14 compilation accepts ordinary/empty/null/named suppression arguments; a
  trailing comma in an attribute argument list is rejected by the compiler and
  explicitly tested as no-action, not claimed as supported consumer syntax.
- Global solution/full-suite/combined-coverage and final NuGet refresh remain with
  the orchestrator/infrastructure. CA/CS areas lacking a verified safe upstream
  gap remain deferred as described above; module coverage does not close that scope.
