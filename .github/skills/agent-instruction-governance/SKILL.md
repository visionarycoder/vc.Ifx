---

name: agent-instruction-governance

description: Enforce approved repository locations for Copilot instructions, prompts, skills, and related guidance.

title: Agent Instruction Governance

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

appliesTo: '**/*.md'

tags:

  - governance

  - copilot

  - instructions

  - prompts

  - skills

  - ste

---

# Agent Instruction Governance



Agent enforces valid locations for Copilot instruction assets.



## Valid Location Map



| Asset Type | Canonical Location |

|---|---|

| Repository instruction | `.github/copilot-instructions.md` |

| Targeted instruction | `.github/instructions/*.instructions.md` |

| Skill | `.github/skills/<skill-name>/SKILL.md` |

| Prompt | `.github/prompts/*.prompt.md` |

| Long-form guidance, reports, plans | `docs/instructions/**` |



## Invalid Placement Signals



| Pattern | Governance Result |

|---|---|

| Loose root instruction files | Violation |

| Instruction files outside `docs/instructions/**` under `docs/**` | Violation |

| Discoverable skill mirrors in `docs/instructions/skills/**` without explicit request | Violation |

| Legacy `*.agent.md` prompt files | Rename or relocate review |



## Workflow



| Step | Agent Action | Output |

|---|---|---|

| 1. Inventory | Agent finds instruction-like files, skill files, and prompt files. | Evidence table |

| 2. Classify | Agent marks each file as valid, invalid, or exception. | Classification set |

| 3. Plan | Agent assigns `move`, `merge`, `archive`, or `delete` for each invalid file. | Remediation plan |

| 4. Apply | Agent relocates or consolidates files into canonical surfaces. | Remediated assets |

| 5. Verify | Agent re-runs the inventory and updates cross-links. | Compliance result |



## Decision Rules



Agent places discoverable Copilot assets in `.github/**`.

Agent places long-form narrative guidance in `docs/instructions/**`.

Agent removes or archives duplicate canonical instruction content.

Agent records exceptions when explicit user direction keeps a mirror or archive.



## Quality Gate



| Check | Test | Pass Criteria |

|---|---|---|

| Placement compliance | Compare each changed instruction asset to the valid location map. | Every changed instruction asset matches its canonical location or documented exception. |

| Duplicate control | Review canonical surfaces and mirrors after remediation. | No unintended duplicate canonical instruction files remain. |

| Cross-links | Review references to moved or renamed assets. | Every changed cross-link resolves to the new location. |

| Front matter | Run `npm run frontmatter:validate` when tooling exists. | Command returns zero validation errors. |



## Output Contract



Agent reports compliance status.

Agent reports files moved, merged, archived, deleted, or renamed.

Agent reports remaining follow-up items.

