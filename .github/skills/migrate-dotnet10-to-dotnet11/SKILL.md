---
name: migrate-dotnet10-to-dotnet11
license: MIT
title: .NET 10 → .NET 11 Migration
description: Upgrade .NET 10 projects to .NET 11 preview with version-aware fixes, compatibility shims, and explicit verification.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1500
prerequisites:
  - migrate-dotnet9-to-dotnet10
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - migrate-dotnet8-to-dotnet9
  - migrate-dotnet9-to-dotnet10
appliesTo: '**/*.{cs,csproj,sln,slnx,props,targets,json,config,md}'
tags:
  - dotnet
  - migration
  - net11
---
# .NET 10 → .NET 11 Migration

Agent upgrades .NET 10 code to .NET 11 preview by applying source fixes, compatibility shims, runtime reviews, and infrastructure updates that match preview guidance.

## When to Use

| Condition | Use |
|---|---|
| Agent upgrades `net10.0` to `net11.0` | Use this skill |
| Agent resolves preview breaking changes in C# 15, ASP.NET Core 11, or EF Core 11 | Use this skill |
| Agent updates SDK pins, containers, or deployment checks for preview SDK use | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Project already builds and tests cleanly on `net11.0` preview | Use maintenance work instead of migration |
| Upgrade starts below .NET 10 | Complete intermediate migrations first |
| Team cannot accept preview dependencies | Stay on .NET 10 until approval exists |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Entry point | Yes | `.csproj`, `.sln`, or `.slnx` |
| Existing build/test commands | Yes | Preserve repo validation commands |
| Stack profile | No | ASP.NET Core, EF Core, containers, crypto, native interop, desktop |
| Preview policy | No | Confirm preview SDK and package use is acceptable |

## Migration Workflow

| Step | Agent action | Output |
|---|---|---|
| 1 | Agent confirms the .NET 11 preview SDK and captures a clean .NET 10 baseline | Baseline status |
| 2 | Agent changes TFM and aligns Microsoft package families to 11.x preview versions | Updated project metadata |
| 3 | Agent fixes source breaks such as span collection-expression changes, `nameof(this.)`, `with(...)` parsing, `NamedPipeClientStream` obsoletion, Microsoft.OpenApi v3 changes, and EF design package transitivity | Clean build |
| 4 | Agent adds compatibility shims or code-path updates for runtime changes such as Cosmos async-only operations, BackgroundService crash behavior, SqlClient Entra ID separation, TAR or ZIP validation, and hardware constraints | Reviewed runtime compatibility list |
| 5 | Agent updates preview SDK pins, Docker images, and deployment assumptions | Updated infrastructure |
| 6 | Agent rebuilds, reruns tests, and smoke-checks the highest-risk runtime paths | Verification evidence |

Test: Agent runs the existing restore, build, and test commands on the preview target.
Pass: Agent observes zero new restore errors. Agent observes zero new build errors. Agent observes zero new test failures on `net11.0` preview.

## Review Matrix

| Area | Agent reviews |
|---|---|
| Compiler | C# 15 parsing and safe-context changes |
| Core libraries | Compression, MemoryStream, pipes, TAR, ZIP, cryptography |
| EF Core | Cosmos async-only behavior, design package references, SQL vector loading |
| Hosting | BackgroundService exception behavior, Blazor overscan defaults |
| Infrastructure | Preview SDK pins, Docker tags, hardware/runtime assumptions |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Restore verification | `dotnet restore [entry]` | Zero restore errors |
| Compile verification | `dotnet build [entry] --no-incremental` | Zero build errors |
| Test verification | `dotnet test [entry]` or existing targeted commands | Zero new failures |
| Runtime verification | Existing smoke paths for changed subsystems | No new runtime failures in reviewed areas |

## Verification Checklist

Agent verifies:
- [ ] All intended targets moved from `net10.0` to `net11.0`
- [ ] Preview package and SDK usage is explicit
- [ ] Compatibility shims cover project-specific preview breaks
- [ ] Operational files no longer pin 10.x SDK or images
- [ ] Build and tests pass on the preview target

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Treating preview migration as stable-runtime work | Call out preview scope and verify package versions explicitly |
| Ignoring hardware baseline changes | Validate deployment targets before rollout |
| Missing async conversion for EF Cosmos paths | Replace sync calls with async APIs |
| Updating code without preview CI changes | Move SDK and image pin updates into the same pass |
