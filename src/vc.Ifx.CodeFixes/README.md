# vc.Ifx.CodeFixes

Reviewed code actions for vc.Ifx analyzers and selected upstream diagnostics.
This Roslyn component targets `netstandard2.0` and C# 14. It consumes diagnostic
metadata; it never emits duplicate CA/CS diagnostics or infers application policy.

## Installation Contract

Install `vc.Ifx.Analyzers` separately alongside `vc.Ifx.CodeFixes` to receive IFX
diagnostics and their available fixes. CodeFixes alone exports the supported fixes
but does not emit IFX diagnostics or carry an implicit analyzer dependency.
CA1062 still comes from the host's Microsoft.CodeAnalysis.NetAnalyzers installation.

Only `vc.Ifx.CodeFixes.dll` and its private `vc.Ifx.Roslyn.dll` ship as compiler
extension binaries. The unused analyzer project reference and bundled analyzer DLL
were removed after package consumers demonstrated duplicate IFX1000 diagnostics
when both packages were installed. Existing CodeFixes-only users who relied on that
implicit analyzer must add the separate package. Provider APIs and actions are unchanged.

## Diagnostic Debt Fixes

`DiagnosticDebtCodeFixProvider` supports:

- `IFX1000`: inserts a `Tracked-by:` scaffold inside an ordinary line/block comment
  referencing a recognized CA, CS or IFX ID. XML documentation is left for manual editing.
- `IFX1001`: adds justification to an existing active disable pragma or a resolved
  framework `SuppressMessage`/`UnconditionalSuppressMessage` attribute. Attribute
  edits preserve argument syntax; no new suppression or wider scope is introduced.

The generated text uses `TODO` deliberately. A generic code fix cannot know whether a diagnostic should be fixed, tracked, or intentionally suppressed, so it creates a clear review point instead of silently hiding the issue.

## Fix-All

Diagnostic debt actions support document, project and solution scopes with stable
equivalence keys and deduplicated edits. Several IDs on one comment/pragma receive
one scaffold. Existing markers are not duplicated or silently replaced. Cancellation,
encoding and line endings are preserved.

## Reviewed Single Actions

- `CA1062`: explicitly insert `ArgumentNullException.ThrowIfNull` for the sole
  non-optional, non-nullable reference parameter of a public synchronous block-bodied
  method. Reject ref parameters, iterators, directives, existing guard-shaped code,
  unresolved parameter types and runtimes without the guard API. Null-input behavior
  changes deliberately, so no Fix All is offered. The SDK's actual analyzer is tested.
- `IFX005`: reorder separate single-attribute lists containing only resolved ASP.NET
  `Authorize`, `AllowAnonymous`, `Route` and `ApiController` attributes. Metadata order
  changes require review. Preserve list trivia; reject directives, documentation
  trivia, unknown attributes and combined lists. Never remove scope/authorization.

## Legacy Compatibility

Fifteen legacy provider types remain constructible but are no longer exported and
offer no actions. Their old blanket suppressions, syntax-only API rewrites and
duplicate upstream fixes were unsafe. In particular, CA2207 concerns struct static
initialization, not enum zero values; the old enum rewrite is withdrawn, not relabeled.
Use upstream fixes where available and review domain-specific decisions manually.

The repository support matrix `docs/roslyn/code-fix-support-matrix.md` records every
provider, diagnostic source, retirement reason, risk and test boundary.

## Verification

Run the serialized whole-module gate from the repository root:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.CodeFixes -TestSourceScope Roslyn/CodeFixes -WarningsAsErrors
```

The scoped suite uses real C# compilations, stable debt diagnostics, SDK CA1062,
multi-document/project Fix All, no-action and cancellation cases. Scope results
are not a claim that the full repository or final packaged artifacts passed.
The unit-test project directly references Analyzers for the debt test harness; it
is not a CodeFixes production dependency. Source-only test selection intentionally
retains that explicit test reference instead of selecting only CodeFixes references.

Packaged installation/discovery regression:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project scripts/packaging/CompilerHostSmoke.proj -WarningsAsErrors
```
