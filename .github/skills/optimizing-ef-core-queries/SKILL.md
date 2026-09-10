---
name: optimizing-ef-core-queries
description: Optimize EF Core queries with measurable reductions in round trips, tracking cost, and materialized data.
license: MIT
title: Optimizing EF Core Queries
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1350
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - efcore-dbcontext-design
related_skills:
  - efcore-testing-patterns
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - efcore
  - queries
  - performance
---
# Optimizing EF Core Queries

Agent reduces EF Core query cost by removing unnecessary round trips, tracking, and over-fetching.

## When to Use

| Condition | Use |
|---|---|
| Agent investigates slow LINQ queries or high SQL volume | Use this skill |
| Agent sees N+1 access, large includes, or heavy materialization | Use this skill |
| Agent tunes read paths in handlers, APIs, or background jobs | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Bottleneck is clearly an index or schema problem outside code scope | Coordinate with database design work |
| Work creates a new data layer from scratch | Start with `efcore-dbcontext-design` |
| Issue is only test reliability | Use `efcore-testing-patterns` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Slow query path | Yes | LINQ, handler, endpoint, or repository method |
| Evidence | Yes | SQL logs, profiler output, or reproducible symptom |
| Read/write intent | Yes | Read-only and mutating paths need different tracking choices |
| Data shape | No | Cardinality and relationship depth improve fix selection |

## Optimization Workflow

Agent performs these steps:
1. Agent measures the current query count, result size, and tracking mode.
2. Agent removes N+1 access by projecting, including, or reshaping the query intentionally.
3. Agent applies `AsNoTracking()` or `AsNoTrackingWithIdentityResolution()` for read-only paths.
4. Agent limits selected columns and rows before materialization.
5. Agent uses compiled queries or bulk APIs only on proven hot paths.
6. Agent reruns the same scenario and compares query count, duration, or allocation evidence.

Test: Agent runs the existing targeted test, benchmark, or reproducible request while query logging is enabled.
Pass: Agent observes fewer or equal SQL statements, smaller materialized shape, and zero functional regressions in changed scope.

## Pattern Matrix

| Symptom | Agent checks | Preferred fix |
|---|---|---|
| N+1 queries | Navigation access after materialization | Projection or intentional include |
| High allocation on read path | Tracking enabled for read-only flow | `AsNoTracking()` |
| Large join explosion | Many collection includes | `AsSplitQuery()` or reshaped projection |
| Full-table materialization | `ToList()` before filters | Push `Where`, `Select`, `Take` earlier |
| Repeated hot-path compilation | Same query shape executed frequently | Compiled query after measurement |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Functional verification | Existing targeted tests | Zero failures |
| SQL-count verification | Logging or profiler on the same scenario | Query count reduced or unchanged with better shape |
| Tracking verification | Inspect changed query chain | Read-only paths use explicit tracking choice |
| Regression verification | Repeat the same request twice | Same result payload each time |

## Verification Checklist

Agent verifies:
- [ ] Query count is measured before and after the change
- [ ] Read-only paths use explicit tracking intent
- [ ] Projection removes unused columns or graphs
- [ ] Split-query choice matches collection cardinality
- [ ] Functional behavior remains unchanged

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Adding `Include` after projecting DTOs | Remove `Include`; projection already defines shape |
| Treating compiled queries as the first optimization | Measure first; use compiled queries only for hot paths |
| Keeping lazy loading enabled during investigation | Measure actual SQL and remove hidden navigation fetches |
| Optimizing without baseline evidence | Capture query count or timing before edits |
