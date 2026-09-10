---
mode: agent
title: VBD Conformance Planning
description: Run conformance tests for a selected VBD design. Write a remediation-ready findings report.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 860
invokes_skills:
  - vbd-architecture-conformance
  - vbd-operational-contracts
  - vbd-drift-analyzer-design
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
# VBD Conformance Planning

Agent proves that the selected VBD design still holds.

## Scope

| Scope Item | Agent Action | Test | Pass |
|---|---|---|---|
| Solution scope | Agent uses the complete selected solution or the explicit `allowed_paths` scope. | Agent reads scope input. | Scope is explicit. |
| Write scope | Agent writes findings and remediation tasks only. | Agent reads output actions. | Prompt applies no direct fixes. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads selected-design rules from `vbd-system-design` and prior decision records. | Agent lists dependency, naming, and vault rules. | Rule catalog is loaded. |
| 2 | Agent runs dependency and symbol assertions from `vbd-architecture-conformance`. | Agent runs graph assertions for proxy-only calls, no Manager-to-Manager calls, Contract-only caller references, vault isolation, Ifx purity, and naming compliance. | Every rule has a result. |
| 3 | Agent runs the bus-provider suite for every configured provider. | Agent tests local in-memory, deployed in-memory, and durable providers. | Every configured provider has a result. |
| 4 | Agent verifies observability contract fields with `vbd-operational-contracts`. | Agent reads correlation, causation, schema version, health, metrics, and trace fields. | Every component has a field result. |
| 5 | Agent classifies every finding as blocking or advisory. | Agent reads finding rows. | Every finding has one class. |
| 6 | Agent routes remediation to `vbd-boundary-contract-mapping` or `vbd-drift-analyzer-design`. | Agent reads remediation links. | Every finding links to one remediation route. |
| 7 | Agent instantiates analyzer, code-fix, or generator templates for rules that lack automation. | Agent compares rule results to automation coverage. | Every manual-only rule has an automation task. |
| 8 | Agent writes a `CONF-*` report with owners and remediation order. | Agent reads report schema. | Report is schema-valid. |

## Output Contract

| Output | Agent Content | Test | Pass |
|---|---|---|---|
| Conformance report | Agent writes rule ID, status, evidence, owner, and route. | Agent reads report rows. | Every rule row is complete. |
| Bus-provider matrix | Agent writes one row per provider and test. | Agent counts provider rows. | Every configured provider appears. |
| Observability report | Agent writes one row per component and required field. | Agent counts component rows. | Every component appears. |
| Remediation backlog | Agent writes ordered tasks with owner and route. | Agent reads backlog rows. | Every blocking finding has a task. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent does not redesign boundaries. | Agent reads findings text. | Findings target the selected design only. |
| Agent does not waive violations silently. | Agent reads waiver rows. | Every waiver has expiration and rationale. |
| Agent uses golden-ratio deviations as informational notes only. | Agent reads failure criteria. | Golden ratio never triggers failure. |
| Agent does not apply fixes outside an approved remediation pass. | Agent reads actions. | Output contains findings and tasks only. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Automation gate | Agent verifies every rule has an automated result. | Zero manual-only assertions remain without a task. |
| Provider gate | Agent verifies every configured provider was tested. | Zero untested configured providers remain. |
| Observability gate | Agent verifies every component field set was read. | Zero component field gaps remain unreported. |
| Ownership gate | Agent verifies every finding has an owner. | Zero ownerless findings remain. |
