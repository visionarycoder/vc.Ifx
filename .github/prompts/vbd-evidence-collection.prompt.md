---
mode: agent
title: VBD Evidence Collection
description: Build or refresh baseline evidence, symbolic mapping, evidence manifest, and outcome-focused use cases for one selected solution.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 820
invokes_skills:
  - vbd-symbolic-mapping
  - vbd-use-case-reconstruction
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
# VBD Evidence Collection

Agent applies `vbd-symbolic-mapping` and `vbd-use-case-reconstruction` to one selected solution.

## Inputs

| Input | Test | Pass |
|---|---|---|
| Selected solution root | Agent reads input path. | Path exists. |
| Allowed and forbidden paths | Agent reads scope filters. | Filters are explicit. |
| Prior evidence manifest | Agent reads manifest input when present. | Input is present or `none`. |
| Known requirements and operations evidence | Agent reads supplied records. | Inputs are recorded. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent analyzes the complete selected solution inside the allowed scope. | Agent reads analyzed path list. | Every allowed path is analyzed. |
| 2 | Agent captures build, test, coverage, analyzer, package, benchmark, framework, and deployment baselines. | Agent reads baseline records. | Every baseline category has one record. |
| 3 | Agent inventories source and generated-code boundaries. | Agent reads boundary inventory. | Every boundary has one classification. |
| 4 | Agent incrementally maps symbols, calls, resources, effects, failures, cancellation, tests, coverage, change drivers, and confidence. | Agent reads `SYM-*` records. | Every mapped row has evidence and confidence. |
| 5 | Agent reconstructs one document per observable use-case outcome. | Agent reads `UC-*` records. | Every primary outcome has one use-case record. |
| 6 | Agent records code and use-case conflicts plus unsupported requirements. | Agent reads conflict ledger. | Every conflict has evidence. |
| 7 | Agent writes stable `SYM-*` and `UC-*` traceability under the analysis tree. | Agent reads output paths. | Every record uses the expected prefix and path. |
| 8 | Agent hashes and freezes the evidence package. | Agent reads manifest hash. | Manifest hash exists. |

## Evidence Rules

| Rule | Test | Pass |
|---|---|---|
| Agent treats code as authoritative only when code aligns with required use-case outcomes. | Agent reads conflict notes. | Misaligned code is flagged. |
| Agent preserves compatibility at the use-case outcome level, not internal component boundaries. | Agent reads compatibility notes. | Compatibility notes target outcomes only. |
| Agent marks evidence as observed, inferred, declared, or unknown. | Agent reads evidence labels. | Every evidence row has one label. |
| Agent keeps UI and Schedule analysis conditional. | Agent reads optional scope notes. | Optional scope is explicit. |
| Agent does not propose candidate components during evidence collection. | Agent reads outputs. | Outputs contain no candidate design. |

## Output Contract

| Output | Agent Content | Test | Pass |
|---|---|---|---|
| Baseline package | Agent writes baseline records under `docs/analysis/baseline/`. | Agent reads output paths. | Path set exists. |
| Symbolic mapping | Agent writes `SYM-*` records under `docs/analysis/symbolic-mapping/`. | Agent reads record IDs. | Every ID uses `SYM-*`. |
| Use-case package | Agent writes `UC-*` records under `docs/analysis/use-cases/`. | Agent reads record IDs. | Every ID uses `UC-*`. |
| Evidence manifest | Agent writes source revision, inputs, tools, outputs, and hashes. | Agent reads manifest fields. | Every field exists. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Entry-point gate | Agent verifies entry points are mapped or unresolved explicitly. | Zero hidden entry-point gaps remain. |
| Traceability gate | Agent verifies every use-case flow links to evidence or a gap. | Zero unlinked use-case steps remain. |
| Conflict gate | Agent verifies conflicts and confidence are visible. | Zero unlabeled conflicts remain. |
| Freeze gate | Agent verifies the evidence package has a manifest hash. | Zero unfrozen packages remain. |
