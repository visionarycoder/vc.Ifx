# vc.Ifx.Roslyn

Shared support library for vc.Ifx Roslyn analyzers, code fixes, and generators.

This project contains dependency-light constants and helpers that must be consistent across Roslyn packages. Runtime framework code should not depend on this package.

## Contents

- `DiagnosticIdentifiers` defines analyzer IDs emitted by vc.Ifx Roslyn components.
- `DiagnosticCategories` and `DiagnosticPropertyNames` define stable descriptor categories and diagnostic property keys.
- `DiagnosticDescriptors.All` is an immutable, ID-ordered catalog of IFX1000 (Info) and IFX1001 (Warning). Both rules are enabled by default.
- `DiagnosticIdPattern.Matcher` retains the original CA/CS-only matching behavior. `ReferenceMatcher` additionally recognizes four-digit IFX references and legacy IFX001 through IFX006, using ASCII digits. Neither matcher determines whether a diagnostic actually exists.

## Shared Diagnostic Contract

The [diagnostic catalog](https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md) defines ownership, severity, help links, current remediation availability, and consumer handoff requirements. CA/CS diagnostics remain Microsoft-owned. IFX1000/1001 describe diagnostic debt rather than re-emitting those upstream diagnostics.

```csharp
Diagnostic diagnostic = Diagnostic.Create(
    DiagnosticDescriptors.DiagnosticReferenceRequiresDisposition,
    location,
    ImmutableDictionary<string, string?>.Empty.Add(DiagnosticPropertyNames.DiagnosticId, "CA1822"),
    "CA1822");
```

The property value and message argument are the referenced ID; `diagnostic.Id` is IFX1000. The caller provides the source location covering the reference. Existing analyzer/code-fix consumers remain compatible and can adopt the shared descriptor/key on handoff. IFX matching requires explicit consumer adoption and corresponding scope tests.

## Compatibility and Verification

Targets `netstandard2.0` for Roslyn host compatibility with stable C# 14. The repository plan's earlier `preview` instruction is superseded for this project by the user's stable-language requirement. The only direct package dependency is `Microsoft.CodeAnalysis.Common`, which supplies types exposed in the public descriptors. There are no runtime-framework or workspace dependencies. The analyzer packaging owner must ensure the shared assembly and Roslyn host dependencies are available when loading tools.

Focused tests live under `tests/unit/vc.Ifx.UnitTests/Roslyn/SharedContracts`:

```powershell
dotnet test tests/unit/vc.Ifx.UnitTests/vc.Ifx.UnitTests.csproj --filter FullyQualifiedName~Roslyn.SharedContracts -m:1 -p:BuildInParallel=false -p:GeneratePackageOnBuild=false -p:CollectCoverage=true -p:IfxCoveragePackage=vc.Ifx.Roslyn
```

Coverage measures executable descriptors and matchers; constants are inlined by the compiler. There are no syntax/semantic helpers, DI registrations, asynchronous operations, or source-metric/report models in this package to test.

On 2026-09-09, all 42 shared-contract tests passed with 100% line coverage (22/22 sequence points) and 100% reported branch coverage (no branch points). Package build succeeded with one NU5104 warning from the centrally pinned prerelease build-analyzer dependency being included in the package. Final package/integration verification is handed to Orchestrator; solution-wide zero-warning and coverage gates remain open.

## Design Rules

- Keep this project small and stable.
- Do not place framework runtime abstractions here.
- Do not add workspace APIs unless both analyzers and code fixes need them.
