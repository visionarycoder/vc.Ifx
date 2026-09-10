---

name: vbd-operational-contracts

title: VBD Operational Contracts

description: Standardize versioning, compatibility, observability, health, and correlation contracts across VBD components.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1280

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - vbd-system-design

  - ifx-component-communication

related_skills:

  - vbd-boundary-contract-mapping

  - vbd-use-case-migration

  - vbd-architecture-conformance

appliesTo: '**/*.{cs,csproj,json,md,yml,yaml}'

tags:

  - vbd

  - contracts

  - observability

---



# VBD Operational Contracts



Agent defines the non-functional contract that keeps decomposed components compatible, diagnosable, and operable.



## When to Use



| Condition | Agent Action |

|---|---|

| Component boundary, versioning, or compatibility rules change | Use this skill |

| Logging, metrics, traces, or health contracts need alignment | Use this skill |

| Work only maps data shapes between services | Pair with `vbd-boundary-contract-mapping` |



## Contract Matrix



| Contract Area | Agent Verifies | Fix |

|---|---|---|

| API and message versioning | Version identifier, compatibility window, and retirement path are explicit | Add version and deprecation rules |

| Error semantics | Caller-correctable and server-failure paths are stable | Align status or error code taxonomy |

| Correlation | Request, message, and job flows carry one correlation scheme | Add correlation field propagation |

| Logging | Log event names and key dimensions are stable | Normalize structured log schema |

| Metrics | Counters, histograms, and dimensions reflect stable semantics | Align metric names and units |

| Tracing | Spans and attributes describe boundary operations consistently | Normalize activity names and tags |

| Health | Readiness and liveness signals match dependency reality | Add health endpoint or check policy |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Define boundary set | Agent lists the components, APIs, jobs, and events in scope. | Agent records changed boundaries. | Every changed boundary appears once. |

| 2. Normalize operational contract | Agent aligns versioning, correlation, logs, metrics, traces, and health semantics across those boundaries. | Inspect changed configs, code, and docs. | Each changed boundary has one explicit operational contract. |

| 3. Preserve compatibility | Agent isolates compatibility shims and deprecation paths where older callers remain. | Review changed compatibility markers or adapters. | Compatibility scope is explicit and localized. |

| 4. Verify observability and health | Agent runs existing build and operational tests in scope. | Run existing build and test commands in scope. | Zero build errors and zero failing tests. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Contract verification | Inspect changed public docs, configs, or code paths. | Versioning, correlation, and health rules are explicit. |

| Observability verification | Run existing telemetry or integration tests in scope when they exist. | Changed logs, metrics, or spans use stable names and dimensions. |

| Compatibility verification | Run existing compatibility tests in scope. | Older supported callers continue to work inside the preserved window. |



## Outputs



- Operational contract summary for changed boundaries

- Compatibility and retirement notes for preserved versions

- Observability alignment evidence in changed scope

