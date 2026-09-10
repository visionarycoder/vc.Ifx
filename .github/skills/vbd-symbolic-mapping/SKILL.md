---

name: vbd-symbolic-mapping

title: VBD Symbolic Mapping

description: Build traceable symbol-level evidence that links code, resources, tests, effects, and change drivers.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1180

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - vbd-system-design

related_skills:

  - vbd-use-case-reconstruction

  - vbd-evidence-quality

  - vbd-artifact-automation

appliesTo: '**/*.{cs,csproj,sln,slnx,json,md,yml,yaml}'

tags:

  - vbd

  - symbolic-mapping

  - evidence

---



# VBD Symbolic Mapping



Agent creates method-level and type-level evidence that explains how source code participates in use cases, dependencies, and modernization risk.



## When to Use



| Condition | Agent Action |

|---|---|

| Modernization analysis needs code-grounded evidence | Use this skill |

| Team needs caller, callee, resource, or side-effect mapping | Use this skill |

| Task only judges evidence quality | Pair with `vbd-evidence-quality` |



## Mapping Matrix



| Evidence Type | Agent Captures | Pass Condition |

|---|---|---|

| Symbols | Namespace, type, method, file, and stable ID | Every in-scope record resolves to a concrete source location. |

| Relationships | Callers, callees, dependencies, and external boundaries | Relationship direction is explicit and traceable. |

| Effects | I/O, persistence, messaging, scheduling, and security effects | Each material effect is named once per symbol. |

| Tests and coverage | Existing tests, gaps, and confidence notes | Each critical symbol has test evidence or gap note. |

| Change drivers | Volatility, ownership, coupling, and modernization pressure | Each high-risk symbol records at least one driver. |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Establish source set | Agent records the projects, files, and use-case scope under analysis. | Agent lists source inputs. | Every mapped symbol comes from the recorded source set. |

| 2. Discover symbols | Agent inventories the types, methods, and entry points that matter for the scope. | Inspect changed symbolic records. | Every key entry point appears once. |

| 3. Link behavior | Agent records callers, callees, resources, and side effects for each critical symbol. | Review cross-links in symbolic records. | Critical symbols have explicit relationship and effect data. |

| 4. Link evidence | Agent ties symbols to use cases, tests, gaps, and confidence notes. | Inspect references to use cases and evidence. | Every in-scope use case resolves to supporting symbols or gaps. |

| 5. Validate outputs | Agent runs repository artifact validation when the analysis tree exists. | Run `vbd-artifacts validate` or repository wrapper in scope. | Zero blocking validation failures. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Traceability verification | Inspect changed symbolic records and downstream references. | Every changed symbol record resolves from source to use case or gap. |

| Validation verification | Run repository artifact validation in scope. | Zero blocking failures. |

| Freshness verification | Compare changed symbol records with touched source files. | Mapped evidence reflects current source state. |



## Outputs



- Symbolic records for the changed analysis scope

- Use-case and dependency links for mapped symbols

- Gap notes for unmapped or weak-evidence areas

