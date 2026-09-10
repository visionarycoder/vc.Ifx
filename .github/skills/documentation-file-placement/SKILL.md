---

name: documentation-file-placement

description: Place documentation files in canonical locations and preserve valid navigation.

title: Documentation File Placement Skill

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: low

estimated_tokens: 500

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - frontmatter-standard

related_skills:

  - documentation-frontmatter

  - documentation-governance

  - documentation-link-repair

  - documentation-maintenance

appliesTo: '**/*.md'

tags:

  - documentation

  - placement

  - ste

---

# Documentation File Placement Skill



Agent classifies document purpose and places each file in a canonical location.



## Canonical Location Map



| Content Type | Canonical Location |

|---|---|

| Copilot instructions, prompts, skills | `.github/**` |

| Architecture decisions | `docs/adr/**` |

| Developer guidance | `docs/developer-guide/**` |

| Long-form plans, reports, governance | `docs/instructions/**` |

| Retired content | `docs/archive/**` |



## Workflow



| Step | Agent Action | Output |

|---|---|---|

| 1. Classify | Agent identifies document purpose and lifecycle. | Content classification |

| 2. Map | Agent selects the canonical destination path. | Target path |

| 3. Move | Agent renames or relocates the file with minimal content edits. | Moved file |

| 4. Repair | Agent updates inbound and outbound links. | Corrected navigation |

| 5. Verify | Agent checks indexes, nearby READMEs, and references. | Placement confirmation |



## Naming Rules



Agent uses sibling naming patterns first.

Agent uses descriptive kebab-case names when no local pattern exists.

Agent keeps abbreviations limited to established project terms.



## Quality Gate



| Check | Test | Pass Criteria |

|---|---|---|

| Canonical placement | Compare each moved file to the location map. | Every moved file matches the classified content type. |

| Navigation | Review parent indexes and related READMEs. | Every moved file remains discoverable from nearby navigation. |

| Links | Check changed local links after the move. | Zero broken local links remain in changed files. |

