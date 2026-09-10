---

name: vbd-artifact-automation

title: VBD Artifact Automation

description: Use the dependency-free `vbd-artifacts.js` CLI to validate, graph, plan, score, checkpoint, and report on VBD analysis artifacts.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1330

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - vbd-phased-modernization

related_skills:

  - vbd-phased-modernization

  - vbd-evidence-quality

  - vbd-cutover-migration

  - vbd-change-simulation

  - solution-markdown-sync

appliesTo: 'scripts/vbd-artifacts.js, scripts/tests/vbd-artifacts.test.js, docs/analysis/**/*.json'

tags:

  - vbd

  - automation

  - validation

  - schema

  - ci

---



# VBD Artifact Automation



Agent uses `scripts/vbd-artifacts.js` as the repository authority for VBD artifact validation and reporting.



## When to Use



| Condition | Agent Action |

|---|---|

| New `docs/analysis` tree starts | Use `init` |

| Artifact integrity, references, or schemas need verification | Use `validate` |

| Traceability or dependency view is needed | Use `graph`, `plan`, `score`, or `dashboard` |

| Construction status needs persisted evidence | Use `checkpoint` |



## Command Matrix



| Command | Agent Uses For | Test | Pass |

|---|---|---|---|

| `node scripts/vbd-artifacts.js init <root>` | Standard analysis tree creation | Inspect created folders. | Seven standard analysis directories exist. |

| `node scripts/vbd-artifacts.js validate <root> [--source-root <dir>]` | Schema, ID, reference, cycle, and traceability checks | Run command. | Command returns pass or warn only for accepted conditions. |

| `node scripts/vbd-artifacts.js graph <root> <out.md>` | Mermaid traceability graph | Render graph output. | Graph lists resolved edges and orphan markers. |

| `node scripts/vbd-artifacts.js plan <root> [out.md]` | Critical-path and wave planning | Run command on task set. | Output lists waves and dependency ordering. |

| `node scripts/vbd-artifacts.js score <root> [out.md]` | Advisory candidate scoring | Inspect score output. | Output preserves rationale and weights. |

| `node scripts/vbd-artifacts.js dashboard <root> <out.md>` | Portfolio or engagement rollup | Inspect report output. | Output aggregates status and readiness signals. |

| `node scripts/vbd-artifacts.js checkpoint <root> <phase> <seq>` | Self-validating checkpoint | Run command and inspect file. | Checkpoint records validation evidence and next-ready tasks. |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Classify artifact work | Agent identifies init, validate, render, planning, or checkpoint scope. | Agent records chosen command path. | Each changed artifact task maps to one command path. |

| 2. Run repository tooling | Agent uses `vbd-artifacts.js` instead of ad hoc validation logic. | Review changed scripts or docs in scope. | Zero duplicate validation engine appears in changed scope. |

| 3. Persist evidence | Agent stores validation or reporting output in the expected analysis location. | Inspect generated artifact or report path. | Output path aligns with the VBD analysis tree. |

| 4. Verify automation | Agent runs script tests and validation commands that already exist. | Run `npm run vbd:artifacts:test` and validation command in scope. | Zero failing automation tests and zero blocking validation failures. |



## Validation Coverage



Agent expects the validator to cover:

- Directory presence for the analysis tree

- Well-formed JSON

- Stable ID uniqueness and reference resolution

- Schema conformance for recognized artifact kinds

- Dependency cycle detection

- Optional source and manifest verification

- Orphaned traceability as warning-level findings



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Script test verification | `npm run vbd:artifacts:test` | Zero failing tests. |

| Artifact validation | `node scripts/vbd-artifacts.js validate <root>` or existing npm wrapper | Zero blocking validation failures. |

| Buildless portability verification | Inspect dependencies for the script path. | No new runtime dependency is added for artifact automation. |



## Guardrails



- Agent keeps validation logic in `vbd-artifacts.js` and its tests.

- Agent keeps warning-level orphan findings visible instead of hiding them.

- Agent updates markdown solution inclusion separately when new documentation files appear.

