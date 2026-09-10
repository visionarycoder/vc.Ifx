---
name: test-fixes
description: Bundled remediation workflow for test quality audits, gap analysis, assertion quality, deep smell detection. Routes to correct specialized test skill.
license: MIT
metadata:
  author: WSDOT Financial IDL
  version: "1.0.0"
title: Test Fixes Bundle
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: low
estimated_tokens: 750
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - test-gap-analysis
  - test-anti-patterns
  - grade-tests
  - run-tests
appliesTo: '**/*.{cs,csproj,xml,json,md}'
tags:
  - test
---
# Test Fixes Bundle

Agent routes test-focused analysis, remediation to correct specialist.

## Bundle Coverage Matrix

| Area | Specialized skill source | Router reference |
|---|---|---|
| Pragmatic anti-pattern audit | `.github/skills/test-anti-patterns/SKILL.md` | `references/test-analysis-routing-table.md` |
| Mutation-style gap analysis | `.github/skills/test-gap-analysis/SKILL.md` | `references/test-analysis-routing-table.md` |
| Academic 19-smell catalog review | `.github/skills/test-smell-detection/SKILL.md` | `references/test-analysis-routing-table.md` |
| Assertion depth/diversity audit | `.github/skills/assertion-quality/SKILL.md` | `references/test-analysis-routing-table.md` |

## When To Use

Use when user asks to:
- Audit test quality or find weak tests
- Find missing edge cases tests miss
- Run test-smell review using academic smell catalog
- Evaluate assertion quality, diversity
- Decide which specialized test-analysis surface to use

## Router Rules

Agent routes:

| User Request Contains | Route To | Reason |
|---|---|---|
| "pragmatic" / "clean code" | `test-anti-patterns` | Practical pattern audit |
| "mutation" / "escaped defect" | `test-gap-analysis` | Coverage gap detection |
| "test smell" / "academic" | `test-smell-detection` | 19-smell catalog |
| "assertion depth" / "assertion diversity" | `assertion-quality` | Assertion analysis |

## Required Workflow

### Phase 1: Intent classification

Agent performs:
1. Agent classifies request into one or more analysis tracks
2. Agent loads consolidated routing reference
3. Agent declares codebase scope, language

### Phase 2: Execute targeted analysis

Agent performs:
1. Agent runs only relevant specialized skill
2. Agent keeps findings evidence-based with file references
3. Agent avoids unrelated framework migrations or test generation suggestions unless asked

### Phase 3: Summarize and next actions

Agent performs:
1. Agent returns severity-ranked findings where applicable
2. Agent identifies highest-value follow-up fixes
3. Agent suggests when to switch to test generation or test execution tools

## Non-Goals

- This bundle does not generate new tests directly
- This bundle does not run test frameworks by itself
- For writing tests use `code-testing-agent` or `writing-mstest-tests`
- For executing tests use run-tests tooling.
