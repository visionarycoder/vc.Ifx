---
name: build-perf-baseline
license: MIT
title: Build Performance Baseline & Optimization
description: Establish repeatable MSBuild performance baselines, compare cold warm no-op builds, and apply first-line optimization choices before deeper diagnostics.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1180
prerequisites:
  - binlog-generation
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - build-perf-diagnostics
  - build-parallelism
  - incremental-build
appliesTo: '**/*.{csproj,sln,slnx,props,targets,binlog}'
tags:
  - msbuild
  - performance
  - baseline
  - build
  - diagnostics
---
# Build Performance Baseline & Optimization

Agent establishes repeatable measurements before optimization work starts.

## When to Use

Agent uses this skill when:
- Build slowness is real but root cause is not proven
- User wants before and after evidence for performance work
- No-op or warm builds look too expensive
- Agent needs the correct entry point before deeper diagnostics

## When Not to Use

Agent does not use this skill when:
- Build system is not MSBuild based
- Agent already knows the failing target, task, or incremental rule
- User needs a narrow fix for one custom target

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Build scope | Yes | Solution, project, or filtered subset |
| Environment | Yes | Local machine, CI runner, clean image, or warmed cache |
| Scenario set | Yes | Cold, warm, no-op, or all three |
| Guardrails | No | Limits on server, graph, layout, or caching changes |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Define scope | Agent fixes one build command, one configuration, and one environment | Agent writes the exact command set | Same command set stays stable across all runs |
| 2. Capture baseline | Agent runs cold, warm, and no-op builds with binlogs | Agent preserves timings and binlogs for each run | Agent has one comparable result per scenario |
| 3. Record findings | Agent records total duration, restore time, rebuild breadth, and no-op cost | Agent compares scenarios side by side | Agent identifies one or more evidence-backed red flags |
| 4. Apply first-line changes | Agent applies only changes supported by baseline evidence | Agent reruns the same scenario set | Agent sees measured improvement or rejects the change |
| 5. Escalate deliberately | Agent routes unresolved problems to the next specialist skill | Agent maps each symptom to one follow-up skill | Agent avoids speculative tuning |

## Baseline Matrix

| Scenario | Agent measures | Red flag |
|---|---|---|
| Cold build | Full restore, full compile, total wall clock | Restore or evaluation dominates unexpectedly |
| Warm build | Small-change rebuild scope and duration | Too many projects or targets rerun |
| No-op build | Immediate rebuild with zero source changes | Wall clock stays noticeably high or custom targets rerun |

## First-Line Optimization Map

| Evidence | Agent uses | Expected outcome |
|---|---|---|
| Repeated CLI startup overhead | `msbuild-server` | Lower warm and no-op startup time |
| Idle CPU during multi-project builds | `build-parallelism` guidance | Lower wall clock with the same graph |
| Output collisions or cache confusion | Artifacts layout or `check-bin-obj-clash` guidance | Cleaner bin and obj isolation |
| Warm builds touching too much of the graph | Dependency trimming or graph cleanup | Smaller rebuild blast radius |
| No-op builds still expensive | `incremental-build` guidance | More skipped targets on second build |

## Validation

| Check | Test | Pass |
|---|---|---|
| Scenario consistency | Agent reruns the same command in the same environment | Inputs remain unchanged across before and after runs |
| Evidence preservation | Agent confirms binlogs exist for comparison | Each run has a retained binlog or equivalent timing record |
| Improvement proof | Agent compares before and after timings | At least one target scenario improves measurably or the change is rejected |
| Correct escalation | Agent maps unresolved issues to a follow-up skill | Next step matches the observed symptom |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent optimizes before measuring | Agent captures baseline data first |
| Agent compares different scopes or machines | Agent keeps scope, configuration, and environment fixed |
| Agent treats one fast cold build as enough evidence | Agent measures warm and no-op separately |
| Agent jumps into deep diagnostics too early | Agent escalates only after baseline evidence exists |
