---
mode: ask
title: VBD Quick Hints
description: Give concise VBD hints for boundary, communication, proxy, evolution, and implementation decisions.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: low
estimated_tokens: 740
invokes_skills:
  - vbd-system-design
  - vbd-boundary-contract-mapping
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
# VBD Quick Hints

Agent gives concise VBD guidance for consolidation decisions.

## Hint Workflow

| Priority | Agent Hint | Test | Pass |
|---|---|---|---|
| 1 | Agent starts with credible change drivers and volatility analysis. | Agent reads hint order. | Volatility analysis appears first. |
| 2 | Agent groups work by activity and policy authority. | Agent reads boundary hint. | Hint avoids source-tool grouping. |
| 3 | Agent sizes boundaries by change containment and total communication cost. | Agent reads size hint. | Hint cites containment and communication cost. |
| 4 | Agent states what each boundary owns, decides, and rejects. | Agent reads ownership hint. | Hint includes ownership and non-responsibility. |
| 5 | Agent uses shared Engines only when policy volatility and one stable contract justify reuse. | Agent reads Engine hint. | Hint includes both conditions. |
| 6 | Agent merges chatty boundaries unless independent change, authority, scale, or failure evidence supports a split. | Agent reads merge hint. | Hint includes split evidence types. |
| 7 | Agent verifies latency, availability, idempotency, ordering, cancellation, and recovery for every interaction. | Agent reads interaction hint. | Hint includes all interaction qualities. |
| 8 | Agent adds a proxy when the proxy shields named location, protocol, security, version, routing, resilience, or aggregation volatility. | Agent reads proxy hint. | Hint includes explicit volatility triggers. |
| 9 | Agent separates logical Services from projects, processes, containers, repositories, teams, and data stores. | Agent reads Service hint. | Hint keeps logical and physical layers separate. |
| 10 | Agent maps approved boundaries to Contract plus Service plus optional Orm. | Agent reads mapping hint. | Hint uses repository implementation profile. |
| 11 | Agent keeps caller references on Contract assemblies only. | Agent reads caller hint. | Hint blocks Service and Orm references. |
| 12 | Agent simulates high-value changes and compares contract, caller, deployment, and data-migration blast radius. | Agent reads simulation hint. | Hint includes four blast-radius types. |
| 13 | Agent routes component calls through the generic proxy and Manager coordination through the bus. | Agent reads routing hint. | Hint includes both routes. |
| 14 | Agent includes Ifx and Aspire work in every candidate estimate. | Agent reads estimate hint. | Hint includes both platform scopes. |
| 15 | Agent uses coverage targets for solution scope, records, contract data types, and Ifx. | Agent reads coverage hint. | Hint includes all target scopes. |

## Response Contract

| Section | Agent Content | Test | Pass |
|---|---|---|---|
| Prioritized hints | Agent writes top recommendations in priority order. | Agent reads section order. | Order matches hint workflow. |
| Exceptions | Agent writes deviations and rationale. | Agent reads exception rows. | Every deviation has rationale. |
| Boundary notes | Agent writes volatility ownership and caller direction. | Agent reads boundary rows. | Every boundary note is explicit. |
| Size and scope snapshot | Agent writes change ownership, authority, split or merge rationale, and interaction cost. | Agent reads snapshot rows. | Snapshot is complete. |
| Proxy and evolution notes | Agent writes proxy scope, deployment mapping, compatibility, migration, and rollback notes. | Agent reads proxy rows. | Notes are complete. |
| Development and quality notes | Agent writes Ifx, Aspire, coverage, analyzers, code fixes, generators, integration tests, benchmarks, and item 3.8 Client estimate notes. | Agent reads quality rows. | Notes are complete. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Priority gate | Agent verifies hints stay in architectural impact order. | Zero out-of-order hints remain. |
| Terminology gate | Agent verifies VBD terms use Service, Manager, Engine, and volatility analysis. | Zero prohibited VBD terms remain. |
| Scope gate | Agent verifies response sections are complete. | Zero response sections remain blank. |
