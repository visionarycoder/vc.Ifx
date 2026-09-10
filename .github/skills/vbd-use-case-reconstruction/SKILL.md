---
name: vbd-use-case-reconstruction
title: VBD Use-Case Reconstruction
description: Reconstruct outcome-focused use cases from symbolic evidence and record conflicts, gaps, and confidence with stable `UC-*` links.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1064
prerequisites:
  - vbd-symbolic-mapping
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-evidence-quality
  - vbd-use-case-migration
appliesTo: '**/*.{cs,json,md,yml,yaml}'
tags:
  - vbd
  - use-cases
  - reconstruction
  - conflicts
---

# VBD Use-Case Reconstruction

Agent converts implementation evidence into stable, outcome-focused use cases.

## When to Use

Use when symbolic mapping exists and business outcomes are still unclear.
Use when code, tests, docs, or operations disagree.
Use when candidate design work needs a frozen, implementation-neutral use-case set.

## When Not to Use

Do not use when the same evidence revision already has a frozen use-case set.
Do not use when the prompt asks for construction planning after a selected candidate.

## Inputs

| Input | Required | Description |
|---|---|---|
| Symbolic mapping | Yes | Structured symbol and call evidence |
| Entry-point inventory | Yes | User, system, Schedule, event, and operator triggers |
| Existing requirements | No | Docs, tickets, procedures, and acceptance notes |
| Operational evidence | No | Logs, incidents, support notes, and telemetry |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Group outcomes | Agent groups symbols by actor-visible or system-visible outcome. | Outcome groups | Agent reads outcome groups. | Every group names one actor, one trigger, and one result. |
| 2. Generate use cases | Agent generates one `UC-*` record per outcome. | `UC-*` records | Agent reads each record. | Every record includes actor, trigger, preconditions, outcome, main flow, alternate flow, failure flow, authorization, data, transaction, nonfunctional notes, symbols, gaps, and confidence. |
| 3. Reconcile conflicts | Agent ranks evidence by requirement, aligned code, aligned tests, then docs and operations. | Conflict log | Agent reads the log. | Every conflict names the winning source and the losing source. |
| 4. Record conditional concerns | Agent records UI, Schedule, async, security, privacy, and recovery behavior only when evidence exists. | Conditional matrix | Agent reads the matrix. | Zero conditional concern appears without source evidence. |
| 5. Link traceability | Agent links flow steps to `SYM-*` records and unsupported behavior to `GAP-*` records. | Traceability map | Agent reads the map. | Every flow step links to at least one `SYM-*` or `GAP-*` ID. |
| 6. Freeze evidence | Agent writes the evidence-manifest hash into the use-case set. | Frozen index | Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`. | Exit code = 0. IDs resolve. Manifest links resolve. |

## Output Contract

| Output | Minimum content |
|---|---|
| `docs/analysis/use-cases/index.md` | Ordered use-case index with manifest hash |
| `UC-*.md` files | One outcome per file |
| `use-cases.json` | Machine-readable use-case set |
| `evidence-conflicts.md` | Ranked conflicts and decisions |
| `requirements-gaps.md` | Unsupported requirements linked to `GAP-*` |

## Verification

- [ ] Agent keeps one outcome per `UC-*` record.
- [ ] Agent links every flow step to `SYM-*` or `GAP-*`.
- [ ] Agent records compatibility at outcome level.
- [ ] Agent records UI and Schedule concerns only when evidence exists.
- [ ] Agent freezes the evidence-manifest hash.

Test: Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`.
Pass: Exit code = 0. Zero duplicate IDs. Zero unresolved references.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Endpoint list becomes the use-case set | Agent groups by outcome across entry points |
| Current component names drive requirements | Agent writes actor-visible outcomes only |
| Happy path is the only flow | Agent writes alternate, failure, recovery, and authorization flows |
| Unsupported requirement disappears | Agent writes a `GAP-*` record |
