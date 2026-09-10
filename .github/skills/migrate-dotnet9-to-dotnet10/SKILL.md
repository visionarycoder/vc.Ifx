---
name: migrate-dotnet9-to-dotnet10
license: MIT
title: .NET 9 to .NET 10 Migration
description: Upgrade .NET 9 projects to .NET 10 with warning-driven fixes, compatibility review, and explicit pass criteria.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1083
prerequisites:
  - migrate-dotnet8-to-dotnet9
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - migrate-dotnet10-to-dotnet11
appliesTo: '**/*.{cs,csproj,sln,slnx,props,targets,json,config,md}'
tags:
  - dotnet
  - migration
  - net10
---
# .NET 9 to .NET 10 Migration

Agent upgrades .NET 9 code to .NET 10 through a targeted pass over TFM changes, package alignment, breaking changes, and infrastructure updates.

## When to Use

| Condition | Use |
|---|---|
| Agent upgrades `net9.0` to `net10.0` | Use this skill |
| Agent resolves .NET 10 SDK, ASP.NET Core 10, EF Core 10, or compiler breaks | Use this skill |
| Agent updates CI, Docker, or `global.json` for the new SDK | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Project already runs cleanly on `net10.0` | Use the next migration skill if `net11.0` is the target |
| Upgrade starts from .NET 8 or earlier | Complete the earlier migration first |
| Work targets .NET Framework | Use a framework migration workflow |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Entry point | Yes | `.csproj`, `.sln`, or `.slnx` |
| Existing build/test commands | Yes | Keep repo commands unchanged when possible |
| Stack profile | No | ASP.NET Core, EF Core, JSON, crypto, desktop, containers, interop |
| Reference set | No | Load only matching reference families |

## Migration Workflow

| Step | Agent action | Output |
|---|---|---|
| 1 | Agent confirms the .NET 10 SDK and captures a clean .NET 9 baseline | Baseline status |
| 2 | Agent changes TFM and aligns Microsoft package families to 10.x | Updated project metadata |
| 3 | Agent fixes compiler, SDK, and runtime-facing code issues that appear in the selected reference set | Clean build |
| 4 | Agent reviews high-impact runtime differences such as shutdown behavior, serializer or HTTP changes, background service behavior, and EF Core changes | Runtime review notes |
| 5 | Agent updates Docker, CI, scripts, and `global.json` | Updated infrastructure |
| 6 | Agent rebuilds and reruns tests | Verification evidence |

Test: Agent runs the existing restore, build, and test commands after the TFM change.
Pass: Agent observes zero new restore errors. Agent observes zero new build errors. Agent observes zero new test failures on .NET 10.

## Review Matrix

| Area | Agent reviews |
|---|---|
| TFM and packages | `net10.0`, Microsoft package alignment |
| Compiler changes | `field`, `extension`, overload binding, and syntax changes called out by the loaded references |
| Runtime behavior | Shutdown, configuration nulls, HTTP, URI, serializer, and hosted-service changes |
| Framework changes | ASP.NET Core, EF Core, cryptography, desktop, and interop changes that match the project |
| Infrastructure | Docker images, CI SDK pins, restore policy, `global.json` |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Restore verification | `dotnet restore [entry]` | Zero restore errors |
| Compile verification | `dotnet build [entry] --no-incremental` | Zero build errors |
| Test verification | `dotnet test [entry]` or existing targeted commands | Zero new failures |
| Runtime verification | Existing smoke paths for changed subsystems | No new runtime failures in reviewed areas |

## Verification Checklist

Agent verifies:
- [ ] All intended targets moved from `net9.0` to `net10.0`
- [ ] Microsoft package families align to 10.x where required
- [ ] Runtime review covered project-specific stacks
- [ ] Operational files no longer pin 9.x SDK or images
- [ ] Build and tests pass on .NET 10

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Stopping after the TFM edit compiles | Review runtime and infrastructure changes too |
| Loading every breaking-change reference | Load only the families that match the project |
| Forgetting smoke validation for behavioral changes | Exercise the affected app path after tests |
| Leaving Docker or CI on 9.x | Update version pins in the same change |
