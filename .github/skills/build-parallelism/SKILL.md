---
name: build-parallelism
title: Build Parallelism
description: Optimize MSBuild multi-project scheduling with max CPU count, graph build analysis, and dependency graph cleanup.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1210
prerequisites:
  - build-perf-baseline
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - build-perf-diagnostics
  - incremental-build
appliesTo: '**/*.{csproj,sln,slnx,props,targets,binlog}'
tags:
  - msbuild
  - parallelism
  - build
  - performance
  - scheduling
---
# Build Parallelism

Agent improves wall clock time by increasing useful parallel work across the MSBuild project graph.

## When to Use

Agent uses this skill when:
- Multi-project builds leave CPU capacity idle
- `-m` gives little or no benefit
- User wants graph-build or scheduling guidance
- Build time is dominated by graph shape rather than one slow task

## When Not to Use

Agent does not use this skill when:
- Build scope is one project with no graph breadth
- Primary issue is broken incremental skipping
- Primary issue is one slow compile or analyzer step inside one project

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Build scope | Yes | Solution, solution filter, or multi-project entry point |
| Current command | Yes | Existing build command and `-m` usage |
| Binlog or replay log | No | Helpful for proving node utilization and critical path |
| Constraints | No | CI limits, toolchain limits, or graph-mode restrictions |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Confirm graph scope | Agent verifies that the build includes parallelizable projects | Agent lists the participating projects or solutions | Graph contains independent branches |
| 2. Confirm worker settings | Agent checks `/maxcpucount` or `-m` usage | Agent inspects the command line | Parallel worker count exceeds 1 |
| 3. Measure scheduling | Agent compares wall clock to total project time and node activity | Agent reads binlog or replay evidence | Build shows either good overlap or a clear serial bottleneck |
| 4. Fix graph or command | Agent applies `-m`, `/graph`, `BuildInParallel`, or dependency cleanup | Agent ties change to the observed limit | Change targets the real bottleneck |
| 5. Verify | Agent reruns the same build scope | Agent compares wall clock and project overlap | Wall clock improves or the graph limit is proven structural |

## Decision Matrix

| Evidence | Agent uses | Expected result |
|---|---|---|
| Command runs with one worker | `-m` or `/maxcpucount` | More concurrent project builds |
| Many projects evaluate redundantly | `/graph` | Better scheduling and fewer redundant evaluations |
| Custom `MSBuild` task batches sequentially | `BuildInParallel="true"` | More concurrency inside custom orchestration |
| One project exists only for build order | `ReferenceOutputAssembly="false"` or dependency cleanup | Shorter critical path |
| Only a subset matters | `.slnf` or narrower entry point | Less unnecessary graph work |

## Diagnostic Signals

| Signal | Interpretation |
|---|---|
| Wall clock is close to sum of project times | Graph is mostly serial or one project dominates |
| Slowest project dwarfs all others | Critical path optimization matters more than extra workers |
| Idle nodes appear while one project runs | Dependency chain or custom target serialization exists |
| `/graph` is unavailable because references are dynamic | Project discovery pattern limits graph scheduling |

## Validation

| Check | Test | Pass |
|---|---|---|
| Worker use | Agent reruns with `-m` | Multiple worker nodes appear in evidence |
| Scheduling gain | Agent compares wall clock before and after | Wall clock drops measurably for the same scope |
| Graph correctness | Agent confirms graph mode works with existing references | Build succeeds without dynamic-reference regressions |
| Critical-path proof | Agent reports whether one project still dominates | Remaining limit is explicit rather than guessed |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent expects `-m` to help a single-project build | Agent treats that case as intra-project work, not graph work |
| Agent adds workers without reading the critical path | Agent measures serial bottlenecks first |
| Agent uses `/graph` on dynamic project discovery blindly | Agent confirms `ProjectReference`-driven graph compatibility |
| Agent ignores custom `MSBuild` task serialization | Agent checks `BuildInParallel` where custom orchestration exists |
