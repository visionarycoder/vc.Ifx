# Framework Tests

The two centralized MSTest projects directly reference all 28 source projects. Unit
tests belong in package-specific folders under `unit/vc.Ifx.UnitTests`; integration
tests belong under `integration/vc.Ifx.IntegrationTests`. Shared deterministic
helpers should be introduced only when a real test needs them. This workstream adds
no speculative clock, network, or SDK abstractions.

## Serialized Commands

Run these commands from the repository root using PowerShell 7. The wrapper takes
a repository-scoped named mutex before any restore, build, test, or instrumentation.
All workers must use it: direct `dotnet` calls cannot honor the mutex. Never run a
whole-suite command without coordinating with the orchestrator first.

```powershell
# Targeted tests; rebuilds as needed, no coverage threshold.
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -Filter 'FullyQualifiedName~Primitives'

# Isolate compilation while other package test folders are being edited.
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -TestSourceScope Filtering -TestPackage vc.Ifx.Filtering -Filter 'FullyQualifiedName~Filtering'

# Targeted package coverage; exact 100% line AND branch enforcement.
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Primitives -Filter 'FullyQualifiedName~Primitives'

# Diagnostic measurement only; summary.passed remains false when below 100%.
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Primitives -Filter 'FullyQualifiedName~Primitives' -ReportOnly

# Coordinated baseline and combined unit + integration coverage.
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -FullCoverage -Configuration Release

# Serialized targeted build.
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project src/vc.Ifx.Primitives/vc.Ifx.Primitives.csproj

# Infrastructure regression tests; no package build or test execution.
pwsh -NoProfile -File tests/infrastructure/coverage/Test-CoverageInfrastructure.ps1
```

`-TestSourceScope` accepts one relative test folder such as `Filtering` or
`Secrets/Abstractions`, or a quoted semicolon-delimited set. Existing single-folder
commands remain valid. Each folder includes its descendants. Root `.cs` files can
be named literally for legacy aggregator tests; no root wildcard is supported.
Root Usings/GlobalUsings, AssemblyInfo (root or Properties), and Helpers are always
retained. For example:

```powershell
# Nested folders with a single compatible package reference.
./scripts/Invoke-FrameworkTests.ps1 -TestSourceScope 'Filtering/EntityFrameworkCore;Infrastructure' -TestPackage vc.Ifx.Filtering.EntityFrameworkCore

# Explicit root legacy files alongside a package folder.
./scripts/Invoke-FrameworkTests.ps1 -TestSourceScope 'Aggregator;ConstantsTests.cs;FrameworkConstantsTests.cs' -TestPackage vc.Ifx

# Scattered legacy folders need multiple package references; retain the full set.
./scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Proxy -TestSourceScope 'Proxy;Authentication;Authorization;Caching;Logging'
```

Selectors are literal, exact-case paths within the centralized test project.
Whitespace around entries is trimmed, separators normalized, duplicates removed,
and entries sorted ordinally. Empty entries, traversal, absolute paths, wildcards,
MSBuild expressions/escapes, `bin`/`obj`, missing paths and symbolic links (including
nested directory links) are rejected. File selectors are root `.cs` files only;
shared files remain included without an extra selector. No expression evaluation
or per-package target import is involved.

`-TestPackage` optionally
limits direct project references to one explicitly selected package; its transitive
references remain. Omit that option when the selected tests or shared helpers need
multiple packages. The runner never infers extra references from test namespaces.
In particular, Authentication tests need `vc.Ifx.Proxy.AspNetCore` and Logging tests
need the `vc.Ifx` aggregator: those five legacy folders cannot compile with only
`-TestPackage vc.Ifx.Proxy`. Owner test changes may still be required independently
of source selection. `-FullCoverage` forbids scopes, TestPackage and Filter; CI is
unchanged and uses all test sources/references with no test filter.

Scoped tests use separate `obj/scoped/<key>` and `bin/scoped/<key>` paths,
including separate restore assets. The key uses 128 bits of SHA-256 over the
versioned canonical selectors and reference selection; the full SHA-256 is retained
in evidence. Single folders retain their folder prefix with a selection hash below
it; combined/file selections use `multi-<hash>`. This requires one new scoped build
after upgrading the runner before using `-NoBuild`. Reordering/deduplication produces
the same key; changing selected references or sources produces a different key.
Normal runs retain all sources/references and the original unscoped output paths.
Every run writes `run-context.json` recording the canonical scopes, selection
version/fingerprint/output key, reference,
configuration, and filter. Scoped execution is package evidence only and cannot
establish full-suite integration. The compatibility coverage entry point accepts
the same scope options. No per-package Verification.targets files are required.
The shared parser is `scripts/testing/TestSourceSelection.psm1`; the wrapper passes
validated, escaped MSBuild item lists and never passes raw selectors as output paths.
Direct MSBuild single-folder evaluation remains compatible for infrastructure
checks; use the wrapper for execution and multi-source identity/validation.

`-NoBuild` is suitable only when the binaries were already built for the same
configuration and source revision. Fresh coverage output alone cannot prove fresh
binaries. The default rebuild is therefore preferred. Test/build commands use
`-m:1 -p:BuildInParallel=false` and disable automatic package generation; packaging
and the warning-free solution build remain separate gates.

The compatible coverage entry point is
`scripts/coverage/Invoke-Coverage.ps1 -Package <assembly> -Filter <filter>`; it uses
the same mutex. `Invoke-CoverageCore.ps1` is an implementation detail and must not
be called directly. A second coverage file lock protects against accidental direct
concurrent coverage calls. No command deletes the checkout or prior result folders.

