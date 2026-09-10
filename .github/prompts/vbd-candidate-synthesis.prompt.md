---
mode: agent
title: VBD Candidate Synthesis
description: Normalize and compare independent VBD candidates. Write a decision package and pause for developer selection.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 760
invokes_skills:
  - vbd-candidate-estimation
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
# VBD Candidate Synthesis

Agent compares candidates that share one frozen evidence package.

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies every candidate uses the same evidence-manifest hash. | Agent compares candidate metadata. | Every candidate uses one hash, or Agent flags the mismatch. |
| 2 | Agent normalizes component names without changing candidate intent. | Agent compares normalized names to source names. | Intent stays unchanged. |
| 3 | Agent normalizes estimates with `vbd-candidate-estimation`. | Agent compares estimate fields across candidates. | Estimate fields use one schema. |
| 4 | Agent compares use-case fit, volatility analysis, communication cost, reuse, proxy work, bus work, Ifx, Aspire, tests, coverage, benchmarks, Client, UI, analyzers, generators, migration, operations, risks, assumptions, confidence, and critical path. | Agent reads the comparison matrix. | Every candidate has one row per criterion. |
| 5 | Agent records the Manager/Engine golden ratio as an informational note. | Agent reads the scoring section. | Agent uses no golden-ratio score or gate. |
| 6 | Agent writes a pros and cons decision package and scorecard. | Agent reads output sections. | Package contains comparison matrix, notes, and summary. |
| 7 | Agent pauses for developer selection or explicit hybridization. | Agent reads the final instruction. | Prompt ends with a pause. |

## Comparison Matrix

| Criterion | Agent Evidence | Test | Pass |
|---|---|---|---|
| Use-case outcomes | `UC-*` coverage and unsupported outcomes | Agent compares candidate scope to `UC-*` records. | Every gap is explicit. |
| Volatility analysis | Boundary fit for change drivers and invariants | Agent reads rationale for each boundary. | Every boundary has volatility analysis evidence. |
| Communication | Call count, latency, failure, and availability coupling | Agent reads interaction matrix. | Every major interaction has a quality note. |
| Reuse and gaps | `SYM-*` reuse and missing symbols | Agent compares claims to mapping records. | Every reuse claim is cited. |
| Delivery scope | Contract, Service, Access, Ifx, Aspire, tests, benchmarks, Client, UI, analyzers, generators | Agent reads estimate lines. | Every work item is represented. |
| Migration and operations | Routing, rollback, data, observability, and risk | Agent reads migration notes. | Every candidate has migration notes. |
| Estimate quality | Hours, duration, tokens, review, confidence, and critical path | Agent compares estimate schema. | Every estimate uses one schema. |

## Output Contract

| Output | Agent Content | Test | Pass |
|---|---|---|---|
| Comparison matrix | Agent writes one row per candidate and criterion. | Agent counts matrix rows. | Every candidate appears in every criterion group. |
| Scorecard | Agent writes numeric score and qualitative note per criterion. | Agent reads score cells. | Every score has one note. |
| Decision package | Agent writes pros, cons, risks, assumptions, and unresolved decisions. | Agent reads package sections. | Every section exists. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent does not merge candidates silently. | Agent reads synthesis notes. | Every hybrid note is explicit. |
| Agent does not invent a fourth design. | Agent counts candidate IDs. | Output uses input candidate IDs only. |
| Agent does not choose a winner automatically. | Agent reads final section. | Output ends with developer selection pause. |
| Agent separates normalization from judgment. | Agent reads matrix notes. | Facts and judgments appear in separate sections. |
| Agent preserves candidate-specific assumptions and uncertainty. | Agent reads assumption rows. | Every candidate retains its own assumptions. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Evidence gate | Agent verifies one evidence hash across candidates. | Zero unflagged hash mismatches remain. |
| Scoring gate | Agent verifies every score has a note. | Zero note-free scores remain. |
| Pause gate | Agent verifies no selection text appears. | Zero automatic selection statements remain. |
