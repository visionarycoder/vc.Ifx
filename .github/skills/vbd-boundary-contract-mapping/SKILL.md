---
name: vbd-boundary-contract-mapping
title: VBD Boundary Contract Mapping
description: Fix volatility-driven contract coupling by restoring local DTO ownership, local mapper code, and immediate-layer dependencies.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1219
prerequisites:
  - vbd-system-design
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-change-simulation
  - vbd-contract-first-construction
  - vbd-operational-contracts
appliesTo: '**/*.cs'
tags:
  - vbd
  - contracts
  - scopes
  - remediation
---

# VBD Boundary Contract Mapping

Agent fixes VolatilityAnalyzer findings by restoring local contract ownership per layer scope.

## When to Use

Use when VolatilityAnalyzer reports caller code using DTOs from a lower layer.
Use when client code references engine or access contract namespaces directly.
Use when a call chain needs local DTO ownership at each client, manager, engine, and access transition.

## When Not to Use

Do not use when the prompt asks for broad redesign without a concrete coupling finding.
Do not use when no DTO crosses a layer scope.
Do not use when the prompt asks for packaging guidance only. Use `vbd-advanced-design`.

## Inputs

| Input | Required | Description |
|---|---|---|
| Scope mode | Yes | `single-scope` or `solution-wide` |
| Target finding | Yes when `single-scope` | File path, symbol, and reported finding |
| Dry run | Yes | `true` generates plan only. `false` writes approved files |
| Allowed paths | Yes | Explicit write allow-list |
| Forbidden paths | Yes | Explicit deny-list |
| Approval mode | Yes | `per-finding` or `bulk-approved` |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Verify input scope | Agent verifies scope mode, dry run, allow-list, deny-list, and approval mode. | Input summary | Agent reads the summary. | Zero missing required inputs remain. |
| 2. Read findings | Agent reads VolatilityAnalyzer output and groups related findings by call chain. | Finding groups | Agent reads grouped findings. | Every group names caller scope, callee scope, and affected DTOs. |
| 3. Generate write plan | Agent generates exact file writes for DTO copies, mapper writes, using changes, and project dependency changes. | Dry-run plan | Agent reads the plan. | Every planned write stays inside `allowed_paths` and outside `forbidden_paths`. |
| 4. Write local DTOs | Agent writes one local DTO per layer that owns the payload shape. | DTO files | Agent reads target files. | Every caller uses a local DTO. |
| 5. Write mapper code | Agent writes mapper code in client mapper locations or service projects only. | Mapper files | Agent reads mapper files. | Zero mapper code exists in contract projects. |
| 6. Write caller updates | Agent writes namespace, type, and dependency updates for caller code. | Caller diffs | Agent reads caller diffs. | Callers depend on immediate contracts only. |
| 7. Verify finding removal | Agent runs build and analyzer verification after each approved finding group. | Verification log | Run `dotnet build Wa.Wsdot.Fin.Idl.slnx`. | Exit code = 0. Selected finding group no longer appears. |

## Placement Rules

| Layer | DTO location | Mapper location |
|---|---|---|
| Client without Contract project | Client-local DTO or models path | Client mapper path |
| Manager | Local Contract project | Local Service project |
| Engine | Local Contract project | Local Service project |
| Access | Local Contract project when upstream caller needs local ownership | Local Service project |

## Output Contract

| Output | Minimum content |
|---|---|
| Dry-run plan | Finding ID, file list, symbol list, write order |
| Remediation packet | Updated DTOs, mapper code, dependency changes |
| Traceability | Links to `SYM-*`, `GAP-*`, and `CMP-*` when analysis artifacts exist |

## Verification

- [ ] Agent keeps all writes inside `allowed_paths`.
- [ ] Agent writes local DTO ownership for each affected layer scope.
- [ ] Agent keeps mapper code out of contract projects.
- [ ] Agent removes direct client-to-engine and client-to-access DTO use.
- [ ] Agent runs build and analyzer verification after each approved finding group.

Test: Run `dotnet build Wa.Wsdot.Fin.Idl.slnx`.
Pass: Exit code = 0. Zero selected contract-coupling findings remain.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| One scope in a four-scope chain gets fixed alone | Agent writes the full affected call chain in one approved group |
| Mapper code lands in contract projects | Agent writes mapper code in service or client mapper locations only |
| Namespace stays on the source DTO | Agent writes namespace to the local owner scope |
| Solution-wide mode writes unrelated files | Agent groups by finding and writes one approved group at a time |
