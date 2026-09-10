---
mode: agent
title: VBD Migration Rehearsal
description: Rehearse one migration wave with routing, shadow execution, tolerance rules, compatibility, replay, rollback, and drift monitoring.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 910
invokes_skills:
  - vbd-use-case-migration
  - vbd-cutover-migration
  - vbd-operational-contracts
  - vbd-change-simulation
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - vbd-dictionary
related_skills: []
appliesTo: '**/*'
tags:
  - prompts
  - prompt
  - ste
  - vbd
---
# VBD Migration Rehearsal

Agent rehearses one planned migration wave before cutover readiness review.

## Scope

| Scope Item | Agent Action | Test | Pass |
|---|---|---|---|
| Wave scope | Agent uses one migration wave with an explicit use-case set and traffic slice. | Agent reads scope input. | Scope is explicit. |
| Entry scope | Agent uses one logical entry point that reaches old and new implementations. | Agent reads routing input. | Both implementations are reachable. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies the use-case catalog, acceptance criteria, and selected-plan hash from `vbd-cutover-migration`. | Agent compares input records. | Inputs match the selected plan. |
| 2 | Agent configures router states for default, canary, and full cutover. | Agent reads router state table. | Every state is explicit. |
| 3 | Agent enables shadow execution with old-path authority. | Agent runs shadow traffic and reads caller-visible results. | New path never changes caller-visible results. |
| 4 | Agent defines one outcome tolerance policy per field class and per use case. | Agent reads tolerance rows. | Every affected field has one rule. |
| 5 | Agent runs shadow comparison on a representative sample. | Agent reads diff rate and severity per use case. | Every in-scope use case has comparison results. |
| 6 | Agent verifies contract compatibility with `vbd-operational-contracts`. | Agent reads compatibility notes. | Every changed contract is additive-only or versioned. |
| 7 | Agent rehearses replay after a change. | Agent runs replay against recorded traffic. | Replay results are recorded. |
| 8 | Agent rehearses rollback with a real route revert and data reconciliation review. | Agent runs rollback steps and reads results. | Old path resumes correctly and reconciliation evidence exists. |
| 9 | Agent defines the post-cutover drift-monitoring plan. | Agent reads signal, threshold, and observation-window rows. | Plan is complete. |
| 10 | Agent compares findings to the shared `vbd-change-simulation` scenarios. | Agent compares rehearsal findings to scenario results. | Every material scenario is addressed. |
| 11 | Agent writes a schema-valid `MIG-*` record and `OPS-*` state. | Agent reads output schema. | Output records are valid. |

## Output Contract

| Output | Agent Content | Test | Pass |
|---|---|---|---|
| Router plan | Agent writes states and transition rules. | Agent reads router rows. | Every transition has a trigger. |
| Diff log | Agent writes severity, trend, and affected fields per use case. | Agent reads diff rows. | Every use case has one diff result. |
| Compatibility report | Agent writes contract findings and deprecation notes. | Agent reads contract rows. | Every changed contract has one finding row. |
| Rehearsal evidence | Agent writes replay, rollback, and reconciliation evidence. | Agent reads rehearsal rows. | Every rehearsal step has evidence. |
| Drift plan | Agent writes signals, thresholds, and observation window. | Agent reads drift rows. | Drift plan is ready to run. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent does not let the new path affect caller-visible results before cutover. | Agent reads shadow results. | Zero caller-visible new-path changes occur. |
| Agent does not use one global tolerance threshold. | Agent reads tolerance rows. | Every field class has its own rule. |
| Agent runs rollback. | Agent reads rollback evidence. | Rollback evidence exists. |
| Agent uses additive-first versioning rules for contract changes. | Agent reads compatibility rows. | Narrowing changes use a version bump. |
| Agent keeps drift monitoring active through the full observation window. | Agent reads drift plan. | Observation window is explicit. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Diff gate | Agent verifies every in-scope use case meets the agreed diff threshold. | Zero unapproved diff overruns remain. |
| Compatibility gate | Agent verifies every changed contract is compatible or versioned. | Zero unplanned narrowing changes remain. |
| Rollback gate | Agent verifies a real route revert occurred. | Zero dry-run-only rollback evidence remains. |
| Drift gate | Agent verifies the drift plan is ready for immediate cutover use. | Zero missing drift controls remain. |

