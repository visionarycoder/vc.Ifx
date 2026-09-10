---

name: rfc-9457-compliance

title: HTTP Problem Details RFC 9457 Compliance

description: Standardize RFC 9457 Problem Details contracts, handlers, and tests across HTTP APIs.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1120

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - rfc-fixes-bundle

  - rfc-7807-compliance

  - rfc-8091-compliance

  - rfc-8259-compliance

  - webapi-error-contract-hardening

appliesTo: 'AzureAPI/src/Client.Portal.WebApi/**'

tags:

  - rfc

  - 9457

  - problem-details

---



# HTTP Problem Details RFC 9457 Compliance



Agent uses this skill for HTTP error contracts, centralized handlers, and validation or exception responses.



## When to Use



| Condition | Agent Action |

|---|---|

| Endpoint error payload shape changes | Use this skill |

| Shared exception or validation handler changes | Use this skill |

| Preserved legacy RFC 7807 client contract blocks full migration | Pair with `rfc-7807-compliance` |

| Stream already started | Pair with `rfc-8091-compliance` for post-start failure handling |



## Core Rules



| Rule | Agent Verifies | Fix |

|---|---|---|

| RFC9457-001 | Error media type is `application/problem+json` | Set content type explicitly |

| RFC9457-002 | Payload includes `type`, `title`, and `status` | Add canonical fields |

| RFC9457-003 | `detail` stays safe for clients | Remove stack traces, SQL text, and secrets |

| RFC9457-004 | `instance` or trace field supports diagnostics when useful | Add stable request identifier mapping |

| RFC9457-005 | Problem type URIs stay consistent by error category | Centralize type mapping |

| RFC9457-006 | Validation finishes before streaming starts | Return Problem Details before first stream byte |

| RFC9457-007 | Error formatting stays centralized | Use shared middleware, filter, or exception handler |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Map error categories | Agent identifies validation, authorization, not-found, conflict, and server-failure paths in scope. | Agent lists changed error categories or endpoints. | Every changed error path appears once. |

| 2. Normalize contract | Agent applies canonical fields, media type, and stable type URIs. | Inspect changed payloads or API tests. | Changed responses use one Problem Details contract shape. |

| 3. Centralize handling | Agent keeps formatting in shared handlers instead of ad hoc controller code. | Review changed files for repeated per-action payload construction. | Zero new ad hoc Problem Details builders appear in changed scope. |

| 4. Verify outcomes | Agent runs existing API tests for changed paths. | Run existing build and API test commands in scope. | Zero build errors and zero failing tests. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Contract verification | Run existing API tests that hit changed error paths. | Error responses use `application/problem+json` and canonical fields. |

| Safety verification | Inspect changed `detail` and extension fields. | Zero sensitive diagnostics appear in client payloads. |

| Streaming verification | Trigger one pre-stream validation failure when streaming path is touched. | Service returns Problem Details before stream start. |



## Guardrails



- Agent uses RFC 9457 as the default Problem Details contract.

- Agent records compatibility reasons when RFC 7807 behavior stays in place.

- Agent keeps stable machine-readable extensions when clients already depend on them.

