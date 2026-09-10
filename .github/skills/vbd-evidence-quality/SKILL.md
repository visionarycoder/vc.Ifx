---

name: vbd-evidence-quality

title: VBD Evidence Quality

description: Audit VBD evidence for coverage, confidence, staleness, traceability, and candidate readiness.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1120

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - vbd-symbolic-mapping

  - vbd-use-case-reconstruction

related_skills:

  - vbd-candidate-estimation

  - vbd-artifact-automation

appliesTo: 'docs/analysis/**/*.{json,md,yml,yaml}'

tags:

  - vbd

  - evidence

  - audit

---



# VBD Evidence Quality



Agent grades modernization evidence by checking whether the evidence set supports a defensible decomposition or migration decision.



## When to Use



| Condition | Agent Action |

|---|---|

| Evidence package supports candidate selection or cutover planning | Use this skill |

| Traceability or confidence gaps need triage | Use this skill |

| Work only generates raw symbol maps | Start with `vbd-symbolic-mapping` |



## Quality Matrix



| Dimension | Agent Verifies | Test | Pass |

|---|---|---|---|

| Integrity | Artifact parses, IDs resolve, and required files exist | Run repository validation in scope. | Zero blocking validation failures. |

| Coverage | Use cases, symbols, gaps, and dependencies are represented | Inspect evidence cross-links. | Every in-scope use case has supporting evidence or an explicit gap. |

| Strength | Findings cite concrete code, config, data, or runtime signals | Review evidence entries. | High-confidence claims have direct evidence. |

| Freshness | Fingerprints, hashes, or timestamps match current source where the workflow records them | Compare against recorded source inputs. | No stale evidence remains unmarked in changed scope. |

| Traceability | Candidate, decision, and task artifacts link back to evidence | Inspect references. | Every downstream claim resolves to upstream evidence. |

| Readiness | Gaps and contradictions are visible | Review readiness summary. | Blocking gaps are explicit, not hidden inside narrative text. |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Verify integrity | Agent runs artifact validation or equivalent repository checks. | Run existing validation command in scope. | Zero blocking validation failures. |

| 2. Measure coverage | Agent maps evidence to use cases, symbols, and gaps. | Inspect changed analysis records. | Every in-scope use case has support, gap, or exclusion. |

| 3. Audit strength and freshness | Agent flags stale, weak, or contradictory evidence. | Review changed evidence notes or reports. | Weak claims are downgraded or removed. |

| 4. Classify readiness | Agent records ready, conditional, or blocked status for the decision target. | Inspect readiness section or report output. | Readiness state is explicit and justified. |

| 5. Verify report accuracy | Agent updates existing tests or validation artifacts when the workflow includes them. | Run existing buildless checks in scope. | Zero failing checks in scope. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Validation verification | Run `vbd-artifacts validate` or repository wrapper in scope. | Zero blocking failures. |

| Traceability verification | Inspect changed references for candidate, task, or decision outputs. | Every changed downstream artifact resolves to evidence. |

| Freshness verification | Compare changed evidence inputs to source fingerprints when recorded. | Stale evidence is absent or explicitly marked. |



## Outputs



- Evidence quality summary for changed scope

- Gap and contradiction list

- Readiness classification with supporting references

