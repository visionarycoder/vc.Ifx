---
name: check-bin-obj-clash
title: Detecting OutputPath and IntermediateOutputPath Clashes
description: Identify MSBuild evaluations that write to the same output or intermediate path and report the minimal path-isolation fix.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1520
prerequisites:
  - binlog-generation
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - msbuild-antipatterns
  - directory-build-organization
appliesTo: '**/*.csproj'
tags:
  - msbuild
  - diagnostics
  - build
---
# Detecting OutputPath and IntermediateOutputPath Clashes

Agent finds project evaluations that share the same `OutputPath` or `IntermediateOutputPath` and recommends the narrowest isolation fix.

## When to Use

Agent uses this skill when:
- Restore reports file-create collisions
- Parallel builds fail intermittently with file-in-use or overwrite errors
- Multi-target or RID builds produce missing or overwritten outputs

## When Not to Use

Agent does not use this skill when:
- File locking is unrelated to MSBuild output paths
- Build scope is one project and one target framework with isolated paths
- Build system is not MSBuild based

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Binlog | Yes | Existing `.binlog` from the failing build |
| Project scope | Yes | Project or solution represented by the binlog |
| Replay log | No | Fallback only when MCP is unavailable |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Capture evidence | Agent opens a binlog or replay log | Agent confirms the file is readable | Evidence source is available |
| 2. Extract evaluations | Agent records project path, evaluation ID, global properties, `OutputPath`, and `IntermediateOutputPath` | Agent builds one record per evaluation | Evaluation table is complete enough to compare paths |
| 3. Normalize paths | Agent resolves every candidate path to an absolute normalized value | Agent groups matching absolute paths | Equivalent paths collapse into one group |
| 4. Apply exclusions | Agent excludes `BuildProjectReferences=false` metadata queries and excludes restore-phase `OutputPath` checks while keeping restore-phase `IntermediateOutputPath` checks | Agent marks excluded rows explicitly | False-positive groups disappear |
| 5. Report clash groups | Agent lists each colliding group with differing global properties | Agent compares group size and property diffs | Each clash has evidence and a minimal fix |
| 6. Verify | Agent rebuilds after the fix | Agent checks for vanished collision errors | Original file collision does not recur |

## Decision Matrix

| Detected pattern | Agent fix |
|---|---|
| Multi-target output path lacks `$(TargetFramework)` | Add `$(TargetFramework)` or enable `AppendTargetFrameworkToOutputPath` and `AppendTargetFrameworkToIntermediateOutputPath` |
| Shared intermediate directory across projects | Give each project a unique `BaseIntermediateOutputPath` |
| RID builds share one output path | Add `$(RuntimeIdentifier)` or enable `AppendRuntimeIdentifierToOutputPath` |
| Same project builds from multiple solution contexts | Separate the invocations or isolate configuration and platform values |
| Path-neutral global property forks create duplicate instances | Remove the property, condition it, or use `RemoveGlobalProperties` on `ProjectReference` |
| Cross-project publish uses nested `<MSBuild>` with extra properties | Consume producer outputs instead of forking publish inside the consumer |

## Evidence Fields

| Field | Agent records |
|---|---|
| ProjectPath | Exact project file path |
| EvaluationID | Distinct evaluation identifier |
| GlobalProperties | Exact property set, not inferred defaults |
| OutputPath | Absolute normalized output path |
| IntermediateOutputPath | Absolute normalized intermediate path |
| Restore marker | Presence or absence of `MSBuildRestoreSessionId` |

## Validation

| Check | Test | Pass |
|---|---|---|
| Evidence quality | Agent reports exact evaluation rows and property diffs | Each clash is traceable to raw evidence |
| False-positive control | Agent applies exclusion rules consistently | Metadata-query rows do not drive findings |
| Fix precision | Agent chooses the smallest path-isolation change | Unrelated build structure stays intact |
| Rebuild proof | Agent reruns a clean or no-incremental build | Collision errors disappear |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent compares raw relative paths only | Agent normalizes to absolute paths first |
| Agent treats restore and build evaluations the same | Agent uses restore-specific rules |
| Agent infers missing global properties | Agent reports only observed property values |
| Agent restructures the repo before trying append flags or unique obj paths | Agent starts with the smallest isolation fix |
