# vc.Ifx

[![Build & Test](https://github.com/visionarycoder/vc.Ifx/actions/workflows/publish.yml/badge.svg)](https://github.com/visionarycoder/vc.Ifx/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

vc.Ifx is a reusable .NET infrastructure framework. It supplies runtime contracts,
filtering and query composition, component proxy and interceptor pipelines, provider
integrations, and Roslyn generators, analyzers, and code fixes.

Applications select these mechanisms under their accepted system and detailed
designs. Ifx does not define application business behavior or choose component
boundaries. The Method and volatility-based decomposition govern those design
decisions through vc.Governance.

## Choose a package

The repository contains eight packable libraries, all on the initial **0.1.0**
development line.

| Package | Target | Contents |
| --- | --- | --- |
| [Ifx](src/ifx/README.md) | net10.0 | Core contracts and primitives; filtering and query composition; pipelines and proxies; BCL archiving; local storage and secrets; in-memory messaging |
| [Ifx.Integrations](src/Ifx.Integrations/README.md) | net10.0 | EF Core query execution, FTP, RabbitMQ, gRPC, SevenZip, ASP.NET Core hosting and Web API support, and general interceptors |
| [Ifx.Integrations.Azure](src/Ifx.Integrations.Azure/README.md) | net10.0 | Azure configuration, identity, queues, Service Bus, Blob/Table storage, and Key Vault integrations |
| [Ifx.Integrations.Windows](src/Ifx.Integrations.Windows/README.md) | net10.0-windows | Windows-specific MSMQ integration |
| [Ifx.Roslyn](src/Ifx.Roslyn/README.md) | netstandard2.0 | Shared compiler-tooling contracts and models |
| [Ifx.Roslyn.Analyzers](src/Ifx.Roslyn.Analyzers/README.md) | netstandard2.0 | Compiler diagnostics within the documented active and opt-in rule scopes |
| [Ifx.Roslyn.CodeFixes](src/Ifx.Roslyn.CodeFixes/README.md) | netstandard2.0 | IDE code fixes for the documented supported diagnostics |
| [Ifx.Roslyn.Generators](src/Ifx.Roslyn.Generators/README.md) | netstandard2.0 | Annotation-driven compile-time generation, including endpoints, interceptors, enumerations, and mappers |

Start with Ifx for portable runtime mechanisms. Add integrations when the accepted
design needs their providers. The core library does not reference EF Core, Azure,
Windows messaging, or the ASP.NET Core shared framework. General Integrations
requires the ASP.NET Core shared framework; Azure and Windows inherit that dependency.

Generator marker attributes are supplied by Ifx. Compiler-extension packages load
under the compiler or IDE host; runtime applications do not reference their
implementation projects. See [project ownership and migration](docs/architecture/root-library-consolidation.md)
for dependency direction, retained public namespaces, and replacement of older packages.

## Reuse in an application

Inspect existing APIs before adding infrastructure. Select packages, contracts,
provider configuration, proxy wiring, and lifetimes against the approved design.
Verify the selected mechanisms in the application's own contract, composition,
and integration tests.

- [Filtering](docs/filtering/filter-contract.md) represents portable predicates.
- [Query composition](docs/filtering/query-contract.md) defines provider-side query
  shape, ordering, projection, and paging.
- FilterSpec can cross a component boundary through its portable FilterNode
  representation. QuerySpec, IQueryable, and executable expression trees remain
  inside the data-access boundary.
- Proxy and interceptor capabilities require explicit application wiring. Installing
  a package does not prove that every component call traverses the approved chain.
- Compiler diagnostics and generated code supplement architectural review; they do
  not authenticate approvals or establish semantic design correctness.

### Local package feed

Packable projects share the root `.nupkgs/` output configured in
[Directory.Build.props](Directory.Build.props). The repository's
[NuGet.config](NuGet.config) registers it as `Ifx-Local`.

From an Ifx checkout that contains the validated packages, register the resolved
feed path for a consuming solution:

```powershell
$ifxFeed = (Resolve-Path .nupkgs).Path
dotnet nuget add source $ifxFeed --name Ifx-Local
```

Then add the selected package to the application's project, using version 0.1.0.
The local feed does not imply that the package exists on a public registry.
Public namespaces can differ from package names; use the package documentation
and current ownership map when updating references.

## Build and verify this repository

Use Git, PowerShell 7, Python 3.10 or later, and the stable .NET SDK selected by
[global.json](global.json): **10.0.401**, with stable patch roll-forward.
The shared language version is **C# 14.0**. Node.js 20 or later runs the
dependency-free documentation and artifact tools in [package.json](package.json).

Run from the repository root:

```powershell
python scripts/Test-ProjectHierarchy.py
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project scripts/packaging/CompilerHostSmoke.proj -Configuration Release -WarningsAsErrors
```

The shared runner serializes framework restore, build, and test operations.
The package smoke check verifies package/symbol pairs and isolated compiler and
IDE consumers. Coverage and full-evidence reporting are optional diagnostics;
historical coverage tooling retains limitations documented in its guides.

The solution contains **19 projects**: eight libraries, eight MSTest projects,
two BenchmarkDotNet projects, and the non-packable
[Ifx.Roslyn.Reporting executable](src/Ifx.Roslyn.Reporting/README.md).
Reporting owns physical report I/O and source metrics.

- [Onboarding](docs/onboarding/readme.md): prerequisites and verification commands.
- [Tests](tests/README.md): test ownership and current versus historical commands.
- [Benchmarks](performance/README.md): runner, measurement modes, and evidence.
- [Reporting](docs/reporting/README.md): executable and legacy evidence workflow.

## Repository responsibilities

| Repository or environment | Owns |
| --- | --- |
| [vc.Governance](https://github.com/visionarycoder/vc.Governance) | The Method, owner architecture constraints, roles, approval routes, lifecycle, and artifact contracts |
| [vc.Agentic](https://github.com/visionarycoder/vc.Agentic) | Reusable agent skills, supporting task resources, templates, and illustrative scaffolds |
| vc.Ifx | Reusable implementation mechanisms, executable tests, benchmarks, reporting tools, and package delivery |
| Consuming application | Business requirements, accepted system and detailed designs, selected mechanisms, implementation, and operational evidence |
| Selected execution platform | Worker isolation, identity, permissions, orchestration, and protected integration |

Ifx also contains repository-local instructions, prompts, and an older skill
collection under [.github](.github/). Those skills overlap with vc.Agentic and can
differ from its maintained versions. They are not a second source of Method or
approval authority. Follow [AGENTS.md](AGENTS.md), current ownership decisions,
and the consuming project's accepted inputs. The existing
[GitHub skill index](.github/skills-index.md) is a historical inventory.

## Verification evidence and publishing

The [framework plan](docs/planning/framework-upgrade-parallel-plan.md) records
dated implementation and verification checkpoints. Its 2026-11-08 checkpoint
reports 4,050 passing solution tests and eight validated 0.1.0 package/symbol
pairs, plus a clean external consumer of the local package. These are historical
results for that tested state, not a certification of later edits.

Earlier [local](docs/planning/local-verification-20260910.md) and
[hosted](docs/planning/external-acceptance-20260910.md) records describe the former
28-library layout. Their project counts and coverage totals do not describe the
current eight-library structure.

The [configured workflow](.github/workflows/publish.yml) runs quality checks for
pull requests, merge queues, manual runs, main pushes, and version tags.
Successful main pushes publish validated packages to GitHub Packages. Matching
stable `vX.Y.Z` tags publish to NuGet.org. A README update, local package, or
recorded test result does not establish that a remote publication succeeded.

## Development and documentation

Read [AGENTS.md](AGENTS.md) and the
[framework plan](docs/planning/framework-upgrade-parallel-plan.md) before changing
implementation. Preserve assigned work, the approved SDK and analyzer settings,
and current package ownership. Propose architectural changes through the applicable
design and review route.

- [Documentation index](docs/index.md)
- [Architecture reviews](docs/reviews/readme.md)
- [Architecture decisions](docs/adr/index.md)
- [Component starting point](templates/component/README.md)
- [License details](docs/LICENSE-INFO.md)

## License

Original repository material uses the [MIT License](LICENSE). Third-party material
retains its own licenses and notices.

Last checked against repository contents: 2026-11-09.
