---

name: openapi-typescript-client-generation

title: OpenAPI TypeScript Client Generation

description: Generate deterministic TypeScript clients from ASP.NET Core OpenAPI documents by using pinned tools, versioned outputs, and runtime wrappers.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: high

estimated_tokens: 1540

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - webapi-error-contract-hardening

  - vue3-msal-authentication

  - frontend-module-consolidation

appliesTo: '**/*.{ts,json,yaml,yml,cs,csproj}'

tags:

  - openapi

  - typescript

  - client

  - generation

---



# OpenAPI TypeScript Client Generation



Agent treats the OpenAPI document as the server contract authority and keeps generated client code isolated from handwritten application logic.



## When to Use



| Condition | Agent Action |

|---|---|

| TypeScript or Vue app consumes ASP.NET Core REST APIs | Use this skill |

| Routes, DTOs, or API versions already exist in OpenAPI | Use this skill |

| Server has no stable machine-readable contract | Do not use this skill |

| Streaming JSON-seq client is the main task | Use protocol-specific guidance instead |



## Contract Rules



| Rule | Agent Verifies | Fix |

|---|---|---|

| OAPI-001 | One versioned OpenAPI document exists per API version | Export or generate versioned document |

| OAPI-002 | Operation IDs are stable and unique | Fix server-side operation metadata |

| OAPI-003 | Generated output lives in a dedicated folder | Isolate generated code from handwritten code |

| OAPI-004 | Generator version is pinned | Pin NSwag or OpenAPI Generator version |

| OAPI-005 | Auth and correlation stay in handwritten wrappers | Add client factory or transport wrapper |

| OAPI-006 | Error mapping preserves Problem Details | Parse `application/problem+json` in wrapper code |

| OAPI-007 | CI detects drift | Add regeneration and diff check |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Export contract | Agent produces one checked or reproducible OpenAPI document per API version. | Inspect generated or committed document path. | Versioned document exists for the changed API scope. |

| 2. Pin generator | Agent selects one generator path and pins its version. | Inspect package or tool configuration. | Generator version is explicit. |

| 3. Generate isolated client | Agent writes generated files into a dedicated versioned folder. | Run existing generation command in scope. | Generated output lands only in the intended folder. |

| 4. Wrap runtime concerns | Agent keeps auth, correlation, and Problem Details mapping outside generated code. | Inspect changed TypeScript wrapper files. | Handwritten runtime logic stays outside generated output. |

| 5. Verify drift and compile | Agent runs generation, diff, and existing app build or test commands in scope. | Run existing generation and build commands in scope. | Clean regeneration produces no unexpected diff and compile succeeds. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Generation verification | Run existing client generation command in scope. | Generated files update deterministically. |

| Drift verification | Run existing diff or check command in scope. | Clean regeneration produces zero unexpected diff. |

| Compile verification | Run existing TypeScript or app build command in scope. | Zero compile errors. |



## Guardrails



- Agent does not hand-edit generated files.

- Agent fixes contract issues at the OpenAPI source when possible.

- Agent keeps versioned client outputs side by side during migrations.

