---

name: documentation-refactor-review

description: Review and refactor documentation to reduce drift, duplication, and navigation friction.

title: Documentation Refactor Review

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 950

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - frontmatter-standard

related_skills:

  - documentation-file-placement

  - documentation-frontmatter

  - documentation-governance

  - documentation-link-repair

appliesTo: '**/*.{md,cs,ts,js,json,yml,yaml,csproj,sln,slnx,props,targets,ps1,sql}'

tags:

  - documentation

  - refactor

  - ste

---

# Documentation Refactor Review



Agent reviews documentation for drift, duplication, and clarity.



## Objectives



Agent aligns documentation with current repository structure.

Agent reduces duplicate or conflicting guidance.

Agent preserves one canonical document per topic.

Agent improves navigation and task-focused readability.



## Workflow



| Step | Agent Action | Output |

|---|---|---|

| 1. Inventory | Agent lists documentation areas and source-of-truth code artifacts. | Scope inventory |

| 2. Detect | Agent identifies drift, redundancy, stale paths, and missing coverage. | Findings list |

| 3. Plan | Agent selects canonical files and consolidation actions. | Refactor plan |

| 4. Apply | Agent updates, consolidates, retires, or relinks documentation. | Refactored docs |

| 5. Validate | Agent checks references, placement, front matter, and navigation. | Validation results |



## Canonicalization Rules



| Topic | Canonical Home |

|---|---|

| Onboarding and quick reference | `docs/start-here/**`, `docs/ref/**` |

| Operations | `docs/operations/**`, `docs/ops/**` |

| Architecture and decisions | `docs/architecture/**`, `docs/adr/**` |

| Plans and findings | `docs/instructions/**` |

| Copilot assets | `.github/**` |



## Quality Gate



| Check | Test | Pass Criteria |

|---|---|---|

| Duplicate guidance | Compare related documents for repeated procedures. | One canonical procedure remains for each topic. |

| Drift | Validate paths, project names, scripts, routes, and config keys against repository files. | No known stale references remain in changed scope. |

| Navigation | Review index files and cross-links after consolidation. | Readers reach canonical docs from prior entry points. |
| Front matter and links | Run front matter validation and link review when tooling exists. | Zero validation errors and zero broken local links remain in changed files. |

