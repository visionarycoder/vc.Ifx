---
name: dotnet-api-client-resilience
title: .NET API Client Resilience
description: Apply `IHttpClientFactory`, bounded resilience policies, token refresh handling, and diagnostics to .NET HTTP clients.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1174
prerequisites:
  - dotnet-resilience-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-api-client-generation
  - wpf-rest-client-integration
  - jwt-authentication-dotnet
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - httpclient
  - resilience
  - polly
  - dotnet
  - diagnostics
---

# .NET API Client Resilience

Agent hardens HTTP clients with explicit registration, bounded policies, and observable failure handling.

## When to Use

| Condition | Use |
|---|---|
| Agent hardens external HTTP client calls | Use this skill |
| Agent standardizes retry, timeout, or breaker behavior | Use this skill |
| Agent handles 401, 429, or 503 responses | Use this skill |
| Agent adds request telemetry and cancellation token flow | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Task is client surface generation only | Route to `dotnet-api-client-generation` |
| Dependency is in-process | Use direct method call |
| Workload is queue or event driven | Use transport-specific resilience guidance |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Target dependency | Yes | Base address or service contract |
| Failure modes | Yes | Timeout, throttling, outage, or auth expiry |
| Client registration style | Yes | Named, typed, or generated client |
| Timeout budget | No | Caller-visible latency budget |
| Telemetry sink | No | Logs, traces, or metrics |

## Workflow

1. Agent classifies endpoint safety, timeout budget, and auth behavior.
2. Agent registers the client through `IHttpClientFactory`.
3. Agent applies bounded retry, timeout, breaker, and token-refresh handlers.
4. Agent verifies status handling, cancellation token flow, and telemetry.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero direct `new HttpClient()` calls in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| HTTPRES-001 | Client uses `IHttpClientFactory` | Direct `new HttpClient()` in feature code | Register named or typed client |
| HTTPRES-002 | Retry covers safe transient cases only | Retry on unsafe POST, auth, or verification defects | Limit retry by method and status |
| HTTPRES-003 | Timeout is explicit and bounded | Hidden default timeout or infinite wait | Set pipeline timeout and pass cancellation token |
| HTTPRES-004 | 401 uses refresh or re-auth path | 401 handled by blind retry | Add auth handler |
| HTTPRES-005 | 429 and 503 respect server guidance | Retry ignores `Retry-After` | Use header-aware delay |
| HTTPRES-006 | Breaker or concurrency guard protects local resources | Outage or fan-out starves caller | Add breaker or bulkhead |
| HTTPRES-007 | Diagnostics expose request outcomes | Retry or breaker actions have no telemetry | Add structured logs or traces |
| HTTPRES-008 | Wrapper code stays outside generated client code | Auth or resilience edits touch generated file | Move logic to handler or adapter |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| Idempotent GET | Bounded retry plus timeout | Blind infinite retry |
| Unsafe POST | No retry or idempotency key | Same retry path as GET |
| 401 response | Refresh token or re-auth handler | Looping transport retry |
| 429 or 503 response | Respect `Retry-After` and cap attempts | Fixed retry delay only |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed files for `new HttpClient()` or blind 401 retry loops | Zero unauthorized matches |
| Behavior verification | Run `dotnet test [test-project].csproj --filter HttpClient` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] Client uses `IHttpClientFactory`
- [ ] Retry rules match safe transient cases only
- [ ] Timeout and cancellation token are explicit
- [ ] 401, 429, and 503 have explicit paths
- [ ] Telemetry exposes request outcomes and policy actions

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Feature code creates `HttpClient` directly | Register factory-managed client |
| Retry loop covers 401 | Add refresh or re-auth path |
| Timeout budget is hidden in defaults | Set explicit timeout |
| Generated file contains custom auth logic | Move logic to handler or adapter |
