---
name: breaking-primitive-obsession
title: Breaking Primitive Obsession
description: Replace ambiguous primitives in .NET domain code with value objects, state types, and analyzer-backed modeling rules.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1255
appliesTo: '**/*.{cs,csproj,md}'
prerequisites:
  - dotnet-value-object-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - roslyn-analyzer-authoring
  - roslyn-codefix-authoring
  - roslyn-source-generator-authoring
  - dotnet-code-quality-standards
tags:
  - domain-modeling
  - value-objects
  - analyzers
  - generators
  - primitives
---

# Breaking Primitive Obsession

Agent replaces ambiguous primitives with domain-safe types at the highest-risk scope first.

## When to Use

| Condition | Use |
|---|---|
| Agent finds raw `string`, `int`, `decimal`, `bool`, or `DateTime` values with business meaning | Use this skill |
| Agent sees repeated verification or formatting across call sites | Use this skill |
| Agent models state with flags or magic strings | Use this skill |
| Agent adds analyzer or generator support for strong types | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Primitive is local and carries no business meaning | Keep primitive |
| Scope stays primitive and strong type already exists behind it | Keep adapter scope primitive |
| Change is naming-only with no modeling defect | Use naming guidance |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Primitive hotspots | Yes | Parameters, properties, fields, or DTO members |
| Domain meaning | Yes | Identity, status, money, amount, or date rule |
| Replacement style | Yes | Value object, state type, or generated wrapper |
| Persistence and serialization scope | No | EF Core, JSON, or messaging needs |

## Workflow

1. Agent ranks primitive hotspots by defect risk and reuse.
2. Agent selects value object, state type, or stronger temporal type by scenario.
3. Agent centralizes invariants inside the new type.
4. Agent verifies analyzers, generators, and adapters match the chosen model.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero duplicated invariant logic remains in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| PO-001 | High-risk scope is migrated first | Primitive defect fix starts in low-value local helper | Start with public API, domain, or persistence scope |
| PO-002 | New type encodes real domain meaning | Wrapper type has no invariant or behavior | Add invariant or keep primitive |
| PO-003 | Verification is centralized | Same regex, range, or normalization repeats across callers | Move rule into type constructor or factory |
| PO-004 | State modeling avoids magic values | Strings, ints, or flag clusters represent status | Introduce state type or value object |
| PO-005 | Temporal modeling is explicit | Time zone meaning is hidden inside `DateTime` | Use `DateTimeOffset` or dedicated temporal type |
| PO-006 | Persistence and serialization adapters are explicit | Strong type breaks EF Core or JSON path silently | Add converter or adapter |
| PO-007 | Analyzer or code-fix rules reinforce stable pattern | Tooling generates different model than codebase uses | Align tooling with chosen type pattern |
| PO-008 | Migration scope stays focused | Broad churn touches unrelated files | Limit change to affected scope |

## Pattern Matrix

| Primitive smell | Use | Avoid |
|---|---|---|
| `string` identifier | Value object such as `OrderId` | Repeated `Guid.Parse` or regex checks |
| `decimal` money | Money type with amount and currency | Raw decimal with formatting rules everywhere |
| `string` or `int` status | State type with named values | Magic constants in feature code |
| Ambiguous `DateTime` | `DateTimeOffset` or temporal value object | Local time assumption hidden in comments |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed files for duplicated regex, range guards, or magic status constants | Zero repeated invariant blocks |
| Behavior verification | Run `dotnet test [test-project].csproj --filter ValueObject` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] High-risk scope is migrated first
- [ ] New type carries real domain meaning
- [ ] Invariants live in one scope
- [ ] State values avoid magic strings, ints, or flags
- [ ] Adapters exist for persistence and serialization when required

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Wrapper has no invariant | Keep primitive or add invariant |
| Migration starts in low-value helper | Start at domain scope |
| Magic status constants remain in feature code | Introduce state type |
| EF Core or JSON path breaks after type change | Add converter or adapter |
