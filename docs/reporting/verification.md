---
title: Reporting Verification: 2026-09-09
doc_type: report
status: active
last_updated: 2026-09-10
---
# Reporting Verification: 2026-09-09

## Current Status

Gate 5 and all 28 libraries are locally accepted as of 2026-09-10. The
[final local checkpoint](../planning/local-verification-20260910.md) supersedes
initial handoff status and records complete solution, suite, coverage, reporting,
and package acceptance. Hosted/IDE checks and publishing configuration remain open.

## Historical Worker Record

The sections below preserve the initial 2026-09-09 scoped implementation evidence,
not current repository-wide counts or outstanding agent assignments.

## Proven Results

- `pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Roslyn.Reporting -TestSourceScope Reporting -TestPackage vc.Ifx.Roslyn.Reporting -WarningsAsErrors`
  passed 9/9 tests, no skips, strict 229/229 lines and 244/244 branches (100%).
  Evidence: `TestResults/coverage/vc.Ifx.Roslyn.Reporting/83965187fcd14149931c73851dda53c3/summary.json`.
- `pwsh -NoProfile -File tests/infrastructure/reporting/Test-ReportingInfrastructure.ps1`
  passed 8 host checks. Synthetic fixtures: `TestResults/reporting-host-tests/570036ba64eb454eb54834279103f1fd/`.
- `pwsh -NoProfile -File tests/infrastructure/coverage/Test-CoverageInfrastructure.ps1`
  passed 15 checks, including dynamic 28-package references and scoped/full isolation.
  Evidence: `TestResults/coverage-infrastructure/1290e240d0eb405a9fdb043eae0dea3a/`.
- A mutex build of the reporting package with `-ReportBuildDirectory` produced
  actual compiler SARIF, Compile fingerprints, symbols and build-complete metadata,
  with zero warnings/errors. The script host produced a passed scoped report from
  this actual SARIF and strict current-run coverage:
  `TestResults/reporting/reporting-smoke-20260909-v3/report-v1/`.
  This is scoped evidence, not full-integration evidence.
- Mutex-wrapped `scripts/packaging/Validate-Packages.ps1 -PackageId vc.Ifx.Roslyn.Reporting -Pack -SkipDocsCheck -PackageDirectory TestResults/reporting-pack-final`
  validated the 1.0.0 package/symbol pair and 6 existing non-packable test/benchmark
  projects. The first attempt correctly rejected leaked build-analyzer dependencies;
  an explicit private Microsoft.CodeAnalysis.Analyzers reference fixed that leak.
  No warnings were suppressed. Docs checks and all-package repack were not rerun.
- New library evaluates net10.0/C#14.0/IsPackable=true; command host evaluates
  net10.0/C#14.0/IsPackable=false/GeneratePackageOnBuild=false. Evaluations used
  `dotnet msbuild ... -getProperty:... -m:1 -p:BuildInParallel=false`.
- Dependency audit passed for 34 source/test/benchmark projects. The separate
  non-packable command host is also listed in the solution and builds successfully.
- actionlint 1.7.12 accepted the authoritative workflow and expanded composite;
  PyYAML structural checks verified reporting order, exact full coverage, run
  binding, read-only PR quality, failure preservation and no allowed-failure flags.
  Four reporting/runner PowerShell scripts parsed; tracked-file whitespace checks
  passed. No full solution build, broad test rerun, hosted actions or publishing.

## Changed Files

- `src/vc.Ifx.Roslyn.Reporting/`: project, README, request/report records,
  ReportEngine and SourceMetrics.
- `tests/unit/vc.Ifx.UnitTests/Reporting/ReportEngineTests.cs`.
- `scripts/reporting/`: non-packable host project, Report.cs and PowerShell runner.
- `tests/infrastructure/reporting/Test-ReportingInfrastructure.ps1`.
- `docs/reporting/`: design contract, usage and this verification record.
- `Directory.Build.targets`: opt-in compiler SARIF and Compile fingerprints.
- `scripts/Invoke-FrameworkTests.ps1`: fresh reporting builds and revision metadata.
- `scripts/coverage/Invoke-CoverageCore.ps1`: revision/build binding and current-run CI handoff.
- `vc.Ifx.slnx`, both centralized test csprojs, coverage-infrastructure inventory,
  `tests/README.md`, `.infra/yaml/framework-quality/action.yml` and its README.
- Only Gate 5's claim/evidence block was updated in the shared plan.

## Explicit Shared Requests

- Centralized the generator's private Microsoft.AspNetCore.Routing 2.3.12 pin in
  `Directory.Packages.props`; removed only its local VersionOverride, retaining
  PrivateAssets=all, netstandard2.0 and analyzer-only bundling. Shared-wrapper
  `-BuildOnly -Project src/vc.Ifx.Generators/verification/PackageSmoke.proj -WarningsAsErrors`
  passed with zero warnings/errors. Fresh package/extracted consumer evidence:
  `TestResults/generator-package/f5fd4aaa8c2d47809d68c693141ad04e/`. The generator
  archive has zero public dependencies and zero lib/ref/runtimes assets; both
  Routing DLLs occur only under analyzers/dotnet/cs. Resolved Routing remains 2.3.12.
  Generator behavior/coverage was not rerun for this version-location-only change.
- Added root `AGENTS.md` with VBD, stable .NET/C#, no underscore-prefix, preservation,
  serialized verification and authoritative plan/claim rules. Existing shared
  language settings were preserved; newer explicit user decisions take precedence.
- Added central Polly 8.6.4 and direct Proxy/Pipeline references. Removed unused
  Polly.Caching.Memory, Polly.Caching.MemoryCache and Polly.Contrib.WaitAndRetry
  references/pins after scanning active source/tests/benchmarks, excluding the
  uncompiled Proxies prototype. Both restored graphs contain only Polly, Core,
  Extensions and RateLimiting 8.6.4. Pipeline builds warning-free; Proxy restore
  passed. Package behavior tests remain with their owners after ongoing edits.
- Removed only gRPC's project-wide CS8981 NoWarn after confirming generated files'
  own pragmas. Shared-wrapper gRPC build with `-WarningsAsErrors` passed with zero
  warnings/errors. The current gRPC README records the removed project-wide suppression;
  no owner relay remains outstanding for this historical change.

## Limitations

Metrics are the documented ordinary-method syntax policy, not semantic CFG/CA1502
or whole-language callable coverage. Original SARIF locations remain in retained
SARIF; the report summarizes diagnostics and metric facts. Generated-code exclusions
are explicit and do not remove handwritten records/async methods. Complete
28-package reporting passed in the later local checkpoint. Hosted Linux and
publishing configuration remain unverified external checks.
