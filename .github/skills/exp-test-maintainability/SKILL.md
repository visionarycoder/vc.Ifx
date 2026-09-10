---
name: exp-test-maintainability
description: Analyze .NET test suites for repeated setup, copy-paste structure, and maintainability refactors without editing code.
license: MIT
title: Test Maintainability Assessment
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1495
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - writing-mstest-tests
  - exp-mock-usage-analysis
  - test-anti-patterns
appliesTo: '**/*.{cs,csproj,xml,json,md}'
tags:
  - exp
  - test
  - maintainability
---
# Test Maintainability Assessment

Agent analyzes test suites for repeated structure that now costs more than it clarifies.

## When to Use

| User prompt | Use |
|---|---|
| User asks where tests are duplicated | Use this skill |
| User asks which tests fit data-driven conversion | Use this skill |
| User asks for refactoring opportunities in test setup or fixtures | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to write new tests | Use `writing-mstest-tests` or another test-writing skill |
| User asks for defect or smell audit | Use `test-anti-patterns` or `test-smell-detection` |
| User asks to execute the refactor automatically | Perform the refactor directly instead of using this analysis-only skill |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Test code | Yes | Agent needs one or more test files or a test project. |
| Production context | No | Agent reads it when helper boundaries are unclear. |
| Scope | No | Agent limits findings to one class or spans the suite. |

## Maintainability Pattern Table

| Pattern | Detection signal | Threshold | Agent fix |
|---|---|---|---|
| Repeated object construction | Same complex `new` graph appears across tests | 3+ occurrences | Agent suggests a factory, helper, or focused fixture. |
| Repeated assertion block | Same property or collection assertions repeat | 3+ occurrences | Agent suggests an assertion helper or verifier method. |
| Copy-paste test methods | Methods differ mainly by literals | 3+ occurrences | Agent suggests `DataRow`, `DynamicData`, `Theory`, or equivalent. |
| Duplicated setup or teardown | Multiple classes repeat lifecycle code | 3+ occurrences | Agent suggests shared fixture composition. |
| Repeated infrastructure scaffold | Same mocks, HTTP handlers, or config appear across classes | 3+ occurrences | Agent suggests a harness or reusable fake. |

## Calibration Rules

| Rule | Agent action | Pass |
|---|---|---|
| Small repetition is acceptable | Agent ignores two-off duplication that aids readability. | Findings focus on material repetition. |
| Simple construction is acceptable | Agent skips trivial `new Calculator()` style setup. | Suggestions target costly boilerplate only. |
| Explicit local setup is healthy | Agent notes clear self-contained tests as strengths. | Report avoids DRY-for-DRY's-sake advice. |
| Extraction has a coupling cost | Agent states trade-offs for each suggestion. | Recommendations balance reuse and locality. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent gathers the requested test scope. | Review selected files. | Scope is explicit. |
| 2 | Agent inventories repeated setup, assertions, and method bodies. | Review repetition candidates. | Each candidate has comparable code evidence. |
| 3 | Agent filters candidates through the 3+ threshold and triviality rules. | Review kept findings. | Report excludes weak repetition. |
| 4 | Agent proposes the smallest useful refactor. | Review suggestions. | Each suggestion reduces repetition without hiding behavior. |
| 5 | Agent reports trade-offs and priority. | Review final report. | High-value, low-risk refactors appear first. |

## Report Contract

| Section | Requirement |
|---|---|
| Summary | Agent states whether the suite is mostly clean or repetition-heavy. |
| Findings | Agent includes category, locations, repeated sample, and concrete refactor. |
| Impact | Agent estimates simplified tests, lines, or fixtures. |
| Trade-offs | Agent states readability gain versus indirection cost. |

## Verification Checklist

- [ ] Agent required 3+ meaningful occurrences before recommending extraction.
- [ ] Agent skipped trivial constructors and natural AAA similarity.
- [ ] Agent tied every finding to concrete locations.
- [ ] Agent proposed the smallest useful refactor shape.
- [ ] Agent acknowledged trade-offs for each suggestion.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent flags every similar AAA layout | Agent differentiates structure from duplicated code. |
| Agent recommends inheritance by default | Agent prefers helpers, builders, or composition first. |
| Agent ignores readability cost | Agent states when duplication is the clearer choice. |
| Agent reports two examples as a pattern | Agent waits for material repetition. |
