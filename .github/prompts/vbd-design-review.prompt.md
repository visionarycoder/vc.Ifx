---
mode: ask
title: VBD Design Review
description: Produce an evidence-based VBD design review with volatility analysis, boundary authority, communication analysis, and physical mapping.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1320
invokes_skills:
  - vbd-system-design
  - vbd-advanced-design
  - vbd-boundary-contract-mapping
  - ifx-component-communication
  - dotnet-aspire-vbd-development
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
# VBD Design Review

Agent produces a defensible VBD design review from source evidence.

## Scope

| Scope Item | Test | Pass |
|---|---|---|
| Solution scope | Agent reads input scope. | Scope is explicit. |
| Output scope | Agent reads output paths. | Paths use the logical `docs/analysis/**/*` tree. |
| Code scope | Agent reads write targets. | Writes stay under `tools/`. |
| Ifx scope | Agent reads Ifx references. | References use `tools/Data.Management/src/Ifx` only. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent builds a volatility analysis matrix from workflow, history, roadmap, and operations evidence. | Agent reads matrix rows. | Every change driver has source, cadence, impact, confidence, and owner. |
| 2 | Agent proposes logical boundaries before physical mapping. | Agent reads section order. | Logical boundaries appear first. |
| 3 | Agent verifies boundary size, authority, overlap, communication cost, scaling, and failure behavior. | Agent reads comparison rows. | Every boundary has evidence-backed rationale. |
| 4 | Agent maps approved boundaries to Manager, Engine, Access, Contract, Service, and optional Orm structures. | Agent reads mapping rows. | Every logical boundary has one physical mapping row. |
| 5 | Agent writes platform, quality, simulation, and estimate findings. | Agent reads final sections. | Every required section exists. |

## Required Findings

| Topic | Agent Content | Test | Pass |
|---|---|---|---|
| Workflow evidence | Source entry points and workflow evidence | Agent reads workflow map. | Every primary entry point appears or is unresolved explicitly. |
| Boundary catalog | Owned volatility, authority, invariants, stable outcomes, and non-responsibilities | Agent reads boundary rows. | Every boundary row is complete. |
| Size and scope | Split or merge alternatives, interaction cost, propagation cost, overlap decisions, and one Manager/Engine golden-ratio note | Agent reads comparison rows. | Every alternative has rationale. |
| Communication quality | Caller direction, frequency, payload, latency, availability coupling, timeout, cancellation, retry, idempotency, ordering, duplication, recovery, chatty-call findings, cycle findings, and violations | Agent reads interaction rows. | Every major interaction has one row. |
| Proxy and bus | Hidden volatility, generic proxy route, ordered interceptors, logical Address, logical Naming, Manager bus provider contract, startup failure behavior, retry, ordering, and dead-letter behavior | Agent reads platform rows. | Every proxy and bus item appears. |
| Ifx and Aspire | Ifx proxy, interceptors, call context, envelopes, analyzers, code fixes, generators, tests, AppHost, Service Defaults, Dashboard verification, and distributed integration-test topology | Agent reads platform rows. | Every Ifx and Aspire item appears. |
| Project structure | Contract and Service requirements, optional Orm use, DB decoupling, and caller-to-Contract-only evidence with one example | Agent reads structure rows. | Every component has a structure row and one caller example exists. |
| Quality targets | Coverage rules, generated-code test policy, benchmarks with run instructions, item 3.8 Client estimate, and conditional UI estimate | Agent reads quality rows. | Every target is explicit. |
| Simulations | At least three high-value change simulations and one failure simulation | Agent counts simulation rows. | Count meets threshold. |
| Estimate ranges | Person-hours, calendar duration, tokens, review range, assumptions, and unresolved decisions | Agent reads estimate rows. | Every estimate field exists. |

## Candidate Baseline

| Layer | Example Names | Test | Pass |
|---|---|---|---|
| Manager | `FileReplacementManager`, `DatabaseLoadManager` | Agent reads example list. | Example list exists. |
| Engine | `ValidatingEngine`, `TransformingEngine`, `ResolvingEngine` | Agent reads example list. | Example list exists. |
| Access | `StorageAccess`, `CostAccountingAccess`, `CheckpointAccess` | Agent reads example list. | Example list exists. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent uses volatility analysis before physical mapping. | Agent reads section order. | Volatility analysis precedes physical mapping. |
| Agent uses the Manager/Engine golden ratio as a sanity note only. | Agent reads scoring language. | Golden ratio triggers no redesign gate. |
| Agent records direction violations explicitly. | Agent reads communication findings. | Zero hidden direction violations remain. |
| Agent keeps caller references on Contract projects only. | Agent reads caller rows. | Zero caller rows reference Service or Orm directly. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Evidence gate | Agent verifies entry points and workflows are mapped. | Zero unmapped primary entry points remain without an unresolved note. |
| Boundary gate | Agent verifies every boundary has authority and non-responsibility notes. | Zero incomplete boundary rows remain. |
| Communication gate | Agent verifies every major interaction has a quality row. | Zero major interactions remain unreviewed. |
| Estimate gate | Agent verifies every estimate field exists. | Zero estimate fields remain blank. |
