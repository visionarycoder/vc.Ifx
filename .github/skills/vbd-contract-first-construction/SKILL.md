---
name: vbd-contract-first-construction
title: VBD Contract-First Construction
description: Convert a selected candidate into a dependency graph with contract-first sequencing, parallel component work, traceability, gates, and resumable checkpoints.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1147
prerequisites:
  - vbd-boundary-contract-mapping
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-cutover-migration
  - vbd-modernization-portfolio
appliesTo: '**/*.{cs,csproj,sln,slnx,json,md,yml,yaml}'
tags:
  - vbd
  - construction
  - contract-first
  - checkpoints
---

# VBD Contract-First Construction

Agent converts a selected candidate into an executable construction graph without reopening candidate selection.

## When to Use

Use when the developer selected a candidate or approved a hybrid candidate.
Use when contracts and component responsibilities are stable.
Use when work needs explicit sequencing, gates, and checkpoints.

## When Not to Use

Do not use when candidate comparison is still open.
Do not use when evidence or use cases changed after selection.
Do not use when the prompt asks for design comparison only.

## Inputs

| Input | Required | Description |
|---|---|---|
| Selected candidate | Yes | Approved design and approved hybrid notes |
| Frozen evidence | Yes | Manifest, `UC-*`, and symbolic map |
| Estimates | Yes | `EST-*` records, dependencies, and assumptions |
| Quality policy | Yes | Coverage, analyzer, benchmark, and doc gates |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Verify selection integrity | Agent verifies candidate ID, manifest hash, approved modifications, and unresolved decisions. | Selection packet | Agent reads the packet. | Candidate ID and manifest hash match the decision record. |
| 2. Generate shared foundations | Agent generates Ifx, proxy, messaging, config, Aspire, template, analyzer, and test-helper prerequisites. | Foundation packet | Agent reads the packet. | Shared prerequisites exist before dependent component work starts. |
| 3. Generate component DAGs | Agent generates nodes for contract, tests, service, optional data code, integration tests, benchmarks, client, and UI work. | Task DAG | Agent reads DAG edges. | Every dependent node points to required contract nodes. |
| 4. Link traceability | Agent links `UC-*`, `SYM-*`, `GAP-*`, `CMP-*`, `TASK-*`, tests, and benchmarks. | Traceability matrix | Agent reads the matrix. | Every `TASK-*` row has source evidence and output links. |
| 5. Write gates | Agent writes build, analyzer, test, coverage, proxy, bus, doc, and artifact gates. | Gate list | Agent reads the list. | Every gate has one test and one pass rule. |
| 6. Write checkpoints | Agent writes resumable `CP-*` checkpoints after stable gates. | Checkpoint plan | Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`. | Exit code = 0. Checkpoint and task links resolve. |

## Standard Component Node Order

| Order | Node |
|---|---|
| 1 | Contract |
| 2 | Contract tests |
| 2a | Simulator (optional, recommended for complex components) |
| 3 | Service |
| 4 | Service unit tests |
| 5 | Optional ORM, simulator, emulator |
| 6 | Integration tests |
| 7 | Benchmarks and run docs |
| 8 | Client |
| 9 | Conditional UI work |

## Simulator Integration

Agent creates production-quality simulators when component dependencies are not ready or when parallel development is needed.

### When to Build Simulator

| Condition | Build Simulator |
|---|---|
| Database schema is not finalized | Yes - simulator uses in-memory collections |
| External service is unavailable during development | Yes - simulator implements service contract |
| UI team needs to start before backend is ready | Yes - simulator enables parallel work |
| Fast feedback loop is critical | Yes - simulator starts instantly |
| Offline development or demos are required | Yes - simulator needs no infrastructure |
| Component logic is complex and testable independently | Yes - simulator proves business logic works |

### When to Skip Simulator

| Condition | Skip Simulator |
|---|---|
| Component is trivial CRUD with no business logic | Skip - effort exceeds value |
| Real implementation is immediately available | Skip - build real implementation directly |
| Component is pure data pass-through | Skip - no logic exists to simulate |

### Simulator Node Placement

Agent creates simulator after contract and contract tests at node `2a` so parallel development starts early and contract drift stays visible.

- UI team wires against simulator while service implementation is in progress.
- Tests run against simulator for fast feedback.
- Service implementation uses simulator as a business-logic reference.

Test: Solution contains `*Simulator.cs` files that implement contract interfaces.
Pass: Simulators use in-memory storage, enforce business rules, and pass the same tests as the real implementation contract.

## Output Contract

| Output | Minimum content |
|---|---|
| Selected-plan manifest | Candidate ID, manifest hash, approved hybrid notes |
| Task DAG | `TASK-*` records and dependencies |
| Gate packet | Test and pass rules per gate |
| Checkpoint packet | `CP-*` records and resume notes |
| Traceability matrix | `UC -> SYM/GAP -> CMP -> TASK -> test -> benchmark` |

## Verification

- [ ] Agent keeps contract nodes before dependent work.
- [ ] Agent links every task to evidence, output, and gate rules.
- [ ] Agent keeps shared foundations before proxy-dependent components.
- [ ] Agent writes resumable checkpoints after stable gates.
- [ ] Agent keeps selection integrity fixed during construction planning.

Test: Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`.
Pass: Exit code = 0. Zero unresolved task, checkpoint, or traceability links.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| All projects start together | Agent stabilizes contracts first |
| Tasks follow file layout only | Agent groups by component outcome and dependency |
| Candidate design changes silently | Agent requires explicit approved hybrid notes |
| Tests wait until the end | Agent writes gates per component increment |
