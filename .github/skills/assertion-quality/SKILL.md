---
name: assertion-quality
license: MIT
title: Assertion Diversity Analysis
description: Analyze whether tests use meaningful, varied assertions and identify missing, trivial, or single-style verification patterns.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 980
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - test-analysis-extensions
  - grade-tests
  - test-gap-analysis
  - test-anti-patterns
appliesTo: '**/*.{cs,js,ts,py,java,go,rb,rs,swift,kt,ps1,cpp,cc,cxx,h,hpp}'
tags:
  - testing
  - assertions
  - quality
  - analysis
  - polyglot
---
# Assertion Diversity Analysis

Agent evaluates whether tests prove behavior with meaningful and varied assertions.

## When to Use

| User prompt | Use |
|---|---|
| User asks whether assertions are too shallow | Use this skill |
| User asks for zero-assert or trivial-assert detection | Use this skill |
| User asks for suite-wide assertion category balance | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to write or repair tests | Use a test-writing skill |
| User asks for general suite defects beyond assertions | Use `test-anti-patterns` |
| User asks for runtime coverage | Use `coverage-analysis` or `test-gap-analysis` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Test scope | Yes | Agent needs files, classes, projects, or named tests. |
| Production context | No | Agent reads it when business significance is unclear. |
| Framework | No | Agent infers it, then reads `test-analysis-extensions`. |

## Assertion Category Table

| Category | Agent counts | Weak signal |
|---|---|---|
| Equality and comparison | Expected values, ranges, ordering, tolerances | Many equality checks only |
| Boolean and null | Presence, absence, flags, state guards | Tautologies or null-only suites |
| Exception and negative | Expected failures or forbidden paths | Broad or detail-free exception checks |
| Type and structure | Runtime type, object shape, snapshots, collection contents | Stringified structure only |
| State and side effect | Mutations, persistence, messages, collaborator interactions | Verification of setup rather than outcomes |

## Quality Pattern Table

| Pattern | Signal | Agent fix |
|---|---|---|
| Zero-assert test | No assert, verify, or snapshot exists | Agent flags false confidence. |
| Trivial-only test | Assertions prove existence only | Agent recommends behavior-specific checks. |
| Single-style suite | Nearly all tests use one assertion category | Agent recommends missing categories. |
| Happy-path-only verification | Negative or exceptional outcomes are absent | Agent recommends failure-path assertions. |
| Structural-blind test | Complex outputs receive scalar checks only | Agent recommends structural or state assertions. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads framework guidance. | Review extension file. | Framework-specific assertion forms are recognized. |
| 2 | Agent inventories assertion forms in scope. | Review extracted assertions. | Every test has a counted verification style or a zero-assert note. |
| 3 | Agent groups assertions by category and depth. | Review category map. | Each assertion lands in one primary category. |
| 4 | Agent calculates concise metrics. | Review metrics table. | Output includes zero-assert, trivial-only, and category spread. |
| 5 | Agent reports gaps without confusing volume for strength. | Review findings. | Duplicate equality checks do not inflate diversity claims. |

## Output Contract

| Output | Requirement |
|---|---|
| Suite summary | Agent reports counts and category spread. |
| Weak tests | Agent identifies zero-assert and trivial-only cases. |
| Missing categories | Agent names underused negative, structural, or state assertions. |
| Recommendations | Agent gives category-targeted next steps. |

## Verification Checklist

- [ ] Agent loaded framework guidance before classifying assertions.
- [ ] Agent counted mock verifications and snapshots when they prove behavior.
- [ ] Agent separated trivial checks from meaningful single-assert tests.
- [ ] Agent reported diversity by category, not raw volume only.
- [ ] Agent connected recommendations to specific tests or suite gaps.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent treats every boolean assertion as trivial | Agent flags only tautological or low-signal boolean checks. |
| Agent penalizes valid exception-only tests | Agent treats strong exception assertions as complete for that intent. |
| Agent misses framework-specific assertion helpers | Agent reads the extension file first. |
| Agent calls high-volume equality suites diverse | Agent measures spread across categories. |
