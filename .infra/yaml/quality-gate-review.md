# Independent CI Quality-Gate Review

Date: 2026-09-10. Reviewer: Agent J. Remediation route: Orchestrator to Mendel.

## Disposition

Three findings: one P1 CI blocker and two P2 provenance/publication gaps. Gate 4
remains In-flight. No workflow, action, shared script, package source, settings,
deployment, publishing, commit or push changes were made by this audit. Only this
document and Gate 4's ownership/Notes line are reviewer-owned.

The inspected working tree includes uncommitted infrastructure changes. Its HEAD
was `0bd966ac3d6d42d7783101888eecbfc5554209a4`; that commit alone does not identify
the reviewed content. Selected content hashes are recorded below. The read-only
dependency audit passed for exactly 35 projects, including 28 source libraries,
two test projects, four benchmarks and one reporting host.

## Findings

### F1 - P1: Expected reporting failure makes the GitHub PowerShell step fail

Locations: `tests/infrastructure/reporting/Test-ReportingInfrastructure.ps1:71`,
`scripts/reporting/Invoke-FrameworkReport.ps1:81`, and
`.infra/yaml/framework-quality/action.yml:38`.

The last reporting self-test intentionally supplies empty coverage. The report
host returns native exit code 1, the report runner throws, and `Test-Invocation`
catches the expected failure and prints PASS. Subsequent JSON checks and
`Write-Host` do not clear `$LASTEXITCODE`. The script finishes successfully as a
PowerShell script but leaves the native status at 1. The GitHub `pwsh` step's
native-exit-code epilogue therefore fails the step before full coverage and pack.
This is not a failing product test; it is a successfully asserted negative test
leaking its expected native status into the CI step result.

Evidence: extracted the actual `Test-Invocation` function with the PowerShell AST
and invoked it in memory with a runner that executes `pwsh -Command 'exit 1'`
then throws the expected reporting exception. The assertion passed and the native
status remained 1. A child PowerShell process ending with the modeled GitHub
epilogue exited 1 while printing `Self-test assertions passed: 1`. No fixture files
or framework binaries were changed. This isolates the shell semantics; it is not
a claim that the complete hosted workflow or reporting self-test suite was run.

Requested fix: explicitly return a successful process/native status only after
all self-test assertions finish, without masking unexpected exceptions or failed
assertions. Alternatively isolate expected-failure subprocess status from the
outer runner. Add a process-level regression using the GitHub-equivalent `pwsh`
epilogue: successful negative-test suites exit 0; a deliberately broken assertion
exits nonzero. Actionlint and YAML shape checks cannot establish this behavior.

### F2 - P2: NoBuild provenance labels do not identify the reused assemblies

Locations: `scripts/Invoke-FrameworkTests.ps1:105`,
`scripts/coverage/Invoke-CoverageCore.ps1:60`,
`scripts/coverage/Invoke-CoverageCore.ps1:75`,
`scripts/reporting/Invoke-FrameworkReport.ps1:40`, and
`Directory.Build.targets:51`.

The build completion record captures revision, configuration and selection, but
no DLL/PDB or test-assembly identities. Coverage with `-NoBuild` writes the current
HEAD and the caller's `IFX_REPORT_BUILD` string into a new run record, then reuses
whatever binaries occupy the normal output directories. It does not first prove
that those files came from the referenced build. Reporting compares those labels
and the current library Compile-file hashes. It neither identifies the tested
binaries nor fingerprints test sources, project/imported build settings or assets.

Consequently replacing an output assembly/PDB, or changing test/build inputs such
as DefineConstants without changing the recorded library source text, is not
rejected by the provenance checks themselves. Old test binaries can still be
labeled as current-build evidence. The same commit ID is especially insufficient
for this uncommitted local upgrade. A mutex prevents simultaneous tools from
writing outputs; it does not bind sequential runs to the same bytes.

The CI sequence performs a fresh solution build first, which reduces exposure on
an untouched ephemeral checkout. This finding does not claim that the observed
local coverage was stale or that a stale binary was injected during this audit.
It identifies the missing enforcement behind the same-build claim, including the
shared local `NoBuild` path and later packaging that rebuilds after tests.

Requested fix: capture a build inventory covering both test assemblies and their
framework dependencies, DLL/PDB hashes and relevant build inputs/SDK/assets; check
it before NoBuild instrumentation and retain the exact consumed identity in the
coverage record. Bind packing to that tested inventory rather than silently
rebuilding an unattested snapshot. Add a regression that swaps a DLL/PDB or changes
test/build input under the same HEAD and requires rejection before testing/reporting.

### F3 - P2: Publishing selects archives outside the validated package set

Locations: `scripts/packaging/Validate-Packages.ps1:16`,
`scripts/packaging/Validate-Packages.ps1:59`,
`.github/workflows/publish.yml:60`, `.github/workflows/publish.yml:97`, and
`.github/workflows/publish.yml:111`.

