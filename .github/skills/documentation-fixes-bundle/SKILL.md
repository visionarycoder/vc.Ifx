---

name: documentation-fixes-bundle

description: Route documentation cleanup requests to the primary STE documentation workflow.

title: Documentation Fixes Bundle Alias

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: low

estimated_tokens: 350

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - frontmatter-standard

related_skills:

  - documentation-fixes

  - documentation-governance

  - documentation-link-repair

  - agent-instruction-governance

appliesTo: '**/*.md'

tags:

  - documentation

  - routing

  - ste

---

# Documentation Fixes Bundle Alias



Agent uses this alias for documentation cleanup entry.



## Routing Table



| Request Type | Primary Route | Supporting Skills |

|---|---|---|

| General documentation cleanup | `documentation-fixes` | Load targeted documentation skills as needed |

| Link and front matter repair | `documentation-fixes` | `documentation-link-repair`, `documentation-frontmatter` |

| Instruction asset placement | `documentation-fixes` | `agent-instruction-governance` |



## Workflow



| Step | Agent Action | Output |

|---|---|---|

| 1. Receive | Agent captures the documentation request. | Request summary |

| 2. Route | Agent forwards primary remediation to `documentation-fixes`. | Primary workflow selection |

| 3. Narrow | Agent loads specialized skills only for active sub-tasks. | Focused skill list |

| 4. Report | Agent returns a concise change summary. | Files updated, moved, or archived |



## Quality Gate



| Check | Test | Pass Criteria |

|---|---|---|

| Primary routing | Compare the request to the routing table. | `documentation-fixes` owns the main workflow. |

| Skill loading | Review loaded skills against active sub-tasks. | No unrelated documentation skill loads. |

| Summary quality | Review final summary content. | Summary lists files changed and actions taken. |

