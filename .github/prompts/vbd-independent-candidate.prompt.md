---
mode: agent
title: Independent VBD Candidate
description: Produce one independent VBD candidate design and one normalized estimate from one immutable evidence package.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 900
invokes_skills:
  - vbd-advanced-design
  - vbd-system-design
  - ifx-component-communication
  - dotnet-aspire-vbd-development
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
# Independent VBD Candidate

Agent uses one frozen evidence package and ignores other candidate outputs.

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies the evidence-manifest hash. | Agent compares the input hash to the manifest hash. | Hashes match. |
| 2 | Agent derives boundaries from use cases and volatility analysis. | Agent reads boundary rationale. | Every boundary cites use-case or volatility evidence. |
| 3 | Agent applies `vbd-advanced-design` and `vbd-system-design`. | Agent reads skill outputs. | Both skill outputs are represented. |
| 4 | Agent maps `SYM-*` records to retain, move, wrap, split, retire, or generate decisions. | Agent reads mapping rows. | Every mapped symbol has one decision. |
| 5 | Agent defines components, vault contents, contracts, calls, state ownership, failures, and physical development topology. | Agent reads candidate sections. | Every required section exists. |
| 6 | Agent applies `ifx-component-communication` and `dotnet-aspire-vbd-development`. | Agent reads platform sections. | Ifx and Aspire sections exist. |
| 7 | Agent applies `vbd-candidate-estimation`. | Agent reads estimate schema. | Estimate schema is complete. |
| 8 | Agent records the Manager/Engine golden ratio as a sanity note. | Agent reads design notes. | Golden ratio triggers no forced design change. |
| 9 | Agent writes the candidate under `docs/analysis/candidates/candidate-<id>/`. | Agent reads output path. | Path matches the candidate ID. |

## Required Candidate Content

| Topic | Agent Content | Test | Pass |
|---|---|---|---|
| Use-case scope | Use-case coverage and unsupported outcomes | Agent reads `UC-*` links. | Every major outcome is linked. |
| Boundary design | Boundary catalog and volatility analysis rationale | Agent reads boundary rows. | Every boundary has rationale. |
| Communication | Communication matrix and quality notes | Agent reads interaction rows. | Every major interaction has one row. |
| Reuse and gaps | Existing-code reuse and gap maps | Agent reads `SYM-*` rows. | Every reuse claim links to symbols. |
| Platform | Generic proxy, interceptors, logical Address, logical Naming, bus provider design | Agent reads platform rows. | Every platform item exists. |
| Delivery scope | Ifx work, Aspire work, projects, methods, tests, benchmarks, item 3.8 Client, conditional UI | Agent reads estimate and scope rows. | Every delivery item appears. |
| Tooling | Analyzers, code fixes, generators, and generated-code test policy | Agent reads tooling rows. | Every tooling item appears. |
| Estimates | Person-hours, duration, tokens, review range, risks, assumptions, confidence, unresolved decisions | Agent reads estimate rows. | Every estimate field exists. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent writes to one candidate folder only. | Agent reads output paths. | Zero writes target other candidate folders. |
| Agent does not select a winner. | Agent reads final section. | Output contains no selection statement. |
| Agent does not blend candidates. | Agent reads references. | Output references one evidence package only. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Evidence gate | Agent verifies one manifest hash. | Zero hash mismatches remain. |
| Scope gate | Agent verifies every required content topic exists. | Zero required topics remain blank. |
| Estimate gate | Agent verifies every estimate field exists. | Zero estimate fields remain blank. |
