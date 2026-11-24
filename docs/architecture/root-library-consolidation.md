---
title: Ifx Project Ownership and Reusable Slices
doc_type: reference
status: active
last_updated: 2026-11-23
---

# Ifx Project Ownership and Reusable Slices

The repository now consolidates general, Azure, and Windows integrations into
the `Ifx` project, assembly, and package. This supersedes the earlier guidance
that assigned those provider families to separate projects and package IDs.

## Production packages

| Project and package | Target | Responsibility |
| --- | --- | --- |
| Ifx | net10.0; net10.0-windows | Runtime, general integrations, Azure integrations, and Windows MSMQ |
| Ifx.Roslyn | netstandard2.0 | Shared compiler contracts and architecture models |
| Ifx.Roslyn.Analyzers | netstandard2.0 | Compiler diagnostics |
| Ifx.Roslyn.Generators | netstandard2.0 | Source generation |
| Ifx.Roslyn.CodeFixes | netstandard2.0 | Workspace-based IDE code fixes |

The executable `src/Ifx.Roslyn.Reporting/Ifx.Roslyn.Reporting.csproj` targets
net10.0 and owns report-file I/O, command-line execution, and source metrics. It
is not a package.

The three integration source groups stay in their existing folders to preserve
capability ownership and make future slice extraction straightforward:

- `src/Ifx.Integrations`: EF Core, FTP, S3, Kafka, RabbitMQ, Redis, gRPC,
  SevenZip, ASP.NET Core hosting and health checks, OpenTelemetry, and shared
  interceptors.
- `src/Ifx.Integrations.Azure`: Azure configuration, identity, queues, Service
  Bus, Blob and Table storage, and Key Vault.
- `src/Ifx.Integrations.Windows`: MSMQ.

Those directories no longer contain projects. Their C# files are compiled into
`Ifx.dll`; public namespaces are retained. The `net10.0-windows` target alone
includes MSMQ source and the `Experimental.System.Messaging` dependency. The
cross-platform `net10.0` target omits both.

## Dependency and slice policy

The unified `Ifx` package includes the general integration and Azure provider
dependencies, plus the ASP.NET Core framework reference. This gives consumers of
the package a larger dependency graph than the former provider-specific packages.
Windows messaging remains isolated to the Windows target.

The source groups are capability boundaries, not build or package boundaries.
A later slice project should own its contracts, provider dependencies, and tests,
and should be extracted only when the reusable boundary and dependency contract
are explicit. Roslyn projects stay independent: compiler tooling does not enter
the runtime package, and netstandard compiler projects do not reference the
net10 runtime.

The core project compiles the marker attributes from
`src/Ifx.Roslyn.Generators/Attributes` and excludes that source from the
generator assembly to avoid duplicate public definitions. Reporting owns
physical report I/O. No integration source group may add a project reference
back to Ifx.

## Package migration

The shared package version is `0.4.0`. Use the `Ifx` package for code that
previously referenced `Ifx.Integrations`, `Ifx.Integrations.Azure`, or
`Ifx.Integrations.Windows`. Remove those three package references. The provider
namespaces stay the same, but types now reside in the `Ifx` assembly. There are
no assembly-forwarding shims; rebuild consumers and update assembly-qualified
type names or reflection configuration that names an old assembly.

The pre-1.0 version bump communicates this package-layout and dependency change.
Roslyn tooling package IDs remain separate. No source namespace migration is
required for the integration providers.

## Retained data boundaries

FilterSpec remains the serializable predicate contract for component boundaries.
QuerySpec composes data-access queries for provider translation. Executable
expression trees and `IQueryable` values do not cross those boundaries.
`Ifx.Filtering` owns predicate composition and portable filter nodes;
`Ifx.Querying.EntityFrameworkCore` supplies EF execution extensions.

## Validation

Run `python scripts/Test-ProjectHierarchy.py` to check the solution inventory,
project references, upward dependency rules, cycles, target framework policy,
and platform-specific MSMQ dependency placement. Run builds and tests through
`scripts/Invoke-FrameworkTests.ps1`.

The package quality workflow validates all five package and symbol pairs,
solution tests, package mutation and consumer checks, release-tag versions, and
the exact artifact inventory and hashes.
