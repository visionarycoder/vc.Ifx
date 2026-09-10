---
name: dotnet-validation-standards
title: .NET Validation Standards
description: Govern FluentValidation placement, rule ownership, and validation result flow across .NET code.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1300
prerequisites:
  - dotnet-dto-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-dto-standards
  - dotnet-architectural-layers
  - dotnet-code-quality-standards
  - dotnet-naming-standards
appliesTo: "**/*.{cs,csproj}"
tags:
  - dotnet
  - validation
  - fluentvalidation
  - dto
  - result-pattern
  - analyzers
---

# .NET Validation Standards

Agent keeps request validation centralized, testable, and separate from domain rules. Agent routes invalid request shapes out of the workflow before orchestration starts.

## When to Use

| Condition | Use |
|---|---|
| Agent adds validators for requests, commands, queries, or DTOs | Use this skill |
| Agent moves inline validation from handlers, controllers, or services | Use this skill |
| Agent standardizes validation result flow or Problem Details flow | Use this skill |
| Agent adds async validation rules for entry-scope lookups | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Rule is domain invariant inside entity or value object | Keep rule in domain type |
| Rule is authorization policy | Route to auth or authz skill |
| Rule is cross-aggregate business decision | Keep rule in Engine or Manager |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Target type | Yes | Request DTO, command, query, or contract model |
| Rule source | Yes | Required fields, ranges, formats, cross-field rules, or async lookup |
| Entry scope | Yes | Controller, endpoint, handler, queue consumer, or worker |
| Failure contract | No | Typed result, exception, or Problem Details |

## Workflow

Agent follows these steps:

1. Agent separates request-shape rules from domain invariants.
2. Agent writes dedicated validator classes for request-shape rules.
3. Agent routes validator calls to the entry scope before orchestration.
4. Agent verifies consistent failure flow across all entry points.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero duplicated field-level validation in changed entry scope.

## Rule Matrix

| Rule | Agent verifies | Detection Pattern | Fix |
|---|---|---|---|
| VAL-001 | Dedicated validator class owns request-shape rules | Inline `if` checks in controller, handler, or service | Write `AbstractValidator<T>` class |
| VAL-002 | Entry scope runs validation before orchestration | Manager or Engine receives unchecked request | Route validation to entry scope |
| VAL-003 | Domain invariants stay in domain types | Validator encodes aggregate lifecycle rule | Move invariant into domain type or service |
| VAL-004 | Async validation exists only for real external lookup | Database or network rule added by habit | Keep async rule narrow and measurable |
| VAL-005 | Failure contract stays consistent | Different endpoints format validator errors differently | Use one typed result or Problem Details mapping |
| VAL-006 | Validator name matches target type | Generic validator names hide ownership | Use `<TypeName>Validator` |
| VAL-007 | Rule groups stay focused | One validator mixes unrelated request workflows | Split validator by request type or rule set |
| VAL-008 | Registration stays repeatable | Missing or ad hoc validator registration | Use repository registration pattern |

## Owner Matrix

| Rule Kind | Owner |
|---|---|
| Missing field, range, format, or cross-field request rule | Validator |
| Entity or value-object invariant | Domain type |
| Authorization rule | Auth or authz layer |
| Business decision after request is valid | Engine or Manager |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Validator behavior verification | `dotnet test [test-project].csproj --filter Validation` when tests exist | Zero failing tests |
| Inline rule scan | Search changed handlers, controllers, and services for duplicated field-level checks | Zero duplicated checks in changed scope |

## Verification Checklist

Agent verifies:
- [ ] Validator classes own request-shape rules
- [ ] Entry scope runs validation before orchestration
- [ ] Domain invariants stay in domain code
- [ ] Async validation is narrow and justified
- [ ] Failure contract is consistent
- [ ] Validator registration is repeatable

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Handler grows field-level checks | Write dedicated validator |
| Validator replaces domain invariant | Keep invariant in domain code |
| Async lookup appears in every rule | Limit async lookup to real external need |
| Each endpoint formats errors differently | Use one failure contract |
