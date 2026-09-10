---
name: dotnet-exception-handling-standards
title: .NET Exception Handling Standards
description: Govern exception types, catch behavior, wrapping, and result-based error handling across .NET code.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1400
prerequisites:
  - dotnet-validation-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-validation-standards
  - dotnet-resilience-standards
  - global-exception-handling
  - global-error-log-library
appliesTo: "**/*.{cs,csproj,json}"
tags:
  - dotnet
  - exceptions
  - error-handling
  - result-pattern
  - resilience
  - analyzers
---

# .NET Exception Handling Standards

Agent keeps exception flow explicit, diagnosable, and safe. Agent uses typed results for expected outcomes and exceptions for exceptional faults.

## When to Use

| Condition | Use |
|---|---|
| Agent defines application exception types or throw sites | Use this skill |
| Agent replaces generic `Exception` use or broad catches | Use this skill |
| Agent decides between exception flow and `Result<T>` flow | Use this skill |
| Agent routes faults to API, worker, or library edge contracts | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Task is logging-only and error model stays unchanged | Do not use this skill |
| Task is retry or timeout policy tuning | Route to resilience skill |
| Rule is pure input validation | Route to validation skill first |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Failure category | Yes | Validation, business, infrastructure, timeout, or unexpected fault |
| Owning layer | Yes | Engine, Manager, Access, API, worker, or library |
| Caller contract | Yes | Exception, `Result<T>`, or Problem Details |
| Diagnostic data | No | Error code, correlation ID, and safe message requirements |

## Workflow

Agent follows these steps:

1. Agent classifies outcome as expected or exceptional.
2. Agent selects typed result flow for expected outcomes.
3. Agent selects specific exception type for exceptional faults.
4. Agent routes translation to the owning edge layer and verifies safe messages.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero generic exception throw sites in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection Pattern | Fix |
|---|---|---|---|
| EXC-001 | Throw sites use specific exception types | `throw new Exception(...)` or `ApplicationException` | Use `Argument*`, `InvalidOperationException`, or domain-specific exception |
| EXC-002 | Expected business or validation outcomes use typed results | Routine rule failure uses exception flow | Return `Result<T>` or equivalent typed failure |
| EXC-003 | Broad catches do not swallow errors | Empty catch or catch with log-only behavior | Catch specific type, rethrow, or translate deliberately |
| EXC-004 | Wrapped exceptions preserve cause | New exception drops original exception | Pass original exception as inner exception |
| EXC-005 | External contracts are generated at owning edge layer | Lower layer writes HTTP or UI error payload | Route translation to API, worker, or UI edge |
| EXC-006 | Messages are safe for external readers | Secret, SQL text, token, or raw payload appears in message | Keep sensitive detail in logs only |
| EXC-007 | Rethrow keeps original stack | `throw ex;` | Use `throw;` |
| EXC-008 | Custom exception shape stays repeatable | One-off constructor patterns across projects | Use shared constructor and error-code pattern |

## Decision Matrix

| Outcome | Use |
|---|---|
| Missing field or invalid request shape | Typed validation result |
| Business rule denies request | Typed business result |
| Caller misuse or invariant breach | Specific `Argument*` or `InvalidOperationException` |
| Dependency outage or unexpected defect | Technical exception, then edge translation |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Exception flow verification | `dotnet test [test-project].csproj --filter Exception` when tests exist | Zero failing tests |
| Generic exception scan | Search changed files for `throw new Exception` and `throw ex;` | Zero matches in changed scope |

## Verification Checklist

Agent verifies:
- [ ] Throw sites use specific exception types
- [ ] Expected outcomes use typed results
- [ ] Broad catches rethrow or translate deliberately
- [ ] Wrapped exceptions preserve cause
- [ ] External messages stay safe
- [ ] Edge layer owns final contract translation

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Normal validation flow throws exception | Return typed validation result |
| Deep layer logs and swallows exception | Rethrow or route translation to edge layer |
| New exception drops original cause | Add inner exception |
| Client receives raw exception text | Generate safe edge contract |
