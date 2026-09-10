# Package validation

Each source project declares its package identity, description, tags, repository,
and local README. License and Source Link/symbol defaults are inherited from
the infrastructure-owned build configuration. Package README content remains
with the package owner; the packaging pass adds only missing mission statements.

## Commands

Run from the repository root using PowerShell 7 and the .NET 10 SDK:

```powershell
pwsh -NoProfile -File scripts/packaging/Validate-Packages.ps1 -MetadataOnly

# Targeted packing, then archive validation:
./scripts/packaging/Validate-Packages.ps1 -Pack -PackageId vc.Ifx.Roslyn,vc.Ifx.Analyzers,vc.Ifx.CodeFixes,vc.Ifx.Generators,vc.Ifx.Filtering

pwsh -NoProfile -File scripts/packaging/Test-PackageValidation.ps1

# Coordinate repository-wide work before running this release check:
pwsh -NoProfile -File scripts/packaging/Validate-Packages.ps1 -Pack
```

Without `-MetadataOnly`, archives are required in `artifacts/packaging` (override
with `-PackageDirectory`). `-Pack` builds Release packages serially with
`-m:1 -p:BuildInParallel=false -p:GeneratePackageOnBuild=false`. The last property
prevents duplicate automatic packs during an explicit pack operation.
Validation fails on the first error and does not delete existing artifacts.
Use a fresh package directory for a release to avoid confusing old artifacts with
newly built packages. `-PackageId` limits source packages; all test/benchmark
packability and docs checks still run. `-SkipDocsCheck` skips only docs verification.

## Checks

- Evaluated package metadata, source README existence, MIT license, symbols, and
  Source Link settings for each selected source project.
- Actual archive README matches the source; nuspec identity, tags, description,
  license, repository URL, and commit are present.
- Stable packages do not declare prerelease public dependencies.
- Ordinary packages contain their DLL and XML documentation under the declared
  framework. Public direct package/project dependencies remain in the nuspec.
- Small packages do not depend on the `vc.Ifx` aggregator. Test, benchmark, and
  intermediate build outputs do not appear in packages.
- Analyzer, generator, and code-fix assemblies live under `analyzers/dotnet/cs`,
  with private vc.Ifx.Roslyn dependencies beside them. Code fixes also bundle
  vc.Ifx.Analyzers. These three packages expose no runtime dependency groups or
  `lib`, `ref`, or `runtimes` assets, and do not bundle host Roslyn DLLs.
- Roslyn's ordinary library package exposes Microsoft.CodeAnalysis.Common;
  Microsoft.CodeAnalysis.Analyzers is explicitly private, preventing central
  transitive pinning from promoting a build tool into public package dependencies.
- A matching `.snupkg` contains a portable PDB with a nonempty, commit-pinned
  Source Link document map matching the nuspec commit. These checks read PDB
  metadata, not just filenames.
- Every test/benchmark evaluates `IsPackable=false` and
  `GeneratePackageOnBuild=false`.
- Docs restore, build, and rebuild create no `obj`, `bin`, or `.no-output`
  directories. Every recursive docs item has a relative display path. The docs
  project deliberately imports no SDK build targets; clean/pack are no-ops.

The validator tests accept five real packages and reject thirteen mutated archives.
They retain mutation fixtures under the ignored artifacts directory. These tests
do not measure framework line/branch coverage or substitute for analyzer/generator
host execution and IDE code-fix discovery tests.

## References

- [NuGet analyzer conventions](https://learn.microsoft.com/en-us/nuget/guides/analyzers-conventions)
- [Source Link in the SDK](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/8.0/source-link)
- [Shared-file requests](coordination.md)
- [Performance audit](performance-audit.md)
