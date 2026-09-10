---
title: Packaging local milestone
doc_type: report
status: active
last_updated: 2026-09-10
---
# Packaging local milestone

Date: 2026-09-09. Branch: 2020-09-09. Gate 2 remains In-flight for the final
stable-source package snapshot and global quality gates. Shared metadata
corrections are implemented. No commits, pushes, publishing, or hosted settings
changes were performed.

## Historical 27-package checkpoint

- `pwsh -NoProfile -File scripts/packaging/Validate-Packages.ps1 -MetadataOnly`:
  passed for 27 source projects and 6 non-packable test/benchmark projects.
- `pwsh -NoProfile -File scripts/packaging/Validate-Packages.ps1 -Pack`:
  exit 0; built and inspected all 27 nupkg/snupkg pairs in
  `artifacts/packaging`. Every dotnet invocation used
  `-m:1 -p:BuildInParallel=false`. Explicit pack disabled automatic pack-on-build.
  Log: `artifacts/packaging/validation.log`.
- Refreshed Filtering, Secrets.Local, and WebApi with the same targeted pack
  options after concurrent package-owner README edits. A subsequent archive-only
  freshness check still detected concurrent README changes. All 27 passed at
  their serial pack checkpoint; the orchestrator must run a final pack/validation
  after package owners finish editing. This is not a frozen release snapshot.
- `pwsh -NoProfile -File scripts/packaging/Test-PackageValidation.ps1`:
  18 checks passed: 5 real packages and 13 intentionally broken archives.
- Docs restore/build/rebuild passed with no obj/bin/.no-output directories;
  all recursive items expose relative paths. Docs build: 0 warnings, 0 errors.
  This was standalone MSBuild verification, not automatic IDE solution visibility.
  The later [solution integration correction](documentation-project-integration.md)
  replaced manual solution File entries with a real SDK documentation project.
  Its 14 dedicated checks passed with automatic relative items and no documentation
  outputs, including warning-free mixed-solution Release verification. Actual IDE
  loading/file refresh remains explicitly unverified.
- Roslyn repack after adding an explicit private build-tool reference had no
  NU5104. Its nuspec exposes only Microsoft.CodeAnalysis.Common 5.9.0.
- Package builds recorded 1 CS1574, 56 IDE0161, and 11 RS2001 warning occurrences.
  These are source/documentation and analyzer release-tracking issues for owners;
  the local package run does not establish a zero-warning global build.
- Framework line/branch coverage was not measured by this packaging workstream.
  The 18 validator checks are not a claim of 100% framework or script coverage.

## Current shared settings and scope

The hard-coded `RepositoryBranch=main` has been removed; Source Link derives
repository information from Git. `Microsoft.CodeAnalysis.Analyzers` is centrally
pinned to stable `5.9.0`. The SDK/language baseline is `10.0.401` / C# `14.0`, not
preview. NU5104 was fixed through dependency privacy, not warning suppression.

There are now 28 source package projects, including `vc.Ifx.Roslyn.Reporting`.
Mutex-serialized `./scripts/packaging/Validate-Packages.ps1 -MetadataOnly
-SkipDocsCheck` passed for all 28 source projects and six non-packable
test/benchmark projects. Log: `TestResults/packaging-metadata-refresh-20260909.log`.
This refresh performs evaluation only: no package archive or docs rebuild was
produced. Separate `dotnet msbuild scripts/reporting/FrameworkReport.Host.csproj
-getProperty:IsPackable,GeneratePackageOnBuild -m:1 -p:BuildInParallel=false`
returned `false` for both properties. PowerShell parsing and project-discovery
regression checks passed, including Windows/Unix separators, six handwritten
projects retained and five generated benchmark projects excluded at this checkpoint.
Its independent package/symbol validation is recorded in
[reporting verification](../reporting/verification.md); it does not refresh the
historical 27-package archive set. The four benchmark executables and two test
projects remain non-packable. The separate reporting command host is also
non-packable and is not a 29th library. Generated benchmark projects under
`bin`/`obj` are excluded from repository project discovery, not source coverage.

The real benchmark scenarios, commands, and retained execution evidence are in
[the performance audit](performance-audit.md). Benchmarks do not establish
correctness coverage or release readiness.

## Remaining gates

The Orchestrator completed the final warning-free solution build, 3,632 tests,
all 28 strict coverage gates, bound report and tested 28-package archive snapshot.
[Final local verification](../planning/local-verification-20260910.md) records
exact commands and hashes, including the 19 archive-validator checks. Earlier
measurements in this document remain historical, not final-source proof.
Extracted compiler-host/MEF and fresh-cache consumers passed; hands-on IDE behavior,
hosted Linux execution/artifact transfer and repository/feed configuration remain
unverified external acceptance. No publication or remote settings change occurred.

