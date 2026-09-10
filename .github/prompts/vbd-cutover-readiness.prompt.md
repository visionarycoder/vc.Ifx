---
mode: agent
description: Verify one VBD migration wave for traffic readiness. Record parity, routing, data, observability, resilience, performance, rollback, and operations evidence.
title: VBD Cutover Readiness
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 820
invokes_skills:
  - vbd-cutover-migration
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
# VBD Cutover Readiness

Agent verifies one proposed migration wave for traffic readiness.

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies the selected-plan hash, evidence hash, and use-case hash. | Agent compares recorded hashes to current hashes. | Every required hash matches. |
| 2 | Agent verifies the wave completes one observable use-case outcome set. | Agent compares wave scope to `UC-*` records. | Every in-scope outcome is covered. |
| 3 | Agent runs Contract, Service, integration, Client, and conditional UI tests. | Agent reads test results. | Every in-scope test suite passes. |
| 4 | Agent verifies coverage and analyzer gates. | Agent reads gate reports. | Every required gate passes. |
| 5 | Agent verifies component calls use the proxy and Manager coordination uses the configured bus provider. | Agent reads runtime and design evidence. | Zero unapproved call paths remain. |
| 6 | Agent verifies provider selection fails at startup for missing, unknown, or incompatible configuration. | Agent runs configuration failure scenarios. | Every invalid scenario fails safely. |
| 7 | Agent verifies data backfill, reconciliation, authority transfer, retention, and privacy controls. | Agent reads migration and data evidence. | Every data gate has evidence. |
| 8 | Agent verifies security, correlation, logs, traces, metrics, health, alerts, and dashboards. | Agent reads observability evidence. | Every operations signal is present. |
| 9 | Agent verifies benchmarks and load results against documented thresholds. | Agent compares measured values to threshold values. | Every threshold passes or has an approved risk. |
| 10 | Agent verifies failure, cancellation, retry, idempotency, replay, poison handling, and dependency outage behavior. | Agent reads rehearsal and simulation results. | Every resilience scenario has a pass result or blocker. |
| 11 | Agent verifies routing controls, stop thresholds, rollback, forward recovery, and operator authority. | Agent reads runbook and rehearsal evidence. | Every control is documented and rehearsed. |
| 12 | Agent writes one readiness result. | Agent reads final status. | Status is `ready`, `ready-with-approved-risks`, or `not-ready`. |

## Output Contract

| Output | Agent Content | Test | Pass |
|---|---|---|---|
| Readiness result | Agent writes one final status. | Agent reads final status field. | Field uses an allowed value. |
| Gate ledger | Agent writes evidence, blocker, risk, owner, and next task per gate. | Agent reads gate rows. | Every gate row is complete. |
| Risk ledger | Agent writes approved risks and approval references. | Agent reads risk rows. | Every approved risk has a reference. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent does not infer readiness from project completion alone. | Agent reads readiness rationale. | Rationale cites gate evidence. |
| Agent does not unblock decommissioning before observation and rollback windows close. | Agent reads decommission status. | Decommission remains blocked before window close. |
| Agent does not hide blockers inside approved risks. | Agent compares blocker rows to risk rows. | Blocking rows stay blocking. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Outcome gate | Agent verifies every in-scope use-case outcome has evidence. | Zero uncovered outcomes remain. |
| Operations gate | Agent verifies every signal and dashboard exists. | Zero missing operations signals remain. |
| Recovery gate | Agent verifies rollback and forward recovery were rehearsed. | Zero unrehearsed recovery paths remain. |
| Status gate | Agent verifies the final status matches gate evidence. | Zero unsupported status values remain. |
