---
name: writing-mstest-tests
description: Write, update, or modernize MSTest tests with current assertions, data-driven patterns, lifecycle usage, analyzers, and repository-friendly naming.
license: MIT
title: Writing MSTest Tests
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1475
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-unit-testing
  - run-tests
  - migrate-mstest-v1v2-to-v3
  - migrate-mstest-v3-to-v4
appliesTo: '**/*.{cs,csproj,props,targets,json,md}'
tags:
  - writing
  - mstest
  - tests
  - dotnet
---
# Writing MSTest Tests

Agent writes or repairs MSTest tests with modern APIs and concise structure.

## When to Use

| User prompt | Use |
|---|---|
| User asks to create MSTest tests | Use this skill |
| User asks to replace weak MSTest assertions | Use this skill |
| User asks to fix `MSTESTxxxx` diagnostics in tests | Use this skill |
| User asks to modernize `ExpectedException`, `DataTestMethod`, or old lifecycle patterns | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for a suite audit | Use `test-anti-patterns` or `assertion-quality` |
| User asks to run tests | Use `run-tests` |
| User asks for MSTest version migration | Use the migrate-MSTest skills |
| User uses xUnit, NUnit, or TUnit | Use a framework-matched skill |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Production code or behavior | No | Agent reads it when test intent is unclear. |
| Existing MSTest code | No | Agent reads it when the user asks for fixes or modernization. |
| Scenario description | No | Agent uses it when source code is absent. |
| Target MSTest version | No | Agent infers it from packages or SDK usage. |

## Core Structure

| Area | Agent action | Pass |
|---|---|---|
| Class | Agent uses `[TestClass]` on a `sealed` class when neighboring tests follow that pattern. | Test class matches local project conventions. |
| Method | Agent uses `[TestMethod]` and a descriptive `Method_Scenario_Result` name. | Test name states behavior under test. |
| Body | Agent keeps one clear Arrange-Act-Assert flow. | Reader identifies setup, action, and verification in one pass. |
| Scope | Agent keeps one primary behavior per test. | Failure points to one behavior. |

## Assertion Selection

| Intent | Preferred API | Replace |
|---|---|---|
| Value equality | `Assert.AreEqual(expected, actual)` | `Assert.IsTrue(expected == actual)` |
| Null state | `Assert.IsNull` / `Assert.IsNotNull` | `Assert.IsTrue(x == null)` |
| Collection count | `Assert.HasCount` | `Assert.IsTrue(items.Count() == n)` |
| Collection membership | `Assert.Contains` / `Assert.DoesNotContain` | `Assert.IsTrue(items.Contains(x))` |
| Empty collection | `Assert.IsEmpty` / `Assert.IsNotEmpty` | `Count == 0` or `> 0` checks |
| Type check | `Assert.IsInstanceOfType<T>` | Hard cast plus null check |
| Exception | `Assert.ThrowsExactly<T>` or async variant | `[ExpectedException]` |
| String match | `Assert.Contains`, `StartsWith`, `EndsWith`, `MatchesRegex` | `StringAssert` when `Assert` covers it |

## Data and Lifecycle Patterns

| Need | Agent pattern | Pass |
|---|---|---|
| Simple input matrix | Agent uses `[DataRow]`. | Each case stays readable inline. |
| Complex or computed cases | Agent uses `[DynamicData]` with tuples or `TestDataRow<T>`. | Data source stays type-safe and compact. |
| Sync setup | Agent prefers constructor setup when project style allows it. | Fields stay initialized without null-forgiving noise. |
| Async setup | Agent uses `[TestInitialize]` only for async or per-test mutable setup. | Setup awaits work directly. |
| Cleanup | Agent uses `[TestCleanup]`, `DisposeAsync`, or `Dispose` as needed. | Resources release after failure and success. |
| Timeout-sensitive async flow | Agent passes `TestContext.CancellationToken`. | Test cooperates with timeout cancellation. |

## Analyzer Fix Table

| Rule shape | Agent fix | Pass |
|---|---|---|
| `ExpectedException` usage | Agent rewrites to `Assert.Throws` or `Assert.ThrowsExactly`. | Exception type and details stay asserted in code. |
| Swapped `AreEqual` arguments | Agent moves `expected` first. | Failure message reads correctly. |
| Generic boolean assertion | Agent replaces it with a specialized assert. | Assertion states intent directly. |
| Legacy `StringAssert` call | Agent uses equivalent `Assert` API when available. | Assertion API stays consistent. |
| Timeout without cooperative token | Agent flows `TestContext.CancellationToken`. | Timed test cancels cleanly. |
| Duplicate data or test attributes | Agent removes duplicate attributes. | Discovery remains deterministic. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent detects MSTest package and feature level. | Read `.csproj`, `global.json`, or neighboring tests. | Chosen APIs exist in the project version. |
| 2 | Agent reads local test naming and setup style. | Read sibling test files. | New code matches project conventions. |
| 3 | Agent writes or repairs the smallest set of tests that covers the requested behavior. | Review changed tests. | Happy path, edge case, and failure path exist when the target API is public. |
| 4 | Agent upgrades weak assertions to specific APIs. | Review assertions. | Each assertion expresses one behavior clearly. |
| 5 | Agent uses data-driven patterns only when duplication drops materially. | Compare before and after structure. | Parameterization removes repeated logic. |
| 6 | Agent fixes applicable MSTest analyzers instead of suppressing them. | Review diagnostics and code. | Warning source disappears without a suppression comment. |
| 7 | Agent points to `references/examples.md` when the user asks for fuller examples. | Read the response or link. | Main skill stays concise. |

## Output Requirements

| Output | Requirement |
|---|---|
| Test code | Agent returns compilable MSTest code or applies the edit directly. |
| Rationale | Agent explains only the chosen patterns that materially changed behavior. |
| Validation | Agent names the targeted build or test command when execution is in scope. |

## References

Agent reads `references/examples.md` when the user asks for detailed test samples, advanced Moq flows, or larger data-driven patterns.

## Verification Checklist

- [ ] Agent matched local MSTest version and repository conventions.
- [ ] Agent used specific assertion APIs instead of broad boolean checks.
- [ ] Agent covered happy, edge, and failure behavior when the target API is public.
- [ ] Agent used data-driven tests only when repetition justified it.
- [ ] Agent kept the main file within the token budget.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent writes one giant test | Agent splits behavior into focused tests. |
| Agent hides behavior behind excessive helpers | Agent keeps setup readable at the call site. |
| Agent uses old MSTest APIs by habit | Agent checks versioned features first. |
| Agent suppresses analyzers | Agent applies the direct code fix. |
