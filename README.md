# vc.Ifx

[![Build & Test](https://github.com/visionarycoder/vc.Ifx/actions/workflows/publish.yml/badge.svg)](https://github.com/visionarycoder/vc.Ifx/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Modular .NET framework libraries for filtering, database queries, transport,
storage, secrets, messaging, observability, Web APIs, and compiler tooling.
Package boundaries follow Volatility-based Decomposition.

## Quickstart

Use Git, PowerShell 7, and the stable SDK selected by [global.json](global.json):
10.0.401 with stable patch roll-forward. The shared language version is C# 14.0.

```powershell
git clone https://github.com/visionarycoder/vc.Ifx.git
Set-Location vc.Ifx
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly `
  -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors
if ($LASTEXITCODE -ne 0) { throw 'Solution build failed.' }
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 `
  -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors
if ($LASTEXITCODE -ne 0) { throw 'Solution tests failed.' }
```

The shared runner restores dependencies and serializes builds/tests. Release checks
require passing solution tests, dependency checks and package consumer validation.
Coverage and reporting are optional diagnostic tools. See [onboarding](docs/onboarding/readme.md)
for targeted work and [reporting](docs/reporting/README.md) for report generation.

## Packages

Eight production libraries group core runtime behavior, general integrations,
Azure, Windows, and the four Roslyn responsibilities. See
[current ownership and migration](docs/architecture/root-library-consolidation.md).

| Library |
| --- |
| [Ifx](src/ifx/README.md) |
| [Ifx.Integrations](src/Ifx.Integrations/README.md) |
| [Ifx.Integrations.Azure](src/Ifx.Integrations.Azure/README.md) |
| [Ifx.Integrations.Windows](src/Ifx.Integrations.Windows/README.md) |
| [Ifx.Roslyn](src/Ifx.Roslyn/README.md) |
| [Ifx.Roslyn.Analyzers](src/Ifx.Roslyn.Analyzers/README.md) |
| [Ifx.Roslyn.CodeFixes](src/Ifx.Roslyn.CodeFixes/README.md) |
| [Ifx.Roslyn.Generators](src/Ifx.Roslyn.Generators/README.md) |

## Local Package Feed

All eight production packages share the initial SemVer version **0.1.0** in
Directory.Build.props. This is the initial development line; no published API
compatibility adapters are retained for the unpublished FilterSpec model.

Every packable project shares one output location: `.nupkgs/` at the
repository root (set via `PackageOutputPath` in [Directory.Build.props](Directory.Build.props)
and registered as the `Ifx-Local` source in [NuGet.config](NuGet.config)).
Building or packing any project restores and packs against this single folder
instead of scattering `.nupkg`/`.snupkg` files per project.

Other solutions on this machine reference these packages by adding the same
folder as a package source:

```powershell
dotnet nuget add source "E:\dev\v\vc.Ifx\.nupkgs" --name Ifx-Local
dotnet add path/to/Consumer.csproj package Ifx --version 0.1.0
```

## Supporting Projects

The solution contains 19 projects: eight libraries, eight test projects, two
benchmark projects under [performance](performance/README.md), and the
[Ifx.Roslyn.Reporting executable](src/Ifx.Roslyn.Reporting/README.md).
Supporting projects are not packable. The documentation project
referenced by historical release checks is absent from the current checkout.

## Verification And Publishing

The [2026-09-10 local checkpoint](docs/planning/local-verification-20260910.md)
records a warning-free Release build, 3,632 passing tests, exact 100% coverage for
all 28 libraries, and 28 validated package/symbol pairs. These are dated results,
not a fresh verification of subsequent edits. The
[hosted follow-up](docs/planning/external-acceptance-20260910.md) passed Linux quality
and transferred-package verification for draft PR #9. Authorized review/merge,
publishing permissions, repository-required checks, and hands-on IDE acceptance
remain open; nothing was published by that verification run.

The configured workflow validates pull requests, merge queues, manual runs, pushes
to main, and version tags. Successful main pushes publish to GitHub Packages;
matching stable version tags publish to NuGet.org. There is no configured nightly
schedule or branch-derived versioning. See the [branching playbook](docs/reviews/branching-strategy.md)
and [CI guide](.infra/yaml/README.md).

## Documentation

- [Repository instructions](AGENTS.md)
- [Documentation index](docs/index.md)
- [Architecture reviews](docs/reviews/readme.md)
- [Architecture decisions](docs/adr/index.md)
- [Framework upgrade plan](docs/planning/framework-upgrade-parallel-plan.md)
- [Test and coverage guide](tests/README.md)
- [License details](docs/LICENSE-INFO.md)

Keep project READMEs aligned with behavioral changes. Propose significant
architectural changes through a reviewed decision before implementation.

## License

Original repository material uses the [MIT License](LICENSE). Third-party
material retains its existing licenses and notices; the repository license does
not replace those terms.

Last synchronized with repository configuration: 2026-11-08.
