---
title: Documentation Governance Instruction
description: Documentation governance and remediation workflow
doc_type: instruction
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 691
prerequisites:
  - .github/instructions/ste-agent-writing-standard.instructions.md
related_skills:
  - documentation-governance
  - documentation-link-repair
  - documentation-frontmatter
  - documentation-file-placement
  - documentation-style-collaboration
appliesTo: '**/*.md'
tags:
  - instructions
  - documentation
---
# Documentation Governance Instruction

Agent applies this guidance when creating, reviewing, moving, or archiving documentation.

## Objectives

Agent organizes markdown assets by purpose and lifecycle.
Agent preserves link validity after file moves and renames.
Agent aligns front matter with repository standards.
Agent uses collaborative language without commanding tone.

## Canonical Locations

| Asset Type | Location | Purpose |
|---|---|---|
| Copilot instructions, prompts, skills | `.github/**` | Agent-discoverable assets |
| Architecture decisions | `docs/adr/**` | ADR records |
| Developer how-to content | `docs/developer-guide/**` | Team guidance |
| Long-form plans, reports, guidance | `docs/instructions/**` | Extended documentation |
| Retired material | `docs/archive/**` | Historical reference |

## Workflow

| Step | Agent Action | Test | Pass Criteria |
|---|---|---|---|
| 1. Verify links | Agent checks all local relative links in changed markdown files. | Run link validator. | Zero broken local links. |
| 2. Normalize front matter | Agent applies standard from `.github/instructions/frontmatter-standard.instructions.md`. | Run `npm run frontmatter:validate`. | Zero validation errors. |
| 3. Verify placement | Agent compares file path to canonical location table. | Check file directory. | File resides in correct canonical location. |
| 4. Update dependents | Agent updates inbound links and indexes when file moves or renames. | Search for old path references. | Zero references to old path remain. |
| 5. Apply tone | Agent uses collaborative phrasing without command language. | Scan for imperative commands. | Documentation avoids commanding phrasing and directive forms. |

## Skill Orchestration

Agent starts with `.github/skills/documentation-governance/SKILL.md` for full workflow.
Agent uses `.github/skills/documentation-link-repair/SKILL.md` for link remediation.
Agent uses `.github/skills/documentation-frontmatter/SKILL.md` for front matter normalization.
Agent uses `.github/skills/documentation-file-placement/SKILL.md` for move or rename decisions.
Agent uses `.github/skills/documentation-style-collaboration/SKILL.md` for wording and tone refinement.
