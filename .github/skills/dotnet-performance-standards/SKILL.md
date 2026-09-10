---
name: dotnet-performance-standards
title: .NET Performance Standards
description: Govern measured hot-path optimization, allocation control, and efficient type choice across .NET code.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1350
prerequisites:
  - analyzing-dotnet-performance
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-async-await-standards
  - analyzing-dotnet-performance
  - microbenchmarking
  - exp-simd-vectorization
appliesTo: "**/*.{cs,csproj}"
tags:
  - dotnet
  - performance
  - allocations
  - linq
  - async
  - analyzers
---

# .NET Performance Standards

Agent optimizes measured hot paths first. Agent keeps clarity until measurement data proves that extra complexity pays for itself.

## When to Use

| Condition | Use |
|---|---|
| Measurement data shows allocation, latency, or throughput pressure | Use this skill |
| Agent reviews hot loops, parsers, serializers, or shared helpers | Use this skill |
| Agent compares `class`, `struct`, pooling, spans, or LINQ alternatives | Use this skill |
| Agent hardens generated code that runs at scale | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Code runs infrequently and has no measurement data | Keep simpler code |
| Optimization is speculative | Gather baseline first |
| Task belongs to build or query optimization scope | Route to narrower skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Baseline measurement | Yes | Profiler, trace, benchmark, or allocation snapshot |
| Hot-path scope | Yes | Method, loop, parser, mapper, or service path |
| Success metric | Yes | Latency, throughput, allocation count, or GC pressure |
| Compatibility constraint | No | API stability, readability, or framework limit |

## Workflow

Agent follows these steps:

1. Agent records baseline measurement for the target scope.
2. Agent applies the rule matrix from dominant cost to minor cost.
3. Agent keeps each optimization isolated and measurable.
4. Agent reruns the same measurement before accepting the optimization.

Test: Run baseline command before and after change.
Pass: Target metric improves. Correctness stays unchanged.

## Rule Matrix

| Rule | Agent verifies | Detection Pattern | Fix |
|---|---|---|---|
| PERF-001 | Hot-path allocations stay minimal | Per-iteration array, list, or object allocation | Reuse buffer, pool object, or move allocation out of loop |
| PERF-002 | Type choice matches data size and copy cost | Tiny immutable data uses heap object or large mutable struct is copied widely | Pick class or struct deliberately |
| PERF-003 | Collections start with useful capacity | Repeated growth in predictable workload | Set initial capacity |
| PERF-004 | String handling avoids repeated churn | Concatenation in loop or repeated casing work | Use `StringBuilder`, `string.Create`, span, or cached normalization |
| PERF-005 | LINQ use is justified on hot path | Multiple enumeration or allocation-heavy chain | Collapse passes or use loop |
| PERF-006 | Async overhead matches measured need | Fake async wrapper or unneeded task churn | Simplify async flow |
| PERF-007 | Boxing is absent on hot path | Value type boxed through interface or non-generic API | Use generic overload or typed collection |
| PERF-008 | Advanced optimization stays evidence-based | Pooling, span, or `ValueTask<T>` added without data | Keep simple code until data exists |

## Optimization Matrix

| Concern | Use | Avoid |
|---|---|---|
| Measurement | Same dataset and environment before and after | Different workload between runs |
| Memory | Localized pooling and clear ownership | Shared pool with unclear lifetime |
| Loops | Simple explicit loop on measured hot path | Multi-pass LINQ chain by habit |
| API shape | Stable external contract | Public contract churn for tiny gain |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Measurement verification | Re-run profiler, benchmark, or trace | Target metric improves |
| Correctness verification | `dotnet test [test-project].csproj` | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] Baseline measurement exists
- [ ] Optimization targets dominant cost
- [ ] Target metric improves measurably
- [ ] Correctness and API behavior stay unchanged
- [ ] Added complexity is localized and justified

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Clever rewrite happens before measurement | Gather baseline first |
| `ValueTask<T>` becomes default | Use `Task<T>` unless data proves benefit |
| Pooling adds unclear ownership | Keep simpler allocation or add clear ownership |
| LINQ is deleted on principle | Replace only when data shows real cost |
