---
name: dotnet-api-client-generation
title: .NET API Client Generation
description: Generate typed .NET API clients from OpenAPI with deterministic tooling, isolated output, and wrapper-friendly integration.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1184
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - openapi-typescript-client-generation
  - dotnet-api-client-resilience
  - webapi-error-contract-hardening
appliesTo: '**/*.{cs,csproj,json,yaml,yml}'
tags:
  - openapi
  - client-generation
  - dotnet
  - nswag
  - kiota
---

# .NET API Client Generation

Agent generates API clients from OpenAPI documents and keeps generated output isolated, deterministic, and easy to regenerate.

## When to Use

| Condition | Use |
|---|---|
| Agent consumes a REST API with stable OpenAPI contract | Use this skill |
| Agent replaces drifting handwritten client code | Use this skill |
| Agent standardizes generator config and regeneration command | Use this skill |
| Agent wraps generated code with auth or error adapters | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| OpenAPI contract is missing or unstable | Fix contract first |
| Consumer needs one trivial call only | Use handwritten client |
| Transport is gRPC, GraphQL, or JSON sequence stream | Use transport-specific skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| OpenAPI document | Yes | JSON or YAML source |
| Generation tool | Yes | NSwag, Kiota, Refit, or AutoRest |
| Output folder and namespace | Yes | Overwrite-safe generated scope |
| Auth and error wrapper needs | No | Message handlers, adapters, or exception translation |

## Workflow

1. Agent verifies the OpenAPI document has stable operation IDs and schemas.
2. Agent selects one generator and pins its version.
3. Agent writes generated output to isolated folder and keeps custom logic outside it.
4. Agent verifies regeneration, compile output, and wrapper integration.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Generated output is overwrite-safe.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| APIGEN-001 | Contract is generation-ready | Missing operation IDs or unstable schema names | Fix OpenAPI contract first |
| APIGEN-002 | Generator version stays pinned | Tool version floats outside source control | Pin tool and config |
| APIGEN-003 | Generated code lives in isolated scope | Handwritten and generated files share folder | Split into `Generated` folder |
| APIGEN-004 | Handwritten auth and resilience stay outside generated files | Generated file edited manually | Move custom logic to handler or adapter |
| APIGEN-005 | Namespace and nullable settings match repository rules | Generated code violates naming or nullable conventions | Adjust generator config |
| APIGEN-006 | Regeneration uses one documented command | Contributors reverse-engineer tool invocation | Commit config and one command |
| APIGEN-007 | Error handling is normalized in wrapper scope | App code handles transport defects per call site | Add wrapper or exception translator |
| APIGEN-008 | CI drift check catches stale output | Generated code changes silently | Regenerate and fail on diff |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| .NET-first client with custom config | NSwag | Manual DTO and client drift |
| Request-builder style client | Kiota | Ad hoc URL string assembly |
| Interface-first simple client | Refit | Large generated folder for tiny surface |
| Azure SDK style contract | AutoRest | Custom handwritten transport stack |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Regeneration verification | Run the committed generation command, then inspect git diff | Zero unexpected diff |
| Behavior verification | Run `dotnet test [test-project].csproj --filter ApiClient` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] OpenAPI contract is generation-ready
- [ ] Generator version and config are pinned
- [ ] Generated code lives in isolated scope
- [ ] Custom auth and resilience stay outside generated files
- [ ] CI drift check exists for regenerated output

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Generated file is edited by hand | Move change to config or wrapper |
| Contract lacks stable operation IDs | Fix OpenAPI contract |
| Generated and handwritten files share folder | Split output into isolated scope |
| Regeneration command is undocumented | Commit one command and config |
