---
name: dotnet-telemetry-standards
title: .NET Telemetry Standards
description: Govern OpenTelemetry setup, correlation, metrics, and safe observability patterns across .NET services.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1350
prerequisites:
  - configuring-opentelemetry-dotnet
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-logging-standards
  - dotnet-exception-handling-standards
  - configuring-opentelemetry-dotnet
  - application-insights-dotnet
appliesTo: "**/*.{cs,csproj,json}"
tags:
  - dotnet
  - telemetry
  - opentelemetry
  - observability
  - tracing
  - metrics
---

# .NET Telemetry Standards

Agent keeps traces, metrics, and correlation stable across service calls. Agent keeps telemetry useful without leaking sensitive data or generating incompatible names.

## When to Use

| Condition | Use |
|---|---|
| Agent configures OpenTelemetry in web app, function, worker, or library code | Use this skill |
| Agent adds spans, metrics, or correlation-aware logs | Use this skill |
| Agent reviews telemetry completeness, naming, or privacy | Use this skill |
| Agent standardizes reusable observability helpers | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Task is logging-only | Route to logging skill |
| Task is temporary local diagnostics | Delete temporary instrumentation before completion |
| Task is one-time profiling without persistent instrumentation | Route to performance or trace collection skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Service type | Yes | Web API, function, worker, library, or shared component |
| Export target | Yes | OTLP, Application Insights, console, or approved exporter |
| Service identity | Yes | Stable service name, version, and component name |
| Custom telemetry need | No | Domain metric, span tag, redaction rule, or correlation key |

## Workflow

Agent follows these steps:

1. Agent selects service identity and export target before writing instrumentation.
2. Agent applies the rule matrix to tracing, metrics, and correlation flow.
3. Agent reuses stable `ActivitySource` and `Meter` names for each component.
4. Agent verifies low-cardinality tags and redaction before completion.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero duplicate source-name drift in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection Pattern | Fix |
|---|---|---|---|
| TEL-001 | Entry scope configures OpenTelemetry centrally | Missing `AddOpenTelemetry()` or split setup drift | Register tracing, metrics, exporters, and resource metadata together |
| TEL-002 | `ActivitySource` and `Meter` names stay stable | New source or meter per file | Reuse one source and meter per component |
| TEL-003 | Correlation flows through calls, queues, and logs | Trace IDs or baggage disappear across hops | Pass current activity context and scope data |
| TEL-004 | Service metrics cover count, duration, and error rate when useful | Trace-only service with no service metrics | Add counters and histograms |
| TEL-005 | Tags and metric labels exclude sensitive data | Email, token, payload, or secret in tags | Omit, hash, or sanitize value |
| TEL-006 | Span names and tags stay stable and meaningful | Generic names such as `Process` or `Step1` | Use component and operation name |
| TEL-007 | Exceptions update activity status | Caught exception never reaches activity status | Record exception and set error status |
| TEL-008 | Reusable helpers keep instrumentation repeatable | Boilerplate source and meter code repeats across features | Route to shared helper or generator |

## Pattern Matrix

| Concern | Use | Avoid |
|---|---|---|
| Service identity | One startup resource definition | Per-file name drift |
| Tags | Low-cardinality dimensions | User IDs or unbounded values |
| Spans | Meaningful workflow spans | Span per private method |
| Correlation | Shared trace and request keys | Parallel ad hoc keys |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Telemetry behavior verification | `dotnet test [test-project].csproj --filter Telemetry` when tests exist | Zero failing tests |
| Instrumentation scan | Search changed files for new `ActivitySource` or `Meter` names | Zero unauthorized name drift |

## Verification Checklist

Agent verifies:
- [ ] Entry scope owns OpenTelemetry setup
- [ ] `ActivitySource` and `Meter` names stay stable
- [ ] Correlation flows through downstream calls and logs
- [ ] Metrics use low-cardinality tags
- [ ] Sensitive values stay out of tags and labels
- [ ] Exceptions update activity status

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Each file generates its own source name | Reuse one source per component |
| User identifier becomes metric label | Keep labels bounded and safe |
| Logs replace metrics | Add counters and histograms |
| Generic span names hide workflow | Use component and operation name |
