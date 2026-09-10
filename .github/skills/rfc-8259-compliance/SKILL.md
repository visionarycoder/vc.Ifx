---

name: rfc-8259-compliance

title: RFC 8259 JSON Compliance

description: Enforce interoperable JSON syntax, encoding, and serializer behavior for external contracts.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 980

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - rfc-fixes-bundle

  - rfc-9457-compliance

  - rfc-7807-compliance

  - rfc-8091-compliance

  - rfc-8725-compliance

appliesTo: '**/*.{cs,ts,js,json,ps1,md}'

tags:

  - rfc

  - 8259

  - json

---



# RFC 8259 JSON Compliance



Agent uses this skill for external JSON payloads, serializers, parsers, examples, and contract files.



## When to Use



| Condition | Agent Action |

|---|---|

| API, file, or tool emits or parses JSON | Use this skill |

| Stream uses JSON text sequences | Pair with `rfc-8091-compliance` |

| Internal object graph never crosses a contract boundary | Use local project conventions only when the boundary stays internal |



## Core Rules



| Rule | Agent Verifies | Fix |

|---|---|---|

| RFC8259-001 | External payload is valid JSON text | Remove comments, trailing commas, and ad hoc framing |

| RFC8259-002 | Producer emits deterministic property names | Use one serializer policy per contract |

| RFC8259-003 | Strings escape control characters correctly | Use serializer output instead of hand-built JSON |

| RFC8259-004 | Number formatting stays JSON-safe | Serialize finite numeric values only |

| RFC8259-005 | UTF-8 contract path stays consistent | Use UTF-8 for external payloads |

| RFC8259-006 | Duplicate semantic fields do not appear | Remove conflicting or repeated names in one object |

| RFC8259-007 | Documentation examples match runtime payloads | Update examples from real contract shape |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Classify contract | Agent identifies external JSON producers, consumers, and schema or example files in scope. | Agent lists changed JSON boundaries. | Every changed boundary appears once. |

| 2. Normalize serialization | Agent replaces hand-built or ambiguous JSON with one deterministic serializer path. | Inspect changed code or contract files. | Changed payloads contain valid JSON syntax only. |

| 3. Align docs and tests | Agent updates examples, snapshots, or schemas with the runtime payload shape. | Run existing parser, serializer, or contract tests in scope. | Zero failing tests in scope. |

| 4. Verify interoperability | Agent parses changed payloads with existing test coverage or tooling. | Run existing validation command in scope. | Changed payloads parse successfully. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Syntax verification | Run existing JSON parser or serializer tests in scope. | Zero parse failures. |

| Contract verification | Inspect changed examples or snapshots. | Examples match runtime field names and structure. |

| Build verification | Run existing build command for touched projects. | Zero build errors. |



## Guardrails



- Agent avoids manual string concatenation for JSON bodies.

- Agent keeps one field meaning per property name.

- Agent preserves backward-compatible field additions in changed public contracts.

