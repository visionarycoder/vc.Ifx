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
$build = Join-Path $PWD ('TestResults/reporting/' + [Guid]::NewGuid().ToString('N'))
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly `
  -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors -ReportBuildDirectory $build
if ($LASTEXITCODE -ne 0) { throw 'Solution build failed.' }
$env:IFX_REPORT_BUILD = $build
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -FullCoverage `
  -Configuration Release -NoBuild -WarningsAsErrors
if ($LASTEXITCODE -ne 0) { throw 'Full coverage failed.' }
```

The shared runner restores dependencies, serializes builds/tests, and enforces
exact 100% line and branch coverage for each library. Keep the captured inputs and
outputs unchanged between build and coverage. See [onboarding](docs/onboarding/readme.md)
for targeted work and [reporting](docs/reporting/README.md) for report generation.

## Packages

All 28 source libraries produce NuGet packages. Runtime libraries target
`net10.0`; Roslyn, Analyzers, CodeFixes, and Generators retain `netstandard2.0`
for compiler-host compatibility. All use C# 14.0.

| Package | Responsibility |
| --- | --- |
| [vc.Ifx](src/vc.Ifx/README.md) | Core composition and shared utilities |
| [vc.Ifx.Abstractions](src/vc.Ifx.Abstractions/README.md) | Request/result and lifecycle contracts |
| [vc.Ifx.Primitives](src/vc.Ifx.Primitives/README.md) | Reusable value types |
| [vc.Ifx.Filtering](src/vc.Ifx.Filtering/README.md) | Provider-neutral filter expressions |
| [vc.Ifx.Filtering.EntityFrameworkCore](src/vc.Ifx.Filtering.EntityFrameworkCore/README.md) | EF Core filtering adapter |
| [vc.Ifx.Querying](src/vc.Ifx.Querying/README.md) | Database-oriented query specifications and serialization |
| [vc.Ifx.Pipeline](src/vc.Ifx.Pipeline/README.md) | Request dispatch and interception |
| [vc.Ifx.Pipeline.Grpc](src/vc.Ifx.Pipeline.Grpc/README.md) | gRPC pipeline transport |
| [vc.Ifx.Proxy](src/vc.Ifx.Proxy/README.md) | Proxy policies and contracts |
| [vc.Ifx.Proxy.AspNetCore](src/vc.Ifx.Proxy.AspNetCore/README.md) | ASP.NET Core proxy integration |
| [vc.Ifx.Proxy.Http](src/vc.Ifx.Proxy.Http/README.md) | HTTP proxy transport |
| [vc.Ifx.WebApi](src/vc.Ifx.WebApi/README.md) | Exception handling and stable HTTP response messages |
| [vc.Ifx.Storage.Abstractions](src/vc.Ifx.Storage.Abstractions/README.md) | Storage provider contracts |
| [vc.Ifx.Storage.Local](src/vc.Ifx.Storage.Local/README.md) | Local filesystem storage |
| [vc.Ifx.Storage.Ftp](src/vc.Ifx.Storage.Ftp/README.md) | FTP storage |
| [vc.Ifx.Storage.Azure.Blobs](src/vc.Ifx.Storage.Azure.Blobs/README.md) | Azure Blob storage |
| [vc.Ifx.Secrets.Abstractions](src/vc.Ifx.Secrets.Abstractions/README.md) | Secret provider contracts |
| [vc.Ifx.Secrets.Local](src/vc.Ifx.Secrets.Local/README.md) | Local secret provider |
| [vc.Ifx.Secrets.Azure.KeyVault](src/vc.Ifx.Secrets.Azure.KeyVault/README.md) | Azure Key Vault secrets |
| [vc.Ifx.Data.Azure.Tables](src/vc.Ifx.Data.Azure.Tables/README.md) | Azure Table data access |
| [vc.Ifx.Messaging.Azure.Queues](src/vc.Ifx.Messaging.Azure.Queues/README.md) | Azure Queue messaging |
| [vc.Ifx.Observability](src/vc.Ifx.Observability/README.md) | Pipeline tracing and metrics adapters |
| [vc.Ifx.Roslyn](src/vc.Ifx.Roslyn/README.md) | Shared compiler tooling contracts |
| [vc.Ifx.Analyzers](src/vc.Ifx.Analyzers/README.md) | Framework diagnostic and code-quality policies |
| [vc.Ifx.CodeFixes](src/vc.Ifx.CodeFixes/README.md) | Bounded, reviewed diagnostic remediation |
| [vc.Ifx.Generators](src/vc.Ifx.Generators/README.md) | Source generation, including attributed minimal API endpoints |
| [vc.Ifx.Generators.Abstractions](src/vc.Ifx.Generators.Abstractions/README.md) | Runtime endpoint attributes |
| [vc.Ifx.Roslyn.Reporting](src/vc.Ifx.Roslyn.Reporting/README.md) | Build diagnostics, source metrics, and coverage reports |

## Supporting Projects

The solution contains 36 projects: 28 libraries, two centralized test projects,
four benchmark projects under [performance](performance/README.md), one reporting
command host, and the documentation project. These eight supporting projects are
not packable.

[docs/docs.csproj](docs/docs.csproj) automatically includes documentation files in
their relative folders and has no build outputs. Its CLI checks pass; hands-on IDE
acceptance remains a separate check.

## Verification And Publishing

The [2026-09-10 local checkpoint](docs/planning/local-verification-20260910.md)
records a warning-free Release build, 3,632 passing tests, exact 100% coverage for
all 28 libraries, and 28 validated package/symbol pairs. These are dated results,
not a fresh verification of subsequent edits. Hosted execution, publishing
permissions, repository-required checks, and hands-on IDE acceptance remain open.

The configured workflow validates pull requests, merge queues, manual runs, pushes
to main, and version tags. Successful main pushes publish to GitHub Packages;
matching stable version tags publish to NuGet.org. There is no configured nightly
schedule or branch-derived versioning. See the [branching playbook](docs/reviews/branching-strategy.md)
and [CI guide](.infra/yaml/README.md).

## Documentation

- [Repository instructions](AGENTS.md)
- [Documentation index](docs/index.md)
- [Best practices](docs/best-practices/readme.md)
- [Architecture decisions](docs/adr/index.md)
- [Framework upgrade plan](docs/planning/framework-upgrade-parallel-plan.md)
- [Test and coverage guide](tests/README.md)
- [License details](docs/LICENSE-INFO.md)

Keep project READMEs aligned with behavioral changes. Propose significant
architectural changes through a reviewed decision before implementation.

Last synchronized with repository configuration: 2026-09-10.
