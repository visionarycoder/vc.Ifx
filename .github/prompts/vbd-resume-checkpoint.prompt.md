---
mode: agent
title: Resume VBD Checkpoint
description: Resume a phased VBD effort from verified manifests and checkpoints without repeated discovery or stale evidence reuse.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 700
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
# Resume VBD Checkpoint

Agent resumes the current VBD phase from durable state.

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads the latest phase checkpoint, evidence manifest, task status, and unresolved decisions. | Agent lists source records. | Every required source record exists. |
| 2 | Agent verifies source, evidence, candidate, and selected-plan hashes. | Agent compares recorded hashes to current hashes. | Every required hash matches or is invalidated. |
| 3 | Agent detects changed files and invalidates affected symbolic records, use cases, candidates, estimates, or tasks only. | Agent reads invalidation list. | Invalidation scope matches changed files. |
| 4 | Agent verifies completed outputs still exist and passed recorded gates. | Agent reads output records and gate evidence. | Every reused output still exists and has evidence. |
| 5 | Agent restores the smallest valid working set and token budget. | Agent reads restored task set. | Restored scope excludes invalidated work. |
| 6 | Agent continues the next ready task. | Agent reads dependency status. | Task has no open dependency. |
| 7 | Agent writes a new checkpoint at the next stable gate. | Agent reads new checkpoint fields. | New checkpoint is complete. |

## Checkpoint Record

| Field | Test | Pass |
|---|---|---|
| Phase and task ID | Agent reads checkpoint field. | Field exists. |
| Source and evidence hashes | Agent reads checkpoint field. | Field exists. |
| Selected candidate hash | Agent reads checkpoint field when applicable. | Field exists or `N/A`. |
| Completed outputs and gate evidence | Agent reads checkpoint field. | Field exists. |
| Invalidated outputs | Agent reads checkpoint field. | Field exists or `none`. |
| Unresolved decisions and risks | Agent reads checkpoint field. | Field exists or `none`. |
| Next ready tasks and dependencies | Agent reads checkpoint field. | Field exists. |
| Consumed and remaining token budget | Agent reads checkpoint field. | Field exists. |

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent does not trust status without persistent output evidence. | Agent reads reused output rows. | Every reused output has evidence. |
| Agent does not rerun full-solution discovery when fingerprints show no change. | Agent reads invalidation scope. | Unchanged scope stays reused. |
| Agent does not reuse candidate or plan outputs after evidence becomes stale. | Agent reads stale-output rows. | Stale outputs are invalidated. |
| Agent does not reinterpret a developer selection silently. | Agent reads decision records. | Selection changes have explicit records. |

## Completion Gates

| Gate | Test | Pass |
|---|---|---|
| Hash gate | Agent verifies all reusable hashes are current. | Zero stale hashes remain unflagged. |
| Reuse gate | Agent verifies reused outputs still exist and passed gates. | Zero unsupported reused outputs remain. |
| Scope gate | Agent verifies restoration scope is minimal. | Zero invalidated tasks remain active. |
| Checkpoint gate | Agent verifies the new checkpoint is complete. | Zero checkpoint fields remain blank. |
