---
name: vbd-cutover-migration
title: VBD Cutover Migration
description: Plan phased cutover with coexistence, routing, data authority changes, observability, rollback, and `CUT-*` wave records.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1259
prerequisites:
  - vbd-contract-first-construction
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-use-case-migration
  - vbd-drift-analyzer-design
appliesTo: '**/*.{cs,csproj,sln,slnx,json,md,yml,yaml}'
tags:
  - vbd
  - cutover
  - migration
  - rollback
---

# VBD Cutover Migration

Agent plans phased migration from the old implementation to the selected VBD implementation.

## When to Use

Use when a selected design is entering phased construction or release.
Use when old and new implementations coexist.
Use when data authority, routing, clients, and operations need one cutover plan.
Use when rollback and decommission decisions need explicit evidence.

## When Not to Use

Do not use when no candidate is selected.
Do not use when use-case outcomes and acceptance rules are unresolved.
Do not use when the prompt asks for construction sequencing only.

## Inputs

| Input | Required | Description |
|---|---|---|
| Selected-plan manifest | Yes | Approved candidate, manifest hash, and task graph |
| Use-case catalog | Yes | Outcome compatibility and acceptance rules |
| Existing topology | Yes | Hosts, routes, data, clients, jobs, and dependencies |
| Target topology | Yes | Components, proxy, providers, Aspire test topology, and deployment plan |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Generate migration waves | Agent groups work by complete `UC-*` outcome, not by project. | Wave table | Agent reads the wave table. | Every wave contains contracts, code, data, routing, telemetry, and rollback notes. |
| 2. Pick coexistence pattern | Agent writes feature-flag, canary, shadow, dual-read, dual-write, or replay rules per wave. | Coexistence matrix | Agent reads the matrix. | Every wave has one explicit coexistence rule and one authority rule. |
| 3. Generate data plan | Agent writes ownership change, transformation, backfill, reconciliation, retention, and rollback limits. | Data plan | Agent reads the data plan. | Authority transfer moment is explicit for every changed store. |
| 4. Write gates | Agent writes acceptance, proxy, bus, security, telemetry, performance, and operator gates. | Gate packet | Agent reads the packet. | Every gate has one metric and one threshold. |
| 5. Rehearse wave | Agent uses Aspire and migration harness rehearsals for failure, cancellation, replay, and rollback. | Rehearsal report | Agent reads the report. | Rehearsal covers dependency failure, rollback, and stale configuration. |
| 6. Close and decommission | Agent writes rollback window, observation window, and old-path removal plan. | Decommission packet | Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]` when governed artifacts exist. | Exit code = 0. `CUT-*` links resolve. |

## Coexistence Matrix

| Pattern | Use when | Pass |
|---|---|---|
| Feature flag or logical route | One caller set needs deterministic routing | Route state is reversible |
| Shadow mode | Outcome parity needs live comparison | Caller-visible result stays on old path |
| Canary | Risk needs partial exposure | Threshold breach triggers route reversal |
| Dual read | Data parity needs comparison | One writer stays authoritative |
| Controlled dual write | Reconciliation and idempotency exist | Duplicate side effects stay zero |
| Event replay | Event-driven cutover needs rehearsal | Replay matches expected order and effect |

## Output Contract

| Output | Minimum content |
|---|---|
| Wave packet | `CUT-*` records and `UC-*` membership |
| Coexistence plan | Routing and authority rules |
| Data migration plan | Transform, reconciliation, retention, and rollback limits |
| Gate packet | Metrics, thresholds, and owner |
| Decommission plan | Old routes, writers, subscriptions, credentials, and compatibility code |

## Verification

- [ ] Agent slices waves by complete use-case outcome.
- [ ] Agent writes authority change and rollback limits explicitly.
- [ ] Agent writes measurable thresholds for cutover and rollback.
- [ ] Agent blocks uncontrolled duplicate writes and handlers.
- [ ] Agent delays decommission until rollback and observation windows close.

Test: Read the cutover packet and wave gate metrics.
Pass: Every wave has measurable thresholds, explicit rollback, and explicit authority change.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Project completion defines the wave | Agent groups by `UC-*` outcome |
| Dual write is assumed safe | Agent writes idempotency and reconciliation rules |
| Rollback ignores migrated data | Agent writes reverse transform or forward recovery rules |
| Old paths remain forever | Agent writes decommission owner and deadline |
