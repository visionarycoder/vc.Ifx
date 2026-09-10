---
name: dotnet-logging-best-practices
title: .NET Logging Best Practices
description: Apply structured, correlated, privacy-safe, and performance-aware logging patterns across .NET services.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1159
prerequisites:
  - dotnet-logging-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - structured-logging-serilog
  - configuring-opentelemetry-dotnet
  - application-insights-dotnet
  - global-error-log-library
appliesTo: "**/*.{cs,csproj,json,config}"
tags:
  - logging
  - observability
  - dotnet
  - correlation
  - redaction
---

# .NET Logging Best Practices

Agent writes logs that stay structured, actionable, privacy-safe, and cost-aware.

## When to Use

| Condition | Use |
|---|---|
| Agent improves log usefulness across APIs, workers, or jobs | Use this skill |
| Agent converts string logs to structured templates | Use this skill |
| Agent adds correlation scope or `LoggerMessage` helpers | Use this skill |
| Agent fixes noisy or privacy-unsafe logging | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Task is sink-specific host setup only | Route to backend-specific skill |
| Task is tracing-only or metrics-only | Use telemetry-specific skill |
| Debug-only spike has no persistent code change | Keep local debug path |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Host type | Yes | API, worker, function, job, or library |
| Logging backend | Yes | `ILogger`, Serilog, OpenTelemetry logging, or mixed |
| Sensitive-data policy | Yes | Secret, token, and PII rules |
| Volume profile | No | Hot path, routine path, or rare diagnostic path |

## Workflow

1. Agent selects durable events that operators need.
2. Agent uses structured templates, stable property names, and correlation scope.
3. Agent adds `LoggerMessage` helpers for hot or repeated log sites.
4. Agent verifies redaction, level choice, and duplicate-log ownership.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero interpolated production log messages remain in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| LOG-001 | Logs describe durable milestones or failures | Entry and exit noise with no operator value | Keep milestone, dependency, and failure events |
| LOG-002 | Templates use named properties | Concatenation or interpolation hides fields | Use structured template |
| LOG-003 | Hot paths use `LoggerMessage` | High-volume templates parse on every call | Add source-generated helper |
| LOG-004 | Correlation is stable | Request, job, or trace ID is missing | Add scope or property |
| LOG-005 | One owning scope logs each failure | Same defect is logged at every layer | Keep one owner log |
| LOG-006 | Sensitive data stays omitted or sanitized | Secrets, tokens, or raw payloads appear in logs | Remove or sanitize value |
| LOG-007 | Level matches operator action | Normal behavior logs as error or failure hides in debug | Align level with response need |
| LOG-008 | Expensive diagnostic work is guarded | Disabled levels still serialize large objects | Guard with `IsEnabled` |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| Business milestone | Stable event name plus key identifiers | Full object dump |
| Dependency call | Endpoint, duration, status, and correlation | Plain string only |
| Failure | One owner log with code and context | Duplicate logs at every catch block |
| Hot-path debug | `LoggerMessage` plus `IsEnabled` guard | Expensive object serialization |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed files for interpolated production logs, raw payload logging, or duplicate failure logs | Zero unauthorized matches |
| Behavior verification | Run `dotnet test [test-project].csproj --filter Logging` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] Durable events stay structured and queryable
- [ ] Hot paths use efficient helpers
- [ ] Correlation fields stay stable
- [ ] One owner logs each failure
- [ ] Sensitive data stays out of logs

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Full request object is logged | Keep safe summary fields only |
| Failure is logged at each layer | Keep one owner log |
| Interpolated strings hide fields | Use named properties |
| Disabled debug path still serializes payload | Guard with `IsEnabled` |
