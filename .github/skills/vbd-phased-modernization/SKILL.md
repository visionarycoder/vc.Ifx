---
name: vbd-phased-modernization
title: Phased VBD Modernization
description: Orchestrate phased modernization from baseline evidence through candidate comparison, selected-plan construction, cutover, and governed artifact checkpoints.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1461
prerequisites:
  - vbd-system-design
  - vbd-boundary-contract-mapping
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - ifx-component-communication
  - dotnet-aspire-vbd-development
  - vbd-artifact-automation
appliesTo: '**/*.{cs,csproj,sln,slnx,json,md,yml,yaml}'
tags:
  - vbd
  - modernization
  - migration
  - architecture
---

# Phased VBD Modernization

Agent orchestrates incremental VBD modernization from evidence capture through production cutover.

## When to Use

Use when modernization spans multiple phases.
Use when existing behavior is incomplete or conflicting.
Use when multiple candidates need comparison before implementation.
Use when contract-first construction and phased cutover need one governed workflow.

## When Not to Use

Do not use when the prompt targets a greenfield system with no legacy evidence.
Do not use when one contract-coupling defect is the only scope.
Do not use when the prompt asks only for a previously selected implementation plan.

## Inputs

| Input | Required | Description |
|---|---|---|
| Solution root | Yes | Default analysis scope |
| Allowed paths | Yes | Read and write scope |
| Constraints | Yes | Compatibility, coverage, naming, deployment, and quality rules |
| Candidate count | No | Default count = 3 |
| UI scope | No | Declares whether UI estimates apply |

## Program Rules

| Rule | Pass |
|---|---|
| Coverage floor | Aggregate coverage is at least 80% |
| Designated coverage | Records, contract data types, and Ifx code stay at 100% |
| Proxy routing | Component calls pass through the approved generic proxy |
| Manager coordination | Manager-to-manager coordination uses the message bus |
| Outcome compatibility | Backward compatibility is measured at `UC-*` outcome level |
| Artifact governance | Governed analysis artifacts fit `docs/analysis/**/*` contract |
| Ratio use | Golden ratio stays a note, not a target |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Baseline | Agent reads builds, tests, coverage, analyzers, packages, benchmarks, target frameworks, and compatibility limits. | Baseline packet | Agent reads the packet. | Pre-existing failures are separated from modernization work. |
| 2. Generate symbolic map | Agent generates symbol evidence with callers, callees, state, side effects, failures, cancellation, tests, coverage, and confidence. | `SYM-*` records | Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`. | Exit code = 0. `SYM-*` records resolve. |
| 3. Generate use cases | Agent routes outcome reconstruction to `vbd-use-case-reconstruction`. | `UC-*` records | Agent reads the use-case set. | Every use case links to evidence. |
| 4. Freeze evidence | Agent writes manifest hashes and identical compact inputs for each candidate. | Evidence package | Agent reads package hashes. | Every candidate receives the same manifest hash. |
| 5. Generate candidates | Agent generates at least three independent candidates with reuse, gaps, contracts, communication, Ifx, Aspire, analyzers, and estimates. | `CAND-*` packets | Agent reads candidate packets. | Candidate packets do not reference each other. |
| 6. Compare candidates | Agent generates a comparison packet and pauses for developer selection. | Decision packet | Agent reads the packet. | Comparison dimensions are consistent. Zero auto-selection occurs. |
| 7. Generate selected plan | Agent routes the selected candidate to contract-first construction. | `TASK-*` and `CP-*` packets | Agent reads the selected plan. | Contract tasks precede dependent tasks. |
| 8. Create simulators | Agent creates simulators for components with complex logic or unavailable dependencies. | Simulator implementations | Run tests against simulators. | Simulators pass the same tests that contract requirements define. |
| 9. Enable parallel development | Agent enables parallel UI and service development through configuration and DI selection. | Configuration and DI setup | Switch between simulator and real implementations. | Both modes run correctly with the same contracts. |
| 10. Run cutover planning | Agent routes selected work to cutover and migration harness skills. | `CUT-*` and `MIG-*` packets | Agent reads cutover packets. | Every migration wave maps to complete `UC-*` outcomes. |
| 11. Track program health | Agent routes scoring and actuals to portfolio tracking. | `PORT-*` packet | Agent reads the packet. | Variance, drift, and token burn are visible. |

## Parallel Development with Simulators

Agent uses simulators to unblock teams when dependencies are not ready.

### Development Flow

1. **Week 1-2:** Agent defines contracts and builds simulators.
   - Contract interfaces are finalized.
   - Simulators implement full business logic with in-memory storage.
   - UI team starts development against simulators.

2. **Week 2-4:** Agent enables parallel work streams.
   - UI team builds screens and wires to simulators.
   - Backend team builds real implementations and database schema.
   - Test team writes tests against simulators and reuses them for real implementations.

3. **Week 4-5:** Agent coordinates integration and cutover.
   - Real implementations replace simulators through configuration and DI selection.
   - The same tests run against real implementations.
   - UI requires no changes because contracts stay stable.

### Benefits

- UI development starts immediately.
- Tests run quickly without infrastructure.
- Business logic is validated early.
- Teams work independently.
- Offline development stays available.

Test: UI application runs against simulator and real implementations.
Pass: No code changes are required to switch modes and observed behavior stays consistent across modes.

## Output Contract

| Output | Minimum content |
|---|---|
| Baseline packet | Current quality and compatibility state |
| Evidence packet | Manifest plus `SYM-*` and `UC-*` references |
| Candidate set | `CAND-*`, `CMP-*`, `GAP-*`, `EST-*`, and comparison packet |
| Selected plan | `TASK-*` and `CP-*` packets |
| Migration packet | `CUT-*`, `MIG-*`, and drift evidence |
| Portfolio packet | `PORT-*` record and actuals trend |

## Verification

- [ ] Agent keeps evidence frozen before candidate comparison.
- [ ] Agent generates at least three independent candidates by default.
- [ ] Agent keeps client estimate as item 3.8 in candidate packets.
- [ ] Agent pauses for developer selection before construction planning.
- [ ] Agent keeps contract-first sequencing, migration waves, and portfolio tracking linked by stable IDs.

Test: Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`.
Pass: Exit code = 0. Required artifact kinds resolve. Comparison packets share identical evidence hashes.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agents infer undocumented behavior | Agent records uncertainty and conflict |
| Candidate packets share intermediate output | Agent freezes evidence and isolates candidate input |
| Infrastructure work is omitted | Agent writes Ifx, Aspire, analyzer, and migration work |
| Aggregate coverage hides weak critical scopes | Agent tracks aggregate and designated coverage separately |
