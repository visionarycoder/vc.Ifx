# Local Integration Acceptance

Owner: Orchestrator; Updated: 2026-09-10.

This checkpoint covers the shared working tree on branch `2020-09-09`, based on
HEAD `0bd966ac3d6d42d7783101888eecbfc5554209a4`. The changes are not claimed
committed, pushed or published. Build-input and output hashes identify the tested
working-tree snapshot; the Git revision alone does not identify uncommitted edits.

## Current Results

- [x] Complete Release solution build: zero warnings and errors, 36 solution projects.
- [x] Unfiltered tests: 3,617 unit plus 15 integration passed; zero skipped or failed.
- [x] All 28 libraries: exact 9,408/9,408 lines and 5,332/5,332 branches covered.
- [x] Build inputs and restored outputs verified before/after coverage, including
  the embedded query schema and gRPC protobuf definition.
- [x] Paired schema-v1 report: passed, zero issues.
- [x] All 28 current NuGet package/symbol pairs verified against tested payloads.
- [x] Archive-layout regressions and final staged-manifest recheck.

Local acceptance is complete for all 28 libraries and Gates 1, 2, 3 and 5.
Gate 4 retains the external acceptance checklist below; local completion does
not claim that the entire release process or IDE verification has run.

## Validated Archives

The final run is `TestResults/package-artifacts/59468a485ff34d26884375e111912ac6`.
Its `validated/` directory contains exactly 28 version `1.0.0` package archives,
28 symbol archives, `global.json` and `package-manifest.json` (58 files total).
Every packaged DLL/PDB matches its path and hash in the tested project outputs.
The manifest binds the archives to the build identity and exact coverage summary.

Manifest SHA256:
`e6c944b8108abb43546b2e5187efa15c37cb354a9693178956b62346228931bc`.
Coverage-summary SHA256:
`39e00dac0043407b48d37cac7e349cc9d3e1c19964f01069453ad6d2705637c1`.

```powershell
pwsh -NoProfile -File scripts/packaging/Test-PackageValidation.ps1 `
  -PackageDirectory TestResults/package-artifacts/59468a485ff34d26884375e111912ac6/raw
Import-Module ./scripts/packaging/PackageArtifactManifest.psm1 -Force
Test-PackageArtifactManifest `
  -Directory TestResults/package-artifacts/59468a485ff34d26884375e111912ac6/validated `
  -Revision 0bd966ac3d6d42d7783101888eecbfc5554209a4 `
  -ManifestSha256 e6c944b8108abb43546b2e5187efa15c37cb354a9693178956b62346228931bc
```

All 19 validator checks passed (five actual packages and 14 rejected mutations).
Mutation fixtures remain under `raw/validator-tests-d24ef81f79034597ae0e51d04374a415`;
they are deliberately outside the exact validated upload inventory. Final manifest
verification passed. No NuGet feed was contacted for publication.

## Reproduction

Run from the repository root. Capture directories must be fresh. The coverage
runner prints a new run directory; use that exact directory for report and pack.
Do not reuse historical coverage, rebuild between coverage and pack, or run another
worker that changes captured inputs/outputs during this sequence.

```powershell
$build = 'TestResults/reporting/global-20260910-03'
$coverage = 'TestResults/coverage/full/d27e86fffbba406da1c533510c9948fb'
$revision = '0bd966ac3d6d42d7783101888eecbfc5554209a4'
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly `
  -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors `
  -ReportBuildDirectory $build
$env:IFX_REPORT_BUILD = [IO.Path]::GetFullPath($build)
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -FullCoverage `
  -Configuration Release -NoBuild -WarningsAsErrors
pwsh -NoProfile -File scripts/reporting/Invoke-FrameworkReport.ps1 `
  -BuildDirectory $build -CoverageRunDirectory $coverage `
  -Revision $revision -Configuration Release
pwsh -NoProfile -File scripts/packaging/Invoke-ValidatedPackageArtifacts.ps1 `
  -BuildDirectory $build -CoverageRunDirectory $coverage -Revision $revision
```

