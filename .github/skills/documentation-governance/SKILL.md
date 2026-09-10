---
name: documentation-governance
title: Documentation Governance
description: Govern markdown placement, links, front matter, and tone across repository documentation.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 1044
prerequisites:
  - documentation-frontmatter
related_skills:
  - documentation-link-repair
  - documentation-frontmatter
  - documentation-file-placement
  - documentation-style-collaboration
appliesTo: '**/*.md'
tags:
  - documentation
  - governance
  - links
  - frontmatter
---
# Documentation Governance

Agent governs markdown placement, link integrity, front matter, and collaborative tone.

## When to Use

| Condition | Use |
|---|---|
| Markdown content is created, moved, or rewritten | Use this skill |
| Link integrity needs validation | Use this skill |
| Front matter or placement drift appears | Use this skill |
| Repository guidance needs consistent tone | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| One file needs front matter only | Use `documentation-frontmatter` |
| One file needs link repair only | Use `documentation-link-repair` |
| One file needs path selection only | Use `documentation-file-placement` |
| One file needs wording refinement only | Use `documentation-style-collaboration` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| File set | Yes | Agent records changed markdown files. |
| Operation | Yes | Agent records create, move, rename, rewrite, or archive. |
| Canonical location | Yes | Agent records the expected destination class. |
| Validation commands | Yes | Agent records link and front matter checks. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent checks placement against the canonical location map. | Review path set. | Every file sits in the correct location. |
| 2 | Agent validates or repairs local relative links. | Run link checks. | Zero broken local links remain. |
| 3 | Agent normalizes front matter values. | Run `npm run frontmatter:validate`. | Exit code `0`. |
| 4 | Agent updates inbound links, indexes, and related references after moves or renames. | Search for old paths. | Zero stale path references remain. |
| 5 | Agent reviews tone for collaborative wording and explicit subjects. | Review changed text. | Tone stays direct and non-commanding. |

## Rule Matrix

| Asset Type | Canonical Location |
|---|---|
| Copilot instructions, prompts, and skills | `.github/**` |
| Architecture decisions | `docs/adr/**` |
| Developer how-to content | `docs/developer-guide/**` |
| Long-form plans, reports, and guidance | `docs/instructions/**` |
| Retired material | `docs/archive/**` |

## Pattern Matrix

| Concern | Pattern |
|---|---|
| Link updates | Update inbound links and indexes in the same pass as the move. |
| Front matter | Use the front matter standard instead of ad hoc keys. |
| Tone | Use collaborative wording with explicit actors. |
| Routing | Use the narrower documentation skill when the scope is small. |

## Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Placement | Compare each path to the canonical map. | Every file matches one canonical location. |
| Links | Resolve local relative links from each changed file. | Zero broken links remain. |
| Front matter | Run repository validation. | Exit code `0`. |
| Tone | Search for prohibited modal wording in changed files. | Zero prohibited matches remain. |

## Verification Checklist

| Checkpoint | Pass Condition |
|---|---|
| Placement verified | Every changed file sits in a canonical directory. |
| Links repaired | Local link graph resolves cleanly. |
| Front matter normalized | Required properties validate successfully. |
| Tone aligned | Collaborative wording replaces commanding phrasing. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Moving files without updating inbound links | Agent updates links in the same pass. |
| Storing long-form guidance outside canonical folders | Agent relocates the file before further edits. |
| Treating tone review as optional | Agent runs the tone check for every changed markdown file. |
