---
name: structured-logging-serilog
title: Structured Logging with Serilog
description: Configure Serilog for .NET hosts with structured events, enrichers, sinks, correlation, and safe filtering.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1153
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - application-insights-dotnet
  - configuring-opentelemetry-dotnet
  - global-exception-handling
  - dotnet-logging-standards
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - serilog
  - logging
  - observability
  - dotnet
  - sinks
---

# Structured Logging with Serilog

Agent configures Serilog as structured operational data, not string-only output.

## When to Use

| Condition | Use |
|---|---|
| Agent introduces Serilog into a .NET host | Use this skill |
| Agent configures sinks, enrichers, or host integration | Use this skill |
| Agent adds correlation fields or bootstrap logger | Use this skill |
| Agent fixes noisy or unsafe Serilog output | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Logging backend is fixed and Serilog cannot be introduced | Keep existing backend |
| Change is tracing-only or metrics-only | Use telemetry-specific skill |
| Host uses local debug logging only and no durable sink exists | Keep simple host logger |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Host type | Yes | ASP.NET Core, worker, WebJob, WPF, or console |
| Sink targets | Yes | Console, file, Seq, or Application Insights |
| Correlation fields | No | Trace ID, request ID, or job ID |
| Sensitive-data policy | Yes | Secrets, tokens, and PII rules |

## Workflow

1. Agent selects sink set and bootstrap logger scope.
2. Agent binds Serilog through host configuration and service integration.
3. Agent adds stable enrichers, correlation fields, and category filters.
4. Agent verifies sink queries, redaction, and startup-failure capture.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Structured properties are queryable in the configured sink.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| SERI-001 | Host uses bootstrap logger for early failures | Startup exceptions occur before full logger exists | Add `CreateBootstrapLogger()` |
| SERI-002 | Host integration reads configuration and services | Logger is configured inline only | Use `UseSerilog` with configuration binding |
| SERI-003 | Events use message-template properties | Interpolated strings hide queryable fields | Use named placeholders |
| SERI-004 | Correlation fields are stable | Request or trace correlation is absent | Add log context enrichers |
| SERI-005 | Sink configuration matches host needs | Sink set is ad hoc per environment | Bind sinks through config |
| SERI-006 | Filters reduce framework noise | Framework categories flood sink | Add category overrides |
| SERI-007 | Sensitive data stays redacted | Tokens, payloads, or PII appear in logs | Omit or sanitize values |
| SERI-008 | Custom logic stays outside sink packages | Business logic leaks into logger setup | Keep logger setup declarative |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| Web API host | Bootstrap logger, config binding, request correlation | Console-only string logs |
| Worker or job | Job ID enrichment and category filter | No durable sink |
| Local debug | Console sink plus readable template | Production sink set for local-only debugging |
| Sensitive payload path | Sanitized summary fields | Full body or token logging |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Sink verification | Write sample event, then query sink by structured property | Event is queryable by property |
| Scope verification | Search changed files for interpolated log strings or token logging | Zero unauthorized matches |

## Verification Checklist

Agent verifies:
- [ ] Bootstrap logger captures startup failures
- [ ] Host integration reads configuration and services
- [ ] Message-template properties stay structured
- [ ] Correlation fields stay stable across events
- [ ] Sensitive data stays out of sinks

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Startup failure happens before logger setup | Add bootstrap logger |
| Logs use string interpolation | Use message-template properties |
| Framework noise floods sink | Add category overrides |
| Token or payload data reaches sink | Omit or sanitize values |
