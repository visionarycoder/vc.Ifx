---

name: webapi-error-contract-hardening

title: WebAPI Error Contract Hardening

description: Standardize HTTP error status codes and Problem Details payloads across changed API endpoints.

doc_type: skill

status: active

last_updated: 2026-08-31

target_audience: ai

complexity: low

estimated_tokens: 780

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - webapi-hardening-controller

  - webapi-input-validation

  - webapi-authz-hardening

  - webapi-compatibility-migration

  - rfc-9457-compliance

appliesTo: '**/*.{cs,csproj,sln,slnx,props,targets,json,config}'

tags:

  - webapi

  - error

  - contract

  - hardening

---



# WebAPI Error Contract Hardening



Agent makes error responses deterministic by aligning status semantics and Problem Details payloads across changed endpoints.



## When to Use



| Condition | Agent Action |

|---|---|

| Error payload or status code behavior changes | Use this skill |

| Validation, not-found, conflict, or server-failure paths drift apart | Use this skill |

| Streamed endpoint already wrote response bytes | Pair with `rfc-8091-compliance` |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Classify failures | Agent maps changed paths to caller-correctable 4xx and server-side 5xx categories. | Agent lists changed failure categories. | Every changed failure path has one intended status family. |

| 2. Normalize payload shape | Agent applies one Problem Details contract and stable extension fields in scope. | Inspect changed handlers or API tests. | Changed error responses use one contract shape. |

| 3. Centralize formatting | Agent moves repeated ad hoc payload code into shared filters, middleware, or helper paths already used by the project. | Review changed files for repeated error builders. | Zero repeated ad hoc builders remain in changed scope. |

| 4. Verify outcomes | Agent runs existing API tests for changed failures. | Run existing build and API test commands in scope. | Zero build errors and zero failing tests. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Status verification | Run existing tests for invalid input, not found, conflict, or server failure paths. | Status codes match intended semantics. |

| Contract verification | Inspect serialized error payloads in tests. | Payloads use Problem Details fields and media type. |

| Safety verification | Inspect changed `detail` and extension fields. | Zero sensitive diagnostics appear in client payloads. |



## Outputs



- Standardized error response behavior in changed scope

- Centralized formatting path for repeated error logic

- Test evidence for changed error categories
