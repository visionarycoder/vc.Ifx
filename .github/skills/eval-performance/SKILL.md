---
name: eval-performance
title: MSBuild Evaluation Performance
description: Diagnose and improve MSBuild evaluation cost by measuring evaluation count, import depth, glob breadth, and expensive property functions.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1290
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - build-perf-diagnostics
  - directory-build-organization
appliesTo: '**/*.{csproj,sln,slnx,props,targets,binlog,md}'
tags:
  - eval
  - performance
  - msbuild
---
# MSBuild Evaluation Performance

Agent diagnoses build slowness that appears before target execution starts.

## When to Use

Agent uses this skill when:
- Build startup feels slow before compilation
- Binlog evidence shows heavy project evaluation time
- Import chains, globs, or property functions look expensive
- Same project evaluates repeatedly with different global properties

## When Not to Use

Agent does not use this skill when:
- Primary cost is compilation, analyzers, or copy I/O
- Task is pure incremental-skip troubleshooting
- Build system is not MSBuild based

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Binlog or replay log | No | Preferred for measured evaluation time |
| Project file or scope | Yes | One project, folder, or solution slice |
| Suspect pattern | No | Globs, imports, property functions, or repeated evaluations |

## Evaluation Phases

| Phase | Agent inspects | Common cost source |
|---|---|---|
| Initial properties | Global and environment values | Property fan-out across project instances |
| Imports and properties | `.props`, `.targets`, `PropertyGroup` | Deep import chains |
| Item definitions | `ItemDefinitionGroup` metadata | Large shared metadata graphs |
| Item evaluation | `ItemGroup`, `Include`, `Remove`, `Update` | Broad globs over large trees |
| UsingTask registration | Task discovery | Unnecessary task declarations |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Measure evaluation | Agent reads evaluation duration and evaluation count from binlog or replay output | Agent separates evaluation from execution time | Evaluation cost is explicit |
| 2. Check duplicate evaluations | Agent groups the same project by global property set | Agent compares counts per project | Redundant evaluations are proven or ruled out |
| 3. Inspect structure | Agent reviews import depth, `/pp` output size, and glob breadth | Agent counts deep imports or oversized output | Structural cause is visible |
| 4. Inspect expensive expressions | Agent looks for file I/O or heavy property functions during evaluation | Agent traces the expression site | Agent names the costly expression |
| 5. Verify | Agent reruns the same project or build command | Agent compares evaluation time or count | Evaluation cost drops measurably |

## Diagnostic Signals

| Signal | Interpretation | Fix path |
|---|---|---|
| Preprocessed output exceeds 10K lines | Import chain or central props are heavy | Consolidate or simplify imports |
| Custom glob walks `node_modules`, `.git`, `bin`, or `obj` | Item evaluation is too broad | Narrow glob scope or extend excludes |
| Same project evaluates many times | Global-property fan-out exists | Normalize properties or use graph build |
| Property function reads files during evaluation | Evaluation performs file I/O | Move work out of evaluation or cache the value |
| `TreatAsLocalProperty` is large or noisy | Child-project property isolation is overused | Reduce local-property surface |

## Useful Commands

| Purpose | Command |
|---|---|
| Replay binlog | `dotnet msbuild build.binlog -noconlog -fl -flp:"v=diag;logfile=full.log"` |
| Preprocess project | `dotnet msbuild -pp:full.xml MyProject.csproj` |
| Find evaluation events | `grep -i "Evaluation started\|Evaluation finished" full.log` |

## Validation

| Check | Test | Pass |
|---|---|---|
| Evaluation visibility | Agent measures evaluation separately from execution | Agent reports a concrete evaluation cost |
| Root-cause proof | Agent ties the fix to imports, globs, duplicate evaluations, or property functions | One dominant evaluation issue is named |
| Fix outcome | Agent reruns the same scope | Evaluation time or evaluation count decreases |
| Scope control | Agent avoids unrelated execution-stage tuning | Changes stay inside evaluation-related files or commands |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent blames compilation for startup slowness | Agent measures evaluation first |
| Agent uses `EnableDefaultItems=false` as a first move | Agent narrows custom globs before removing SDK defaults |
| Agent ignores global-property fan-out | Agent groups evaluations by exact property set |
| Agent leaves file I/O inside property functions | Agent moves expensive work out of evaluation |
