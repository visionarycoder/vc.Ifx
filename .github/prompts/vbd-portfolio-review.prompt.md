---
mode: ask
title: VBD Portfolio Review
description: Score modernization candidates, refresh actuals against plan, and write a metrics dashboard for developer, sponsor, and operations audiences.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 900
invokes_skills:
  - vbd-modernization-portfolio
  - vbd-candidate-estimation
  - vbd-candidate-synthesis
  - vbd-phased-modernization
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
# VBD Portfolio Review

Agent applies `vbd-modernization-portfolio` to write a program-level status view.

## Scope

| Scope Item | Agent Action | Test | Pass |
|---|---|---|---|
| Candidate scope | Agent includes all active candidates that await selection. | Agent reads candidate list. | Every active candidate appears. |
| Delivery scope | Agent includes every construction wave and migration wave in flight. | Agent reads wave list. | Every in-flight wave appears. |
| Decision scope | Agent writes reporting only. | Agent reads output actions. | Prompt selects no candidate and approves no cutover. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent collects candidate estimates from `vbd-candidate-estimation` and blast-radius findings from `vbd-change-simulation`. | Agent reads source records. | Every open candidate has estimate and blast-radius data. |
| 2 | Agent scores each candidate with the weighted criteria table and one qualitative note per score. | Agent reads score rows. | Every score has one note. |
| 3 | Agent records the Manager/Engine golden ratio as an informational note only. | Agent reads scoring rules. | Golden ratio is not a score or gate. |
| 4 | Agent hands normalized scores to `vbd-candidate-synthesis`. | Agent reads synthesis handoff. | Handoff uses normalized schema. |
| 5 | Agent reads actual hours, tokens, and dates for work in flight. | Agent reads actuals ledger. | Every tracked task has actuals or an explicit gap. |
| 6 | Agent calculates variance against the `vbd-contract-first-construction` plan. | Agent reads variance rows. | Every tracked task has variance status. |
| 7 | Agent recalculates critical path and forecast to complete when a task crosses its variance threshold. | Agent reads recalculation triggers. | Recalculation occurs for every triggered task set. |
| 8 | Agent refreshes the modernization dashboard for candidate count, selected count, migrated waves, remaining waves, use-case parity, coverage, drift incidents, rollback count, token burn, and schedule variance. | Agent reads metric rows. | Every dashboard metric has a value. |
| 9 | Agent routes audience-specific dashboard views to developer, sponsor, and operations readers. | Agent reads audience sections. | Every audience has one tailored view. |
| 10 | Agent writes scores, actuals, and metrics in a schema-valid `PORT-*` record. | Agent reads output schema. | Output record is valid. |

## Output Contract

| Output | Agent Content | Test | Pass |
|---|---|---|---|
| Candidate scorecard | Agent writes per-criterion score and note. | Agent reads score rows. | Every score row has a note. |
| Actuals ledger | Agent writes plan, actual, variance, and root cause. | Agent reads ledger rows. | Every tracked task has one row. |
| Critical path update | Agent writes recalculated path when a trigger fires. | Agent reads trigger results. | Every trigger has a recalculation result. |
| Dashboard snapshot | Agent writes segmented metrics per audience. | Agent reads audience sections. | Every audience section exists. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent does not let a numeric score stand without a qualitative note. | Agent reads score rows. | Zero note-free scores remain. |
| Agent does not use the golden ratio as a pass or fail gate. | Agent reads gate rules. | Golden ratio appears as note only. |
| Agent does not select a candidate or approve a cutover. | Agent reads actions. | Output contains no selection or approval action. |
| Agent records actuals as reported data. | Agent reads actuals notes. | Zero retroactive estimates appear as actuals. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Score gate | Agent verifies every score has a note. | Zero missing notes remain. |
| Actuals gate | Agent verifies ledger rows show reported data and variance. | Zero ledger rows omit plan, actual, or variance. |
| Dashboard gate | Agent verifies every audience has relevant metrics. | Zero audience sections remain blank. |
