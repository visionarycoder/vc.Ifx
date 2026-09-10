---

name: documentation-frontmatter

description: Normalize markdown front matter according to repository standards.

title: Documentation Front Matter Skill

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: low

estimated_tokens: 420

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - frontmatter-standard

related_skills:

  - documentation-file-placement

  - documentation-governance

  - documentation-link-repair

  - documentation-maintenance

appliesTo: '**/*.md'

tags:

  - documentation

  - frontmatter

  - ste

---

# Documentation Front Matter Skill



Agent normalizes markdown metadata with the repository front matter standard.



## Standard Fields



| Field | Requirement |

|---|---|

| `title` | Agent sets a human-readable document title. |

| `doc_type` | Agent uses an allowed repository classifier. |

| `status` | Agent uses an allowed lifecycle value. |

| `last_updated` | Agent records the current edit date in `YYYY-MM-DD` format. |



## Workflow



| Step | Agent Action | Output |

|---|---|---|

| 1. Inspect | Agent checks changed markdown files for missing or malformed front matter. | Metadata findings |

| 2. Normalize | Agent adds or corrects required fields. | Updated front matter |

| 3. Preserve | Agent keeps valid existing metadata that matches repository rules. | Stable metadata |

| 4. Validate | Agent runs repository front matter validation when tooling exists. | Validation result |



## Quality Gate



| Check | Test | Pass Criteria |

|---|---|---|

| Required fields | Review changed markdown front matter. | Every changed markdown file contains required fields. |

| Allowed values | Compare values to `.github/instructions/frontmatter-standard.instructions.md`. | Every value matches the allowed schema. |

| Repository validation | Run `npm run frontmatter:validate` when repository tooling exists. | Command returns zero validation errors. |

