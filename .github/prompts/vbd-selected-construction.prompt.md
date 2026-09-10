---
mode: agent
title: Selected VBD Construction Plan
description: Convert the selected VBD candidate into a contract-first construction graph with gates, traceability, token budgets, and checkpoints.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 930
invokes_skills:
  - vbd-contract-first-construction
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
# Selected VBD Construction Plan

Agent applies `vbd-contract-first-construction` after developer selection.

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent records the selected candidate ID, evidence-manifest hash, and approved hybrid changes. | Agent reads selection metadata. | Every selection field exists. |
| 2 | Agent rejects stale selection when evidence changed. | Agent compares current evidence hash to selection hash. | Hash mismatch blocks planning. |
| 3 | Agent plans shared Ifx, proxy, bus provider, Aspire, templates, analyzers, code fixes, generators, and test-helper work. | Agent reads shared-work rows. | Every shared work item exists. |
| 4 | Agent builds one dependency graph per component. | Agent reads component graph rows. | Every component has one graph. |
| 5 | Agent places Contract plus contract tests before dependent implementation. | Agent reads dependency order. | Every Contract task precedes dependent tasks. |
| 6 | Agent links every task through `UC -> component -> project -> operation -> symbol or gap -> test -> benchmark`. | Agent reads traceability rows. | Every task has one trace chain. |
| 7 | Agent adds build, analyzer, test, coverage, dependency, proxy, bus, documentation, and benchmark gates. | Agent reads gate rows. | Every task stage has explicit gates. |
| 8 | Agent calculates critical path, parallel waves, person-hours, calendar duration, and token budgets. | Agent reads estimate rows. | Every estimate field exists. |
| 9 | Agent creates resumable checkpoints after stable gates. | Agent reads checkpoint rows. | Every stable gate has a checkpoint. |
| 10 | Agent writes outputs to `docs/analysis/selected-plan/`. | Agent reads output paths. | Output paths use the selected-plan tree. |

## Component Graph Schema

| Node Type | Agent Content | Test | Pass |
|---|---|---|---|
| Contract | Contract project and contract tests | Agent reads graph nodes. | Nodes exist for every component. |
| Service | Service project and unit tests | Agent reads graph nodes. | Nodes exist for every component. |
| Optional data or simulation | Optional Orm, Simulator, and Emulator work | Agent reads graph nodes. | Optional nodes are explicit or `N/A`. |
| Integration | Integration tests | Agent reads graph nodes. | Nodes exist for every component. |
| Benchmark | Benchmarks and exact run instructions | Agent reads graph nodes. | Benchmark nodes include run instructions. |
| Client | Item 3.8 Client work | Agent reads graph nodes. | Client nodes exist. |
| UI | Conditional UI work | Agent reads graph nodes. | UI nodes are explicit or `N/A`. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent puts Contract work before dependent work. | Agent reads dependency graph. | Zero dependent tasks precede Contract tasks. |
| Agent lets unit tests gate component progress. | Agent reads gate rows. | Every component has a unit-test gate. |
| Agent preserves use-case outcomes and does not preserve obsolete internal compatibility. | Agent reads compatibility notes. | Notes target outcome compatibility only. |
| Agent does not change the selected architecture without developer approval. | Agent reads change notes. | Architecture changes have approval references. |

## Output Contract

| Output | Agent Content | Test | Pass |
|---|---|---|---|
| Dependency graph | Agent writes DAG rows and wave groups. | Agent reads graph rows. | Graph is acyclic and grouped. |
| Gate ledger | Agent writes gate type, evidence, and owner. | Agent reads ledger rows. | Every gate row is complete. |
| Estimate ledger | Agent writes hours, duration, tokens, and critical path. | Agent reads estimate rows. | Every estimate row is complete. |
| Checkpoint plan | Agent writes stable-gate checkpoints and resume inputs. | Agent reads checkpoint rows. | Every checkpoint row is complete. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Selection gate | Agent verifies evidence and selection hashes match. | Zero stale selections remain active. |
| Graph gate | Agent verifies every component has a graph. | Zero component graphs remain missing. |
| Traceability gate | Agent verifies every task has a trace chain. | Zero orphan tasks remain. |
| Gate gate | Agent verifies every stage has explicit gates. | Zero ungated stages remain. |
