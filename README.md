# vc.Ifx

[![Build & Test](https://github.com/visionarycoder/vc.Ifx/actions/workflows/publish.yml/badge.svg)](https://github.com/visionarycoder/vc.Ifx/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

vc.Ifx is a .NET repository containing five packable framework libraries, a
reporting executable, automated tests, benchmarks, documentation, build and
release tooling, and repository-specific development resources.

The centrally configured package version is **0.4.0**. The projects target .NET 10, .NET 10 for Windows, or .NET Standard 2.0 as listed below.

## Packable libraries

| Package | Target framework | Contents |
| --- | --- | --- |
| [Ifx](src/ifx/README.md) | net10.0; net10.0-windows | Core runtime plus general, Azure, and Windows integration code; provider source guides are linked from the package README |
| [Ifx.Roslyn](src/Ifx.Roslyn/README.md) | netstandard2.0 | Shared compiler-tooling contracts and models |
| [Ifx.Roslyn.Analyzers](src/Ifx.Roslyn.Analyzers/README.md) | netstandard2.0 | Roslyn analyzers |
| [Ifx.Roslyn.CodeFixes](src/Ifx.Roslyn.CodeFixes/README.md) | netstandard2.0 | IDE code fixes |
| [Ifx.Roslyn.Generators](src/Ifx.Roslyn.Generators/README.md) | netstandard2.0 | Roslyn source generators for endpoints, interceptors, enumerations, and mappers |

The non-packable [Ifx.Roslyn.Reporting](src/Ifx.Roslyn.Reporting/README.md)
project is a command-line reporting executable. The solution also includes two
benchmark projects, one integration-test project, and seven unit-test projects.

## Repository contents

| Path | Contents |
| --- | --- |
| [src/](src/) | Five libraries and the reporting executable |
| [tests/unit/](tests/unit/) | MSTest projects for the runtime, integrations, archiving, and messaging |
| [tests/integration/](tests/integration/) | Integration tests |
| [tests/infrastructure/](tests/infrastructure/) | Validation and test-support resources for coverage, documentation, packaging, provenance, and reporting |
| [performance/](performance/) | BenchmarkDotNet projects |
| [docs/](docs/index.md) | Architecture and API documentation, ADRs, onboarding, packaging, planning, reviews, reporting, testing, and related guides |
| [scripts/](scripts/) | PowerShell, Python, and JavaScript automation for formatting, hierarchy and dependency checks, tests, benchmarks, package validation, reporting, frontmatter, and VBD artifacts |
| [templates/component/](templates/component/README.md) | A component project starting point |
| [.github/](.github/readme.md) | Repository instructions, 36 prompt files, 17 skill packages, and GitHub Actions workflows |
| [.infra/](.infra/yaml/README.md) | The reusable Framework Quality workflow action and infrastructure notes |
| [.json/](.json/schemas/queryfilter.schema.json) | JSON schemas |
| [.nuget/](.nuget/NuGet/NuGet.config) | An additional NuGet configuration |

At the root, `vc.Ifx.slnx` groups the solution projects. `Directory.Build.props`
and `Directory.Build.targets` hold shared MSBuild settings,
`Directory.Packages.props` centralizes dependency versions, `global.json`
selects the .NET SDK, and `NuGet.config` configures package sources.
`package.json` and its lockfile support the repository's Node-based validation
tools.

The repository has 16 solution projects: five packable libraries, the reporting
executable, eight MSTest projects, and two benchmark projects. The current
solution and project files are authoritative if this inventory changes.

## Build and validation

The repository selects the .NET SDK through [global.json](global.json) and uses
C# 14. Node.js supports the documentation and artifact validation tools; Python
and PowerShell are used by repository scripts. From the repository root:

```powershell
python scripts/Test-ProjectHierarchy.py
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project scripts/packaging/CompilerHostSmoke.proj -Configuration Release -WarningsAsErrors
```

[tests/README.md](tests/README.md) describes test ownership and commands.
[docs/onboarding/readme.md](docs/onboarding/readme.md) documents prerequisites
and setup.

## Package output and publishing

Packable projects write packages and symbol packages to the root `.nupkgs/`
directory, configured in [Directory.Build.props](Directory.Build.props). The
root [NuGet.config](NuGet.config) registers that directory as the `Ifx-Local`
feed. For a consuming solution, register the resolved local feed path:

```powershell
$ifxFeed = (Resolve-Path .nupkgs).Path
dotnet nuget add source $ifxFeed --name Ifx-Local
```

The [publish workflow](.github/workflows/publish.yml) validates packages and
publishes successful main builds to GitHub Packages. Stable version tags publish
to NuGet.org. A package version in source or a successful build does not by
itself mean that the package has been published.

Dated documents under [docs/planning/](docs/planning/) record the repository
state and validation evidence for their stated dates. Some describe earlier
project layouts and package versions.

## License

Original repository material uses the [MIT License](LICENSE). Third-party
material retains its own licenses and notices.
