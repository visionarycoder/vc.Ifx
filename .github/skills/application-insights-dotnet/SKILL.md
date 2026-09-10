---
name: application-insights-dotnet
description: Configure Application Insights telemetry in .NET with automatic collection, custom signals, correlation, sampling, and Kusto validation.
title: Application Insights for .NET
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1290
appliesTo: '**/*.{cs,csproj,json}'
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - structured-logging-serilog
  - configuring-opentelemetry-dotnet
  - global-exception-handling
tags:
  - application-insights
  - telemetry
  - azure
  - observability
---
# Application Insights for .NET

Agent adds Azure Monitor Application Insights telemetry to .NET apps. Agent verifies signal quality, correlation, and cost controls.

## When to Use

| User prompt | Use |
|---|---|
| User asks for request, dependency, or exception telemetry | Use this skill |
| User asks for custom business events or metrics | Use this skill |
| User asks for Kusto validation of telemetry flow | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for a different observability backend only | Use that backend workflow |
| User asks for local logging only | Use logging guidance |
| User asks for vendor-neutral tracing only | Use `configuring-opentelemetry-dotnet` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Host type | Yes | Web app, API, worker, WebJob, or desktop host. |
| Connection string | Yes | Load from configuration or environment. |
| Custom signal plan | No | List events, metrics, or traces beyond auto-collection. |
| Sampling plan | Recommended | Define cost and fidelity targets. |
| Correlation plan | Recommended | Align with `Activity` and structured logging. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent installs the host-appropriate SDK package. | Inspect project file. | Package matches the host type. |
| 2 | Agent reads the connection string from secure configuration. | Inspect config source. | No hard-coded connection string exists. |
| 3 | Agent registers telemetry services. | Inspect startup code. | Automatic request and dependency telemetry are enabled. |
| 4 | Agent adds custom events or metrics only for real business signals. | Review `TelemetryClient` usage. | Custom telemetry names are stable and meaningful. |
| 5 | Agent adds initializers or processors for common context. | Inspect registrations. | Environment, ring, or tenant metadata flows consistently. |
| 6 | Agent configures sampling or filtering. | Review telemetry config. | Noise decreases without hiding important failures. |
| 7 | Agent validates Kusto visibility and correlation. | Run query playbook. | Requests, dependencies, and exceptions join correctly. |

## Pattern Matrix

| Need | Preferred pattern |
|---|---|
| ASP.NET Core auto-collection | `AddApplicationInsightsTelemetry(...)` |
| Business event | `TelemetryClient.TrackEvent(...)` |
| Business metric | `TelemetryClient.GetMetric(...).TrackValue(...)` |
| Stable dimensions | `ITelemetryInitializer` |
| Cost control | Adaptive sampling or processor filters |
| Correlation | Preserve `Activity` context |

## Query Matrix

| Goal | Minimal Kusto pattern |
|---|---|
| Request count and latency | `requests | summarize count(), avg(duration) by name, success` |
| Exceptions by type | `exceptions | summarize count() by type` |
| Dependency failures | `dependencies | where success == false` |
| Correlated trace | Filter by `operation_Id` across tables |

## Validation Checklist

- [ ] Agent used configuration-driven connection strings.
- [ ] Agent enabled automatic request, dependency, and exception collection for the host.
- [ ] Agent added only stable, business-meaningful custom telemetry.
- [ ] Agent configured sampling or filtering intentionally.
- [ ] Agent preserved `Activity`-based correlation.
- [ ] Agent validated telemetry with real Kusto queries.
