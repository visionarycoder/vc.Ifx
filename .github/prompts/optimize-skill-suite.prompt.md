---
mode: agent
title: Optimize Skill Suite
description: Run deterministic optimization across skills, prompts, and instructions in `.github/`.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 860
invokes_skills:
  - skill-suite-optimization
  - create-skill
  - documentation-governance
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - technology-stack-dictionary
related_skills: []
appliesTo: '**/*'
tags:
  - prompts
  - prompt
  - ste
  - optimization
---
# Optimize Skill Suite

Agent optimizes skills, prompts, and instructions in `.github/` for low token cost and deterministic behavior.

## Scope Controls

| Input | Default | Test | Pass |
|---|---|---|---|
| Scope | All files in `.github/` | Read user prompt | Scope is explicit or default is applied. |
| Priority filter | All priorities | Read user prompt | Priority filter is explicit or default is applied. |
| Dry run | `false` | Read user prompt | Dry run value is explicit or default is applied. |
| Report | `true` | Read user prompt | Report value is explicit or default is applied. |

## Priority Rules

| Priority | Condition | Agent Action |
|---|---|---|
| 1 | File token count >5000 | Agent compresses or splits the file. |
| 2 | File is a consolidation bundle | Agent reduces duplication and centralizes repeated rules. |
| 3 | File lacks required metadata | Agent writes missing front matter properties. |
| 4 | File uses long prose | Agent converts prose to tables, lists, or workflows. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Build inventory | Agent counts skills, prompts, and instructions. | Inventory table | Inventory includes every file in scope. |
| 2. Record baseline | Agent records current token counts and missing metadata. | Baseline snapshot | Snapshot includes one row per file. |
| 3. Build queue | Agent sorts files by priority rules. | Queue review | Queue order matches priority table. |
| 4. Optimize files | Agent writes metadata, reduces prose, and compresses repeated guidance. | Diff review | Each changed file has lower or equal token count unless metadata growth is required. |
| 5. Verify front matter | Agent runs `npm run frontmatter:validate`. | Command exit code | Exit code = `0`. |
| 6. Verify token targets | Agent re-counts tokens after edits. | Token report | Each prompt is <1500 tokens. Each skill is within target range. |
| 7. Write report | Agent writes before and after counts, percentage reduction, and residual risk. | Report content | Report includes every changed file and every unresolved item. |
| 8. Update structure file | Agent writes `.github/copilot-instructions.md` when file structure changed. | Diff review | Structure list matches current `.github/` layout. |

## Token Targets

| File Type | Target | Pass |
|---|---|---|
| Prompt | 500-1500 tokens | Token count is <1500. |
| Skill | 1000-2000 tokens | Token count is within range. |
| Instruction | <800 tokens | Token count is <800. |
| Bundle | <2500 tokens | Token count is <2500. |

## Output Contract

| Section | Required Content |
|---|---|
| Baseline | Agent lists original counts and missing metadata. |
| Changes | Agent lists every optimized file and the new count. |
| Reduction | Agent lists absolute and percentage reduction per file. |
| Residual Risk | Agent lists files that remain over target or need user review. |
