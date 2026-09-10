---
name: vbd-change-simulation
title: VBD Change Simulation
description: Simulate credible policy, workflow, integration, failure, scale, and rollout changes against a VBD candidate and record `CSIM-*` blast-radius evidence.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1130
prerequisites:
  - vbd-candidate-estimation
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-boundary-contract-mapping
  - vbd-modernization-portfolio
appliesTo: '**/*.{cs,csproj,sln,slnx,json,md,yml,yaml}'
tags:
  - vbd
  - simulation
  - blast-radius
  - risk
---

# VBD Change Simulation

Agent stress-tests a candidate with credible change scenarios before construction or cutover.

## When to Use

Use when candidate comparison needs objective containment evidence.
Use when a selected design needs risk review before implementation.
Use when change containment, failure isolation, or hosting neutrality is unclear.
Use when a ratio warning needs evidence instead of intuition.

## When Not to Use

Do not use when evidence and use cases are not frozen.
Do not use when the prompt asks for runtime performance only. Use benchmarks.
Do not use when a scenario has no source or stated assumption.

## Inputs

| Input | Required | Description |
|---|---|---|
| Evidence manifest | Yes | Frozen source and use-case evidence |
| Candidate design | Yes | Components, contracts, calls, state, and topology |
| Change scenarios | Yes | Observed, planned, or hypothetical changes |
| Estimate model | No | `EST-*` rows for impact deltas |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Generate scenario set | Agent generates scenario rows for policy, workflow, integration, security, failure, scale, hosting, contract evolution, and rollback. | Scenario catalog | Agent reads the catalog. | Catalog covers all required scenario classes. |
| 2. Trace impact | Agent traces each scenario through `UC-*`, `SYM-*`, `GAP-*`, components, contracts, callers, state, tests, and operations. | Impact traces | Agent reads trace rows. | Every affected artifact has an explicit ID link. |
| 3. Score containment | Agent records affected use cases, components, callers, contracts, migrations, failure domains, and estimate deltas. | Containment matrix | Agent reads the matrix. | Zero unexplained composite score exists. |
| 4. Compare alternatives | Agent compares merge, split, proxy, message, and contract-shape alternatives when coupling is high. | Alternative matrix | Agent reads the matrix. | Every recommendation states one cheaper or safer path. |
| 5. Freeze findings | Agent writes `CSIM-*` records and estimate deltas. | Simulation packet | Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`. | Exit code = 0. Scenario links and estimate links resolve. |

## Scenario Classes

| Scenario class | Required |
|---|---|
| Business policy change | Yes |
| Workflow or use-case change | Yes |
| External resource or persistence change | Yes |
| Security or authorization change | Yes |
| Dependency failure or timeout | Yes |
| Scale or concurrency change | Yes |
| Hosting or deployment change | Yes |
| Contract evolution or rollback | Yes |

## Output Contract

| Output | Minimum content |
|---|---|
| Scenario catalog | Scenario source and assumption label |
| `CSIM-*` records | Per-scenario blast radius and confidence |
| Containment matrix | Counts and rationale |
| Estimate delta packet | Changed `EST-*` rows |
| Recommendation log | Safer alternative when coupling is high |

## Verification

- [ ] Agent covers every required scenario class.
- [ ] Agent links every impact to stable IDs.
- [ ] Agent records caller and contract churn explicitly.
- [ ] Agent records data, test, operational, and rollback cost.
- [ ] Agent labels hypothetical scenarios clearly.

Test: Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]`.
Pass: Exit code = 0. Zero `CSIM-*` records miss source label, impact links, or confidence.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Happy-path changes dominate the catalog | Agent adds failure and rollback scenarios |
| Files changed becomes the only metric | Agent records contracts, callers, data, tests, and operations |
| Hosting topology defines ownership | Agent separates logical and physical impact |
| Scenario set favors one candidate | Agent reuses the same catalog for all candidates |
