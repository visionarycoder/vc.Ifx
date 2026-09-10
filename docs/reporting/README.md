---
title: Framework Reporting
doc_type: readme
status: active
last_updated: 2026-09-10
---
# Framework Reporting

The [v1 contract](report-contract.md) is authoritative. The package is
`vc.Ifx.Roslyn.Reporting` (net10.0); the non-packable
`scripts/reporting/FrameworkReport.Host.csproj` is its command host. Both are in the
solution. The host is not a 29th NuGet package or an additional test project.

## Local Full Evidence

Run from the repository root in PowerShell 7. Run infrastructure checks before
capture, and coordinate with other agents so no captured inputs or outputs change
during this sequence.

```powershell
$build = Join-Path $PWD ('TestResults/reporting/' + [Guid]::NewGuid().ToString('N'))
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly `
  -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors -ReportBuildDirectory $build
if ($LASTEXITCODE -ne 0) { throw 'Solution build failed.' }
$env:IFX_REPORT_BUILD = $build
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -FullCoverage `
  -Configuration Release -NoBuild -WarningsAsErrors
if ($LASTEXITCODE -ne 0) { throw 'Full coverage failed.' }
$coverageRun = Read-Host 'Enter the exact fresh coverage directory printed by the runner'
$revision = git rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw 'Could not read the revision.' }
pwsh -NoProfile -File scripts/reporting/Invoke-FrameworkReport.ps1 `
  -BuildDirectory $build -CoverageRunDirectory $coverageRun `
  -Revision $revision -Configuration Release
if ($LASTEXITCODE -ne 0) { throw 'Report generation failed.' }
```

Output is `$build/report-v1/report.json` and `report.md`. Existing report outputs
are never overwritten. The fresh solution build includes the report host, so bound
reporting can use it without rebuilding captured outputs. Use the exact printed
coverage directory, not a latest-file search or a previous run.

The capture binds the Git revision, configuration, selected tests/references,
evaluated build inputs, and DLL/PDB hashes. Resource and protobuf inputs are
included. Changed inputs, changed outputs, or a new commit require a new capture.
See [build provenance](../testing/build-provenance.md) for the identity contract.
Historical source-only evidence does not establish current no-build provenance.

## Scoped Evidence And Limits

Targeted package coverage is supported by the
[test runner](../../tests/README.md). A scoped report requires matching build and
coverage revision, configuration, and canonical test/reference selection; specify
`-Package` for the selected package. Do not combine a package-only unscoped build
with a differently scoped test assembly and present them as matching evidence.
Scoped results cannot establish full-suite integration.

SARIF summaries preserve rule, severity, message, suppression, and metric facts;
full diagnostic locations remain in retained SARIF. Unknown/missing measurements
are null/Unknown, never fabricated percentages. Partial readable coverage writes
a failed report and exits nonzero. Malformed artifacts, mismatched provenance, and
changed sources are rejected before success. Synthetic host fixtures are marked
revision `fixture`, scope `scoped`; they are not package acceptance evidence.

## CI

The [authoritative workflow](../../.github/workflows/publish.yml) invokes the
[quality composite](../../.infra/yaml/framework-quality/action.yml). Infrastructure
checks precede a fresh full Release build, then unfiltered no-build full coverage,
reporting, and tested-binary packaging. `IFX_REPORT_BUILD` binds the captured build;
`IFX_COVERAGE_RUN` identifies the exact coverage run in CI. A child PowerShell
process does not export its environment variables back to a local parent shell,
which is why the local example asks for the printed coverage directory.

Report generation also runs after failed coverage when a run context exists,
without turning the failed quality gate green. Packaging and publication require
the preceding quality steps to succeed. TestResults retains JSON, Markdown, SARIF,
source manifests, build identity, and test evidence.

The [2026-09-10 checkpoint](../planning/local-verification-20260910.md) records a
passed full 28-package report and local integration acceptance. Required repository
checks, hosted execution, publishing permissions, and hands-on IDE acceptance
remain external checks; these scripts do not configure them.

## Verification

Run infrastructure checks before a build you intend to capture and consume:

```powershell
pwsh -NoProfile -File tests/infrastructure/reporting/Test-ReportingProcessExit.ps1 -Configuration Release
pwsh -NoProfile -File tests/infrastructure/coverage/Test-CoverageInfrastructure.ps1
```

Centralized tests under `tests/unit/vc.Ifx.UnitTests/Reporting` cover serialization,
determinism, exact thresholds, malformed/missing input, suppressions, cancellation,
metric definitions, preprocessor symbols, and generated-code boundaries. Host
checks cover execution and exit status, no overwrite, scope/revision guards,
stale sources, path confinement, and failed-report persistence. These checks do not
replace a full hosted run. [Historical verification](verification.md) preserves
the initial worker evidence separately from final local acceptance.
