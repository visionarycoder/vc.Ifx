---
title: Instruction Location Governance
doc_type: policy
status: active
last_updated: 2026-07-20
target_audience: ai
complexity: low
estimated_tokens: 274
prerequisites:
  - documentation-fixes
related_skills:
  - documentation-fixes
appliesTo: '**/*.md'
tags:
  - documentation
---
# Instruction Location Governance

Use this playbook to enforce valid locations for coding-agent assets.

## Canonical Locations

- `.github/copilot-instructions.md`
- `.github/instructions/*.instructions.md`
- `.github/skills/<skill-name>/SKILL.md`
- `.github/prompts/*.prompt.md`
- `docs/instructions/**` for long-form human guidance

## Actions

1. Detect instruction-like files in invalid locations.
2. Move discoverable assets into `.github/**` locations.
3. Move long-form implementation guidance into `docs/instructions/**`.
4. Archive/remove duplicates and stale mirrors.

## Validation

- No discoverable instruction assets outside canonical `.github/**` surfaces.
- No long-form guidance outside `docs/instructions/**` unless intentionally scoped elsewhere in docs.
- Indexes updated after moves.
