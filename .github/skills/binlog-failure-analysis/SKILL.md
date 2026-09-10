---
name: binlog-failure-analysis
title: Analyzing MSBuild Failures with Binary Logs
description: Analyze MSBuild binary logs to locate the real failing target, property, item, or evaluation cause and report a minimal corrective action.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1140
prerequisites:
  - binlog-generation
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - build-perf-diagnostics
  - msbuild-modernization
appliesTo: '**/*.{binlog,csproj,sln,slnx,props,targets}'
tags:
  - msbuild
  - binlog
  - diagnostics
  - build
  - dotnet
---
# Analyzing MSBuild Failures with Binary Logs

Agent diagnoses MSBuild failures from an existing `.binlog`.

## When to Use

Agent uses this skill when:
- Console output is incomplete or misleading
- Multi-project failures cascade and hide the first cause
- User needs target order, property, or item evidence
- Agent needs proof before editing project files

## When Not to Use

Agent does not use this skill when:
- User has no binlog and only needs capture guidance
- Build system is not MSBuild based
- Task is pure performance tuning rather than failure analysis

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Binlog path | Yes | Existing `.binlog` file |
| Build scope | Yes | Solution or project represented by the binlog |
| Error symptom | No | Error code or observed failure message |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Load the binlog | Agent uses MCP binlog tools when available, else replay workflow | Agent confirms the binlog is readable | Agent has structured or replayed evidence |
| 2. Find the first real failure | Agent identifies the earliest failing target, task, or evaluation event | Agent traces back from later cascade errors | Agent isolates one primary failure point |
| 3. Inspect context | Agent reads relevant properties, items, imports, or embedded files | Agent checks the exact failing project state | Agent explains why the failure happened |
| 4. Recommend a minimal fix | Agent ties one fix to the proven cause | Agent keeps the change narrow | Recommendation matches the evidence |
| 5. Verify | Agent reruns the failing command with a new binlog when possible | Agent checks that the original failure disappears | Build advances past the prior failure point |

## Evidence Map

| Evidence type | Agent extracts | Typical value |
|---|---|---|
| Errors and warnings | First failing code and project | Primary failure point |
| Target execution | Predecessor target chain | Execution order proof |
| Properties | Effective values at failure time | Wrong path, TFM, RID, or toggle |
| Items | `ProjectReference`, `PackageReference`, inputs | Missing or malformed data |
| Imports | Active props and targets chain | Misordered or conflicting build logic |

## Replay Workflow

| Purpose | Command |
|---|---|
| Replay binlog | `dotnet msbuild build.binlog -noconlog -fl -flp:"v=diag;logfile=full.log;performancesummary" -fl1 -flp1:"errorsonly;logfile=errors.log" -fl2 -flp2:"warningsonly;logfile=warnings.log"` |
| Read primary errors | `cat errors.log` |
| Trace a code | `grep -n -B2 -A2 "CS0246" full.log` |
| Trace failing target | `grep -i "CoreCompile.*FAILED\|Build FAILED\|error MSB" full.log` |

## Validation

| Check | Test | Pass |
|---|---|---|
| First-cause isolation | Agent distinguishes first failure from cascade noise | One primary failure point is named |
| Evidence quality | Agent cites target, task, property, item, or import data | Recommendation is traceable to binlog evidence |
| Fix narrowness | Agent limits edits to the proven source | Unrelated build files stay untouched |
| Verification | Agent reruns the failing command when possible | Original failure does not recur |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent treats cascade errors as separate root causes | Agent traces to the earliest failing event |
| Agent reads a `.binlog` as plain text | Agent uses MCP tools or replay workflow |
| Agent assumes source files exist on disk | Agent uses embedded binlog evidence when files are absent |
| Agent over-investigates after the cause is proven | Agent reports once evidence is sufficient |
