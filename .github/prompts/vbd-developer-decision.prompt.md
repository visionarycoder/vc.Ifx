---
mode: ask
description: Present normalized VBD candidate tradeoffs, collect developer selection or explicit hybridization, and record a durable architecture decision.
title: VBD Developer Decision
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 720
invokes_skills:
  - vbd-candidate-estimation
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
# VBD Developer Decision

Agent uses the synthesized decision package after red-team work is complete.

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies every candidate uses the same evidence-manifest hash and scenario-catalog hash. | Agent compares candidate metadata. | Every candidate uses one shared hash set. |
| 2 | Agent presents every candidate with one consistent schema. | Agent reads presentation rows. | Every candidate row contains the same fields. |
| 3 | Agent explains blocking findings and material assumptions. | Agent reads blocker and assumption sections. | Every candidate has explicit blocker and assumption notes. |
| 4 | Agent asks the developer to select one candidate, ask one clarifying prompt, or define one hybrid. | Agent reads the developer-choice section. | Developer choices are explicit. |
| 5 | Agent reruns dependency, estimate, change-simulation, and red-team work for every borrowed hybrid decision. | Agent reads hybrid follow-up tasks. | Every borrowed decision has a follow-up task. |
| 6 | Agent writes the architecture decision record. | Agent reads the record fields. | Every required field exists. |

## Candidate Presentation Schema

| Field | Agent Content | Test | Pass |
|---|---|---|---|
| Use-case fit | Supported and unsupported outcomes | Agent reads field content. | Field exists for every candidate. |
| Volatility analysis | Change containment and invariants | Agent reads field content. | Field exists for every candidate. |
| Communication | Communication cost and operations complexity | Agent reads field content. | Field exists for every candidate. |
| Reuse and gaps | `SYM-*` reuse and gap notes | Agent reads field content. | Field exists for every candidate. |
| Delivery scope | Ifx, Aspire, proxy, bus, tests, coverage, benchmarks, Client, UI | Agent reads field content. | Field exists for every candidate. |
| Migration | Routing, rollback, and coexistence notes | Agent reads field content. | Field exists for every candidate. |
| Estimates | Person-hours, duration, tokens, critical path, risks, confidence | Agent reads field content. | Field exists for every candidate. |

## Decision Record Fields

| Field | Test | Pass |
|---|---|---|
| Decision ID | Agent reads record field. | Field exists. |
| Timestamp | Agent reads record field. | Field exists. |
| Evidence hash | Agent reads record field. | Field matches presented candidates. |
| Selected candidate | Agent reads record field. | Field exists. |
| Approved modifications | Agent reads record field. | Field exists or `none`. |
| Rationale | Agent reads record field. | Field exists. |
| Rejected alternatives | Agent reads record field. | Field exists. |
| Consequences | Agent reads record field. | Field exists. |
| Unresolved items | Agent reads record field. | Field exists or `none`. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent does not select automatically. | Agent reads final decision status. | Decision status depends on developer input. |
| Agent does not hide uncertainty behind aggregate scores. | Agent reads score notes. | Every score has explicit uncertainty notes. |
| Agent does not treat silence as approval. | Agent reads approval path. | Approval path requires developer input. |
| Agent does not start construction before the decision record is complete. | Agent reads next-task section. | Construction starts after record completion only. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Hash gate | Agent verifies all presented candidates share one evidence set. | Zero unflagged hash mismatches remain. |
| Presentation gate | Agent verifies schema consistency. | Zero missing presentation fields remain. |
| Decision gate | Agent verifies record completeness. | Zero required decision fields remain blank. |
