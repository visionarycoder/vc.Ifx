---
name: test-tagging
license: MIT
title: Test Trait Tagging
description: Classify existing tests with a standard trait taxonomy and either apply framework-native tags or emit a report when edits are unsafe.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 900
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - test-analysis-extensions
  - test-gap-analysis
  - test-anti-patterns
  - grade-tests
  - run-tests
appliesTo: '**/*.{cs,csproj,xml,json,md}'
tags:
  - test
  - tagging
  - traits
  - audit
---
# Test Trait Tagging

Agent classifies tests with consistent traits so teams filter and reason about suite composition.

## When to Use

| User prompt | Use |
|---|---|
| User asks to add or audit test tags | Use this skill |
| User asks how tests split across smoke, regression, or integration buckets | Use this skill |
| User asks for tagging recommendations where edits are risky | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to invent project-specific tags | Keep the approved taxonomy or ask for a taxonomy change explicitly |
| User asks to write or migrate tests | Use a test-writing or migration skill |
| User asks to run filtered tests | Use `run-tests` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Test scope | Yes | Agent needs files, directories, projects, or named tests. |
| Mode | No | Agent uses `tag`, `audit`, or `both`. |
| Framework | No | Agent infers it, then reads `test-analysis-extensions`. |

## Taxonomy Table

| Trait | Agent meaning | Common signals |
|---|---|---|
| `positive` | Expected success path | Happy-path assertions |
| `negative` | Expected failure or rejection | Exception or validation assertions |
| `boundary` | Limit behavior | Null, empty, min, max, zero, or large input |
| `smoke` | Fast confidence path | Quick core-flow verification |
| `critical-path` | Business-critical flow | Payment, auth, workflow gate, or other high-impact path |
| `regression` | Bug repro coverage | Issue IDs, regression naming, or linked bug context |
| `integration` / `end-to-end` | Multi-component or full-flow scope | Real DB, HTTP, file system, or cross-layer orchestration |
| `performance` / `security` / `concurrency` / `resilience` | Specialized quality focus | Domain-specific assertions |
| `destructive` / `configuration` / `flaky` | Operational handling | Side effects, environment coupling, or instability |

## Edit Capability Table

| Capability | Agent behavior | Pass |
|---|---|---|
| `auto-edit` | Agent writes native framework tags. | Tags compile and match framework syntax. |
| `report-only` | Agent emits suggested tags without edits. | Source files stay unchanged. |
| `convention-based` | Agent edits only after explicit convention confirmation. | Tags align to repository conventions. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads the matching extension guidance. | Review extension file. | Tag support mode is known before edits. |
| 2 | Agent assigns one primary trait to each test. | Review classification list. | Every test gets `positive` or `negative`. |
| 3 | Agent adds secondary traits only when evidence is explicit. | Review tags. | Optional tags stay evidence-based. |
| 4 | Agent preserves existing valid tags. | Compare before and after tags. | Existing signals remain intact and non-duplicated. |
| 5 | Agent summarizes suite distribution. | Review final report. | Output shows tag counts and notable gaps. |

## Verification Checklist

- [ ] Agent read framework guidance before editing.
- [ ] Agent used only approved taxonomy values.
- [ ] Agent assigned `positive` or `negative` to every tagged test.
- [ ] Agent preserved existing valid tags.
- [ ] Agent reported distribution totals.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent invents ad hoc tags | Agent stays inside the approved taxonomy. |
| Agent edits a report-only framework | Agent emits recommendations only. |
| Agent treats tags as mutually exclusive | Agent applies multiple evidence-backed traits when needed. |
| Agent infers specialized tags from weak hints | Agent adds advanced tags only with explicit evidence. |
