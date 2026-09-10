---
name: webapi-module-consolidation
title: Web API Module Consolidation
description: Consolidate Portal and Scheduler APIs into versioned business modules with stable routes, explicit contracts, shared middleware, and RFC 9457 error behavior.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: high
estimated_tokens: 1242
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - webapi-hardening-controller
  - webapi-input-validation
  - webapi-authz-hardening
  - webapi-compatibility-migration
appliesTo: '**/*.{cs,csproj,json,yaml,yml}'
tags:
  - webapi
  - module
  - consolidation
---
# Web API Module Consolidation

Agent reorganizes Portal and Scheduler APIs into versioned business modules so that module contracts stay explicit while the host owns shared HTTP concerns.

## When to Use

| Condition | Use |
|---|---|
| Agent standardizes API routes under module-based v2 resources. | Agent uses this skill. |
| Agent introduces Scheduler APIs beside existing Portal APIs. | Agent uses this skill. |
| Agent consolidates DTO, middleware, OpenAPI, and authorization patterns across modules. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent fixes one endpoint locally. | Agent uses a focused web API skill. |
| Agent reorganizes frontend modules only. | Agent uses `frontend-module-consolidation`. |
| Agent lacks a compatibility plan for v1 consumers. | Agent defines compatibility first. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Existing controller inventory | Yes | Inventory identifies routes, consumers, and risks. |
| Target module map | Yes | Map identifies Portal and Scheduler capability ownership. |
| Versioning plan | Yes | Plan identifies v1 coexistence and v2 exposure. |
| Contract requirements | Yes | Requirements identify DTOs, errors, and auth policy. |
| Telemetry and OpenAPI rules | No | Rules identify host-owned cross-cutting behavior. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Inventory the current surface | Agent records routes, DTOs, policies, error shapes, and consumers for the existing API surface. | API inventory | Agent reviews the inventory. | Each existing capability has a target module and migration risk. |
| 2. Define versioned module routes | Agent maps capabilities to `/api/v2/{module}/{resource}` routes and keeps v1 behavior isolated. | Route plan | Agent reviews the route plan. | Every v2 route uses consistent module and resource naming. |
| 3. Define module-owned DTOs | Agent creates explicit versioned transport contracts for each module, including Schedule variants where needed. | DTO plan | Agent reviews the transport layer. | Controllers depend on transport DTOs instead of persistence entities. |
| 4. Keep controllers thin | Agent routes validation, authorization, and application work through handlers while controllers stay HTTP-focused. | Controller design | Agent reviews representative endpoints. | Controllers hold HTTP concerns only. |
| 5. Centralize host behavior | Agent keeps correlation, auth, OpenAPI, telemetry, and Problem Details mapping in host infrastructure. | Host infrastructure plan | Agent reviews host registration. | Cross-cutting behavior has one owner. |
| 6. Migrate side by side | Agent adds v2 endpoints, tests them, and moves consumers gradually before deprecating v1. | Migration sequence | Agent compares v1 and v2 behavior. | V1 remains isolated while v2 gains consumers safely. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Build verification | `dotnet build [project].csproj` or `dotnet build Wa.Wsdot.Fin.Idl.slnx` when the changed project belongs to the primary solution | Zero compile errors |
| HTTP verification | Existing integration tests or targeted `dotnet test [test-project].csproj` | Routes, status codes, auth, and content types pass |
| Contract verification | Existing OpenAPI generation path | Operation IDs and schemas stay deterministic |

## Verification Checklist

- [ ] Agent uses `/api/v2/{module}/{resource}` routes.
- [ ] Agent keeps v1 and v2 behavior isolated.
- [ ] Agent uses explicit module DTOs.
- [ ] Agent centralizes Problem Details, telemetry, and auth behavior in the host.
- [ ] Agent preserves cron and calendar-event Schedule semantics where Scheduler contracts need them.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent lets shared DTOs become cross-version coupling points. | Agent creates versioned module transport contracts. |
| Agent routes v2 requests through v1 controllers. | Agent extracts shared application handlers and calls them from both versions. |
| Agent spreads exception mapping across controllers. | Agent centralizes mapping through Problem Details infrastructure. |
| Agent models Schedule as cron only. | Agent keeps a discriminated recurrence contract that supports cron and calendar-event rules. |