## Evidence And Enforcement

Coverage runs write to `TestResults/coverage/<package-or-full>/<unique-run-id>/`.
Each test project contributes `tests.trx`, `coverage.json`, and
`coverage.opencover.xml`. The run root holds `summary.json` (schema version 1) and
`summary.md`. Ordinary tests write fresh TRX files under
`TestResults/tests/<test-project>/<unique-run-id>/`. Missing TRX or zero executed
tests fails the wrapper, including a filter that matches nothing.
Runs also enable VSTest's hang detector with a two-minute inactivity limit and no
memory dump; a stalled test fails the run and records a sequence artifact.

Full coverage discovers source projects, verifies source/test solution membership,
and verifies both test projects directly reference every source project. It runs
unit tests followed by integration tests, merging only the JSON from the current
run. A new package without a test reference fails before collection. Nothing in a
previous result folder is merged. Report generation consumes Coverlet JSON; the
matching OpenCover XML is provided for reporting integrations.

The final gate compares exact covered/total counts for each expected assembly.
Rounded percentages cannot pass. A package with no branches reports `0/0` and a
null branch percentage; all executable lines must still be covered. Missing
modules, empty reports, and modules without executable lines fail explicitly; a
metadata-only package needs a reviewed policy decision, not an invented 100%.
`-ReportOnly` permits measurement below the threshold but does not suppress test,
discovery, reference, or collection failures. It must never be used as a CI gate.

The wrapper sets Coverlet's interim threshold to zero only so the two test projects
can contribute to one report and every failing package can be reported. The final
validator enforces exact 100% per package. Direct opt-in MSBuild collection defaults
to `Threshold=100`, `ThresholdType=line,branch`, `ThresholdStat=minimum`, and is
supplemented by the wrapper's inventory and zero-test checks.

## Coverage Boundaries

Only repository assemblies (`[vc.Ifx*]*`) are instrumented; the two centralized
test assemblies are excluded by exact name. External dependencies are outside the
framework package coverage goal. Generated code is excluded only by
`GeneratedCodeAttribute` or the generated C# build output path `**/obj/**/*.cs`.
These cover gRPC build output, compiler assembly metadata, and emitted source
generator files. Test fixture strings are test code, not production assemblies.

There are no namespace, provider, private-member, method-name, or auto-property
exclusions. `SkipAutoProps=false`; `CompilerGeneratedAttribute` is deliberately
not excluded because it would remove handwritten records, lambdas, and async state
machines. Assemblies with missing source files are not silently discarded
(`ExcludeAssembliesWithoutSources=None`). Do not add `ExcludeFromCodeCoverage`
to handwritten production code to satisfy the gate. Any future exception needs an
explicit reviewed policy; no handwritten exceptions are introduced here.

## Configuration And CI Handoff

`Directory.Build.props` is imported before the project declares `IsTestProject`.
Test-only settings therefore live in `Directory.Build.targets`, after the project
body and Coverlet defaults. The projects reference `coverlet.msbuild` 6.0.4; the
collector package does not implement the MSBuild threshold workflow. The central
package file already supplies this coverage version. Its analyzer-tool pin has
separately moved from prerelease to stable Microsoft.CodeAnalysis.Analyzers 5.9.0.

The effective language version is C# 14.0, including the Roslyn family that retains
`netstandard2.0`. On 2026-09-09, `dotnet --version` resolved the installed stable SDK
10.0.401. The plan's shared standard follows the user requirement for current
stable C#, with SDK and language pins updated together. The targets assignment overrides legacy
project-local `preview` declarations without editing another worker's package.
See Microsoft's [language version table](https://learn.microsoft.com/dotnet/csharp/language-reference/language-versioning)
and Coverlet's [MSBuild integration](https://github.com/coverlet-coverage/coverlet/blob/v6.0.4/Documentation/MSBuildIntegration.md).

`global.json` selects SDK 10.0.401 with stable patch roll-forward only and rejects
preview SDKs. Deterministic compilation is explicit; CI source-path normalization
is enabled for GitHub Actions/Azure Pipelines. The hardcoded RepositoryBranch=main
was removed so the SDK derives package branch metadata from Git; explicit CI
property overrides remain supported. The stable analyzer tool exists on
[NuGet](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Analyzers/5.9.0).
Final restore/repack verification after this pin remains a coordinated handoff.

Gate 4 should run the infrastructure regression script followed by
`Invoke-FrameworkTests.ps1 -FullCoverage -Configuration Release` without
`-ReportOnly`, and publish `TestResults/**` even on failure. Restore/build warnings,
packaging, and dependency verification remain separate CI steps. This workstream
does not claim those gates, a passing full suite, or 100% coverage of unfinished
packages. See Gate 1's notes for the actual measured verification.

No-build runs now require a captured fresh build via `IFX_REPORT_BUILD`. See
[fresh build identity](../docs/testing/build-provenance.md) for DLL/PDB and input
verification, the capture/consume sequence, and the provenance regression command.

The Gate 4 implementation now lives in
[.infra/yaml/framework-quality/action.yml](../.infra/yaml/framework-quality/action.yml),
invoked by the authoritative
[GitHub workflow](../.github/workflows/publish.yml). CI passes
`-WarningsAsErrors` through the shared wrapper and uses unscoped full coverage
after a complete Release build. See [.infra/yaml/README.md](../.infra/yaml/README.md)
for event and publication rules and the remaining hosted/global verification.
