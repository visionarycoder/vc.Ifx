---
name: msbuild-server
title: MSBuild Server for CLI Caching
description: Use MSBuild Server to reduce repeated CLI build startup cost and verify the gain with warm-build measurements.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 860
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - build-perf-baseline
  - build-perf-diagnostics
appliesTo: '**/*.{csproj,sln,slnx,props,targets,binlog,json,md}'
tags:
  - msbuild
  - server
---
# MSBuild Server for CLI Caching

Agent uses MSBuild Server to reuse evaluation and startup state across repeated CLI builds.

## When to Use

Agent uses this skill when:
- CLI warm or no-op builds pay visible startup cost
- User reports that `dotnet build` is slower than IDE builds
- CI or local scripts run repeated builds in the same environment

## When Not to Use

Agent does not use this skill when:
- Build runs inside Visual Studio
- Task is a one-off cold build
- Agent suspects correctness issues and needs a no-server control run

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Shell context | No | Bash, PowerShell, cmd, or persistent Windows setting |
| Build command | No | Same command used for before and after comparison |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Confirm CLI context | Agent verifies the build runs from a shell rather than the IDE | Agent checks the invocation path | MSBuild Server is relevant |
| 2. Enable server use | Agent sets `MSBUILDUSESERVER=1` for the target shell | Agent prints the environment value | Environment value equals `1` |
| 3. Measure cold and warm runs | Agent runs the same build twice | Agent records first and second timings | Second run is measurably faster or neutral |
| 4. Confirm reset path | Agent runs `dotnet build-server shutdown` and rebuilds | Agent checks the server restarts cleanly | Reset path works without build breakage |

## Command Matrix

| Context | Agent uses |
|---|---|
| Bash or CI | `export MSBUILDUSESERVER=1` |
| PowerShell | `$env:MSBUILDUSESERVER = "1"` |
| Persistent Windows setting | `setx MSBUILDUSESERVER 1` |
| Reset | `dotnet build-server shutdown` |

## Validation

| Check | Test | Pass |
|---|---|---|
| Enablement | Agent reads the environment variable | Value is `1` in the active shell |
| Warm-build benefit | Agent compares two sequential builds | Second run is faster for the same scope |
| Safety reset | Agent shuts the server down and rebuilds | Build succeeds after restart |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent expects benefit inside Visual Studio | Agent skips the server for IDE builds |
| Agent treats one cold run as proof | Agent compares a warm second run |
| Agent ignores correctness concerns | Agent uses shutdown as a control path |
| Agent leaves unexplained background state | Agent documents the reset command |
