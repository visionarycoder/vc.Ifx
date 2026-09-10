---
title: Packaging coordination
doc_type: guide
status: active
last_updated: 2026-09-10
---
# Packaging coordination

Gate 2 owns package metadata only. Public contracts, target frameworks, shared props,
central dependency versions, test projects, and the solution remain with their owners.

## Infrastructure requests (2026-09-09)

- Evaluating the unit test project initially returned `IsPackable=false` but
  `GeneratePackageOnBuild=true`. Apply test/benchmark packaging defaults after
  project properties are available, preferably in Directory.Build.targets, with
  cross-platform path handling. Verify both flags for every test and benchmark.
- Remove the hard-coded `RepositoryBranch=main` from shared props. Packages built
  from branch `2020-09-09` must not claim main. Gate 2 removes the same override
  in the aggregator project. SDK-generated repository commit metadata is retained.
- Retain `PublishRepositoryUrl`, `EmbedUntrackedSources`, `IncludeSymbols=true`,
  and `SymbolPackageFormat=snupkg`. The .NET SDK already supplies Source Link.
- Gate 2 modifies no TargetFramework or LangVersion. Infrastructure owns the
  transition from preview to the current stable C# language version.
- Replace the central prerelease Microsoft.CodeAnalysis.Analyzers version
  `5.9.0-1.26328.17` with stable `5.9.0`. Roslyn's explicit private build-tool
  reference already removes the NU5104 public dependency warning without any
  suppression; stable tooling selection is still a shared-file change.

## Worker boundaries

Storage.Abstractions and Secrets.Abstractions README content belongs to their
claimed package workers. Existing source README files are preserved. Missing
README files in unassigned packages receive only a minimal mission statement.

Roslyn package metadata now exposes Microsoft.CodeAnalysis.Common as a NuGet
dependency because its public descriptors use Roslyn types. Compiler extensions
bundle their private vc.Ifx tooling dependencies beside their analyzer assembly
and suppress runtime NuGet dependency groups; they do not bundle host Roslyn DLLs.

No whole-solution build, test, or pack is scheduled by this worker. Targeted
package checks run serially with `-m:1 -p:BuildInParallel=false`.
