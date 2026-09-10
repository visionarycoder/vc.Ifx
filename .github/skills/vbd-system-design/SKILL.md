---
name: vbd-system-design
title: VBD System Design
description: Design and review volatility-based decompositions using change evidence, owned authority, stable contracts, and repository manager-engine-access conventions.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1321
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-advanced-design
  - vbd-symbolic-mapping
  - vbd-boundary-contract-mapping
appliesTo: '**/*.{md,cs,json,yml,yaml}'
tags:
  - vbd
  - design
  - manager-engine-access
  - contracts
---
# VBD System Design

Agent designs or reviews VBD scope from change evidence, owned authority, and stable contracts.

## When to Use

Use when the prompt asks for a new VBD decomposition.
Use when the prompt asks whether manager, engine, and access scopes reflect real change drivers.
Use when the prompt reports unstable contracts, cross-layer churn, or caller coupling.
Use when the prompt needs a logical design before packaging or hosting decisions.

## When Not to Use

Do not use when one DTO leak is the only defect. Use `vbd-boundary-contract-mapping`.
Do not use when the prompt is only about hosting, deployment, or process count.
Do not use when the prompt asks for template-driven layer generation.

## Inputs

| Input | Required | Description |
|---|---|---|
| Business activities | Yes | User-visible or system-visible outcomes with triggers and results |
| Change cases | Yes | Credible policy, workflow, integration, Schedule, or compliance changes |
| Current dependencies | Yes | Calls, state ownership, coupling, and current pain |
| Runtime constraints | No | Latency, reliability, throughput, and consistency limits |
| Repository context | No | Existing manager, engine, access projects and analyzer output |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Baseline activities | Agent groups activities by trigger, outcome, and change driver. | Activity map | Agent reads the map. | Every activity has one trigger, one outcome, and at least one change driver. |
| 2. Generate scope options | Agent groups logic into manager, engine, and access scopes. | Scope option table | Agent reads the option table. | Every scope owns one clear reason to change. |
| 3. Assign authority | Agent assigns orchestration to managers, policy to engines, and resource work to access scopes. | Authority matrix | Agent reads the authority matrix. | Zero scope owns mixed authority without written rationale. |
| 4. Define contracts | Agent generates local contract ownership and mapper points for each caller transition. | Contract ownership table | Agent reads the contract table. | Every caller depends on the immediate contract only. |
| 5. Review communication | Agent reviews call count, payload ownership, and failure coupling. | Communication matrix | Agent reads the matrix. | Zero cyclic flow exists. Zero fine-grained chatty path lacks rationale. |
| 6. Map physical packaging | Agent maps the logical design to contract, service, and optional ORM projects after the logical design is stable. | Packaging summary | Agent reads the summary. | Physical packaging does not change logical authority. |

## Decision Matrix

| Decision | Use when | Do not use when |
|---|---|---|
| Split manager scope | One activity has independent triggers, outcomes, or orchestration change | Two activities always change together |
| Split engine scope | Shared policy changes independently from orchestration | Call flow becomes chatty |
| Split access scope | Resource technology or lifecycle changes independently | Split exists only for naming symmetry |
| Add proxy | A volatile integration needs a stable contract | Proxy takes business authority |
| Duplicate DTOs | Caller shape needs independent evolution without downstream churn | One shared type already has single-owner scope |

## Simulator Decision Matrix

Agent decides whether design work benefits from simulator-first delivery, real-first delivery, or parallel implementation.

| Scenario | Approach | Rationale |
|---|---|---|
| Complex business logic plus unavailable dependencies | Simulator first, real later | Simulator proves logic before infrastructure coupling |
| Simple CRUD plus ready database | Real implementation only | Simulator adds no material value |
| UI development starts before backend completion | Simulator and real implementation in parallel | Teams move independently behind one contract |
| Fast iteration is required | Simulator first | Startup stays instant and tests stay deterministic |
| Offline demos or training are required | Simulator kept long-term | Environment needs no infrastructure |
| Integration testing is the main concern | Real implementation only | Work targets actual integration points |

Test: Design artifacts state the simulator decision and rationale.
Pass: The simulator decision aligns with dependency readiness, workflow needs, and contract stability.

## Output Contract

| Output | Minimum content |
|---|---|
| Design packet | Activity map, scope table, authority matrix, communication matrix |
| Contract plan | Local DTO ownership, mapper points, caller dependencies |
| Rationale log | Split and merge decisions tied to change cases |
| Artifact links | `SYM-*`, `UC-*`, `GAP-*`, and candidate references when available |

## Verification

- [ ] Agent ties every scope to explicit change cases.
- [ ] Agent keeps orchestration in managers, shared policy in engines, and resource work in access scopes.
- [ ] Agent gives each caller one immediate contract dependency.
- [ ] Agent keeps physical packaging after logical design.
- [ ] Agent records `SYM-*` and `UC-*` links when analysis artifacts exist.

Test: Run `npm run vbd:artifacts -- validate <analysis-root> [--source-root <source-root>]` when the design writes governed artifacts.
Pass: Exit code = 0. Zero unresolved required references.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| One deployable unit becomes one logical scope | Agent separates logical design from hosting |
| Engine split follows nouns, not change | Agent splits only on independent policy change |
| Caller uses downstream DTOs directly | Agent generates local DTO ownership and mapping |
| Ratio becomes a design target | Agent records ratio as a note only |
