---
name: dotnet-mapping-standards
title: .NET Mapping Standards
description: Standardize dedicated mapping scope, projection patterns, and compile-time-safe DTO transforms in .NET applications.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1157
prerequisites:
  - dotnet-dto-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-dto-standards
  - dotnet-architectural-layers
  - dotnet-immutability-standards
appliesTo: "**/*.{cs,csproj}"
tags:
  - dotnet
  - mapping
  - projections
  - dto
  - automapper
---

# .NET Mapping Standards

Agent keeps transforms explicit, scope-owned, and isolated from orchestration code.

## When to Use

| Condition | Use |
|---|---|
| Agent converts entities, domain types, DTOs, or requests | Use this skill |
| Agent reviews inline property assignment blocks | Use this skill |
| Agent selects manual mapping, projection, or mapping library | Use this skill |
| Agent standardizes generated or compile-time mapping | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Type construction enforces invariants directly | Route to value-object guidance |
| Code formats one field only | Keep local helper |
| Code clones same type with no scope change | Use simple copy pattern |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source type | Yes | Entity, domain type, DTO, or request |
| Target type | Yes | Scope-owned receiving type |
| Mapping direction | Yes | Read projection, write command, adapter, or internal transform |
| Tool choice | No | Manual code, projection expression, library, or generator |

## Workflow

1. Agent identifies the scope that owns the target type.
2. Agent chooses projection, manual mapping, or library configuration by scenario.
3. Agent moves transforms into dedicated mapper scope.
4. Agent verifies completeness, nullability, and query provider behavior.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero inline mapping drift in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| MAP-001 | Mapping code lives in dedicated scope | Inline mapping in controllers, Managers, or repositories | Extract mapper or profile |
| MAP-002 | Target type belongs to receiving scope | Foreign DTO passes through unchanged | Map to local scope type |
| MAP-003 | Queryable reads project before materialization | `ToList()` appears before simple DTO projection | Project in query |
| MAP-004 | Read and write mappings stay separate | One mapper handles incompatible directions | Split mapper by direction |
| MAP-005 | Required members are complete | Silent dropped fields or hidden defaults | Add explicit member mapping |
| MAP-006 | Nullability stays explicit | Nullable source feeds non-null target without rule | Add guard or explicit default |
| MAP-007 | Library use stays compile-time-safe when possible | Reflection-heavy runtime mapping in critical scope | Use explicit code, source generation, or tested config |
| MAP-008 | Naming exposes source and target | Utility names hide direction | Rename mapper for source and target |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| Query read model | Expression projection | Materialize full entity graph first |
| Command mapping | Manual or generated command mapper | Reuse read projection config |
| External adapter | Dedicated adapter mapper | Domain type bound to external payload directly |
| Large DTO family | Source-generated or tested library mapping | Copy-pasted assignment blocks |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed files for inline property copying in controllers, Managers, or repositories | Zero unauthorized matches |
| Behavior verification | Run `dotnet test [test-project].csproj --filter Mapping` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] Mapping code lives in dedicated scope
- [ ] Target type belongs to receiving scope
- [ ] Query projections run before materialization
- [ ] Read and write mappings stay separate
- [ ] Required fields and nullability are explicit

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Mapper code is copied between files | Centralize mapper scope |
| Read mapper is reused for write command | Split by direction |
| Projection runs after `ToList()` | Project in query |
| Foreign DTO leaks across layer | Map to local type |
