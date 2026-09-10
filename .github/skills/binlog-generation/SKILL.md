---
name: binlog-generation
license: MIT
title: Generate Binary Logs
description: Generate MSBuild binary logs with collision-safe naming so later diagnostics start with preserved evidence.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: low
estimated_tokens: 820
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - binlog-failure-analysis
  - build-perf-baseline
  - build-perf-diagnostics
appliesTo: '**/*.{csproj,sln,slnx,props,targets}'
tags:
  - msbuild
  - binlog
  - build
  - diagnostics
---
# Generate Binary Logs

Agent captures a binary log for every MSBuild-based command that needs later diagnostics.

## When to Use

Agent uses this skill when:
- User runs `dotnet build`, `dotnet test`, `dotnet pack`, `dotnet publish`, `dotnet restore`, or `msbuild`
- User needs failure, performance, restore, or evaluation evidence
- Agent wants preserved build history before troubleshooting starts

## When Not to Use

Agent does not use this skill when:
- Build system is not MSBuild based
- User already has the required `.binlog`
- Task is pure binlog analysis rather than binlog capture

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Build command | Yes | Any MSBuild-backed command |
| Shell context | No | PowerShell needs escaped braces |
| Naming constraint | No | CI artifact pipelines sometimes need a fixed filename |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Choose command | Agent identifies the exact MSBuild-backed command | Agent confirms the command invokes MSBuild | Command scope matches the diagnostic need |
| 2. Add binlog switch | Agent appends `/bl:{}` or PowerShell `-bl:{{}}` | Agent inspects the final command line | Command includes a binary log argument |
| 3. Run build | Agent executes the command once | Agent checks command completion | Build produces a `.binlog` file |
| 4. Preserve evidence | Agent keeps the file name or upload path | Agent lists the captured file | File remains available for later analysis |
| 5. Handle fixed names safely | Agent chooses a non-colliding fixed name when `{}` is unavailable | Agent checks the target directory | New file does not overwrite prior evidence |

## Command Matrix

| Context | Agent uses |
|---|---|
| Bash or cmd | `dotnet build /bl:{}` |
| PowerShell | `dotnet build -bl:{{}}` |
| Fixed name | `dotnet build /bl:build-01.binlog` |
| Test run | `dotnet test /bl:{}` |
| Restore run | `dotnet restore /bl:{}` |

## Validation

| Check | Test | Pass |
|---|---|---|
| Switch presence | Agent inspects the final command | `/bl` appears exactly once |
| File creation | Agent lists `*.binlog` after the run | At least one new `.binlog` exists |
| Collision safety | Agent compares the chosen name against existing files | Prior binlogs remain untouched |
| Preservation | Agent avoids destructive clean patterns | Diagnostic history stays available |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent omits `/bl` | Agent adds `/bl` before running the build |
| Agent uses bare `/bl` repeatedly | Agent uses `{}` or a unique fixed name |
| Agent forgets PowerShell brace escaping | Agent uses `-bl:{{}}` |
| Agent deletes history with `git clean -fdx` | Agent excludes binlogs when cleaning |
