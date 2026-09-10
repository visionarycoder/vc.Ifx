---
title: OpenAPI Generation Reference
doc_type: reference
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 820
---
# OpenAPI Generation Reference

Agent uses this reference when the controller runs document mode or when an API change needs OpenAPI output aligned with the implemented HTTP contract.

## Tooling Selection Matrix

| Condition | Preferred Tooling | Result |
|---|---|---|
| ASP.NET Core app already uses Swashbuckle | Swashbuckle | Generate OpenAPI from controller and DTO metadata |
| Existing codebase already uses NSwag | NSwag | Keep generation flow aligned with existing pipelines |
| Repo publishes checked-in spec artifacts | Existing generator plus committed output path | Keep generated file placement deterministic |
| Repo generates client SDKs from OpenAPI | Existing generator plus schema validation | Preserve client-generation stability |

## Swashbuckle Configuration

| Area | Agent Action | Test | Pass |
|---|---|---|---|
| Service registration | Agent verifies `AddEndpointsApiExplorer()` and `AddSwaggerGen(...)` registration in the host. | Inspect host startup. | Swagger services are registered once. |
| Version documents | Agent defines one Swagger document per API version when the API surface is versioned. | Inspect generated document set. | Each active version has one named document. |
| Security schemes | Agent registers bearer, OAuth, or API-key schemes that match the host auth model. | Inspect `components.securitySchemes`. | Every protected endpoint references a declared scheme. |
| Problem Details | Agent documents RFC 9457 responses for validation, authorization, not-found, conflict, and server-error paths. | Inspect response metadata. | Error responses are present with stable schemas. |
| XML comments | Agent enables XML comments only when the project already emits them or when generation already depends on them. | Inspect build and generator config. | XML comments integrate without new build drift. |

## Annotation Patterns

| Concern | Annotation Pattern | Outcome |
|---|---|---|
| Operation summary | `[ProducesResponseType]`, XML summaries, or equivalent NSwag metadata | Generated docs describe intent clearly |
| Request body | `[FromBody]`, DTO annotations, validation attributes | Request schema stays explicit |
| Route and path parameters | Route templates plus parameter-binding metadata | Generated path variables match implemented routes |
| Query parameters | Explicit method parameters or request objects with clear binding | Optional and required query fields stay visible |
| Authorization | `[Authorize]`, policy names, and security requirements | Consumers can identify protected endpoints |
| Versioning | `ApiVersion`, route segments, or explorer grouping | Versioned docs stay separated and stable |

## Example Response Patterns

| Response Type | Agent Documents | Pass |
|---|---|---|
| Success | Representative 2xx payloads and content types | Consumers can infer the normal contract |
| Validation failure | RFC 9457 Problem Details with validation-specific fields already used by the host | Invalid input contract is visible |
| Unauthorized or forbidden | 401 and 403 responses with auth scheme metadata | Protected-route behavior is visible |
| Not found or conflict | Domain-specific 404 or 409 Problem Details variants | Expected failure paths are visible |
| Server failure | Stable 5xx Problem Details contract without sensitive internals | Unexpected failure contract is visible |

## Generation Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Inventory the implemented surface | Agent lists controllers, minimal APIs, DTOs, auth schemes, and API versions in scope. | Review inventory. | Every in-scope endpoint has metadata inputs. |
| 2. Normalize endpoint metadata | Agent adds missing response annotations, summaries, tags, and security declarations in the smallest scope. | Inspect changed code. | Endpoints expose complete generation metadata. |
| 3. Generate the specification | Agent runs the repository generation path for `openapi.json` or `openapi.yaml`. | Run existing generation command. | Spec file is produced successfully. |
| 4. Validate the specification | Agent runs existing validation or schema checks and inspects a sample of operations manually. | Run existing validation flow. | Spec contains zero blocking validation defects. |
| 5. Publish companion docs | Agent writes markdown API docs and optional Postman output when the repo already uses those artifacts. | Review output set. | Documentation assets match the generated spec. |

## Swashbuckle Implementation Notes

| Topic | Guidance |
|---|---|
| Operation IDs | Agent keeps operation IDs deterministic and stable across rebuilds. |
| Tags | Agent groups operations by controller or business module, matching the repository pattern. |
| Schemas | Agent reuses DTO schemas and avoids anonymous inline object drift where named contracts already exist. |
| Enums | Agent keeps enum serialization and documented values aligned. |
| File uploads | Agent verifies multipart request metadata and content types explicitly. |
| Deprecation | Agent marks deprecated operations in the spec when migration mode records retirement. |

## Validation Checklist

- [ ] Swagger or NSwag services match the host pattern.
- [ ] Every in-scope endpoint appears in the generated document.
- [ ] Security schemes are declared and referenced consistently.
- [ ] RFC 9457 error responses are documented.
- [ ] Generated output validates successfully.
- [ ] Companion markdown or collection output matches the spec path in use.

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Generated spec omits protected-route security requirements | Agent maps auth attributes to explicit OpenAPI security requirements. |
| Generated spec lists 200 responses only | Agent documents common 4xx and 5xx contracts. |
| Versioned endpoints collapse into one document accidentally | Agent groups documents by API version and explorer settings. |
| Examples drift from serialized contracts | Agent derives examples from real DTO shape and response metadata already in code. |
| Generation adds ad hoc paths outside repository conventions | Agent writes spec and docs to the established output path only. |
