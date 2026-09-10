---
name: test-anti-patterns
description: Audit existing tests for high-confidence anti-patterns that create false confidence, flakiness, or avoidable maintenance cost.
title: Test Anti-Pattern Detection
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1020
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - test-analysis-extensions
  - test-gap-analysis
  - grade-tests
  - test-fixes
appliesTo: '**/*.{cs,py,ts,tsx,js,jsx,java,go,rb,rs,ps1,cpp,hpp}'
tags:
  - test
  - anti-patterns
  - quality
  - flakiness
---
# Test Anti-Pattern Detection

Agent audits tests for pragmatic defects that break trust in the suite.

## When to Use

| User prompt | Use |
|---|---|
| User asks why tests feel unreliable | Use this skill |
| User asks for a PR-focused quality review | Use this skill |
| User asks for flaky, over-mocked, or weak-test findings | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks to write tests | Use a test-writing skill |
| User asks for formal smell taxonomy | Use `test-smell-detection` |
| User asks for coverage or CRAP numbers | Use `coverage-analysis` or `crap-score` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Test scope | Yes | Agent needs files, classes, tests, or diff scope. |
| Framework | No | Agent infers it, then reads `test-analysis-extensions`. |
| Production context | No | Agent reads it when coupling or mock relevance is unclear. |

## Anti-Pattern Table

| Pattern | Severity | Signal | Agent fix |
|---|---|---|---|
| Assertion-free test | Critical | Test verifies execution only. | Agent adds outcome assertions or verification. |
| Tautological assertion | Critical | Assertion passes without behavior proof. | Agent replaces it with behavior-specific checks. |
| Swallowed exception | Critical | Manual exception handling hides failure or asserts only inside `catch`. | Agent uses framework exception assertions. |
| Unawaited async work | Critical | Test passes before failure occurs. | Agent awaits the operation or returned task. |
| Timing dependency | High | `Sleep`, fixed delays, or race polling exists. | Agent replaces timing with event or state-based waits. |
| Shared mutable state | High | Test outcome depends on execution order. | Agent isolates state per test. |
| Over-mocking | High | Test configures many collaborators for one behavior. | Agent removes irrelevant mocks or uses stable real objects. |
| Broad or internal assertion | High | Test accepts vague exceptions or verifies internals only. | Agent asserts exact public behavior. |
| Giant or duplicated test | Medium | One test spans many branches or several tests differ only by literals. | Agent splits behavior or parameterizes inputs. |
| Weak structure or dead setup | Low | AAA is unclear, debug noise remains, or setup is unused. | Agent cleans structure and removes scaffolding. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent defines the audit scope. | Read the prompt and selected files. | Audit stays inside the requested boundary. |
| 2 | Agent reads the matching extension guidance. | Read `test-analysis-extensions` and one extension file. | Framework idioms and skips are recognized correctly. |
| 3 | Agent scans for anti-pattern signals. | Review each flagged location. | Each finding maps to one table entry. |
| 4 | Agent calibrates severity by trust impact. | Compare defect to severity table. | Critical and High items reflect false confidence or instability. |
| 5 | Agent collapses repeated symptoms into systemic findings. | Review final report. | Output stays concise and non-repetitive. |
| 6 | Agent includes strengths. | Review final report. | Report shows positive observations as well as defects. |

## Output Contract

| Output | Requirement |
|---|---|
| Summary | Agent reports counts by severity and one-line suite health. |
| Top findings | Agent includes location, risk, and one fix for every Critical or High issue. |
| Lower-severity items | Agent uses a compact table. |
| Positive observations | Agent names trustworthy patterns already present. |

## Verification Checklist

- [ ] Agent read framework guidance before judging patterns.
- [ ] Agent tied every finding to a specific location.
- [ ] Agent gave a concrete fix for every Critical or High issue.
- [ ] Agent reported observed defects only.
- [ ] Agent kept severity aligned to trust impact.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent rates naming nits as severe | Agent reserves top severity for false confidence or flakiness. |
| Agent flags framework idioms as defects | Agent calibrates against the extension file first. |
| Agent repeats the same root issue in every test | Agent reports the systemic cause once, then cites examples. |
| Agent invents missing tests from speculation | Agent reports observable evidence only. |
