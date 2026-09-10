---
name: thread-abort-migration
description: Migrate `Thread.Abort` patterns to cooperative cancellation with explicit replacement paths and measurable verification.
title: Thread.Abort Migration
doc_type: skill
status: active
last_updated: 2026-07-29
license: MIT
target_audience: ai
complexity: high
estimated_tokens: 1550
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-async-await-standards
  - dotnet-cancellation-token-standards
appliesTo: '**/*.{cs,csproj,sln,slnx,props,targets,config,md}'
tags:
  - thread
  - abort
  - migration
  - cancellation
---
# Thread.Abort Migration

Agent replaces preemptive thread termination with cooperative cancellation, explicit wake-up paths, and verifiable shutdown behavior.

## When to Use

| Condition | Use |
|---|---|
| Agent finds `Thread.Abort`, `ThreadAbortException`, `Thread.ResetAbort`, or `Thread.Interrupt` | Use this skill |
| Agent migrates ASP.NET Framework code that uses `Response.End()` or `Response.Redirect(url, true)` | Use this skill |
| Agent resolves `SYSLIB0006` or post-retargeting `PlatformNotSupportedException` | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Code only uses `Thread.Join`, `Thread.Sleep`, or `Thread.Start` with no abort semantics | No migration is required |
| Project remains on .NET Framework | Keep the existing runtime model |
| Abort behavior exists only in third-party code | Isolate or replace the dependency |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source scope | Yes | Project, file set, or solution |
| Target framework | Yes | Modern .NET target |
| Call-site inventory | Yes | `Thread.Abort`, `ThreadAbortException`, `Thread.ResetAbort`, `Thread.Interrupt`, ASP.NET request-abort patterns |
| Existing tests or repro steps | No | Needed for shutdown verification |

## Migration Workflow

| Step | Agent action | Output |
|---|---|---|
| 1 | Agent inventories every abort-related API and classifies the intent | Usage map |
| 2 | Agent replaces each pattern with the correct cooperative mechanism: `CancellationToken`, timeout orchestration, wait-handle signaling, redirect return, or process isolation | Updated code |
| 3 | Agent removes obsolete catch blocks, resets, pragmas, and abort-only cleanup paths after moving cleanup into `finally` or explicit cancellation handling | Clean source |
| 4 | Agent propagates cancellation through the full call chain and updates tests or harnesses | Cancellable call chain |
| 5 | Agent rebuilds, reruns tests, and proves cancellation stops work within an acceptable time | Verification evidence |

Test: Agent runs the existing build and targeted tests or repro path that exercises cancellation.
Pass: Agent observes zero `Thread.Abort` references in changed scope. Agent observes zero `SYSLIB0006` warnings in changed scope. Agent sees work stop through cooperative cancellation instead of runtime abort.

## Replacement Matrix

| Legacy pattern | Agent replaces with | Verification focus |
|---|---|---|
| `Thread.Abort()` on work loop | `CancellationTokenSource.Cancel()` plus checkpoints | Loop exits promptly |
| `ThreadAbortException` control flow | `OperationCanceledException` or `finally` cleanup | Cleanup still executes |
| `Thread.ResetAbort()` | New work-unit boundary plus cancellation check | Execution continues intentionally, not implicitly |
| `Thread.Interrupt()` wake-up | `WaitHandle.WaitAny`, async wait, or token-aware delay | Blocked work wakes on cancel |
| `Response.End()` / `Redirect(..., true)` | Return from action or redirect result | Request completes without abort |
| Uncooperative code | Separate process and `Process.Kill` at host boundary | Host remains responsive |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Search verification | Repo search for abort APIs in changed scope | Zero remaining matches except documented exclusions |
| Compile verification | `dotnet build [entry]` | Zero build errors |
| Warning verification | Build with warnings visible | Zero `SYSLIB0006` warnings in changed scope |
| Cancellation verification | Existing test or repro that triggers cancellation | Work stops through token or host signal |

## Verification Checklist

Agent verifies:
- [ ] Every abort-related usage was classified before replacement
- [ ] Cooperative cancellation reaches each long-running operation
- [ ] Blocking waits have a token-aware wake-up path
- [ ] Cleanup logic moved out of abort-specific exception flow
- [ ] Build and cancellation checks passed

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Adding a token but never checking it | Insert checkpoints in loops and expensive operations |
| Leaving a synchronous blocking call unchanged | Replace it with a token-aware wait or async API |
| Swallowing `OperationCanceledException` everywhere | Catch only at the orchestration boundary |
| Removing `ThreadAbortException` without preserving cleanup | Move cleanup into `finally` or explicit cancellation hooks |
