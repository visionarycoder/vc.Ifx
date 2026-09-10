---
title: Skill Token Optimization Patterns
description: Compression patterns for keeping controller and specialist skills within accurate token budgets.
doc_type: reference
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 1207
prerequisites:
  - skill-lifecycle-controller
related_skills:
  - skill-lifecycle-controller
  - skill-suite-optimization
appliesTo: '.github/**/*.{md,prompt.md,instructions.md}'
tags:
  - tokens
  - optimization
  - references
---
# Skill Token Optimization Patterns

Use this reference when the controller reviews oversized skills or prepares an optimization plan.

## Compression Principles

| Principle | Agent Action | Effect |
|---|---|---|
| Prefer tables | Convert narrative decision logic to compact tables | Reduces repeated lead-in text |
| Front-load activation | Put trigger words and modes near the top | Improves routing with fewer tokens |
| Split catalogs | Move examples, inventories, and long matrices to `references/` | Shrinks the main skill while preserving detail |
| Reuse stable formulas | Keep `Math.Ceiling(content.Length / 4)` and similar formulas in one row | Avoids repeated explanation |
| Collapse repeated routes | Replace duplicated route text with one shared matrix | Lowers cross-file redundancy |

## Main File Budget Rules

| Artifact | Budget Target | Action Above Target |
|---|---:|---|
| Specialist skill | 2,000 tokens | Move examples and catalogs to references |
| Controller skill | 2,000 tokens | Keep workflow and validation in main; move catalogs out |
| Reference file | Practical minimum with complete coverage | Split only when one topic dominates unrelated topics |

## Split-to-Reference Rules

| Move Out | Keep In Main | Reason |
|---|---|---|
| Detailed API catalogs | Mode activation table | Main file needs fast routing |
| Long migration examples | Workflow and pass criteria | Main file needs execution clarity |
| Historical benchmarks | Verification checklist | Main file needs current validation |
| Overlap case studies | Conflict-detection summary | Main file needs controller behavior, not all examples |

## Compression Patterns

| Before | After | Result |
|---|---|---|
| Four paragraphs that explain one route choice | One route table row | Faster parsing and smaller footprint |
| Repeated validation prose in many sections | One verification matrix plus checklist | Lower duplication |
| Full examples inline | Short summary plus reference link | Main file stays scannable |
| Repeated definition of token math | One formula row and one audit row | Lower repetition |

## Example Rewrite Pattern

| Stage | Shape |
|---|---|
| Before | "Agent reviews every related skill reference. Agent checks every path. Agent confirms the target exists. Agent records any missing relation." |
| After | "Cross-references \| Resolve `related_skills` to skill paths \| Zero broken references" |

## Estimate Accuracy Rules

| Rule | Agent Action |
|---|---|
| Recalculate after edits | Update `estimated_tokens` after the content stabilizes |
| Keep variance tight | Treat variance above 10% as inaccurate for review mode |
| Flag severe drift | Treat variance above 50% as audit priority |
| Measure final text | Count the actual file content, not a draft excerpt |

## Audit Prioritization

| Signal | Priority |
|---|---|
| Above 2,000 actual tokens | Highest |
| Estimate variance above 50% | High |
| Heavy overlap plus high tokens | High |
| Repeated reference duplication | Medium |
| Slight estimate variance with clear structure | Low |

## Before and After Example

| Metric | Before | After |
|---|---:|---:|
| Main skill tokens | 2,640 | 1,720 |
| Reference tokens | 0 | 860 |
| Duplicate route paragraphs | 6 | 0 |
| Validation sections | 4 fragmented blocks | 1 matrix + 1 checklist |

## Anti-Patterns

| Anti-Pattern | Why It Fails | Fix |
|---|---|---|
| Chasing a low token count and deleting tests or pass criteria | The skill loses execution value | Keep measurable checks in tables |
| Splitting every section into a reference | The main file stops being actionable | Keep triggers, workflow, and verification in main |
| Leaving stale token estimates after edits | Review results become noisy | Recalculate at the end of the edit pass |
| Copying the same examples into related skills | Suite token burn grows silently | Centralize examples in one reference |

## Review Checklist

| Check | Pass |
|---|---|
| Main skill remains actionable without opening references | Yes |
| Reference file holds only expandable detail | Yes |
| `estimated_tokens` matches the final content closely | Yes |
| Validation and routing tables remain in the main file | Yes |
