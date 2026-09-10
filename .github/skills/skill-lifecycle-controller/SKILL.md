---
name: skill-lifecycle-controller
title: Skill Lifecycle Controller
description: >
  Manages skill lifecycle with modes for create, review, optimize, audit, migrate, and deprecate. Executes workflows directly with technology drift detection and routing conflict analysis.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: high
estimated_tokens: 1975
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - frontmatter-standard
related_skills:
  - create-skill
  - skill-suite-optimization
  - skill-discovery-guide
  - documentation-governance
appliesTo: '.github/**/*.{md,prompt.md,instructions.md}'
tags:
  - skills
  - lifecycle
  - controller
  - optimization
  - governance
---
# Skill Lifecycle Controller

Agent manages the full skill lifecycle through direct execution modes with measurable validation, drift detection, and routing analysis.

## When to Use

| User prompt | Use |
|---|---|
| User asks to create a new skill | Use this controller in `create` mode |
| User asks to review one skill | Use this controller in `review` mode |
| User asks to optimize many skills | Use this controller in `optimize` mode |
| User asks to audit the skill ecosystem | Use this controller in `audit` mode |
| User asks to update skills for a new technology version | Use this controller in `migrate` mode |
| User asks to archive or retire a skill | Use this controller in `deprecate` mode |

## When Not to Use

| Condition | Route |
|---|---|
| One skill needs a small direct edit with no lifecycle workflow | Edit the file directly |
| Only front matter or markdown placement is wrong | Use `documentation-governance` |
| The user names a specialist directly for a narrow task | Use the named skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| User intent | Yes | Agent maps the prompt to one lifecycle mode. |
| Scope | Yes | Agent records one skill, many skills, or the full ecosystem. |
| Validation commands | Yes | Agent records token, front matter, routing, and STE checks before edits start. |
| Target version or archive path | No | Agent records the extra mode input for `migrate` or `deprecate`. |

## Mode Activation Table

| User Intent | Mode | What Controller Does |
|---|---|---|
| "Create a skill for X" | `create` | Delegates to `create-skill`, validates output |
| "Review skill X" | `review` | Validates one skill for STE compliance, front matter, tokens, cross-references, and routing |
| "Optimize skills" | `optimize` | Delegates to `skill-suite-optimization` with enhanced token analysis |
| "Audit ecosystem" | `audit` | Scans all skills and generates a priority report for token burn, drift, and routing conflicts |
| "Update skills for .NET 11" | `migrate` | Detects technology drift, updates versioned guidance, validates results |
| "Archive skill X" | `deprecate` | Updates routing, moves the skill to archive storage, preserves history |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Detect mode | Agent reads user intent and selects one lifecycle mode. | Review the mode mapping table. | One of six defined modes is selected. |
| 2. Execute mode | Agent runs the mode-specific workflow. | Review mode-specific output. | Mode-specific checks pass or residual risk is named. |
| 3. Delegate when appropriate | Agent calls `create-skill` or `skill-suite-optimization` for `create` and `optimize` modes. | Review specialist output. | Delegation succeeds and controller validation completes. |
| 4. Generate report | Agent returns the mode-specific report, file list, and validation results. | Review the final report. | Report contains the required sections and next-state summary. |

## Review Validation Matrix

| Check | Test | Pass |
|---|---|---|
| STE compliance | Scan for prohibited STE modal verbs. | Zero matches |
| Front matter | Parse YAML and compare to required fields and allowed values. | Valid YAML with no missing required field |
| Token budget | Recalculate `Math.Ceiling(content.Length / 4)` and compare to `estimated_tokens`. | Within 10% variance and under 2,000 tokens |
| Cross-references | Resolve every `related_skills` entry to `.github\skills\<name>\SKILL.md`. | Zero broken skill references |
| Routing clarity | Compare `appliesTo` overlap against other skill patterns. | Fewer than five overlapping skills or documented justification |

## Audit Detection Matrix

| Capability | Agent Action | Test | Pass |
|---|---|---|---|
| Token burn detection | Agent scans `.github\skills\*\SKILL.md`, calculates `Math.Ceiling(content.Length / 4)`, compares to `estimated_tokens`, flags files above 2,000 tokens, and flags variance above 50%. | Review the token table. | Violations are sorted by overage percentage. |
| Routing conflict detection | Agent builds an `appliesTo` overlap matrix and groups identical activation conditions. | Review the overlap matrix. | Conflicts are grouped with one resolution recommendation per group. |
| Technology drift detection | Agent scans for version references, deprecated APIs, and superseded patterns. | Review the drift findings. | Findings are grouped by versions, APIs, and patterns. |

## Audit Output Contract

| Section | Content | Ordering |
|---|---|---|
| 1. Token violations | Skills above budget or with estimate variance above 50% | Sort by overage percentage descending |
| 2. Routing conflicts | Overlapping `appliesTo` patterns and identical activation conditions | Group by shared pattern family |
| 3. Technology drift | Version, API, and pattern findings with migration recommendations | Group by category: versions, APIs, patterns |

## Migrate Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent accepts the target version such as `.NET 11` or `C# 14`. | Review recorded target version. | One explicit target version exists. |
| 2 | Agent scans all skills for old version references and drift markers. | Review the file list. | Every affected skill appears once. |
| 3 | Agent generates an update plan with affected files and replacement rules. | Review the plan. | Plan lists file path, old text, and replacement intent. |
| 4 | Agent asks the user for confirmation before bulk updates. | Review the interaction state. | Confirmation gate is present before edits. |
| 5 | Agent applies the approved updates and preserves existing routing intent. | Review diffs. | Updates match the plan with no unrelated edits. |
| 6 | Agent runs front matter and modal validation after edits. | Run repository validation commands. | Validation passes. |
| 7 | Agent reports the updated skill count and any residual drift. | Review the final report. | Report states the count and remaining exceptions. |

## Verification Checklist

- [ ] Main skill file exists at `.github/skills/skill-lifecycle-controller/SKILL.md`.
- [ ] Front matter contains all required fields.
- [ ] All six modes appear in the activation table.
- [ ] Audit mode documents token burn, routing conflict, and technology drift detection.
- [ ] Review mode documents five validation checks.
- [ ] Migrate mode documents the bulk update workflow.
- [ ] Three reference files exist under `references/`.
- [ ] `estimated_tokens` matches the final content within 10%.
- [ ] Modal scan returns zero prohibited STE matches.

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Treating the controller as a router only | Agent executes the lifecycle workflow instead of stopping at route selection. |
| Updating one skill during ecosystem audit | Agent keeps `audit` mode read-first and report-first. |
| Leaving stale routing entries after deprecation | Agent updates redirecting routers in the same pass as the archive move. |
