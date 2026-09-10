---
name: global-error-log-library
title: Global Error Log Library
description: Design a shared .NET error catalog and recording pipeline with explicit classification, correlation, translation, and query behavior.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1450
prerequisites:
  - dotnet-exception-handling-standards
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - global-exception-handling
  - configuring-opentelemetry-dotnet
  - dotnet-code-quality-standards
  - application-insights-dotnet
appliesTo: "**/*.{cs,csproj,json,config}"
tags:
  - dotnet
  - errors
  - result-pattern
  - telemetry
  - problem-details
  - ifx
---

# Global Error Log Library

Agent designs one shared error model that classifies failures, records occurrences, and translates safe external contracts.

## When to Use

| Condition | Use |
|---|---|
| Agent builds or extends a shared `Error` or `Result<T>` library | Use this skill |
| Agent normalizes error recording across APIs, jobs, functions, or workers | Use this skill |
| Agent maps internal failures to Problem Details or other boundary contracts | Use this skill |
| Agent adds queryable correlation-aware error persistence | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Work changes only local log wording | Use normal logging patterns |
| Work fixes one middleware path with no shared model impact | Use targeted exception-handling changes |
| External platform fully owns error identity and storage | Follow the platform contract |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Catalog source | Yes | Existing codes, workbook, or seeded descriptors |
| Host set | Yes | APIs, jobs, functions, workers, or libraries |
| Recording target | Yes | Table, event stream, trace store, or log sink |
| Boundary contract | No | Problem Details, UI contract, or partner contract |
| Correlation fields | No | Trace ID, run ID, job ID, tenant ID, or request ID |

## Required Workflow

Agent performs these steps:
1. Agent defines stable error descriptors with durable codes and categories.
2. Agent separates catalog descriptors from runtime error occurrences.
3. Agent records correlation and sanitized metadata through one shared pipeline.
4. Agent translates internal failures to safe boundary contracts at the edge.
5. Agent validates query, alert, and contract behavior end to end.

Test: Agent runs `dotnet build [project].csproj` and existing tests that cover descriptor lookup, recording, and boundary translation.
Pass: Agent observes zero compile errors. Agent confirms one successful record path and one safe external translation path in the changed scope.

## Error Lifecycle Matrix

| Stage | Agent verifies | Preferred pattern | Avoid |
|---|---|---|---|
| Classification | Error identity stays stable | Shared descriptor with code, title, and category | Free-form strings as the only identity |
| Detection | Expected failure path stays explicit | `Result<T>` or typed domain failure | Throw-only design for routine validation |
| Recording | Runtime occurrence captures context | Shared record with correlation and sanitized metadata | Ad hoc per-service error payloads |
| Translation | Boundary contract stays safe | Problem Details or boundary mapper at the edge | HTTP or UI payload creation in low-level code |
| Query and alert | Operators group errors consistently | Stable code, severity, and domain fields | Analytics built on message text |

## Model Matrix

| Type | Responsibility | Notes |
|---|---|---|
| Descriptor | Static code, title, category, and guidance | Version carefully |
| Runtime error | One occurrence with metadata and correlation | Keep metadata sanitized |
| `Result<T>` | Expected success/failure transport | Use for routine business outcomes |
| Boundary translator | Safe external contract mapping | Keep operator detail internal |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Catalog verification | Existing unit test for descriptor lookup or uniqueness | Codes remain unique and stable |
| Recording verification | Existing integration or unit test for shared recorder | Record contains correlation and sanitized metadata |
| Boundary verification | Existing API or adapter test | External payload contains safe fields only |

## Verification Checklist

Agent verifies:
- [ ] Catalog codes are durable and centrally owned
- [ ] Runtime records stay separate from descriptors
- [ ] Recording pipeline captures correlation identifiers
- [ ] Metadata excludes secrets and unsafe diagnostics
- [ ] Boundary translation occurs only at the edge
- [ ] Query and alert fields stay stable

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Message text acts as the error code | Add stable descriptors |
| Client-safe and operator-safe detail share one field | Split external detail from internal diagnostics |
| Each host defines its own error schema | Centralize recording and translation |
| Error storage omits correlation fields | Add trace, run, request, or tenant identifiers |
