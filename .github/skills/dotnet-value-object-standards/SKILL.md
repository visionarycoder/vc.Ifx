---
name: dotnet-value-object-standards
title: .NET Value Object Standards
description: Enforce immutable, validated, equality-safe value objects that reduce primitive obsession in .NET domain models.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1189
prerequisites:
  - dotnet-engine-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-engine-standards
  - dotnet-performance-standards
  - dotnet-exception-handling-standards
appliesTo: "**/*.{cs,csproj}"
tags:
  - dotnet
  - value-objects
  - immutability
  - domain-modeling
  - equality
---

# .NET Value Object Standards

Agent models domain primitives as immutable types with centralized invariants and complete value equality.

## When to Use

| Condition | Use |
|---|---|
| Agent sees raw primitives with domain meaning | Use this skill |
| Agent defines format, range, or normalization rules | Use this skill |
| Agent reviews equality and parsing behavior | Use this skill |
| Agent replaces duplicated verification across callers | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Type owns identity and lifecycle | Use entity guidance |
| DTO mirrors external payload without domain invariant | Keep DTO pattern |
| Wrapper adds no invariant or semantic value | Keep primitive |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Domain meaning | Yes | Identifier, money, code, or temporal concept |
| Invariants | Yes | Format, range, normalization, or legal combinations |
| Underlying value shape | Yes | `string`, `int`, `decimal`, `DateOnly`, or compound members |
| Serialization needs | No | JSON, EF Core, or contract adapters |

## Workflow

1. Agent writes the invariants before picking `record` or `class`.
2. Agent centralizes invariant enforcement in constructor or factory scope.
3. Agent implements complete value equality and explicit parsing paths.
4. Agent verifies call sites no longer duplicate primitive rules.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero mutable state remains in changed value-object scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| VO-001 | State is immutable after construction | Public setters or mutable fields | Use constructor-only or init-only members |
| VO-002 | Invariants run in one scope | Callers repeat regex, range, or normalization rules | Move rule into constructor or factory |
| VO-003 | Equality covers full semantic value | Missing equality members or partial comparison | Use record or implement full equality |
| VO-004 | Parsing and conversion stay explicit | Implicit lossy conversion or scattered parsing | Add `Parse`, `TryParse`, or explicit operator |
| VO-005 | Name reflects business meaning | Wrapper name mirrors primitive instead of concept | Rename to domain concept |
| VO-006 | Type choice is deliberate | `record` or `class` chosen by habit | Pick type by construction and equality needs |
| VO-007 | Serialization and persistence adapters are explicit | Value object breaks JSON or EF Core path silently | Add converter or adapter |
| VO-008 | Callers stop duplicating invariant logic | Same primitive guards remain in feature code | Replace caller logic with value object |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| Identifier with format rule | Single-value value object | Raw string plus regex at every call site |
| Money or rate | Compound value object | Raw decimal with hidden currency rule |
| Bounded number | Constructor or factory guard | Setter-based range checks |
| Temporal concept | Dedicated temporal value object | Comment-based `DateTime` meaning |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed files for setters, mutable fields, or duplicated invariant guards | Zero unauthorized matches |
| Behavior verification | Run `dotnet test [test-project].csproj --filter ValueObject` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] State is immutable after construction
- [ ] Invariants run in one scope
- [ ] Equality covers full semantic value
- [ ] Parsing and conversion are explicit
- [ ] Callers stop duplicating primitive rules

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Serializer convenience adds setters | Use converter or constructor-friendly serializer path |
| Equality ignores one component | Include every semantic member |
| Implicit conversion hides data loss | Use explicit conversion |
| Wrapper name mirrors primitive only | Rename to domain concept |
