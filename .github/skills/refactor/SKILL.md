---
name: refactor
description: Refactor existing code with clear goals, standards alignment, stepwise plan with validation.
title: Refactor
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 900
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills: []
appliesTo: '**/*'
tags:
  - refactor
---
# Refactor

Agent runs disciplined refactoring workflow preserving behavior while improving structure, readability, maintainability, or performance.

## When to Use

Use when:
- User asks to refactor existing code
- User wants cleaner structure without changing external behavior
- User asks for targeted cleanup with coding standards, architecture rules

## When Not to Use

Do not use when:
- Brand new feature implementation with zero existing code path
- Pure bug fixes where zero structural refactor is needed
- Broad migrations needing dedicated modernization workflow

## Inputs

| Input | Required | Description |
|---|---|---|
| Target code scope | Yes | Files, folders, symbols, or snippets to refactor |
| Refactor goals | Yes | Desired outcomes (readability, complexity reduction, reuse, naming, etc.) |
| Constraints | No | API compatibility, performance limits, style constraints, timeline |
| Validation requirements | No | Build/test commands or acceptance checks |

## Required Workflow

### 1. Clarify scope and goals

Agent performs:
1. Agent verifies what code to refactor
2. Agent verifies constraints, non-goals
3. When missing, agent asks for exact files/symbols or infers from context, states assumptions

### 2. Load repo guidance before edits

Agent performs:
1. Agent reads relevant guidance in `.github/**`, `docs/**` for standards, conventions
2. Agent reads architecture analyzer patterns, rules when present (e.g., projects named `Architecture.Analyzers`)
3. Agent identifies rule conflicts early, resolves before implementation

### 3. Plan refactor

Agent performs:
1. Agent breaks work into small, low-risk steps
2. Agent prioritizes behavior-preserving changes first
3. Agent identifies verification points after each major step

### 4. Execute step-by-step

Agent performs:
1. Agent applies focused edits with minimal unrelated churn
2. Agent preserves public contracts unless change is explicitly requested
3. Agent keeps naming, structure, patterns consistent with codebase

### 5. Validate and report

Agent performs:
1. Agent runs relevant build/test/lint checks
Test: Run `dotnet build` and `dotnet test`
Pass: Zero new build errors. Zero new test failures.
2. Agent verifies behavior is unchanged unless user requested behavior changes
3. Agent summarizes changed files, rationale, validation results

## Validation Checklist

Agent verifies:
- [ ] Scope, goals were explicit before edits
- [ ] Relevant repo guidance reviewed, followed
- [ ] Refactor plan broken into discrete steps
- [ ] Changes behavior-preserving or explicitly approved
- [ ] Build/tests run (or limitation clearly stated)
- [ ] Final report includes what changed, why

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Refactor without clear scope | Verify target files/symbols, goals first |
| Mix refactor with feature work | Separate structural cleanup from behavior changes unless requested |
| Large risky edits in one pass | Use incremental steps with verification checkpoints |
| Ignore repository conventions | Read `.github/**`, `docs/**` guidance before editing |
| Skip validation | Always run or explicitly report missing verification |
