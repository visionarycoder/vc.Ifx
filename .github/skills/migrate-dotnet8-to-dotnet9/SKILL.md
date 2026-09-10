---
name: migrate-dotnet8-to-dotnet9
description: Upgrade .NET 8 projects to .NET 9 with version-aware fixes, targeted breaking-change review, and explicit verification.
license: MIT
title: .NET 8 → .NET 9 Migration
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1600
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - migrate-dotnet9-to-dotnet10
  - migrate-dotnet10-to-dotnet11
appliesTo: '**/*.{cs,csproj,sln,slnx,props,targets,json,config,md}'
tags:
  - dotnet
  - migration
  - net9
---
# .NET 8 → .NET 9 Migration

Agent upgrades existing .NET 8 code to .NET 9 by fixing compile breaks, reviewing runtime changes, and proving the upgraded build stays clean.

## When to Use

| Condition | Use |
|---|---|
| Agent upgrades `net8.0` projects or solutions to `net9.0` | Use this skill |
| Agent resolves post-SDK-update breaks in C# 13, ASP.NET Core 9, or EF Core 9 | Use this skill |
| Agent updates infrastructure tied to the .NET SDK version | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Project already builds and tests cleanly on `net9.0` | Use the next migration skill if a later target is required |
| Migration starts from .NET 7 or earlier | Complete intermediate upgrades first |
| Work targets .NET Framework | Use a framework migration workflow |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Project or solution entry point | Yes | `.csproj`, `.sln`, or `.slnx` |
| Build and test commands | Yes | Use existing repo commands when available |
| Technology profile | No | ASP.NET Core, EF Core, containers, desktop, crypto, JSON, interop |
| Approved references | No | Load only matching breaking-change references |

## Migration Workflow

| Step | Agent action | Output |
|---|---|---|
| 1 | Agent confirms the .NET 9 SDK exists and captures a clean .NET 8 build/test baseline | Baseline status |
| 2 | Agent changes `net8.0` to `net9.0` and aligns Microsoft package families to 9.x | Updated project metadata |
| 3 | Agent fixes source and build breaks, including C# 13 changes, SYSLIB0054-SYSLIB0057, params-span overload shifts, and package restore issues that match the project profile | Clean build |
| 4 | Agent reviews runtime changes that do not produce compile failures, including BinaryFormatter removal, EF Core migration behavior, HttpClientFactory handler changes, DI validation, and environment precedence | Reviewed runtime risk list |
| 5 | Agent updates `global.json`, CI, Docker, and scripts that pin 8.x SDK or images | Updated infrastructure |
| 6 | Agent rebuilds, reruns tests, and smoke-checks high-risk runtime paths | Verification evidence |

Test: Agent runs the existing restore, build, and test commands for the migrated solution.
Pass: Agent observes zero new restore errors. Agent observes zero new build errors. Agent observes zero new failing tests after the `net9.0` change.

## Breaking-Change Focus

| Area | Agent reviews |
|---|---|
| Compiler and BCL | C# 13 parsing and overload changes, `InlineArray`, iterator unsafe context, removed preview APIs |
| Runtime and diagnostics | Floating-point conversion behavior, environment variable precedence, Terminal Logger script impact |
| ASP.NET Core | Development DI validation defaults and keyed-service behavior |
| EF Core | Pending model changes exception, explicit migration transaction rules |
| Networking and serialization | BinaryFormatter hard failure, HttpClientFactory handler changes, header redaction |
| Containers and deployment | 9.0 SDK/runtime image tags, zlib removal, `global.json` updates |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Restore verification | `dotnet restore [entry]` | Zero restore errors |
| Compile verification | `dotnet build [entry] --no-incremental` | Zero build errors |
| Test verification | `dotnet test [entry]` or existing targeted commands | Zero new failures |
| Runtime-risk verification | Existing smoke path for EF migration, DI startup, and HTTP flows when applicable | No new runtime exceptions in reviewed paths |

## Verification Checklist

Agent verifies:
- [ ] Every `net8.0` target changed intentionally to `net9.0`
- [ ] Microsoft package families align to 9.x where needed
- [ ] Breaking changes were reviewed by technology area, not by guesswork
- [ ] Infrastructure references no longer pin 8.x SDK or images
- [ ] Build, test, and smoke checks passed on .NET 9

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Treating the work as a one-line TFM edit | Review source, runtime, and infrastructure changes together |
| Leaving dynamic values in EF `HasData` seeding | Replace with constants before migration verification |
| Ignoring `BinaryFormatter` usage | Stop and replace the serializer path intentionally |
| Updating app code but not CI or Docker | Move all SDK/version pins in the same pass |
