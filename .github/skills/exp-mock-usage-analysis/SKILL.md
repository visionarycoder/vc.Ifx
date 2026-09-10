---
name: exp-mock-usage-analysis
description: Trace .NET mock setups through production control flow to find dead, unreachable, redundant, or replaceable mocks.
license: MIT
title: Mock Usage Analysis
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1290
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - test-anti-patterns
  - exp-test-maintainability
  - writing-mstest-tests
appliesTo: '**/*'
tags:
  - exp
  - mock
  - usage
  - analysis
---
# Mock Usage Analysis

Agent traces each mock setup through production execution to decide whether the setup earns its cost.

## When to Use

| User prompt | Use |
|---|---|
| User asks which mocks are unused or unnecessary | Use this skill |
| User asks whether test setup is over-mocked | Use this skill |
| User asks whether `ILogger`, `IOptions`, or similar mocks become real objects more cleanly | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for general suite quality review | Use `test-anti-patterns` |
| User asks to migrate between mock frameworks | Handle the migration directly |
| User asks to write new tests only | Use a test-writing skill |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Test code | Yes | Agent needs mock setup and verification code. |
| Production code | Yes | Agent needs the executed control flow. |
| Mock framework | No | Agent infers Moq, NSubstitute, or FakeItEasy terminology. |

## Classification Table

| Classification | Signal | Agent conclusion | Agent fix |
|---|---|---|---|
| Used | Production code reaches the setup on this path. | Setup is justified. | Agent keeps it and cites why when useful. |
| Unreachable | Guard clause, exception, or branch prevents the call. | Setup is unnecessary for this test path. | Agent removes or relocates it. |
| Unused | Production method never calls that member. | Setup adds noise without value. | Agent deletes it. |
| Redundant | Multiple tests repeat the same setup. | Shared infrastructure helps when repetition is material. | Agent proposes extraction when repetition is material. |
| Replaceable | Stable framework type does not need a mock. | Real helper is simpler. | Agent swaps to `NullLogger`, `Options.Create`, or a real value object. |

## Mock Pattern Table

| Pattern | Agent interpretation | Pass |
|---|---|---|
| `Setup` plus `Verify` on called dependency | Legitimate interaction proof | Setup remains because observable behavior depends on it. |
| Mocked DTO or record | Likely replaceable | Real object removes indirection. |
| Logging mock without verification | Usually replaceable | `NullLogger<T>.Instance` preserves behavior. |
| Options mock | Usually replaceable | `Options.Create(new T())` preserves behavior. |
| Branch-only setup | Path-specific evidence needed | Agent keeps it only in tests that reach the branch. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads test code and production code together. | Review both scopes. | Findings rely on traced execution, not guesswork. |
| 2 | Agent inventories each setup and verification. | Review extracted list. | Every finding references one concrete setup. |
| 3 | Agent traces the specific test inputs through production control flow. | Review path notes. | Used and unreachable classifications match the code path. |
| 4 | Agent identifies replaceable mocks. | Review setup types. | Stable framework helpers become real objects where safe. |
| 5 | Agent reports only high-confidence findings. | Review final report. | Each item states location, reason, and concrete change. |

## Report Contract

| Section | Requirement |
|---|---|
| Summary | Agent reports setup counts by classification. |
| Findings | Agent reports test, setup line, path reason, and fix. |
| Good usage | Agent names correctly placed mocks for external boundaries. |
| Refactoring opportunities | Agent groups repeated setups into one extraction suggestion. |

## Verification Checklist

- [ ] Agent read production control flow before classifying mocks.
- [ ] Agent used framework-correct terminology.
- [ ] Agent tied every finding to a specific setup line.
- [ ] Agent preserved valid mocks at external boundaries.
- [ ] Agent recommended real helpers only when behavior stayed intact.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent flags every mock as suspicious | Agent preserves mocks at true system boundaries. |
| Agent judges setups without reading production code | Agent traces the actual path first. |
| Agent ignores verify-only interaction tests | Agent credits verifications that prove required side effects. |
| Agent extracts shared setup after two occurrences only | Agent waits for material repetition. |
