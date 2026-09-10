---
name: webapi-hardening-controller
title: Web API Hardening Controller
description: >
  Manages Web API hardening with modes for audit, harden, migrate, and document. Executes multi-track workflow across route standardization, input validation, authorization, error contracts, and compatibility.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 2400
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - webapi-rest-route-standardization
  - webapi-input-validation
  - webapi-authz-hardening
  - webapi-error-contract-hardening
  - webapi-compatibility-migration
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - webapi
  - controller
  - security
  - hardening
  - openapi
---
# Web API Hardening Controller

Agent uses this controller for coordinated Web API hardening, endpoint audit, migration, and OpenAPI generation. Agent selects one mode, loads only the required playbooks, and keeps route, validation, authorization, error-contract, and compatibility work aligned.

## When to Use

| User prompt | Use |
|---|---|
| User asks to audit API endpoints | Use this controller in `audit` mode |
| User asks to harden API security or endpoint behavior | Use this controller in `harden` mode |
| User asks to migrate legacy endpoints or add versioning | Use this controller in `migrate` mode |
| User asks to generate OpenAPI output or API docs | Use this controller in `document` mode |
| User asks for one coordinated pass across several Web API concerns | Use this controller |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for one narrow route fix in one controller | Route to `webapi-rest-route-standardization` |
| User asks for one narrow validation fix | Route to `webapi-input-validation` |
| User asks for one narrow authorization fix | Route to `webapi-authz-hardening` |
| User asks for one narrow Problem Details fix | Route to `webapi-error-contract-hardening` |
| User asks for one narrow compatibility shim only | Route to `webapi-compatibility-migration` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| API scope | Yes | Scope identifies projects, controllers, or endpoint groups in play. |
| User intent | Yes | Intent selects `audit`, `harden`, `migrate`, or `document`. |
| Current endpoint inventory | No | Inventory accelerates controller and route classification. |
| Existing OpenAPI path | No | Path identifies current Swashbuckle, NSwag, or generated spec flow. |
| Compatibility window | No | Window identifies legacy support and retirement timing. |

## Mode Selection Matrix

| User Intent | Mode | What Controller Does |
|---|---|---|
| "Audit API endpoints" | `audit` | Scans all controllers and endpoints, then generates a security and compliance report. |
| "Harden API security" | `harden` | Applies fixes across all five tracks in priority order. |
| "Migrate legacy endpoints" | `migrate` | Adds API versioning, deprecation workflow, and compatibility shims. |
| "Generate OpenAPI spec" | `document` | Creates OpenAPI 3.0 output from the endpoint surface. |

## Bundle Coverage Matrix

| Track | Specialized Skill Source | Controller Reference | Agent Uses When |
|---|---|---|---|
| REST route standardization | `.github/skills/webapi-rest-route-standardization/SKILL.md` | `references/rest-route-standardization.md` | Route cleanup or normalization is in scope |
| Input verification standardization | `.github/skills/webapi-input-validation/SKILL.md` | `references/input-validation-hardening.md` | Request verification behavior is in scope |
| Authorization hardening | `.github/skills/webapi-authz-hardening/SKILL.md` | `references/authz-hardening.md` | Authorization posture is in scope |
| Error contract hardening | `.github/skills/webapi-error-contract-hardening/SKILL.md` | `references/error-contract-hardening.md` | Error payload or status semantics are in scope |
| Compatibility migration | `.github/skills/webapi-compatibility-migration/SKILL.md` | `references/compatibility-migration.md` | Legacy endpoint migration or deprecation is in scope |

## Mode Reference Matrix

| Mode | Required Reference Paths | Output |
|---|---|---|
| `audit` | `references/rest-route-standardization.md`, `references/input-validation-hardening.md`, `references/authz-hardening.md`, `references/error-contract-hardening.md`, `references/compatibility-migration.md` | Endpoint inventory, compliance gaps, top-risk report |
| `harden` | One or more mapped track references from the bundle coverage matrix | Minimal contract-safe fixes plus validation results |
| `migrate` | `references/compatibility-migration.md`, `references/rest-route-standardization.md`, `references/error-contract-hardening.md` | Versioning plan, deprecation flow, migration guide |
| `document` | `references/openapi-generation.md`, `references/error-contract-hardening.md`, `references/authz-hardening.md` | OpenAPI spec, markdown API docs, optional Postman output |

## Audit Mode

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Discover endpoints | Agent scans `[ApiController]` classes and extracts `[Http*]` methods, route templates, verbs, and authorization markers. | Review endpoint inventory. | Inventory lists every discovered controller endpoint once. |
| 2. Analyze security posture | Agent records missing authorization, input-validation gaps, non-standard error contracts, and route conflicts. | Review gap matrix. | Every discovered gap maps to one endpoint or one conflicting route pair. |
| 3. Check compliance | Agent verifies REST verb usage, resource naming, authorization coverage, error-contract standardization, and API versioning presence. | Review compliance matrix. | Every endpoint has one compliance result per audited category. |
| 4. Generate audit report | Agent generates a concise report with totals, top five risk endpoints, and remediation priorities. | Review report format. | Report includes totals, gap counts, and top five risk endpoints. |

### Audit Report Template

```text
API Endpoint Audit:
- Total endpoints: X
- Missing authorization: Y endpoints
- Input validation gaps: Z endpoints
- Route conflicts: N issues

Top 5 Risk Endpoints:
[route] - [verb] - [issues: missing authz, no input validation]
```

