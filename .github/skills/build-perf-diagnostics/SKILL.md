---
name: build-perf-diagnostics
title: Build Performance Diagnostics
description: Diagnose MSBuild performance bottlenecks from binlogs, classify the dominant cost, and validate the smallest effective fix.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1460
prerequisites:
  - binlog-generation
  - build-perf-baseline
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - build-parallelism
  - incremental-build
  - eval-performance
appliesTo: '**/*.{binlog,csproj,sln,slnx,props,targets}'
tags:
  - msbuild
  - performance
  - diagnostics
  - build
  - binlog
---
# Build Performance Diagnostics

Agent diagnoses proven MSBuild slowness from a binlog.

## When to Use

Agent uses this skill when:
- Baseline data already exists
- User needs target, task, analyzer, restore, or evaluation root cause
- Wall clock time stays high after first-line fixes

## When Not to Use

Agent does not use this skill when:
- Agent has not captured a baseline yet
- Build system is not MSBuild based
- User needs pure incremental-skip analysis or pure graph tuning

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Binlog | Yes | Existing `.binlog` from the slow build |
| Build scope | Yes | Project or solution represented by the binlog |
| Baseline context | Yes | Cold, warm, or no-op expectation |
| Validation command | No | Targeted build command for fix verification |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Load evidence | Agent opens the binlog with MCP tools when available, else replay workflow | Agent extracts total duration and dominant summaries | Agent has reliable timing evidence |
| 2. Rank costs | Agent lists slowest projects, targets, tasks, and analyzer time | Agent sorts costs by cumulative impact | Agent identifies the top bottleneck category |
| 3. Classify root cause | Agent maps evidence to RAR, analyzers, copy I/O, restore, evaluation, or graph shape | Agent cites the exact symptom threshold | Agent produces one primary diagnosis |
| 4. Recommend minimal fix | Agent selects the smallest change that addresses the primary diagnosis | Agent ties change to one file or command | Agent avoids unrelated tuning |
| 5. Verify | Agent reruns the targeted command with the same scope | Agent compares timing and summary output | Agent confirms measurable improvement or records no gain |

## Bottleneck Thresholds

| Category | Signal | Action path |
|---|---|---|
| RAR | `ResolveAssemblyReference` stays above 5s per project | Reduce reference load, network paths, or transitive closure |
| Analyzers | Analyzer cost exceeds 30% of `Csc` time | Condition analyzers for inner loop, trim redundant packages |
| Single target domination | One target exceeds 50% of total build time | Optimize or split the dominating step |
| Low parallel efficiency | Wall clock nearly matches total project time sum | Use `build-parallelism` guidance |
| Copy-heavy build | `Copy` time is materially high | Use hardlinks, skip unchanged files, trim output copy set |
| Restore in every build | Restore appears in warm or no-op builds | Separate restore or enable static graph restore |
| Evaluation overhead | Build starts slowly before execution | Use `eval-performance` guidance |

## Workflow Commands

| Purpose | Command |
|---|---|
| Replay binlog | `dotnet msbuild build.binlog -noconlog -fl -flp:"v=diag;logfile=full.log;performancesummary"` |
| Read summaries | `grep "Target Performance Summary\|Task Performance Summary" -A 50 full.log` |
| Check per-project cost | `grep "done building project\|Project Performance Summary" full.log` |
| Check analyzers | `grep -i "Total analyzer execution time\|analyzer.*elapsed\|CompilerAnalyzerDriver" full.log` |
| Check node use | `grep -i "node.*assigned\|Building with\|scheduler" full.log` |

## Validation

| Check | Test | Pass |
|---|---|---|
| Diagnosis quality | Agent ties one recommendation to one dominant cost | Evidence points to a single primary bottleneck |
| Fix scope | Agent changes only the files or commands required | No unrelated build churn appears |
| Timing proof | Agent reruns the targeted build | Duration or dominant summary improves measurably |
| Escalation | Agent routes leftover issues to a specialist skill | Follow-up matches the remaining bottleneck |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent skips baseline context | Agent starts from measured cold, warm, or no-op data |
| Agent reports every slow line item equally | Agent ranks by dominant cost first |
| Agent removes analyzers outright | Agent preserves CI enforcement and narrows inner-loop cost |
| Agent blames parallelism for single-project slowness | Agent separates intra-project cost from graph-level scheduling |
