---
name: target-authoring
title: Custom Target Authoring Patterns
description: Author MSBuild targets with correct hook selection, chain extension, incrementality, and file tracking so custom build logic stays composable.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1490
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - incremental-build
  - msbuild-antipatterns
appliesTo: '**/*.{csproj,sln,slnx,props,targets,binlog,json,md}'
tags:
  - target
  - authoring
  - msbuild
---
# Custom Target Authoring Patterns

Agent authors or reviews custom MSBuild targets using composable SDK-aligned patterns.

## When to Use

Agent uses this skill when:
- User needs a custom target or target-chain review
- Existing targets replace SDK chains unsafely
- Query targets return stale data or custom targets always rerun
- Agent needs the correct hook strategy for new build logic

## When Not to Use

Agent does not use this skill when:
- Task is only graph-level parallel tuning
- Task is only evaluation or restore troubleshooting
- Build system is not MSBuild based

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Target file scope | Yes | `.csproj`, `.props`, or `.targets` file |
| Behavior goal | Yes | Validation, code generation, packaging, query, or orchestration |
| Hook context | No | Existing target chain or SDK entry point |

## Diagnostic Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Choose the hook | Agent decides between `DependsOnTargets`, `BeforeTargets`, and `AfterTargets` | Agent names the owner of the pipeline | Hook matches ownership and timing |
| 2. Extend safely | Agent appends to existing `$(XxxDependsOn)` values instead of overwriting them | Agent compares old and new chain | Existing SDK targets remain present |
| 3. Declare behavior | Agent uses `Returns` for query results and `Inputs` plus `Outputs` for incrementality | Agent checks target purpose against metadata choice | Query and build targets use the correct contract |
| 4. Track generated files | Agent writes generated outputs to managed locations and adds `FileWrites` | Agent inspects clean and incremental behavior | Generated files are tracked correctly |
| 5. Verify | Agent rebuilds the touched project or solution | Agent checks hook order, target execution, and repeat-build behavior | Target runs exactly when intended |

## Hook Matrix

| Mechanism | Agent uses it when | Agent avoids it when |
|---|---|---|
| `DependsOnTargets` | Agent owns the outer target and needs explicit prerequisites | Agent injects into a pipeline owned elsewhere |
| `BeforeTargets` | Agent inserts logic before a target it does not own | Agent needs the target result as a prerequisite chain |
| `AfterTargets` | Agent inserts logic after a target it does not own | Agent needs to alter the target's prerequisite ordering |

## Contract Rules

| Concern | Agent pattern |
|---|---|
| Chain extension | `$(ExistingDependsOn);MyTarget` |
| Query target | `Returns` without side effects |
| Incremental target | `Inputs` and `Outputs` with deterministic paths |
| Generated artifacts | `$(IntermediateOutputPath)` plus `FileWrites` |
| Extensibility hook | Empty `BeforeXxx` and `AfterXxx` targets for user override |

## Validation

| Check | Test | Pass |
|---|---|---|
| Chain preservation | Agent inspects the final depends-on value | Prior SDK targets remain in the chain |
| Contract correctness | Agent matches `Returns` or `Outputs` to target purpose | Query targets stay fresh and incremental targets skip correctly |
| File tracking | Agent runs clean and rebuild scenarios | Generated files are deleted and rebuilt correctly |
| Hook timing | Agent observes target order in a build log | Custom logic runs before or after the intended target |

## Common Pitfalls

| Pitfall | Agent correction |
|---|---|
| Agent overwrites `$(CompileDependsOn)` | Agent appends to the existing chain |
| Agent uses `Outputs` for query-style targets | Agent uses `Returns` instead |
| Agent defines pipeline hooks in `.props` | Agent moves target logic to `.targets` |
| Agent forgets `FileWrites` and cleanup paths | Agent tracks generated outputs explicitly |
