---

name: webapi-authz-hardening

title: WebAPI Authorization Hardening

description: Apply explicit least-privilege authorization policies to HTTP endpoints and supporting handlers.

doc_type: skill

status: active

last_updated: 2026-08-31

target_audience: ai

complexity: low

estimated_tokens: 760

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - webapi-hardening-controller

  - webapi-input-validation

  - webapi-compatibility-migration

  - webapi-error-contract-hardening

appliesTo: '**/*.{cs,csproj,sln,slnx,props,targets,json,config}'

tags:

  - webapi

  - authz

  - hardening

---



# WebAPI Authorization Hardening



Agent hardens endpoint access by making authorization intent explicit at every public route.



## When to Use



| Condition | Agent Action |

|---|---|

| Endpoint sensitivity or policy mapping changes | Use this skill |

| Anonymous access exists for a public endpoint | Use this skill |

| Authentication token issuance changes | Pair with `rfc-8725-compliance` |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Classify endpoints | Agent maps each changed endpoint to anonymous, authenticated, privileged, or owner-scoped access. | Agent lists endpoint-to-policy mapping. | Every changed endpoint has one explicit access class. |

| 2. Apply explicit policy | Agent adds `[Authorize]`, `.RequireAuthorization()`, policy names, or explicit anonymous markers. | Inspect changed endpoint declarations. | Zero changed endpoints rely on implicit defaults. |

| 3. Close broad access | Agent removes overly broad roles, scopes, or fallback paths. | Run existing authorization tests or inspect policies in scope. | Zero unintended broad-access paths remain in changed scope. |

| 4. Verify outcomes | Agent runs existing positive and negative authorization tests in scope. | Run existing build and auth test commands in scope. | Zero build errors and zero failing auth tests. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Anonymous access verification | Run existing anonymous-request tests in scope. | Protected endpoints reject anonymous access. |

| Unauthorized access verification | Run existing role or scope mismatch tests in scope. | Insufficient privilege returns intended `403` or `401`. |

| Mapping verification | Inspect changed controllers, minimal APIs, or handlers. | Every changed public endpoint declares one explicit authorization intent. |



## Outputs



- Endpoint-to-policy mapping for changed routes

- Explicit authorization changes in touched scope

- Test evidence for allowed and denied callers
