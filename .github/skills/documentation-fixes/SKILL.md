---

name: documentation-fixes

description: Route documentation remediation through a single STE-compliant workflow.

title: Documentation Fixes Skill

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 900

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - frontmatter-standard

related_skills:

  - documentation-file-placement

  - documentation-frontmatter

  - documentation-governance

  - documentation-link-repair

  - documentation-maintenance

  - documentation-refactor-review

  - documentation-style-collaboration

  - agent-instruction-governance

appliesTo: '**/*.md'

tags:

  - documentation

  - governance

  - ste

---

# Documentation Fixes Skill



Agent runs documentation remediation from one entry point.



## Coverage Map



| Area | Primary Skill | Result |

|---|---|---|

| Documentation governance | `documentation-governance` | Placement, links, front matter stay consistent |

| Documentation maintenance | `documentation-maintenance` | Code changes ship with matching docs |

| Documentation refactor | `documentation-refactor-review` | Duplicate or stale guidance is reduced |

| Documentation tone | `documentation-style-collaboration` | Wording stays clear and collaborative |

| Agent asset placement | `agent-instruction-governance` | Instruction assets stay in approved locations |



## Workflow



| Step | Agent Action | Output |

|---|---|---|

| 1. Classify | Agent maps the request to one or more coverage areas. | Scope list |

| 2. Inventory | Agent lists affected markdown files and source-of-truth files. | File inventory |

| 3. Select | Agent loads only the relevant supporting skills. | Focused skill set |

| 4. Remediate | Agent applies minimal edits, moves, or consolidations. | Updated files |

| 5. Validate | Agent runs front matter, link, and placement checks. | Validation results |

| 6. Report | Agent returns files changed and follow-up items. | Completion summary |



## Decision Rules



Agent keeps one canonical document per topic.

Agent moves discoverable Copilot assets only inside `.github/**` surfaces.

Agent places long-form guidance in `docs/instructions/**` or another canonical `docs/**` area.

Agent updates related indexes and links after every move or rename.



## Quality Gate



| Check | Test | Pass Criteria |

|---|---|---|

| Front matter | Run `npm run frontmatter:validate` when repository tooling exists. | Command returns zero validation errors. |

| Local links | Check changed markdown links and anchors. | Zero unresolved local links remain in changed files. |

| File placement | Compare each changed file to the canonical location map. | Every changed file matches its canonical location. |

| Change scope | Review changed files against request scope. | No unrelated documentation files change. |



## Output Contract



Agent reports coverage areas executed.

Agent reports files updated, moved, archived, or deleted.

Agent reports validation results.

Agent reports deferred follow-up items.

