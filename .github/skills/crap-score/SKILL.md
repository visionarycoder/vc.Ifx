---
name: crap-score
description: Calculate targeted CRAP scores for a named .NET method, class, or file using coverage and complexity data.
license: MIT
title: CRAP Score Analysis
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 888
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - coverage-analysis
related_skills:
  - coverage-analysis
  - code-testing-agent
appliesTo: '**/*.{cs,csproj,xml,json}'
tags:
  - dotnet
  - testing
  - coverage
  - crap-score
  - analysis
---
# CRAP Score Analysis

Agent calculates CRAP scores for a targeted .NET scope.

## Formula

`CRAP = complexity^2 x (1 - coverage)^3 + complexity`

## Risk Matrix

| Score | Risk |
|---|---|
| `< 5` | Low |
| `5-15` | Moderate |
| `15-30` | High |
| `> 30` | Critical |

## When to Use

| User prompt | Use |
|---|---|
| User asks for CRAP on one method, class, or file | Use this skill |
| User asks which targeted code is risky and undertested | Use this skill |
| User asks how much coverage lowers one score | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for project-wide coverage triage | Use `coverage-analysis` |
| User asks to write tests | Use a test-writing skill |
| User asks to run tests only | Use `run-tests` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Target scope | Yes | Method, class, or file |
| Coverage file | No | Cobertura XML |
| Source scope | No | Project or source path |
| Threshold | No | Default risk threshold is `15` |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent finds Cobertura data or generates it. | Read `TestResults` or run the coverage command. | Cobertura XML exists. |
| 2 | Agent reads the target source scope and counts decision points per method. | Read the complexity table. | Each method has a complexity value of `>= 1`. |
| 3 | Agent maps Cobertura coverage to each method. | Read the coverage table. | Each method has a coverage percentage or documented gap. |
| 4 | Agent computes CRAP for each method in scope. | Spot-check one method with the formula. | Calculated score matches the formula. |
| 5 | Agent sorts results by descending CRAP. | Read the final report. | Highest-risk method appears first. |
| 6 | Agent computes the coverage needed to reach the target threshold when complexity allows it. | Read the recommendation table. | Coverage target exists or the report states complexity blocks the threshold. |

## Required Output

| Section | Requirement |
|---|---|
| Score table | Method, complexity, coverage, CRAP, risk |
| Summary | Count of methods by risk |
| Top offenders | High and critical scores first |
| Recommendations | Add tests, reduce complexity, or both |

## Verification Checklist

- [ ] Agent used Cobertura coverage data.
- [ ] Agent counted complexity per method.
- [ ] Agent applied the CRAP formula correctly.
- [ ] Agent sorted results by risk.
- [ ] Agent stated when testing alone cannot reach the threshold.
- [ ] Agent generated targeted next steps.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent uses stale coverage | Agent regenerates coverage when data is missing or stale. |
| Agent misses async or generated method names | Agent maps by line range when names differ. |
| Agent treats all high scores the same | Agent separates test gaps from complexity-only defects. |
| Agent includes generated files by default | Agent excludes generated files unless the user asks. |
