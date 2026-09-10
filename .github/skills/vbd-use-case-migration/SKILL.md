---
name: vbd-use-case-migration
title: VBD Use-Case Migration Harness
description: Build and run a migration harness with routing, shadow execution, tolerance comparison, replay, rollback, and drift monitoring for `UC-*` outcomes.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1165
prerequisites:
  - vbd-use-case-reconstruction
  - vbd-cutover-migration
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-change-simulation
  - vbd-operational-contracts
  - vbd-architecture-conformance
appliesTo: '**/*.{cs,csproj,sln,slnx,json,md,yml,yaml}'
tags:
  - vbd
  - migration
  - harness
  - shadow-execution
  - drift
---

# VBD Use-Case Migration Harness

Agent builds the harness that proves a migration wave is safe before and after cutover.

## When to Use

Use when a migration wave needs old and new implementations behind one routing decision.
Use when the user needs outcome comparison before live traffic trusts the new path.
Use when captured traffic replays against the new path.
Use when cutover finished and drift still needs observation.

## When Not to Use

Do not use when the prompt asks for wave planning only. Use `vbd-cutover-migration`.
Do not use when no use-case catalog exists. Use `vbd-use-case-reconstruction`.
Do not use when the prompt is only hypothetical impact analysis. Use `vbd-change-simulation`.

## Inputs

| Input | Required | Description |
|---|---|---|
| Use-case catalog | Yes | `UC-*` outcomes and acceptance criteria |
| Old and new implementations | Yes | One logical entry point reaches both |
| Tolerance policy | Yes | Exact, normalized, ignore, and numeric-band rules |
| Observation window | Yes | Time and threshold rules for post-cutover drift |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Configure routing | Agent writes logical routing states for old, canary, and full cutover. | Routing table | Agent reads the routing table. | Every `UC-*` route has explicit old, canary, and full states. |
| 2. Run shadow mode | Agent routes sampled or full traffic through both paths while old stays authoritative. | Shadow run log | Agent reads shadow results. | Caller-visible results come from old path only. |
| 3. Compare outcomes | Agent applies field rules from the tolerance policy. | Diff log | Agent reads diff log. | Every diff record names `UC-*`, field, expected, actual, and severity. |
| 4. Rehearse replay | Agent replays recorded traffic against the new path after each changed build. | Replay report | Agent reads replay report. | Replayed cases meet tolerance thresholds. |
| 5. Rehearse rollback | Agent routes traffic back to the old path and verifies data authority. | Rollback report | Agent reads rollback report. | Old path resumes service and reconciliation rules pass. |
| 6. Monitor drift | Agent keeps comparison and operational telemetry active through the observation window. | Drift report | Agent reads trend report. | Diff rate, latency, and error thresholds stay inside policy for the full window. |

## Tolerance Rules

| Field class | Rule |
|---|---|
| Business-critical value | Exact match |
| Generated identifier or timestamp | Normalize or ignore |
| Numeric result | Use stated absolute or relative band |
| Ordering-insensitive collection | Compare as set |
| Undocumented new field | Flag as diff |

## Output Contract

| Output | Minimum content |
|---|---|
| Routing packet | Current route per `UC-*` |
| Diff log | Severity, count, trend, and tolerance rule |
| Replay report | Recorded batch, result, and drift summary |
| Rollback report | Route reversal and reconciliation evidence |
| Observation record | `MIG-*` rehearsal record and closeout notes |

## Verification

- [ ] Agent keeps routing as configuration, not business logic.
- [ ] Agent keeps old path authoritative during shadow mode.
- [ ] Agent writes per-field tolerance rules.
- [ ] Agent runs replay and rollback drills.
- [ ] Agent keeps drift monitoring active for the full observation window.

Test: Run the harness verification suite for the selected wave.
Pass: Diff rate stays at or below policy threshold. Rollback drill passes. Observation window closes with zero unresolved drift.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Shadow mode becomes dual-write | Agent discards new-path side effects before cutover |
| One global threshold hides material diffs | Agent writes per-field rules |
| Rollback exists on paper only | Agent runs a real rollback drill |
| Drift monitoring stops at cutover | Agent keeps monitoring through the full window |
