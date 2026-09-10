---
mode: agent
title: VBD Candidate Red Team
description: Red-team one VBD candidate. Record hidden coupling, reuse risk, missing failures, estimate gaps, weak evidence, and migration risk.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 720
invokes_skills:
  - vbd-change-simulation
  - vbd-candidate-estimation
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - vbd-dictionary
related_skills:
  - vbd-evidence-quality
appliesTo: '**/*'
tags:
  - prompts
  - prompt
  - ste
  - vbd
---
# VBD Candidate Red Team

Agent red-teams one candidate from one frozen evidence package.

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies the candidate hash and evidence-manifest hash. | Agent compares recorded hashes to input hashes. | Every hash matches. |
| 2 | Agent verifies evidence quality for every claim that shapes the design. | Agent runs `vbd-evidence-quality` on cited claims. | Every material claim has a quality result. |
| 3 | Agent runs `vbd-change-simulation` with the shared scenario catalog. | Agent records the scenario catalog hash. | Every scenario uses the shared catalog. |
| 4 | Agent verifies coupling, communication cost, and volatility analysis gaps. | Agent reads component calls, state ownership, and boundary rationale. | Agent records every defect with cited evidence. |
| 5 | Agent verifies failure handling, security, cancellation, retry, idempotency, and recovery behavior. | Agent reads contracts, tests, and operations notes. | Agent records every missing behavior. |
| 6 | Agent verifies estimate scope for Contract, Service, Access, Ifx, Aspire, tests, benchmarks, Client, UI, analyzers, generators, migration, and review work. | Agent compares estimate lines to candidate content. | Agent records every omitted work item. |
| 7 | Agent classifies findings as blocker, estimate correction, risk, or optional improvement. | Agent reads every finding row. | Every finding has one class. |
| 8 | Agent writes a traceable red-team report in the assigned candidate folder. | Agent verifies report links to evidence, scenario, and candidate sections. | Every finding is traceable. |

## Review Scope

| Topic | Agent Focus | Test | Pass |
|---|---|---|---|
| Use-case fit | Agent verifies supported and unsupported use-case outcomes. | Agent compares candidate outcomes to `UC-*` records. | Every unsupported outcome is explicit. |
| Coupling | Agent verifies sibling, transitive, and availability coupling. | Agent reads call graph and dependency graph. | Every hidden coupling path is listed. |
| Reuse | Agent verifies reuse claims against `SYM-*` records. | Agent compares reuse claims to symbol evidence. | Every reuse claim is observed or flagged. |
| Ownership | Agent verifies state, transaction, and authority ownership. | Agent reads boundary catalog and write paths. | Every shared ownership defect is listed. |
| Infrastructure | Agent verifies proxy, interceptor, bus provider, and configuration behavior. | Agent reads infrastructure design and startup rules. | Every undefined behavior is listed. |
| Migration | Agent verifies coexistence, data migration, rollback, and decommission assumptions. | Agent reads migration notes and change simulation results. | Every unsafe assumption is listed. |

## Output Contract

| Output | Agent Content | Test | Pass |
|---|---|---|---|
| Red-team report | Agent writes an overview, finding table, and correction list. | Agent reads the report sections. | Every section exists. |
| Finding table | Agent records severity, class, evidence, owner, and next task. | Agent reads finding rows. | Every finding row is complete. |
| Estimate delta | Agent records estimate additions or range shifts. | Agent compares corrected estimate lines to candidate lines. | Every estimate defect has a correction. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent does not redesign the candidate. | Agent reads the report for replacement architecture content. | Report contains no replacement design. |
| Agent does not compare sibling candidates. | Agent reads report references. | Report references one candidate only. |
| Agent does not manufacture evidence. | Agent reads citations. | Every claim has a source or a risk label. |
| Agent uses one shared quality threshold. | Agent compares thresholds across sections. | Thresholds are consistent. |
| Agent uses the Manager/Engine golden ratio as a sanity note only. | Agent reads scoring language. | Report uses no golden-ratio gate. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Evidence gate | Agent verifies every material claim has cited evidence. | Zero uncited material claims remain. |
| Classification gate | Agent verifies every finding has one class and one owner. | Zero unclassified findings remain. |
| Traceability gate | Agent verifies every correction links to candidate content. | Zero orphan corrections remain. |
