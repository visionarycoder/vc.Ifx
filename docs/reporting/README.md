# Framework Reporting

The [v1 contract](report-contract.md) is authoritative. The package is
`vc.Ifx.Roslyn.Reporting` (net10.0); the non-packable
`scripts/reporting/FrameworkReport.Host.csproj` is its command host. Both are in the
solution. The host project is not a 29th NuGet package or an additional test project.
It permits normal `-m:1 -p:BuildInParallel=false` builds; SDK 10.0.401 rejects `-m:1`
for file-based application builds, so the host deliberately uses a project.

## Local Scoped Evidence

```powershell
$build = Join-Path $PWD "TestResults/reporting/$([Guid]::NewGuid().ToString('N'))"
./scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project src/vc.Ifx.Roslyn.Reporting/vc.Ifx.Roslyn.Reporting.csproj -ReportBuildDirectory $build -WarningsAsErrors
./scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Roslyn.Reporting -TestSourceScope Reporting -TestPackage vc.Ifx.Roslyn.Reporting -WarningsAsErrors
# Supply the exact fresh coverage directory printed by that invocation.
./scripts/reporting/Invoke-FrameworkReport.ps1 -BuildDirectory $build -CoverageRunDirectory $coverageRun -Package vc.Ifx.Roslyn.Reporting -Revision (git rev-parse HEAD) -Configuration Debug
```

Output is `$build/report-v1/report.json` and `report.md`. Existing report outputs
are never overwritten. The build wrapper requires a fresh directory and forces
compilation so an incremental skip cannot masquerade as a new SARIF collection.
Build-complete metadata binds revision/configuration; SHA-256 Compile manifests
detect changed source text. The library hashes the evidence it actually reads.
New build/coverage contexts also retain versioned canonical test selectors, the
selected direct reference, output key and full SHA-256 selection fingerprint.
The report script validates these fields against the original top-level scope
fields, recomputes the selection fingerprint/output key, and rejects scoped
selection metadata in full reports. Historical contexts without this additive
metadata retain the original scope guards. Compile source-content fingerprints and
the report-v1 source metric policy are unchanged; selection identity is not a claim
that `-NoBuild` binaries match later source edits.
SARIF diagnostic summaries preserve rule, severity, message, suppression and metric
facts; full original locations remain in the retained source SARIF artifact.

Unknown/missing measurements are null/Unknown, never fabricated percentages.
Partial readable coverage writes a failed report and exits nonzero. Malformed
artifacts, mismatched provenance or changed sources are rejected before success.
Synthetic host-test fixtures are marked revision `fixture`, scope `scoped`; they
are not package coverage or full-integration evidence.

## CI

The [authoritative workflow](../../.github/workflows/publish.yml) invokes the
[quality composite](../../.infra/yaml/framework-quality/action.yml). Its fresh
solution build captures each package's SARIF and evaluated Compile paths, source
hashes and preprocessor symbols. `IFX_REPORT_BUILD` binds the coverage run to that
build; `IFX_COVERAGE_RUN` identifies that exact run, without latest-file searches.
Only an unfiltered full run and matching revision/configuration/build binding can
produce a full report. Report generation also runs after failed coverage, when a
run context exists, without turning the failed quality gate green. Packaging and
publishing still require every preceding quality step to succeed.

TestResults artifacts retain JSON, Markdown, SARIF and source manifests. Required
checks, hosted execution and final integration remain with Orchestrator. No hosted
publishing, settings or deployment actions are performed by these scripts.

## Verification

```powershell
./tests/infrastructure/reporting/Test-ReportingInfrastructure.ps1
./tests/infrastructure/coverage/Test-CoverageInfrastructure.ps1
```

Centralized `tests/unit/vc.Ifx.UnitTests/Reporting` verifies serialization,
determinism, exact thresholds, malformed/missing input, suppressions, cancellation,
all four metric definitions, preprocessor symbols, generated exclusions and retained
handwritten records/async methods. Host checks verify real execution, no overwrite,
revision/package/full-scope guards, stale sources, coverage path confinement and
failed report persistence. These checks do not replace a full hosted run.
