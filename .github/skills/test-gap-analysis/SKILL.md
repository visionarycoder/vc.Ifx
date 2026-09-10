---
name: test-gap-analysis
description: Analyze production code with pseudo-mutation reasoning and generate a ranked report of survived mutations and uncovered paths.
license: MIT
title: Test Gap Analysis via Pseudo-Mutation
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1141
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - test-analysis-extensions
  - test-anti-patterns
  - grade-tests
  - run-tests
  - test-fixes
appliesTo: '**/*.{cs,csproj,xml,json,md}'
tags:
  - test
  - gap
  - analysis
  - mutation
---
# Test Gap Analysis via Pseudo-Mutation

Agent analyzes whether current tests detect plausible code defects.

## When to Use

| User prompt | Use |
|---|---|
| User asks whether tests catch subtle defects | Use this skill |
| User asks where tests are blind | Use this skill |
| User asks for mutation-style review without running a mutation tool | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to write tests | Use a test-writing skill |
| User asks for anti-pattern audit | Use `test-anti-patterns` |
| User asks for assertion diversity only | Use `assertion-quality` |
| User asks to run Stryker, PIT, mutmut, or another mutation tool | Use the tool directly |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Production code | Yes | Files or symbols to analyze |
| Test code | Yes | Tests that cover the production code |
| Language | No | Agent infers language when omitted |
| Focus category | No | Boundary, logic, return, exception, arithmetic, or null |

## Mutation Categories

| Category | Defect shape |
|---|---|
| Boundary | Off-by-one or inclusion flip |
| Logic | Boolean, negation, or branch flip |
| Return | Null, default, empty, or wrong scalar |
| Exception | Missing guard or swallowed error |
| Arithmetic | Sign, operator, or increment flip |
| Null | Removed null guard or coalescing path |

## Verdict Matrix

| Verdict | Meaning |
|---|---|
| Killed | At least one test fails |
| Survived | No covering test fails |
| No coverage | No test reaches the code path |
| Equivalent | Mutation does not change behavior |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads `test-analysis-extensions` for the target language. | Read the matching extension file. | Guidance matches the language and framework. |
| 2 | Agent reads production code and matching tests. | Read the selected files. | Each analyzed method has covering tests or an explicit no-coverage note. |
| 3 | Agent marks meaningful mutation points. | Read the mutation table. | Each mutation point maps to one category. |
| 4 | Agent traces which tests reach each point. | Read the coverage reasoning. | Each mutation point has covering tests or `No coverage`. |
| 5 | Agent decides killed, survived, no coverage, or equivalent. | Read the verdict table. | Each mutation point has one verdict. |
| 6 | Agent ranks survived and uncovered items by business risk. | Read the final report. | High-risk logic appears before low-risk formatting code. |
| 7 | Agent generates targeted test recommendations. | Read recommendations. | Each survived or uncovered item has one concrete test fix. |

## Required Report

| Section | Requirement |
|---|---|
| Summary | Counts for mutation points, killed, survived, no coverage, and equivalent |
| Survived mutations | Location, mutation, why tests miss it, and one fix |
| No-coverage paths | Location and missing path |
| Strengths | Short note on killed mutations |
| Next steps | Ranked test improvements |

## References

Agent reads `references/mutation-catalog.md` when the user asks for full category examples.

## Verification Checklist

- [ ] Agent read language-specific guidance.
- [ ] Agent skipped trivial or generated code.
- [ ] Agent assigned one verdict per mutation point.
- [ ] Agent ranked findings by business risk.
- [ ] Agent generated one concrete fix per survived or uncovered item.
- [ ] Agent kept the main file under the token budget.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent analyzes trivial accessors | Agent skips boilerplate and trivial getters. |
| Agent reports equivalent mutations as gaps | Agent marks them `Equivalent` and excludes them from scoring. |
| Agent ignores helper call chains | Agent traces tests through helpers and setup code. |
| Agent writes one new test per mutation blindly | Agent groups mutations when one test kills several. |
