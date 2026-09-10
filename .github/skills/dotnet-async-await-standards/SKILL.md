---
name: dotnet-async-await-standards
title: .NET Async Await Standards
description: Govern async signatures, await usage, and cancellation flow across .NET code.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1350
prerequisites:
  - dotnet-performance-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-performance-standards
  - dotnet-exception-handling-standards
  - analyzing-dotnet-performance
appliesTo: "**/*.{cs,csproj}"
tags:
  - dotnet
  - async
  - cancellation
  - performance
  - threading
  - analyzers
---

# .NET Async Await Standards

Agent keeps I/O call chains async end-to-end. Agent avoids blocking waits and accidental task lifetime defects.

## When to Use

| Condition | Use |
|---|---|
| Agent adds database, HTTP, file, stream, or queue calls | Use this skill |
| Agent refactors sync I/O into async code | Use this skill |
| Agent fixes dropped cancellation token flow or blocking waits | Use this skill |
| Agent reviews library await patterns or hot-path task cost | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Work is CPU-bound and stays local | Use synchronous code or explicit parallelism |
| Code is fire-and-forget with no owned lifetime | Define owned lifetime before using async |
| API is truly synchronous and cannot route to async I/O | Keep the API synchronous |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Work type | Yes | I/O-bound or CPU-bound |
| Cancellation source | Yes | Request token, job token, or explicit token source |
| Code scope | Yes | App code, library code, Roslyn tooling, or UI handler |
| Measurement data | No | Use when `ValueTask<T>` or pooling is under consideration |

## Workflow

Agent follows these steps:

1. Agent classifies work type before changing method shape.
2. Agent routes I/O call chains to async APIs from entry point to leaf call.
3. Agent applies the rule matrix to awaits, cancellation flow, and task return types.
4. Agent verifies correctness, cancellation, and measurement data before using advanced optimizations.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero sync-over-async defects in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection Pattern | Fix |
|---|---|---|---|
| ASYNC-001 | I/O calls use async APIs | Sync database, HTTP, file, or stream calls in request paths | Route to true async APIs |
| ASYNC-002 | Callers do not block on tasks | `.Result`, `.Wait()`, `GetAwaiter().GetResult()` | Await task directly |
| ASYNC-003 | Async methods use async suffix | `Task` or `ValueTask` method without `Async` | Rename method and callers |
| ASYNC-004 | Cancellation token flows through every owned async hop | Token accepted but not forwarded | Pass token to nested async calls |
| ASYNC-005 | `ConfigureAwait(false)` appears only in library-style scope | Blind use in app entry code | Keep default await in app code. Use `ConfigureAwait(false)` in reusable libraries |
| ASYNC-006 | `ValueTask<T>` appears only with measurement data | Premature `ValueTask<T>` use | Use `Task<T>` by default |
| ASYNC-007 | `async void` appears only on event handlers | Fire-and-forget methods returning `void` | Return `Task` |
| ASYNC-008 | Background task lifetime stays observable | Started task has no owner, logging, or exception path | Route work through owned service or await path |

## Pattern Matrix

| Concern | Use | Avoid |
|---|---|---|
| Method shape | `Task<T>` with token last | Sync wrapper over async call |
| Library await | `await call.ConfigureAwait(false)` | Context capture by habit |
| App await | `await call` | `ConfigureAwait(false)` by habit |
| Hot path | Measurement before advanced optimization | Speculative `ValueTask<T>` or pooling |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Async behavior verification | `dotnet test [test-project].csproj --filter Async` when async tests exist | Zero failing tests |
| Blocking scan verification | Search changed files for `.Result`, `.Wait()`, `GetAwaiter().GetResult()` | Zero matches in changed scope |

## Verification Checklist

Agent verifies:
- [ ] I/O paths stay async end-to-end
- [ ] Blocking waits are absent
- [ ] Async methods use `Async` suffix
- [ ] Cancellation token flow reaches nested async calls
- [ ] `async void` stays limited to event handlers
- [ ] Advanced optimization has measurement data

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| `Task.Run` wraps sync I/O | Route to true async API or keep sync code |
| Async leaf call keeps sync caller | Write the caller chain to async too |
| `ConfigureAwait(false)` appears everywhere | Limit it to reusable library scope |
| `ValueTask<T>` appears without evidence | Use `Task<T>` |