These are the exact recorded build/test/report/pack invocations, not commands to
overwrite this completed capture. Choose a new `$build`, current `$revision` and
newly printed `$coverage` when repeating them.

Build identity SHA256:
`d46c1830a4c7ae870a6b98af5d3850a9d0128aa059b1f7d5145a0fd7952dd9c1`.
Evidence: `build-complete.json`, `build-identity.json` and `report-v1/report.json`
under the build directory; both `tests.trx`, `summary.json`,
`consumed-build-identity.json` and `build-identity-verified.json` under the coverage
directory. No handwritten production coverage exclusions were introduced.

## Supporting Verification

- Build provenance: 29 real isolated SDK fixture checks passed, including a
  reproduced embedded-resource defect and six resource/protobuf regressions.
  Evidence `TestResults/provenance-regressions/189c7b1d41ea49ed813f7438c839cabf`;
  command `pwsh -NoProfile -File tests/infrastructure/provenance/Test-BuildProvenance.ps1`.
- Package artifact identity: 31 checks passed; wrapper: 12 checks passed, including
  culture-qualified paths and changed private DLL/PDB rejection. Real wrapper and
  identity modules execute; native tools/archive-layout validator are stubbed in
  wrapper fixtures. Exact commands and evidence: `../../tests/infrastructure/packaging/README.md`.
  Final wrapper rerun `be2f28fb4b8942acba6b5b84893a3306` also requires
  `BuildProjectReferences=false`; the real first all-package attempt
  `b9e8e1bc02b0445388f82933ca6bd4c3` stopped with NETSDK1085 before this correction.
  No reference build or restore is permitted during verified packaging.
- Dependency inventory: 36 projects, 28 libraries, no cycles/forbidden declared
  edges; `pwsh -NoProfile -File scripts/Test-FrameworkDependencies.ps1` passed.
  The same command with `-SelfTest` passed all 25 probes, evidence
  `TestResults/dependency-audit/58812f271952465d9734fc70a42a3945`.
- Coverage infrastructure: 48 checks; reporting process-exit regressions: three
  checks. Exact commands are in their scripts under `tests/infrastructure`.
- Compiler packages: 48 extracted-host checks, 19 archive checks, six compiler
  probes and two fresh-cache NuGet consumers passed; combined IFX1000 emitted
  once. Evidence `b096e65d29104d539734b3e9247c8502`, documented in
  `../../scripts/packaging/compiler-host/README.md`.
- Benchmarks: all 54 Dry cases passed on the integrated runtime source in
  `TestResults/benchmarks/dcba41ef8dca40d3b7cd9f484c8d472c` using
  `pwsh -NoProfile -File scripts/Invoke-FrameworkBenchmarks.ps1 -Job Dry`.
  Dry execution and the recorded Short smoke are not reliable performance baselines.
- Documentation project: 14 CLI checks passed, artifact
  `TestResults/docs-project/97a0183382a744cb9ffcb6d700d3678c`; the real solution
  contains the SDK documentation project, with no docs obj/bin/.no-output directories.

## External Acceptance

These checks remain open and do not become proven from local unit coverage:

- [ ] Run the quality workflow on GitHub's Linux runner and verify the actual
  upload/download and exact manifest checks on transferred artifacts.
- [ ] Configure/verify the required quality check and feed/token permissions;
  perform publication only when explicitly authorized.
- [ ] Open the real solution in development Visual Studio, verify recursive
  documentation add/remove refresh and no build/design-time outputs.
- [ ] Verify IDE analyzer/code-fix discovery and light-bulb/Fix All interaction
  in the supported IDE host. Older untested hosts remain documented compatibility limits.

No development Visual Studio installation is available here; no installation,
repository settings, credential changes or remote publication were performed.
Live cloud/FTP interoperability and representative performance measurements remain
documented environmental limits, not results established by SDK fakes or Dry runs.
