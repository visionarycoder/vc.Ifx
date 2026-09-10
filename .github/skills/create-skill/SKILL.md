---
name: create-skill
title: Create Skill
description: Scaffold a repository skill with STE-compliant frontmatter, workflow tables, and verification steps.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1091
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - skill-suite-optimization
appliesTo: '**/*'
tags:
  - create
  - ste
  - skills
---
# Create Skill

Agent scaffolds a new repository skill using STE wording, required frontmatter, and token-budget rules.

## When to Use

| User prompt | Use |
|---|---|
| User asks for a new skill | Use this skill |
| User asks for a new `SKILL.md` scaffold | Use this skill |
| User asks for STE-compliant skill structure | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to edit an existing skill | Edit the file directly |
| User asks for broad suite cleanup | Use `skill-suite-optimization` |
| User asks for non-skill markdown | Use the matching documentation skill |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Skill name | Yes | Use lowercase letters, numbers, and hyphens |
| Skill purpose | Yes | State the outcome in one short paragraph |
| Entry conditions | Yes | State when the agent uses the skill |
| Optional assets | No | List `references`, `scripts`, or `assets` only when needed |

## Required Frontmatter

| Field | Value |
|---|---|
| `name` | Directory name |
| `title` | Human-readable title |
| `description` | One sentence with scope and trigger |
| `doc_type` | `skill` |
| `status` | `active` |
| `last_updated` | Current date |
| `prerequisites` | Include `ste-agent-writing-standard` and `terminology-dictionary` |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies `skill-name` matches `^[a-z0-9]+(-[a-z0-9]+)*$`. | Read the planned directory name. | Name matches the pattern. |
| 2 | Agent writes `.github/skills/<skill-name>/SKILL.md`. | Read the target directory. | Directory contains `SKILL.md`. |
| 3 | Agent writes required frontmatter. | Run `rg "^(name|title|description|doc_type|status|last_updated):" .github\skills\<skill-name>\SKILL.md`. | Six required fields exist. |
| 4 | Agent writes sections for use, inputs, workflow, verification, and pitfalls. | Run `rg "^## (When to Use|When Not to Use|Required Inputs|Workflow|Verification Checklist|Common Pitfalls)$" .github\skills\<skill-name>\SKILL.md`. | All headings exist. |
| 5 | Agent applies STE wording from `ste-agent-writing-standard`. | Run the repository STE violation grep on the new file. | Zero modal or prohibited phrase matches exist. |
| 6 | Agent moves verbose examples to `references/` when the main file exceeds the token budget. | Read `estimated_tokens` in frontmatter. | `estimated_tokens < 2000`. |
| 7 | Agent updates `.github/CODEOWNERS` when the repository uses skill ownership entries. | Run `rg -n "<skill-name>" .github\CODEOWNERS`. | Match exists or repository has no skill entry pattern. |
| 8 | Agent runs frontmatter verification. | Run `npm run frontmatter:validate`. | Zero validation errors. |

## Structure Rules

| Topic | Rule |
|---|---|
| Main file size | Keep `estimated_tokens < 2000` |
| Decision logic | Use tables, not prose lists |
| Examples | Write large examples under `references/` |
| Terminology | Use approved terms from `terminology-dictionary` |
| Suite alignment | Keep `related_skills` aligned with `skill-suite-optimization` when the skill routes or bundles work |

## Verification Checklist

- [ ] Agent wrote the skill under `.github/skills/<skill-name>/`.
- [ ] Agent wrote required frontmatter fields.
- [ ] Agent added STE prerequisites.
- [ ] Agent used tables for workflow and decision logic.
- [ ] Agent kept the main file under the token budget.
- [ ] Agent ran frontmatter verification.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Name uses uppercase or spaces | Agent rewrites the name with lowercase hyphenated text. |
| Description hides the trigger | Agent states what the skill does and when the agent uses it. |
| Main file grows too large | Agent moves examples to `references/`. |
| Workflow uses prose only | Agent rewrites the workflow as a table. |
| STE rules are missing | Agent adds subjects, tests, and pass criteria. |
