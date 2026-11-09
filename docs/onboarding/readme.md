---
title: Developer Onboarding
doc_type: readme
status: active
last_updated: 2026-11-08
---
# Developer Onboarding

## Prerequisites

- Git.
- PowerShell 7 (`pwsh`), required by the shared verification scripts.
- The stable .NET SDK selected by [global.json](../../global.json): 10.0.401,
  with stable patch roll-forward and no preview SDKs.
- Network access to the configured package feeds for dependency restore.
- Optional: an IDE that supports the pinned SDK and C# 14.0. CLI verification
  does not require an IDE.

Runtime libraries target .NET 10. The four compiler-host libraries retain
netstandard2.0 while using C# 14.0. Local framework tests do not require live Azure
accounts, FTP credentials, or permission to publish packages.

## Clone And Verify Tools

Run these commands in PowerShell:

```powershell
git clone https://github.com/visionarycoder/vc.Ifx.git
Set-Location vc.Ifx
dotnet --version
pwsh -NoProfile -Command '$PSVersionTable.PSVersion'
```

Read [AGENTS.md](../../AGENTS.md) and the
[parallel plan](../planning/framework-upgrade-parallel-plan.md) before claiming
work. Do not reclaim a completed workstream based on an older verification note.

## Build And Run All Tests

Run from the repository root; all builds and tests use the shared serialized runner.

```powershell
python scripts/Test-ProjectHierarchy.py
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -Project vc.Ifx.slnx -Configuration Release -WarningsAsErrors
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project scripts/packaging/CompilerHostSmoke.proj -Configuration Release -WarningsAsErrors
```

The final command checks all eight package/symbol pairs, rejects malformed packages,
and loads extracted compiler extensions and IDE code fixes in isolated hosts.
It does not assert installed IDE UI behavior or publication.

## Choose A Project

Use [current project ownership](../architecture/root-library-consolidation.md).
Ifx owns core runtime behavior. General, Azure, and Windows integrations have
separate dependency boundaries. Shared Roslyn support, analyzers, generators, and
code fixes have separate projects. Ifx.Roslyn.Reporting is a supporting executable
for report I/O. Tests and benchmarks remain supporting projects.

## Coverage And Release Acceptance

The owner removed mandatory exact 100% coverage and report-provenance gates on
2026-11-08. The release workflow builds and runs every solution test project,
checks dependency boundaries, validates packages and isolated compiler consumers,
and verifies release versions and artifact hashes before publication. Coverage and
reporting remain optional diagnostics. Their legacy full-evidence scripts retain
old inventory assumptions and are not release requirements. Local checks do not
establish hosted workflow execution or publication.
