---
name: dotnet-resilience-standards
title: .NET Resilience Standards
description: Apply retry, timeout, circuit-breaker, fallback, and isolation patterns safely to external .NET dependencies.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1176
prerequisites:
  - dotnet-cancellation-token-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-cancellation-token-standards
  - dotnet-logging-standards
  - dotnet-exception-handling-standards
appliesTo: "**/*.{cs,csproj,json}"
tags:
  - dotnet
  - resilience
  - retry
  - timeout
  - circuit-breaker
---

# .NET Resilience Standards

Agent applies resilience policies deliberately and keeps failures visible, bounded, and safe for the dependency.

## When to Use

| Condition | Use |
|---|---|
| Agent hardens calls to HTTP APIs, queues, storage, or SDKs | Use this skill |
| Agent selects retry, timeout, circuit-breaker, or fallback policy | Use this skill |
| Agent reviews direct `HttpClient` construction | Use this skill |
| Agent standardizes policy registration in composition root | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Call stays in-process | Fail fast without transport resilience |
| Operation is not safe to repeat | Use no-retry design |
| Debug session needs raw failure signal | Remove transient-fault wrapper in debug scope |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Dependency type | Yes | HTTP API, queue, storage, database, or SDK |
| Failure modes | Yes | Timeout, throttling, transient error, or outage |
| Idempotency | Yes | Safe or unsafe to repeat |
| Degradation scope | No | Fallback or fail-fast behavior |

## Workflow

1. Agent classifies dependency type, failure mode, and idempotency.
2. Agent selects bounded policies at the client or adapter scope.
3. Agent adds telemetry for retries, breaker state, and fallbacks.
4. Agent verifies policy safety against user-visible correctness.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero unsafe retry rules in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| RES-001 | External dependency has explicit failure policy | Remote call with no timeout or recovery strategy | Register policy at client scope |
| RES-002 | Retry applies to safe transient faults only | Retry on verification defect, auth defect, or non-idempotent command | Limit retry to safe transient cases |
| RES-003 | Execution time is bounded | Remote call hangs indefinitely | Add timeout and cancellation token |
| RES-004 | Circuit breaker stops repeated outage amplification | Same failing dependency is called continuously | Add breaker with explicit threshold |
| RES-005 | Fallback preserves correctness | Fallback returns silent fake success | Use explicit degraded result or fail fast |
| RES-006 | Isolation protects local resources | Fan-out calls starve thread or connection pools | Add bulkhead or concurrency limit |
| RES-007 | Policies stay observable | Retry and breaker actions have no logs or counters | Add structured telemetry |
| RES-008 | Policy registration stays centralized | Inline ad hoc wrappers differ by caller | Move policy creation to composition root |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| Idempotent GET | Bounded retry with jitter and timeout | Infinite retry |
| Non-idempotent POST | Fail fast or idempotency key | Blind retry |
| Repeated outage | Circuit breaker and health signal | Hammer failing dependency |
| Optional dependency | Explicit degraded result | Silent fake success |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Policy verification | Search changed files for blind retry blocks or `new HttpClient()` | Zero unauthorized matches |
| Behavior verification | Run `dotnet test [test-project].csproj --filter Resilience` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] Retry scope matches safe transient faults only
- [ ] Timeout and cancellation token work together
- [ ] Breaker and isolation limits are explicit
- [ ] Fallback preserves correctness
- [ ] Telemetry exposes policy behavior

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Retry covers every non-success code | Retry safe transient cases only |
| Fallback hides correctness defect | Use explicit degraded result or fail fast |
| Timeout exists without cancellation token | Propagate cancellation token |
| Policies differ across callers | Centralize registration |