The validator packs into a reusable `artifacts/packaging` directory and validates
one expected versioned archive per discovered source project plus its symbols.
It does not require a fresh destination, reject additional root archives, or emit
an exact validated-file manifest consumed downstream. Upload and both publish
loops instead enumerate every root `*.nupkg`/`*.snupkg`. Thus an extra package or
an old version left in that directory is outside project validation but inside
publication selection. On main, only a nonzero package count is checked. The tag
loop checks versions, not membership; an unexpected package with the matching
version is still not rejected as an unexpected identity.

The nested malformed-archive fixtures are correctly excluded by the root globs;
this finding concerns additional files at the root. A clean hosted checkout
normally starts without ignored artifacts, but the script contract neither
requires nor proves that condition. Current local root inventory still contained
27 historical archives during inspection, while the source inventory is now 28;
this observation is not a claim that `-Pack` would omit the new package.

Requested fix: use a new run-specific package directory and reconcile exactly the
28 expected package IDs/versions and symbol pairs, rejecting extra files. Emit
validated archive hashes and the bound build/revision in a manifest; upload and
publish only that manifest's exact set. Check package repository commit against
the intended revision as well as internal Source Link consistency. Add a regression
with a decoy root archive and an older version: neither may reach publication.

## Controls Verified In Source

- Trusted push plus successful `quality` dependency gates publishing; PR, merge
  queue and manual events do not publish. The publish job does not check out and
  execute repository scripts. The quality job has read-only repository permission.
- The workflow uses SHA-pinned actions and non-persisted checkout credentials.
  Hosted action availability and credential permissions were not queried.
- The authoritative full-coverage invocation has no package/source/name filter,
  ReportOnly switch or allowed failure. Full-coverage parameter sets reject scoped
  selectors. Exact per-package integer line/branch counts reject missing modules,
  empty coverage and rounded-but-incomplete percentages.
- Both centralized projects are checked before building; execution also requires
  nonzero TRX execution/pass counts and successful native test exit status. This
  is not independent proof of complete test-case discovery or absence of skips.
- Reporting requires matching revision/configuration/scope labels and current
  captured source hashes; missing coverage becomes failed/Unknown evidence, not a
  fabricated 100%. These useful controls do not close F2.
- Failure evidence upload uses `always()`. The reporting step can run after a
  failed coverage gate when a build and coverage-run path exist. Package upload
  and publish remain success-gated; the report step does not erase earlier failure.
- Stable release tags must match archive versions. Reused versions retain the
  documented `--skip-duplicate` behavior; this is not a new versioning finding.

## Verification And Limits

Executed read-only `pwsh -NoProfile -File scripts/Test-FrameworkDependencies.ps1`:
35 projects / 28 source libraries passed the static XML dependency/inventory gate.
Executed the in-memory F1 function/child-process probes described above. Inspected
both YAML files, coverage/test-selection runners, reporting capture/host runners,
package validator and its mutation checks, shared build metadata and prior evidence.
No framework builds, tests, coverage collection, packaging or shared self-test
suites were executed by this review. No new framework coverage percentage is claimed.

The earlier actionlint, 43 structural assertions, 15 coverage checks and later
expanded selection checks remain owner-reported evidence, not this review's proof
of workflow semantics. Linux, Visual Studio compiler-host loading and hosted action
execution/artifact transfer remain unverified. Local WSL inventory found Ubuntu,
but no Linux `pwsh` or `dotnet` executable; nothing was installed or reconfigured.
Windows-authored paths, case-sensitive filesystem behavior and Linux native loading
require actual Linux CI tests, not a regex-only path check. Required checks/rulesets,
fork approvals, secret/feed permissions and real package/symbol publication remain
Orchestrator-owned. No hosted requests were made.

## Reviewed Content Hashes

SHA256 at inspection; later owner edits require a targeted re-review.

| File | SHA256 |
| --- | --- |
| `.infra/yaml/framework-quality/action.yml` | `B21436FA18BC7F834D821914A0C5F170A481C196D15CC4FE1A462146631890DE` |
| `.github/workflows/publish.yml` | `845495154A8E72A0C82C97CEAFBBCAAEFE121A18652610AA5DC08DA7FD3B502A` |
| `scripts/Invoke-FrameworkTests.ps1` | `C6A6244E84AA79316C95698BEB4302529D6F6A7FD02467475EA3F2C30FEE1E2E` |
| `scripts/coverage/Invoke-CoverageCore.ps1` | `E50FAFEC61829DE693C96DD66A514E8837C3033A4F838C8D4DBB3113DF6FE6A4` |
| `scripts/reporting/Invoke-FrameworkReport.ps1` | `62707BBC0A8CF9CF3D7CC482B513866135244E4DB2C303DF6676FAC40F9E4215` |
| `tests/infrastructure/reporting/Test-ReportingInfrastructure.ps1` | `4AE85E331B5684CE82EDB398510362E7699078B83BA4029C5CFB21E068889BF6` |
| `scripts/packaging/Validate-Packages.ps1` | `6D4416344324DDEF7A5ED51BE152756D6FDAF45817D70711C38F4305A7DEAAE9` |
| `Directory.Build.targets` | `1213E2ADE2CB5E6DE5C09B75C740CE9B3A19E1872FFA5307F7AE4186AA1C0180` |
