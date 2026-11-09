---
title: Ifx Project Ownership and Compression
doc_type: reference
status: active
last_updated: 2026-11-08
---

# Ifx Project Ownership and Compression

The owner's final 2026-11-08 instructions establish eight production libraries
and one reporting executable. These assignments supersede earlier consolidation
counts, the five-library proposal, and the temporary combined Roslyn assembly.
Tests and benchmarks remain supporting projects.

All nine source projects now reside directly under src. The former src/ifx
container is removed as a grouping level; its Windows-locked directory is reused
for the core project at src/ifx/Ifx.csproj. Other projects are sibling folders,
including src/Ifx.Integrations and src/Ifx.Roslyn.Reporting. References preserve
actual directory casing for case-sensitive checkouts.

Benchmarks are grouped into performance/Ifx.Benchmarks for core behavior and
performance/Ifx.Integrations.Benchmarks for integration policies. Local storage
and pipeline dispatch remain core benchmarks. All existing cases are retained.

## Production libraries

| Project | Target | Responsibility |
| --- | --- | --- |
| Ifx | net10.0 | Runtime contracts, primitives, filters, query composition, pipelines, proxy pipeline, BCL archiving, local storage, local secrets, and in-memory channels |
| Ifx.Integrations | net10.0 | EF Core, FTP, RabbitMQ, gRPC, SevenZip, hosting, and general interceptors |
| Ifx.Integrations.Azure | net10.0 | Azure Blobs, Tables, Queues, Service Bus, Key Vault, identity, and App Configuration |
| Ifx.Integrations.Windows | net10.0-windows | MSMQ and its platform-specific dependency |
| Ifx.Roslyn | netstandard2.0 | Shared diagnostic contracts and architecture models |
| Ifx.Roslyn.Analyzers | netstandard2.0 | Compiler diagnostics |
| Ifx.Roslyn.Generators | netstandard2.0 | Source generation |
| Ifx.Roslyn.CodeFixes | netstandard2.0 | Workspace-based IDE code fixes |

The executable `src/Ifx.Roslyn.Reporting/Ifx.Roslyn.Reporting.csproj` targets net10.0
and owns report-file I/O, command-line execution, and source metrics. It is not
a production library package. Its reporting model retains the existing
Ifx.Roslyn.Reporting namespace.

## Dependency direction

Integrations references Ifx. Azure and Windows reference Integrations. Each Roslyn
child references the shared Roslyn library. Parents never reference child projects;
child Abstractions projects remain the sole permitted exception if later needed.
The runtime projects do not reference compiler tooling. Netstandard compiler
projects cannot reference the net10 runtime root.

Source folders and namespaces express capabilities without introducing projects
for every provider. Integrations uses Archiving, Messaging, Storage, Querying,
Secrets, Transport, Proxy, and Hosting folders. Azure uses Configuration, Messaging,
Storage, and Secrets. Windows uses Messaging/Msmq. No Modules container is used.

The root library contains no Azure SDK, EF, RabbitMQ, FTP, gRPC, JWT, Polly,
SharpCompress, Windows messaging, or ASP.NET Core framework reference. Its
Microsoft.Extensions dependencies provide configuration, logging, DI, caching,
and hosting abstractions for core runtime behavior. Marker-attribute source files
remain in Roslyn.Generators/Attributes and are linked into Ifx; the generator
assembly excludes them to avoid duplicate public definitions.

The general Integrations package includes the ASP.NET Core framework requirement.
Azure and Windows inherit that base dependency. Neither Ifx nor Integrations has
an Azure or Windows project/package dependency. Passive option names may retain
Azure terminology without referencing an SDK.

## Compiler and I/O boundaries

Analyzers and generators remain in separate projects as requested. They have no
Workspaces reference and consume compilation inputs and AdditionalFiles. Rule
RS1038 rejected the proposed assembly combining them with workspace-based code
fixes; no suppression was introduced. CodeFixes depends on shared Roslyn contracts,
not the analyzer or generator implementation assemblies. Reporting owns physical
file access. Compiler tooling performs no direct file-system operations.

Compiler-extension packages place assemblies in analyzers/dotnet/cs, with their
shared Ifx.Roslyn dependency beside them. Host-owned Roslyn compiler/workspace
assemblies are not bundled. The standalone shared Roslyn package exposes its
netstandard2.0 library and Microsoft.CodeAnalysis.Common dependency.

