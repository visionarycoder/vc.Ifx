---
name: dotnet-logging-standards
title: .NET Logging Standards
description: Govern structured logging, correlation, redaction, and level discipline across .NET services.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1300
prerequisites:
  - dotnet-cancellation-token-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - configuring-opentelemetry-dotnet
  - dotnet-code-quality-standards
  - dotnet-naming-standards
  - global-error-log-library
appliesTo: "**/*.{cs,csproj,json}"
tags:
  - dotnet
  - logging
  - structured-logging
  - opentelemetry
  - redaction
  - analyzers
---

# .NET Logging Standards

Agent keeps logs structured, searchable, correlation-friendly, and safe. Agent writes logs that help operators without leaking secrets or generating noise.

## When to Use

| Condition | Use |
|---|---|
| Agent adds or reviews logging in any .NET layer | Use this skill |
| Agent converts interpolated logs to structured templates | Use this skill |
| Agent standardizes correlation IDs or telemetry-enriched logs | Use this skill |
| Agent reviews sensitive-data exposure or log-level drift | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Log line is temporary local debugging | Delete log line before completion |
| Task changes only metrics or spans | Route to telemetry skill |
| Task designs user-facing error text | Route to API or UX contract skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Workflow scope | Yes | Request, batch job, command handler, or background service |
| Failure modes | Yes | Warning and error outcomes operators need |
| Sensitive fields | Yes | Secret, token, PII, or payload data to omit or redact |
| Correlation key | No | Trace ID, request ID, job ID, or domain key |

## Workflow

Agent follows these steps:

1. Agent selects operator-facing events for the workflow.
2. Agent applies the rule matrix to message templates, property names, and levels.
3. Agent routes correlation keys through scopes and downstream calls.
4. Agent verifies redaction and noise level before completion.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero interpolated logging defects in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection Pattern | Fix |
|---|---|---|---|
| LOG-001 | `ILogger` placement stays predictable | Mixed constructor or method parameter order | Use repository parameter order |
| LOG-002 | Repeated logs use `LoggerMessage` source generation | Copy-pasted templates across files | Route to generated logging helper |
| LOG-003 | Logs use structured templates | String interpolation or concatenation in log call | Use template with named properties |
| LOG-004 | Correlation flows through scopes and downstream calls | Log lines cannot route to request or activity | Add scope or correlation properties |
| LOG-005 | Sensitive values stay omitted or redacted | Secret, token, PII, or full payload in log | Omit, hash, or sanitize value |
| LOG-006 | Level matches operator action | Error log for expected outcome or information log for chatter | Lower or raise level to match action |
| LOG-007 | Expensive payload generation stays guarded | Serialization or formatting on disabled level | Guard with `IsEnabled` |
| LOG-008 | Event names and properties stay stable | Same event uses different text or property names | Reuse one event contract |

## Pattern Matrix

| Concern | Use | Avoid |
|---|---|---|
| Message text | Short operator-facing text | Raw payload dump |
| Properties | Stable named properties | Positional or unnamed data |
| Failure logging | One owning layer logs recovery decision | Same exception logged at every layer |
| Correlation | Scope plus trace or request key | Ad hoc identifiers per file |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Logging behavior verification | `dotnet test [test-project].csproj --filter Logging` when tests exist | Zero failing tests |
| Template scan verification | Search changed files for `$"` inside logger calls | Zero matches in changed scope |

## Verification Checklist

Agent verifies:
- [ ] Logs use structured templates
- [ ] Sensitive values stay omitted or redacted
- [ ] Correlation data is present
- [ ] Log level matches operator action
- [ ] Expensive payload generation is guarded when needed
- [ ] Repeated events use stable names and properties

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Whole request object is logged by habit | Log only needed fields |
| Same exception is logged at every layer | Log once at owning layer |
| One identifier uses many property names | Reuse one property name |
| Debug chatter becomes permanent | Keep only durable operator-facing logs |
