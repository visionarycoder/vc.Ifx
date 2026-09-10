---
mode: agent
title: Phased VBD Modernization Planning
description: Analyze one .NET solution, reconstruct behavior, generate independent VBD candidates, pause for selection, and prepare a contract-first implementation graph.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1290
invokes_skills:
  - vbd-phased-modernization
  - vbd-system-design
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
# Phased VBD Modernization Planning

Agent applies `vbd-phased-modernization` to one selected solution.

## Defaults

| Default | Test | Pass |
|---|---|---|
| Solution scope | Agent reads scope input. | Scope is the complete selected solution. |
| Compatibility scope | Agent reads compatibility notes. | Notes target use-case outcomes only. |
| Evidence rule | Agent reads conflict notes. | Misaligned code is flagged. |
| Optional scope | Agent reads estimate rows. | UI and Schedule rows are explicit. |
| Output scope | Agent reads output paths. | Paths use the logical `docs/analysis/**/*` tree. |
| Coverage rule | Agent reads coverage notes. | Generated artifacts are excluded and generated behavior stays tested. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent captures build, test, coverage, analyzer, framework, package, and benchmark baselines. | Agent reads baseline records. | Every baseline category has one record. |
| 2 | Agent applies `vbd-symbolic-mapping` and `vbd-use-case-reconstruction`. | Agent reads `SYM-*` and `UC-*` records. | Symbol and use-case evidence exist. |
| 3 | Agent applies `vbd-evidence-quality`, fixes analysis blockers, and freezes the evidence package. | Agent reads blocker ledger and manifest hash. | Blockers are explicit and manifest hash exists. |
| 4 | Agent launches at least three independent candidate designs. | Agent counts candidate IDs. | Candidate count is three or greater. |
| 5 | Agent verifies every candidate uses one shared requirement schema. | Agent compares candidate sections. | Every candidate contains the same required topics. |
| 6 | Agent red-teams every candidate with one shared `vbd-change-simulation` scenario catalog. | Agent compares scenario hashes. | Every candidate uses one scenario hash. |
| 7 | Agent writes the decision package. | Agent reads decision sections. | Package is complete. |
| 8 | Agent routes to the developer-decision prompt and pauses for selection. | Agent reads final instruction. | Prompt pauses for developer input. |
| 9 | Agent applies `vbd-contract-first-construction` after selection. | Agent reads post-selection tasks. | Construction starts after selection only. |
| 10 | Agent applies `vbd-cutover-migration` before moving use-case traffic. | Agent reads migration gate. | Traffic move is gated. |

## Candidate Requirement Schema

| Topic | Agent Content | Test | Pass |
|---|---|---|---|
| Boundary design | Use-case-driven boundaries, volatility analysis, and reuse or gap mapping | Agent reads candidate rows. | Every candidate has cited boundary rationale. |
| Delivery scope | Contract, Service, optional Orm, Simulator, Emulator, unit tests, integration tests, benchmarks, UI, and item 3.8 Client | Agent reads estimate rows. | Every candidate uses the same delivery fields. |
| Platform | Ifx content, generic proxy, interceptors, bus provider contract, local in-memory provider, deployable providers, `appsettings.json` selection, logical Address, and logical Naming | Agent reads platform rows. | Every platform item appears. |
| Aspire and tooling | AppHost, Service Defaults, Dashboard, distributed tests, analyzers, code fixes, and generators | Agent reads platform rows. | Every Aspire and tooling item appears. |
| Quality and estimates | Coverage targets, golden-ratio note, benchmark rules, person-hours, duration, tokens, review, risks, assumptions, and confidence | Agent reads quality rows. | Every quality and estimate field exists. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent uses the latest stable .NET release and C# 14 or later when the project supports it. | Agent reads technology notes. | Version note is explicit. |
| Agent does not use underscore-prefixed identifiers. | Agent reads generated identifiers. | Zero underscore-prefixed identifiers appear. |
| Agent does not invent Schedule requirements when scheduling is absent. | Agent reads scope notes. | Schedule content appears only when evidence exists. |
| Agent blocks direct Manager sibling calls and proxy bypasses. | Agent reads communication rows. | Zero direct sibling calls and zero proxy bypasses remain. |
| Agent does not copy local Microsoft or IDesign reference implementations. | Agent reads citation notes. | Zero forbidden copies appear. |
| Agent fails explicitly for missing, unknown, or incompatible bus-provider configuration. | Agent reads configuration rules. | Failure behavior is explicit. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Traceability gate | Agent verifies evidence links use cases to symbols. | Zero unlinked use-case outcomes remain. |
| Independence gate | Agent verifies candidates are independent. | Zero blended candidate outputs remain. |
| Estimate gate | Agent verifies estimates use ranges and assumptions. | Zero estimate rows omit range or assumption fields. |
| Platform gate | Agent verifies Ifx and Aspire costs are visible. | Zero hidden platform costs remain. |
| Construction gate | Agent verifies Contract projects precede dependent projects. | Dependency graph honors Contract-first order. |
| Quality gate | Agent verifies unit tests, coverage, analyzers, documentation, and benchmarks gate progression. | Every gate is explicit. |

