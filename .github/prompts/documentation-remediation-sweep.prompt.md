---
mode: ask
model: GPT-5.3-Codex
title: Documentation Remediation Sweep
description: Run a deterministic documentation remediation workflow across repository markdown files.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 560
invokes_skills:
  - documentation-governance
  - documentation-file-placement
  - documentation-frontmatter
  - documentation-link-repair
  - documentation-maintenance
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - documentation-governance
related_skills: []
appliesTo: '**/*'
tags:
  - prompts
  - prompt
  - ste
  - documentation
---
# Documentation Remediation Sweep

Agent runs repository-wide documentation remediation across `.github/**` and `docs/**`.

## Scope

| Asset | Test | Pass |
|---|---|---|
| Markdown inventory | `glob .github/**/*.md` and `glob docs/**/*.md` | Agent records every markdown file in scope. |
| Local links | Search markdown link targets | Agent records every relative link in changed files. |
| Front matter | Read changed markdown files | Agent records required and missing properties. |
| Placement | Compare path to canonical locations | Agent records misplaced files and target paths. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Read scope | Agent reads all markdown files in scope. | Inventory count | Inventory count >0. |
| 2. Verify placement | Agent compares each changed file to `.github/instructions/documentation-governance.instructions.md`. | Path review | Each move has one canonical target path. |
| 3. Fix links | Agent writes updated relative links after each move or rename. | Search old path references | Zero changed files contain stale relative paths. |
| 4. Normalize front matter | Agent writes front matter that matches `.github/instructions/frontmatter-standard.instructions.md`. | `npm run frontmatter:validate` | Exit code = `0`. |
| 5. Preserve meaning | Agent limits wording changes to placement, front matter, link fix, and collaborative tone. | Diff review | No changed file adds unrelated content. |
| 6. Record residual risk | Agent lists items that need user review. | Report content | Report includes every unresolved path, link, or wording decision. |

## Move Rules

| Condition | Agent Action | Test | Pass |
|---|---|---|---|
| File path violates canonical location | Agent writes the file to the canonical path. | Path review | New path matches governance table. |
| File name violates local naming pattern | Agent renames the file to match sibling files. | Directory review | New name matches sibling casing and delimiter pattern. |
| Move changes link targets | Agent writes inbound and outbound link updates. | Search changed references | Zero changed references point to the old path. |

## Report Format

| Section | Required Content |
|---|---|
| Findings | Agent groups findings by severity and impact. |
| Changes | Agent lists moved files, renamed files, and fixed links. |
| Residual Risk | Agent lists unresolved items that need user review. |
