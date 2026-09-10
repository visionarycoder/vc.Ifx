---
name: subagent-orchestration
description: Designs multi-agent workflows with explicit delegation boundaries, coordination rules, and result aggregation.
title: Subagent Orchestration
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1400
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - run-tests
  - refactor
appliesTo: '**/*'
tags:
  - subagent
  - orchestration
  - parallelism
  - ste
---
# Subagent Orchestration

This skill designs multi-agent workflows with explicit independence checks and aggregation rules.
This skill treats delegation as a costed tool, not as a default action.

## Delegation Decision Matrix

| Situation | Agent action |
|---|---|
| One linear task in a small scope | Agent works directly. |
| Multiple independent research threads | Agent delegates to separate explore or research agents. |
| Long-running validation plus independent editing work | Agent starts a background task agent and continues its own independent work immediately. |
| Sequential dependency where each step needs the prior result | Agent keeps the work in one context instead of spawning agents. |
| Small discovery-edit-verify loop | Agent works directly in the main context. |

## Agent Selection Table

| Agent type | Best fit |
|---|---|
| `explore` | Independent codebase research and symbol hunting |
| `task` | Build, test, lint, or other command execution where pass or fail is the main output |
| `general-purpose` | Complex autonomous work with real multi-step reasoning needs |
| `code-review` | Read-only diff review for high-confidence bugs or security issues |
| `research` | Verified external or GitHub-backed research |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Decompose | Agent verifies that subtasks are truly independent. | Task graph |
| 2. Contextualize | Agent writes complete prompts with scope, constraints, and success criteria. | Ready subtask prompts |
| 3. Launch | Agent chooses sync mode by default and background mode only when parallel work exists immediately. | Running agents |
| 4. Continue | Agent performs its own independent work while background agents run. | Parallel progress |
| 5. Aggregate | Agent reads results, resolves conflicts, and synthesizes one answer. | Consolidated outcome |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Independence | Review each delegated subtask. | No delegated subtask depends on another delegated subtask's unfinished output. |
| Prompt completeness | Review agent prompts. | Each prompt contains sufficient context to execute without follow-up clarification. |
| Parallelism value | Review background usage. | Background mode appears only when the parent context performed independent work in parallel. |
| Result synthesis | Review the final answer. | The final answer integrates delegated results instead of forwarding raw output. |
