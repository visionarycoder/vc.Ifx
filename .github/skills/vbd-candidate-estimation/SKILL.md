---
name: vbd-candidate-estimation
title: VBD Candidate Estimation
description: Generate normalized candidate estimates for contracts, services, tests, clients, Ifx, Aspire, analyzers, rollout work, and stable `EST-*` records.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1096
prerequisites:
  - vbd-evidence-quality
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-change-simulation
  - vbd-modernization-portfolio
appliesTo: '**/*.{json,md,yml,yaml}'
tags:
  - vbd
  - estimation
  - tokens
  - risk
---

# VBD Candidate Estimation

Agent generates comparable estimates without hiding uncertainty or omitted infrastructure.

## When to Use

Use when a candidate has component, reuse, and gap inventories.
Use when several candidates need comparable hours, duration, and token ranges.
Use when planning needs explicit staffing, dependency, and review assumptions.

## When Not to Use

Do not use when evidence or use cases are not frozen.
Do not use when the candidate has no traceable component or gap model.

## Inputs

| Input | Required | Description |
|---|---|---|
| Frozen evidence | Yes | Manifest, symbolic map, and use cases |
| Candidate design | Yes | Components, contracts, communication, and topology |
| Reuse and gap map | Yes | Retain, move, wrap, split, retire, and generate decisions |
| Delivery assumptions | Yes | Staffing, parallelism, review, environments, and dependencies |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Normalize taxonomy | Agent uses one work taxonomy for every candidate. | Work taxonomy | Agent reads the taxonomy. | Taxonomy includes contracts, tests, client item 3.8, Ifx, Aspire, analyzers, migration, docs, and review. |
| 2. Generate ranges | Agent writes optimistic, expected, and pessimistic ranges for hours, duration, and tokens. | `EST-*` records | Agent reads estimate rows. | Every row has three-point ranges and assumptions. |
| 3. Build dependency graph | Agent writes dependency edges before deriving duration. | Dependency graph | Agent reads graph edges. | Contract work precedes dependent work. Shared Ifx work precedes proxy-dependent work. |
| 4. Price quality work | Agent writes explicit effort for coverage, generated behavior tests, integration tests, and benchmarks. | Quality estimate packet | Agent reads packet. | Zero quality gate is hidden inside another row. |
| 5. Verify completeness | Agent reviews omissions, exclusions, and risk contingency. | Estimate review log | Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`. | Exit code = 0. Required estimate dimensions exist. |

## Standard Work Taxonomy

| Item | Required |
|---|---|
| Contract and contract tests | Yes |
| Service and unit tests | Yes |
| Optional ORM, simulator, emulator | When design includes them |
| Integration tests | Yes |
| Benchmarks and run docs | Yes |
| Client estimate item 3.8 | Yes |
| Conditional UI and UI tests | When UI scope exists |
| Utilities and Ifx foundation | Yes |
| Aspire topology | Yes |
| Analyzer, code-fix, generator work | Yes when required |
| Migration, rollout, rollback, docs, review | Yes |

## Output Contract

| Output | Minimum content |
|---|---|
| Estimate workbook | Structured `EST-*` rows |
| Human summary | Per-candidate totals and assumptions |
| Dependency graph | Critical path and parallel work |
| Exclusion log | Explicit non-scope items |

## Verification

- [ ] Agent uses one taxonomy for every candidate.
- [ ] Agent writes three-point ranges for hours, duration, and tokens.
- [ ] Agent derives duration from dependencies and capacity.
- [ ] Agent includes Ifx, Aspire, coverage, migration, and review work.
- [ ] Agent keeps client estimate as item 3.8.

Test: Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`.
Pass: Exit code = 0. Zero estimate records miss required range fields.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Hours equal duration | Agent derives duration from critical path and capacity |
| Existing code is free | Agent prices adaptation work from the reuse map |
| Tests appear as percentage uplift | Agent writes explicit quality rows |
| Infrastructure work disappears | Agent writes Ifx, Aspire, analyzer, and environment rows |
