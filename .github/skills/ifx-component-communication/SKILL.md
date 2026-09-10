---
name: ifx-component-communication
description: Designs VBD component communication with generic proxies, ordered interceptors, logical addressing, and manager bus patterns.
title: Ifx Component Communication
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1220
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - vbd-system-design
related_skills:
  - dotnet-aspire-vbd-development
  - vbd-boundary-contract-mapping
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - vbd
  - components
  - communication
  - ste
---
# Ifx Component Communication

This skill designs VBD component communication with explicit boundaries and traceable message flow.
This skill keeps contracts typed, interception ordered, and addressing logical.

## Communication Surface Map

| Concern | Preferred surface | Agent rule |
|---|---|---|
| Cross-component call shape | Generic proxy | Agent defines one proxy contract per boundary. |
| Cross-cutting behavior | Ordered interceptors | Agent assigns a deterministic interceptor order. |
| Routing | Logical addressing | Agent keeps addresses stable and environment-independent. |
| Ambient data | Typed context | Agent avoids stringly typed bags for durable context. |
| Manager coordination | Manager message bus | Agent routes broadcast or fan-out behavior through the manager bus. |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Define | Agent identifies the boundary, contract, and delivery semantics. | Communication contract |
| 2. Select | Agent chooses proxy, direct call, or bus flow based on coupling and fan-out needs. | Channel decision |
| 3. Order | Agent assigns interceptor order for auth, telemetry, retries, and validation. | Ordered pipeline |
| 4. Contextualize | Agent defines typed context data and logical addressing. | Stable envelope |
| 5. Validate | Agent verifies request flow, error flow, and observability hooks. | Verified communication design |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Contract clarity | Review public contracts and payload types. | Contracts are typed and boundary-specific. |
| Interceptor determinism | Review interceptor registration order. | Cross-cutting behaviors run in a stable documented order. |
| Address stability | Review route or topic identifiers. | Identifiers do not encode environment-specific physical details. |
| Failure path | Review or run targeted tests. | Errors propagate with correlation data through the chosen path. |
