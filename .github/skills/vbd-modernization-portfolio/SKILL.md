---
name: vbd-modernization-portfolio
title: VBD Modernization Portfolio Management
description: Score candidates with visible rationale, track actuals against plan, recompute critical path, and publish program metrics in `PORT-*` records.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1203
prerequisites:
  - vbd-candidate-estimation
  - vbd-change-simulation
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-phased-modernization
  - vbd-contract-first-construction
  - vbd-cutover-migration
appliesTo: '**/*.{json,md,yml,yaml}'
tags:
  - vbd
  - portfolio
  - scoring
  - estimation
  - dashboard
---

# VBD Modernization Portfolio Management

Agent tracks modernization as a program with visible scoring logic, actuals, and trend reporting.

## When to Use

Use when candidate comparison needs a scored input with visible rationale.
Use when construction is running and actuals compare against plan.
Use when sponsors, developers, and operations need one shared dashboard.

## When Not to Use

Do not use when the prompt asks for raw candidate estimates only. Use `vbd-candidate-estimation`.
Do not use when the prompt asks for narrative candidate comparison only. Use `vbd-candidate-synthesis`.
Do not use when the prompt asks for automatic candidate selection. This skill never selects.

## Inputs

| Input | Required | Description |
|---|---|---|
| Candidate estimates | Yes | `EST-*` output per candidate |
| Change-simulation results | Yes | `CSIM-*` blast-radius results |
| Construction task graph | Yes | `TASK-*` output and dependency graph |
| Actuals feed | Yes | Recorded hours, tokens, and dates per task |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Score candidates | Agent scores use-case fit, change containment, communication cost, reuse, risk, and cost with one note per score. | Scorecards | Agent reads the scorecards. | Every numeric score has one adjacent note. |
| 2. Flag ratio note | Agent records manager and engine ratio as a note only. | Score note log | Agent reads note log. | Ratio is absent from weighted scoring rows. |
| 3. Compare plan and actuals | Agent records planned and actual hours, duration, and tokens per task. | Actuals ledger | Agent reads the ledger. | Every completed `TASK-*` row has actuals. |
| 4. Recompute forecast | Agent recomputes critical path and forecast when threshold variance occurs. | Forecast packet | Agent reads forecast packet. | Threshold breach produces one recomputed forecast. |
| 5. Publish dashboard | Agent publishes role-based metrics for developers, sponsors, and operations. | Dashboard snapshot | Agent reads the dashboard. | Every audience sees its required metrics. |
| 6. Freeze portfolio record | Agent writes `PORT-*` records and history. | Portfolio packet | Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`. | Exit code = 0. Portfolio links resolve. |

## Scoring Matrix

| Criterion | Weight guidance | Required note |
|---|---|---|
| Use-case fit | High | Weak or unsupported outcomes |
| Change containment | High | `CSIM-*` blast-radius note |
| Communication cost | Medium | Chatty or cyclic flow note |
| Existing-code reuse | Medium | Reuse confidence note |
| Risk and confidence | High | Named risk note |
| Cost | Medium | Range and assumption note |

## Dashboard Metrics

| Audience | Required metrics |
|---|---|
| Developer | Task variance, forecast to complete, drift incidents |
| Sponsor | Waves migrated, parity rate, schedule variance |
| Operations | Drift incidents, rollback count, coverage |

## Output Contract

| Output | Minimum content |
|---|---|
| Candidate scorecards | Weighted scores and notes |
| Actuals ledger | Planned versus actual hours, duration, and tokens |
| Forecast history | Critical path recalculation history |
| Dashboard | Audience-specific metrics |
| Portfolio record | `PORT-*` packet |

## Verification

- [ ] Agent gives every score one visible note.
- [ ] Agent keeps ratio as a note, not a gate.
- [ ] Agent records actuals per task, not after-the-fact estimates.
- [ ] Agent recomputes forecast on threshold breach.
- [ ] Agent publishes required metrics to each audience.

Test: Read the scorecards, ledger, and dashboard snapshot.
Pass: Zero score lacks rationale. Zero weighted row uses ratio. Every audience metric set is present.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Composite score hides weak areas | Agent keeps criterion rows visible |
| Ratio becomes a hard gate | Agent records ratio as note only |
| Actuals are reconciled at project end only | Agent records actuals per task |
| Dashboard shows status only | Agent includes drift, rollback, and variance |
