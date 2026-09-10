---
name: dotnet-immutability-standards
title: .NET Immutability Standards
description: Govern immutable design for DTOs, value objects, events, and configuration models in .NET code.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1300
prerequisites:
  - dotnet-dto-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-dto-standards
  - dotnet-value-object-standards
  - dotnet-code-quality-standards
appliesTo: "**/*.{cs,csproj}"
tags:
  - dotnet
  - immutability
  - records
  - readonly
  - value-objects
  - design
---

# .NET Immutability Standards

Agent uses immutable design for state-carrying types unless real mutation is required. Agent keeps hidden state changes out of DTO, event, and value-oriented models.

## When to Use

| Condition | Use |
|---|---|
| Agent defines DTOs, value objects, messages, events, or options models | Use this skill |
| Agent reviews mutable properties, fields, or collection exposure | Use this skill |
| Agent chooses between record, sealed class, or readonly struct | Use this skill |
| Agent refactors state carriers for concurrency or caching safety | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Type is ORM entity with lifecycle mutation | Preserve controlled mutation |
| Type is builder used during construction | Keep builder mutable |
| Low-level interop or performance scope requires mutation | Use explicit mutable design |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Type role | Yes | DTO, event, value object, options model, or entity |
| Mutation need | Yes | Whether any member changes after construction |
| Collection ownership | Yes | Whether nested collections are copied, wrapped, or exposed |
| Framework constraint | No | Serializer, binder, or ORM rule that limits shape choice |

## Workflow

Agent follows these steps:

1. Agent classifies type as state carrier or lifecycle-driven type.
2. Agent selects immutable shape first for state carriers.
3. Agent applies the rule matrix to fields, collections, equality, and copy behavior.
4. Agent verifies framework compatibility without opening broad mutation paths.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero mutable collection leaks in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection Pattern | Fix |
|---|---|---|---|
| IMM-001 | State carriers use immutable shape by default | DTO or event with mutable public setters | Use record or init-only sealed class |
| IMM-002 | Stable fields are readonly | Constructor-assigned field stays reassignable | Mark field readonly |
| IMM-003 | Mutable collections are not exposed directly | Public `List<T>`, `Dictionary<TKey,TValue>`, or array leak | Expose readonly wrapper or defensive copy |
| IMM-004 | Real mutation stays explicit | Ad hoc setter calls across callers | Route mutation through focused method or builder |
| IMM-005 | Value semantics use value-oriented type | Equality-sensitive data uses mutable reference bag | Use record, record struct, or readonly struct |
| IMM-006 | Normalization happens once at construction | Repeated casing or trim logic across callers | Normalize in constructor or factory |
| IMM-007 | Nested state is truly immutable | Readonly property points to mutable object | Copy or wrap nested object |
| IMM-008 | Mutable exceptions stay documented | Serializer or ORM convenience opens public mutation | Isolate and document exception |

## Type Matrix

| Scenario | Use |
|---|---|
| DTO or event payload | Record or sealed class with init-only properties |
| Small value-oriented type | Readonly struct or record struct |
| Options snapshot after binding | Immutable model after bind step |
| ORM entity | Controlled mutable type |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Behavior verification | `dotnet test [test-project].csproj --filter Immutability` when tests exist | Zero failing tests |
| Collection scan verification | Search changed files for public mutable collections | Zero matches in changed scope unless documented |

## Verification Checklist

Agent verifies:
- [ ] State carriers use immutable shape by default
- [ ] Stable fields are readonly
- [ ] Collections do not leak mutable ownership
- [ ] Equality semantics match type role
- [ ] Mutable exceptions are explicit and documented

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Readonly reference points to mutable nested object | Copy or wrap nested object |
| Serializer convenience adds open setters | Use constructor or converter when available |
| Lifecycle-driven entity gets blanket immutability | Preserve controlled mutation |
| Internal collection is returned directly | Return readonly wrapper or copy |
