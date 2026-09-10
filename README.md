# vc.Ifx

[![Build & Test](https://github.com/visionarycoder/vc.Ifx/actions/workflows/publish.yml/badge.svg)](https://github.com/visionarycoder/vc.Ifx/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A modular, enterprise-grade .NET 10 framework of independently versioned building-block libraries (`vc.Ifx.*`) covering filtering/querying, storage, secrets, messaging, observability, gRPC pipeline, and Roslyn tooling.

---

## 🚀 Quickstart

```powershell
# Clone
git clone https://github.com/visionarycoder/vc.Ifx.git
cd vc.Ifx

# Restore
dotnet restore vc.Ifx.slnx

# Build & Test
dotnet build vc.Ifx.slnx --configuration Release
dotnet test vc.Ifx.slnx --configuration Release
```

---

## 📦 Solution Overview

`vc.Ifx.slnx` aggregates the framework's foundational libraries and their tests. `Databases/` and `tools/` are independent solutions and are not part of `vc.Ifx.slnx`.

### Projects (`src/`)

- `vc.Ifx` — Core library and shared utilities
- `vc.Ifx.Abstractions` — Provider-agnostic interfaces
- `vc.Ifx.Primitives` — Value objects and primitive types
- `vc.Ifx.Filtering`, `vc.Ifx.Filtering.EntityFrameworkCore` — Filtering subsystem and EF Core adapter
- `vc.Ifx.Querying` — Query serialization and helpers
- `vc.Ifx.Pipeline`, `vc.Ifx.Pipeline.Grpc` — Execution pipeline and gRPC transport
- `vc.Ifx.Proxy`, `vc.Ifx.Proxy.AspNetCore`, `vc.Ifx.Proxy.Http` — Proxy abstractions and hosts
- `vc.Ifx.Storage.Abstractions`, `vc.Ifx.Storage.Azure.Blobs`, `vc.Ifx.Storage.Ftp`, `vc.Ifx.Storage.Local` — Storage providers
- `vc.Ifx.Secrets.Abstractions`, `vc.Ifx.Secrets.Azure.KeyVault`, `vc.Ifx.Secrets.Local` — Secrets providers
- `vc.Ifx.Data.Azure.Tables` — Azure Table Storage data access
- `vc.Ifx.Messaging.Azure.Queues` — Azure Storage Queues messaging
- `vc.Ifx.Observability` — Logging, tracing, and metrics helpers
- `vc.Ifx.Analyzers`, `vc.Ifx.CodeFixes`, `vc.Ifx.Generators`, `vc.Ifx.Roslyn` — Roslyn analyzers, code fixes, and source generators (netstandard2.0 / C# 8)

### Tests (`tests/`)

- `tests/unit/vc.Ifx.UnitTests` — Unit tests
- `tests/integration` — Integration test projects

## 🗃️ Repository Structure (High-Level)

```text
/.github        # Copilot instructions, skills, prompts, workflows
/docs           # Architecture decisions, best-practice guidance
/src            # vc.Ifx.* libraries
/tests
  /unit         # vc.Ifx.UnitTests
  /integration  # Integration test projects
/Databases      # Independent solution (Databases.slnx)
/tools          # Independent standalone tool solutions
```

## 📚 Documentation & Roadmap

- Architectural Decision Records (ADRs): `docs/adr/`
- Living architecture playbook: `docs/index.md`
- License details: `docs/LICENSE-INFO.md`

## 🤝 Contributing

Contributions are welcome. Please open an issue or ADR proposal for large architectural changes. Keep PRs focused and update project-level READMEs when moving code.

---

This document is the canonical solution-level index.

Last synchronized with solution structure: 2026-09-09
