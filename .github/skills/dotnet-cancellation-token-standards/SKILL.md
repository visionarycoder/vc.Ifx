---
name: dotnet-cancellation-token-standards
title: .NET Cancellation Token Standards
description: Govern cancellation token placement, flow, and handling in async .NET code.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1250
prerequisites:
  - dotnet-async-await-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-async-await-standards
  - dotnet-code-quality-standards
  - dotnet-naming-standards
appliesTo: "**/*.{cs,csproj}"
tags:
  - dotnet
  - async
  - cancellation
  - task
  - signatures
  - analyzers
---

# .NET Cancellation Token Standards

Agent keeps long-running async work cancellable. Agent keeps token ownership visible from caller to leaf call.

## When to Use

| Condition | Use |
|---|---|
| Agent adds async methods that outlive trivial local work | Use this skill |
| Agent routes cancellation through repositories, HTTP calls, streams, or queues | Use this skill |
| Agent refactors signatures to repository conventions | Use this skill |
| Agent reviews loops or batch work for prompt cancellation | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Method is synchronous and has no meaningful cancellation point | Do not add token |
| Framework signature is fixed | Preserve framework signature |
| Helper scope is trivial and token stays unused | Keep helper simple |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Method shape | Yes | Signature and ownership |
| Call chain | Yes | Nested async dependencies that accept token |
| Work type | Yes | Request path, loop, hosted service, or batch step |
| Token owner | No | Request pipeline, job runner, or explicit token source |

## Workflow

Agent follows these steps:

1. Agent decides whether work has a real cancellation point.
2. Agent places token last in every owned signature.
3. Agent routes token through nested async calls and loop checkpoints.
4. Agent verifies prompt stop behavior and non-error cancellation handling.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero token-placement defects in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection Pattern | Fix |
|---|---|---|---|
| CT-001 | `CancellationToken` is last parameter | Token before business parameters | Move token to final position |
| CT-002 | Owned public async APIs use `= default` when caller convenience matters | Required token with no clear reason | Add `= default` |
| CT-003 | Nested async calls receive caller token | Repository, HTTP, stream, or queue call without token | Pass same token through |
| CT-004 | Long-running loops observe cancellation | Loop exits only after full batch | Use `ThrowIfCancellationRequested` or prompt exit checkpoint |
| CT-005 | Token sources stay intentional | New token source hides caller token | Use caller token or linked source with clear reason |
| CT-006 | Expected cancellation is not logged as error | Error-level log for caller cancellation | Lower level or silent exit |
| CT-007 | Token parameter is meaningful | Token accepted but unused | Use token or delete token from trivial helper |
| CT-008 | Token naming stays stable | Mixed aliases such as `token`, `ct`, `cancel` in one scope | Use one stable name such as `cancellationToken` |

## Scenario Matrix

| Scenario | Use |
|---|---|
| Public async API under local control | Token last with `= default` |
| Nested async dependency accepts token | Forward caller token |
| Long-running loop | Add checkpoint at safe interval |
| Caller cancellation is routine | Stop quietly or log below error |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Token flow verification | `dotnet test [test-project].csproj --filter Cancellation` when tests exist | Zero failing tests |
| Signature scan verification | Search changed files for `CancellationToken` not in final parameter position | Zero matches in changed scope |

## Verification Checklist

Agent verifies:
- [ ] Token placement is stable
- [ ] Token flow reaches nested async calls
- [ ] Long-running work stops promptly
- [ ] Expected cancellation stays out of error logs
- [ ] New token sources have clear ownership

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Token parameter exists but never flows to nested call | Pass token through or delete it |
| Each method generates a new token source | Reuse caller token |
| Error log records routine cancellation | Lower level or silent exit |
| Loop reads large batch before first checkpoint | Add earlier checkpoint |