## Harden Mode

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Classify scope | Agent identifies controllers, endpoints, and tracks in scope. | Review mapping. | Every endpoint maps to track(s). |
| 2. Read playbooks | Agent reads only mapped track playbooks. | Playbooks loaded. | One playbook per track. |
| 3. Write minimal fixes | Agent applies the smallest contract-safe fixes. | Review diff. | Diff stays in mapped tracks. |
| 4. Verify build and API tests | Agent runs existing build and API tests. | Tests pass. | Zero failures. |
| 5. Report impact | Agent lists changed endpoints and compatibility effects. | Review summary. | All changes are documented. |

### Harden Mode Track Activation

1. Route standardization → `webapi-rest-route-standardization`
2. Input validation → `webapi-input-validation`
3. Authorization → `webapi-authz-hardening`
4. Error contracts → `webapi-error-contract-hardening`
5. Compatibility → `webapi-compatibility-migration`

## Migrate Mode

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Detect version gaps | Agent identifies endpoints without API versioning and records deprecated or drift-prone endpoints. | Review version inventory. | Every in-scope endpoint has a versioning state. |
| 2. Define migration strategy | Agent adds API versioning middleware, creates v2 endpoints for breaking changes, and maps legacy endpoints to compatibility shims. | Review migration map. | Every breaking path has one versioned target and one compatibility plan. |
| 3. Apply deprecation workflow | Agent adds `[Obsolete]` annotations, sunset dates, `Sunset` headers, and migration notes where the host pattern supports them. | Review annotations and response behavior. | Every deprecated endpoint has one visible retirement signal. |
| 4. Verify old and new behavior | Agent runs existing build, API, and compatibility tests in scope. | Run existing validation commands. | Zero build errors and zero failing compatibility tests. |

## Document Mode

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Generate spec surface | Agent configures Swashbuckle or NSwag annotations for endpoints, parameters, responses, security schemes, and RFC 9457 error responses. | Review generated spec. | Every in-scope endpoint appears in the OpenAPI document. |
| 2. Validate output | Agent validates the OpenAPI document against schema rules and checks examples, response contracts, and security entries. | Run existing OpenAPI validation flow. | Validation returns zero schema defects in scope. |
| 3. Produce documentation assets | Agent writes `openapi.json` or `openapi.yaml`, writes markdown API documentation, and writes an optional Postman collection when the repo already generates it. | Review output files. | Required documentation outputs exist and match the generated surface. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Detect mode | Agent reads user intent and selects mode. | Review mode mapping. | Mode is `audit`, `harden`, `migrate`, or `document`. |
| 2. Execute mode | Agent runs the mode-specific workflow. | Review mode output. | Mode-specific output is generated. |
| 3. Delegate to tracks when harden mode is active | Agent invokes track specialists. | Specialists return fixes. | Fixes are applied. |
| 4. Verify changes | Agent runs build and API tests. | Tests pass. | Zero failures. |
| 5. Generate report | Agent creates a mode-specific summary. | Review report. | Report contains required sections. |

## Track Verification Matrix

| Track | Agent Verifies | Test | Pass |
|---|---|---|---|
| Routes | Route templates and verbs match the intended contract. | Run existing endpoint tests or inspect route attributes in changed scope. | Each changed endpoint exposes one intentional route pattern. |
| Input verification | Caller defects return deterministic 4xx responses. | Run existing API tests with invalid input in changed scope. | Invalid input returns an intentional 4xx response. |
| Authorization | Anonymous and unauthorized calls return intended auth results. | Run existing authorization tests in changed scope. | Protected endpoints return intended `401` or `403` results. |
| Error contracts | Error responses use one contract shape. | Run existing API tests or inspect serialized error response. | Changed error responses use the intended status code and one Problem Details contract shape. |
| Compatibility | Preserved legacy contracts remain callable until retirement. | Run existing compatibility tests in changed scope. | Legacy route or contract passes until the retirement step is recorded. |

## Verification Checklist

- [ ] Agent selected one execution mode.
- [ ] Agent loaded only the references required for the selected mode.
- [ ] Agent preserved five hardening tracks.
- [ ] Agent generated the required mode-specific output.
- [ ] Agent ran existing build or API validation commands in scope.
- [ ] Agent kept wording free of STE violations.

## Non-Goals

| Out-of-Scope Prompt | Agent Route |
|---|---|
| Generic C# style refactor | Route to code-quality skill |
| New endpoint implementation planning with no hardening, migration, audit, or documentation goal | Route to scope-specific Web API skill |
| Broad vulnerability sweep outside API behavior | Route to dedicated CWE or security skill |
| Frontend-only API client generation | Route to OpenAPI or frontend client specialist skill |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Audit mode skips anonymous endpoints because no policy attribute is present | Agent records missing authorization as a finding instead of excluding the endpoint. |
| Harden mode rewrites unrelated controller patterns | Agent limits edits to mapped tracks and changed endpoints. |
| Migrate mode adds v2 routes with no retirement path | Agent records deprecation and sunset behavior beside the compatibility shim. |
| Document mode publishes incomplete error responses | Agent includes RFC 9457 responses and security schemes in the generated spec. |
| Controller reads every playbook for every prompt | Agent loads only the playbooks required by the selected mode. |