## Migration

The owner's subsequent namespace compression removes Abstractions folders and
namespace segments. Shared artifacts reside in the immediate owning parent.
The following namespace changes supersede the earlier preservation statement:

| Previous namespace | Owning parent namespace |
| --- | --- |
| Ifx.Abstractions | Ifx |
| Ifx.Pipeline.Abstractions | Ifx.Pipeline |
| Ifx.Proxy.Interceptor.Abstractions | Ifx.Proxy |
| Ifx.Filtering.Abstractions | Ifx.Filtering |
| vc.Ifx.FilterSpec.Abstractions | Ifx.Filtering |
| vc.Ifx.Messaging.Abstractions | vc.Ifx.Messaging |
| Ifx.Analyzers.Abstractions | Ifx.Roslyn |
| Ifx.Generators.Abstractions.Attributes | Ifx.Generators.Attributes |

General contracts move directly into Ifx; Component and Data retain their owning
subfolders. Storage contracts move into Storage. Shared Roslyn types move into the
Roslyn project root. The gRPC protocol file moves from Dispatch/Abstractions into
Dispatch without changing its wire definitions. Retained contract READMEs are
named Contracts.md where the parent already has a README.

Public namespaces outside the table above are retained. Assembly identities change for merged providers,
shared architecture models, analyzers, generators, code fixes, and reporting.
Rebuild consumers, replace old package references, and update assembly-qualified
type names. No binary-forwarding compatibility shims or publication are provided.

Select Azure and Windows packages explicitly. Use Ifx.Roslyn.Analyzers and
Ifx.Roslyn.Generators as compiler tooling, and Ifx.Roslyn.CodeFixes in an IDE host.
Runtime consumers obtain generator marker attributes from Ifx.

The previous shared StorageExtensions class was split into provider-owned static
classes; fluent builder methods remain extension methods. Existing namespaces are
preserved. Archived provider READMEs retain API guidance; previous project paths
and installation commands in those historical notes are superseded here.

## FilterSpec and QuerySpec

The owner's distinction remains: FilterSpec represents a serializable expression
contract across component boundaries; QuerySpec composes data-access queries that
an EF relational provider translates into SQL. Executable expressions and
IQueryable objects do not cross the boundary. Access components validate allowed
fields and operators before applying caller-supplied specifications.

Ifx.Filtering owns the shared predicate composition, portable FilterNode tree,
expression translation and hydration. FilterSpec<T>.ToFilterNode snapshots a
predicate for transmission; FromFilterNode hydrates it locally. QuerySpec owns
query shape and provider execution, including ordering, projection and paging.
It remains in the Access component and lets the relational provider generate SQL.

The owner confirmed no versioned release has been published and rejected
compatibility adapters. The duplicate vc.Ifx.FilterSpec types and adapters are
removed. Ifx.Filtering is the public predicate namespace, Ifx.Querying is the public
query namespace, and Ifx.Querying.EntityFrameworkCore supplies execution extensions.
QuerySpec is immutable and uses one internal QueryPlan for provider composition,
ordering, offset/cursor paging and eager-load hints. QueryPlan is not exported.
No expression tree or source-bound query is a boundary DTO. FilterGroupBuilder
belongs to Filtering; sort and page contracts belong to Querying. All production
packages use the shared initial SemVer version 0.1.0.

## Verification

Run `python scripts/Test-ProjectHierarchy.py` to check the adopted project set,
solution inclusion, reference existence, upward dependencies, cycles, and platform
dependency isolation. Run builds and tests through scripts/Invoke-FrameworkTests.ps1.

Run the serialized wrapper with `-BuildOnly -Project
scripts/packaging/CompilerHostSmoke.proj -Configuration Release -WarningsAsErrors`
to verify all eight package/symbol pairs and standalone compiler/workspace loading.
This does not assert Visual Studio UI acceptance, publication, or full coverage.

The owner's 2026-11-08 remediation removes obsolete test-project count and missing
documentation-project checks, mandatory exact 100% coverage, and mandatory report
provenance from release acceptance. The active release workflow retains builds,
all solution tests, dependency boundaries, package mutation and consumer checks,
release-tag version checks, and exact artifact inventory/hash verification.
Schema v2 release manifests describe revision, SDK and package hashes without
claiming coverage. Schema v1 verification remains available for historical evidence.