## Changed files owned by Gate 2

Metadata-only edits (concurrent owners may also have independent edits here):

- `src/vc.Ifx/vc.Ifx.csproj`
- `src/vc.Ifx.Abstractions/vc.Ifx.Abstractions.csproj`
- `src/vc.Ifx.Analyzers/vc.Ifx.Analyzers.csproj`
- `src/vc.Ifx.CodeFixes/vc.Ifx.CodeFixes.csproj`
- `src/vc.Ifx.Data.Azure.Tables/vc.Ifx.Data.Azure.Tables.csproj`
- `src/vc.Ifx.Filtering/vc.Ifx.Filtering.csproj`
- `src/vc.Ifx.Filtering.EntityFrameworkCore/vc.Ifx.Filtering.EntityFrameworkCore.csproj`
- `src/vc.Ifx.Generators/vc.Ifx.Generators.csproj`
- `src/vc.Ifx.Generators.Abstractions/vc.Ifx.Generators.Abstractions.csproj`
- `src/vc.Ifx.Messaging.Azure.Queues/vc.Ifx.Messaging.Azure.Queues.csproj`
- `src/vc.Ifx.Observability/vc.Ifx.Observability.csproj`
- `src/vc.Ifx.Pipeline/vc.Ifx.Pipeline.csproj`
- `src/vc.Ifx.Pipeline.Grpc/vc.Ifx.Pipeline.Grpc.csproj`
- `src/vc.Ifx.Primitives/vc.Ifx.Primitives.csproj`
- `src/vc.Ifx.Proxy/vc.Ifx.Proxy.csproj`
- `src/vc.Ifx.Proxy.AspNetCore/vc.Ifx.Proxy.AspNetCore.csproj`
- `src/vc.Ifx.Proxy.Http/vc.Ifx.Proxy.Http.csproj`
- `src/vc.Ifx.Querying/vc.Ifx.Querying.csproj`
- `src/vc.Ifx.Roslyn/vc.Ifx.Roslyn.csproj`
- `src/vc.Ifx.Secrets.Abstractions/vc.Ifx.Secrets.Abstractions.csproj`
- `src/vc.Ifx.Secrets.Azure.KeyVault/vc.Ifx.Secrets.Azure.KeyVault.csproj`
- `src/vc.Ifx.Secrets.Local/vc.Ifx.Secrets.Local.csproj`
- `src/vc.Ifx.Storage.Abstractions/vc.Ifx.Storage.Abstractions.csproj`
- `src/vc.Ifx.Storage.Azure.Blobs/vc.Ifx.Storage.Azure.Blobs.csproj`
- `src/vc.Ifx.Storage.Ftp/vc.Ifx.Storage.Ftp.csproj`
- `src/vc.Ifx.Storage.Local/vc.Ifx.Storage.Local.csproj`
- `src/vc.Ifx.WebApi/vc.Ifx.WebApi.csproj`

New minimal mission README files; all existing README contents preserved:

- `src/vc.Ifx.Abstractions/README.md`
- `src/vc.Ifx.Data.Azure.Tables/README.md`
- `src/vc.Ifx.Filtering.EntityFrameworkCore/README.md`
- `src/vc.Ifx.Generators.Abstractions/README.md`
- `src/vc.Ifx.Messaging.Azure.Queues/README.md`
- `src/vc.Ifx.Observability/README.md`
- `src/vc.Ifx.Pipeline/README.md`
- `src/vc.Ifx.Pipeline.Grpc/README.md`
- `src/vc.Ifx.Primitives/README.md`
- `src/vc.Ifx.Proxy/README.md`
- `src/vc.Ifx.Proxy.AspNetCore/README.md`
- `src/vc.Ifx.Proxy.Http/README.md`
- `src/vc.Ifx.Secrets.Azure.KeyVault/README.md`
- `src/vc.Ifx.Secrets.Local/README.md`
- `src/vc.Ifx.Storage.Azure.Blobs/README.md`
- `src/vc.Ifx.Storage.Ftp/README.md`
- `src/vc.Ifx.Storage.Local/README.md`

Documentation and packaging validation:

- `docs/docs.csproj`
- `docs/planning/framework-upgrade-parallel-plan.md` (Gate 2 block only)
- `docs/packaging/README.md`
- `docs/packaging/coordination.md`
- `docs/packaging/performance-audit.md`
- `docs/packaging/verification.md`
- `scripts/packaging/PackageValidation.psm1`
- `scripts/packaging/Validate-Packages.ps1`
- `scripts/packaging/Test-PackageValidation.ps1`
