---
name: incremental-build
license: MIT
title: Incremental Build Optimization
description: Diagnose and fix MSBuild incremental-build regressions by proving why targets rerun and restoring correct skip behavior.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1230
prerequisites:
  - binlog-generation
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - build-perf-baseline
  - build-perf-diagnostics
  - eval-performance
appliesTo: '**/*.{csproj,sln,slnx,props,targets,binlog,json,md}'
tags:
  - msbuild
  - incremental-build
  - performance
  - diagnostics
---
# Incremental Build Optimization

Agent restores correct MSBuild skip behavior when repeated builds rerun work unnecessarily.

## When to Use

Agent uses this skill when:
- Second builds stay slow without meaningful source changes
- Custom targets run on every build
- User needs proof for why a target reran instead of skipping
- Visual Studio and CLI build behavior differ

## When Not to Use

Agent does not use this skill when:
- User only needs a first performance baseline
- Problem is pure project-graph parallelism
- Build system is not MSBuild based

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Build scope | Yes | Project or solution with bad incremental behavior |
| Comparison evidence | Yes | At least two builds, ideally with binlogs |
| Suspect targets | No | Helpful for generators, packaging, or codegen steps |
| IDE context | No | Helpful when Visual Studio differs from CLI |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Capture comparison | Agent collects first and second builds for the same scope | Agent preserves the second-build evidence | Comparison data exists |
| 2. Read second-build behavior | Agent finds which targets ran, skipped, or ran incrementally | Agent reads the second build rather than the first alone | Agent names one rerunning target |
| 3. Prove the reason | Agent traces `Inputs`, `Outputs`, timestamps, globs, or file tracking | Agent cites the exact stale input or missing declaration | Cause is evidence-backed |
| 4. Apply a fix | Agent adds correct `Inputs`, `Outputs`, stable paths, or `FileWrites` | Agent keeps generated files in managed locations | Skip rules become explicit |
| 5. Verify | Agent reruns the same build twice | Agent checks skip messages on the second run | Target now skips or rebuilds only when inputs change |

## Diagnostic Signals

| Signal | Interpretation |
|---|---|
| `Building target completely` | Outputs are missing or incrementality is not declared |
| `Building target incrementally` | Some outputs are stale |
| `Skipping target ... all output files are up-to-date` | Target behaves correctly |
| `is newer than output` | Specific input forced the rerun |

## Fix Patterns

| Area | Agent pattern |
|---|---|
| Custom generators | Use `Inputs`, `Outputs`, `BeforeTargets`, generated file in `$(IntermediateOutputPath)`, and `FileWrites` |
| Output locations | Use stable deterministic paths instead of timestamps or GUIDs |
| Returned items | Use `Returns` for query results instead of abusing `Outputs` |
| Visual Studio differences | Compare FUTDC evidence with CLI binlog evidence |

## Validation

| Check | Test | Pass |
|---|---|---|
| Second-build focus | Agent analyzes the comparison build | At least one rerunning target is named from second-build evidence |
| Declaration quality | Agent adds correct `Inputs` and `Outputs` | MSBuild has enough data to skip work |
| File tracking | Agent registers generated files in `FileWrites` | Clean and incremental behavior stay aligned |
| Rerun proof | Agent rebuilds twice | Second build skips the repaired target |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent guesses without reading second-build evidence | Agent starts from the comparison build |
| Agent micro-optimizes a target that still reruns | Agent restores skip behavior first |
| Agent writes generated files to unstable paths | Agent keeps outputs in deterministic managed locations |
| Agent blames Visual Studio before checking FUTDC | Agent compares IDE and CLI evidence directly |
