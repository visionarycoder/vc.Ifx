# Fresh Build Identity

`Invoke-FrameworkTests.ps1 -BuildOnly -ReportBuildDirectory <fresh-directory>`
forces non-incremental compilation and creates `build-identity.json` only after
the build succeeds. `build-complete.json` anchors its SHA256. Existing capture
directories cannot be reused. All operations use the shared repository mutex.

## Captured Contract

Schema version 1 records the repository/revision, configuration, canonical test
selection, and each compiled library/test project's `name`, `projectPath`,
`targetPath`, `isTestProject`, `settings`, `compilePaths`, `nonCodeInputPaths`, `projectReferences`,
`absentInputs`, `inputs` and `outputs`. File entries contain absolute `path` and
SHA256 `sha256`. This is machine-local evidence, not a portable signed attestation.

Inputs include handwritten/generated compile files, compiler reference assemblies,
analyzers, additional files, embedded resources, protobuf definitions, restore assets, MSBuild-reported imports, effective
Directory.Build props/targets, generated NuGet props, central packages, global.json
and analyzer configurations. Missing optional configurations are recorded as absent.
The settings query runs the SDK's implicit-constant target, so comparison uses
the same stable C#14/net10 constants as compilation. Declared project references
are captured before the SDK adds transitive references.

Outputs include every file under each library/test TargetDir: DLL/PDB, dependency
and runtime configuration files, satellite and native runtime assets. Copied
framework DLL/PDB files must match the freshly compiled project payload. Added,
removed or changed output files fail verification. Benchmark and reporting-host
projects are outside this capture; no historical BenchmarkDotNet output tree is
walked. The no-output docs project imports no capture targets.

## Consumption

Every `-NoBuild` test/coverage run requires `IFX_REPORT_BUILD` pointing to its
fresh capture. Verification checks hashes, required projects, revision,
configuration, source/resource/protobuf/reference inventories, selection and evaluated SDK/compiler
settings before instrumentation. Coverage rechecks before each test project and
after Coverlet restores the assemblies. It retains the exact
`consumed-build-identity.json` and `build-identity-verified.json` hash/restore marker.
Failed or aborted runs cannot produce the successful restoration marker.

`Assert-IfxBuildProvenance -BuildDirectory <build> -CoverageRunDirectory <run>`
returns `{path, sha256, manifest}` for reporting and Jason's package-artifact
manifest consumer. Full reporting requires this identity and an unfiltered full
solution run. Bound reporting runs the already-built reporting host with
`--no-build`; build it before capture, or include it in the authoritative solution
build. Packing must use explicit `--no-build --no-restore` and recheck the identity;
environment-only restore flags are not sufficient.

```powershell
# Orchestrator only: run infrastructure self-tests before this fresh build.
./scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project vc.Ifx.slnx `
  -Configuration Release -WarningsAsErrors -ReportBuildDirectory $build
$env:IFX_REPORT_BUILD = $build
./scripts/Invoke-FrameworkTests.ps1 -FullCoverage -NoBuild `
  -Configuration Release -WarningsAsErrors
```

Scoped captures use exactly the same source/package selection for build and test.
They do not prove full integration. A prior build without these identities cannot
be retroactively attested, even if its tests and coverage passed.

## Verification And Limits

`tests/infrastructure/provenance/Test-BuildProvenance.ps1` creates an isolated real
SDK project with zero project references and a handwritten record. Its 29 checks
cover changed/missing DLL and PDB, source/props/project/assets mutations, added
source/output/config, changed constants, wrong configuration/selection, uncaptured
test projects, missing/tampered identities, consumed-identity mismatches, missing
consumed evidence, restoration markers and successful restoration of all fixtures.
Six checks reject changed, missing and newly globbed embedded JSON/protobuf inputs.
The fixture compiles the embedded resource; its protobuf item tests capture without
a gRPC dependency. The full solution capture additionally records the real gRPC input.
The tests never mutate runtime-package files. All 29 checks passed at
`TestResults/provenance-regressions/189c7b1d41ea49ed813f7438c839cabf/results.json`.
The prior implementation failed the embedded-schema mutation regression at
`TestResults/provenance-regressions/8a9b373174674abaa022ec87df091d31`.
The real wrapper and identity modules also passed all seven isolated package
regressions at `TestResults/package-wrapper-tests/f7643fb54a274fcaa2bd1b2591426706`;
native tools and the independently tested archive-layout validator are stubbed there.

Final local capture command:

```powershell
$build = Join-Path $PWD 'TestResults/provenance/ef-build-05'
./scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Configuration Release `
  -TestSourceScope 'Filtering/EntityFrameworkCore;Infrastructure' `
  -TestPackage vc.Ifx.Filtering.EntityFrameworkCore `
  -ReportBuildDirectory $build -WarningsAsErrors
$env:IFX_REPORT_BUILD = $build
./scripts/Invoke-FrameworkTests.ps1 -Configuration Release -NoBuild `
  -TestSourceScope 'Filtering/EntityFrameworkCore;Infrastructure' `
  -TestPackage vc.Ifx.Filtering.EntityFrameworkCore `
  -CoveragePackage vc.Ifx.Filtering.EntityFrameworkCore -WarningsAsErrors
./scripts/reporting/Invoke-FrameworkReport.ps1 -BuildDirectory $build `
  -CoverageRunDirectory (Join-Path $PWD 'TestResults/coverage/vc.Ifx.Filtering.EntityFrameworkCore/490f245891254af7a2396eb852d1cee2') `
  -Revision (git rev-parse HEAD) -Package vc.Ifx.Filtering.EntityFrameworkCore `
  -Configuration Release
```

Build passed with zero warnings; 5 tests passed with no failures/skips, strict
9/9 lines and 4/4 branches. The bound report passed with zero issues. These are
scoped results, not the final global checkpoint. Existing coverage infrastructure
regressions also passed 48/48 at
`TestResults/coverage-infrastructure/f64bcdc1a082410fbcd745e75d83744b`.
The Orchestrator subsequently ran reporting process compatibility (three process
checks plus the underlying reporting checks) before the authoritative global
capture. [Final local verification](../planning/local-verification-20260910.md)
records the fresh build, all 28 coverage gates, bound report and tested archives.

This guards accidental stale/replaced inputs and cooperating concurrent workers;
it is not a trust boundary against an actor who can rewrite all manifests and
files, or an unrelated process that ignores the mutex. Newly introduced custom
build-input mechanisms must extend capture. Global Release build, 28-module
coverage, report and archive verification passed at that separate final checkpoint.
Hosted execution and IDE behavior are not claimed by these checks.
