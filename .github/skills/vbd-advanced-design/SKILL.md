---
name: vbd-advanced-design
title: Advanced Volatility-Based Decomposition Design
description: Review split, merge, communication, proxy, compatibility, scaling, and ownership decisions after base VBD scope exists.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1168
prerequisites:
  - vbd-system-design
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vbd-boundary-contract-mapping
  - vbd-change-simulation
appliesTo: '**/*.{md,cs,json,yml,yaml}'
tags:
  - vbd
  - design
  - communication-cost
  - ownership
---

# Advanced Volatility-Based Decomposition Design

Agent reviews hard VBD decisions after base manager, engine, and access scopes exist.

## When to Use

Use when the prompt asks whether to split, merge, or keep a logical scope.
Use when the prompt asks for sync, async, or proxy decisions.
Use when scaling, failure isolation, compatibility, or ownership questions dominate the design.

## When Not to Use

Do not use when the prompt is the first pass at manager, engine, and access design. Use `vbd-system-design`.
Do not use when one DTO leak is the only defect. Use `vbd-boundary-contract-mapping`.
Do not use when service count is driven only by hosting or org chart preference.

## Inputs

| Input | Required | Description |
|---|---|---|
| Candidate scopes | Yes | Current logical design |
| Ranked change cases | Yes | Frequency, source, urgency, and blast radius |
| Interaction map | Yes | Caller pairs, payloads, latency, and failure coupling |
| Runtime constraints | No | Compatibility, recovery, locality, and scaling limits |
| Ownership context | No | Team, approval, and support constraints |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Re-rank change cases | Agent ranks the highest-value change cases again before deep design decisions. | Ranked case list | Agent reads the list. | Top cases have source, urgency, and blast radius. |
| 2. Compare split and merge options | Agent writes explicit split and merge tradeoffs per candidate scope. | Option matrix | Agent reads the matrix. | Every option lists cost, benefit, and coupling effect. |
| 3. Review communication | Agent writes sync, async, and proxy fit against latency, failure, and observability needs. | Communication matrix | Agent reads the matrix. | Each interaction has one chosen style with rationale. |
| 4. Review compatibility | Agent writes contract evolution, caller coexistence, and rollout impact. | Compatibility packet | Agent reads the packet. | Caller stability rules exist for every changed contract. |
| 5. Review ownership | Agent writes operational ownership only after logical authority is stable. | Ownership summary | Agent reads the summary. | Team layout does not redefine logical authority. |

## Decision Matrix

| Decision | Use when | Do not use when |
|---|---|---|
| Split scope | Change drivers and authority differ materially | Split adds chatty calls |
| Merge scope | Change drivers move together and one coarse decision is cheaper | Merge couples unrelated policy change |
| Async messaging | Buffering, replay, fan-out, or recovery independence matters | Async hides tight sync coupling |
| Sync call | Caller needs immediate answer and coupling cost is accepted | Availability multiplication is unacceptable |
| Proxy | Volatile integration needs stable translation | Proxy owns business policy |

## Output Contract

| Output | Minimum content |
|---|---|
| Change-case matrix | Ranked cases and blast radius |
| Design option packet | Split, merge, sync, async, and proxy decisions |
| Compatibility plan | Caller stability and rollout notes |
| Ownership summary | Team and support implications |

## Verification

- [ ] Agent compares split and merge options explicitly.
- [ ] Agent keeps logical authority ahead of team or hosting constraints.
- [ ] Agent writes retry, idempotency, timeout, and correlation expectations for chosen interactions.
- [ ] Agent uses proxies only for volatile translation.
- [ ] Agent records compatibility impact before recommending change.

Test: Read the design option packet.
Pass: Every changed interaction has one chosen style, one stated tradeoff, and one compatibility note.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Microservice count drives design | Agent starts from volatility and authority |
| Async hides a poor contract | Agent fixes the contract or merges scopes |
| Deployment topology redefines the model | Agent keeps logical design first |
| Every hypothetical change gets equal weight | Agent ranks credible high-value cases only |
