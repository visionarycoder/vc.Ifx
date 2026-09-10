# Developer Onboarding

## Prerequisites

- Git.
- PowerShell 7 (`pwsh`), required by the shared verification scripts.
- The stable .NET SDK selected by [global.json](../../global.json): 10.0.401,
  with stable patch roll-forward and no preview SDKs.
- Network access to the configured package feeds for dependency restore.
- Optional: an IDE that supports the pinned SDK and C# 14.0. CLI verification
  does not require an IDE.

Runtime libraries target .NET 10. The four compiler-host libraries retain
netstandard2.0 while using C# 14.0. Local framework tests do not require live Azure
accounts, FTP credentials, or permission to publish packages.

## Clone And Verify Tools

Run these commands in PowerShell:

```powershell
git clone https://github.com/visionarycoder/vc.Ifx.git
Set-Location vc.Ifx
dotnet --version
pwsh -NoProfile -Command '$PSVersionTable.PSVersion'
```

Read [AGENTS.md](../../AGENTS.md) and the
[parallel plan](../planning/framework-upgrade-parallel-plan.md) before claiming
work. Do not reclaim a completed workstream based on an older verification note.

## Build And Run All Tests

Coordinate full-suite runs with other agents. From the repository root:

```powershell
$build = Join-Path $PWD ('TestResults/reporting/' + [Guid]::NewGuid().ToString('N'))
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly `
  -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors -ReportBuildDirectory $build
if ($LASTEXITCODE -ne 0) { throw 'Solution build failed.' }
$env:IFX_REPORT_BUILD = $build
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -FullCoverage `
  -Configuration Release -NoBuild -WarningsAsErrors
if ($LASTEXITCODE -ne 0) { throw 'Full coverage failed.' }
```

The build restores dependencies and captures a fresh identity. The full run uses
those same binaries, executes both centralized test projects, and requires exact
100% line and branch coverage for every library. Stop on failure; do not continue
to reporting or packaging with failed evidence.

Retain the exact build directory and the coverage directory printed by the runner.
Do not edit captured inputs, rebuild, run output-mutating checks, or commit between
capture and consumption. A new Git revision invalidates the old capture even when
source text is unchanged. Repeat the sequence with a fresh directory after changes.

## Targeted Development

For a smaller feedback loop, run package tests through the same shared runner:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Filtering `
  -Filter 'FullyQualifiedName~Filtering' -Configuration Release -WarningsAsErrors
```

This retains all test references; it is package evidence, not full-suite acceptance.
The [test guide](../../tests/README.md) explains narrower source selection and its
reference constraints. Never insert targeted builds between a captured full build
and its coverage/report/package sequence.

## Reports, Packages, And Documentation

- [Reporting](../reporting/README.md) consumes the exact captured build and fresh
  coverage run; it does not discover the newest directory automatically.
- [CI and packaging](../../.infra/yaml/README.md) defines infrastructure checks,
  tested-binary packaging, manifest validation, and publishing rules.
- [Benchmarks](../../performance/README.md) run separately. Dry smoke runs prove
  execution, not representative performance.
- Open [docs.csproj](../docs.csproj) for documentation-only browsing. Files are
  automatically included in relative folders without producing obj/bin outputs.
  Hands-on IDE acceptance remains open; the CLI contract has been checked.

## Troubleshooting

If SDK resolution fails, compare the installed SDK with global.json. If another
agent holds the verification lock, coordinate rather than bypassing the wrapper.
For restore failures, inspect the configured feed and credentials; publish keys
are not needed for local tests. A coverage or provenance failure requires fresh,
matching evidence, not relaxed thresholds or reuse of historical results.

The [dated local checkpoint](../planning/local-verification-20260910.md) records
completed local acceptance and distinguishes remaining hosted/IDE checks.
