---
name: dotnet-unit-testing
description: Generate or update MSTest unit tests for .NET projects with repository-aligned naming, isolation, assertions, and dependency usage.
title: .NET Unit Testing
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1580
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - writing-mstest-tests
  - dotnet-dependency-injection-standards
  - run-tests
appliesTo: '**/*.{cs,csproj,sln,slnx,props,targets,json,config}'
tags:
  - dotnet
  - unit
  - testing
---
# .NET Unit Testing

Agent generates or updates repository-style MSTest unit tests for .NET code.

## When to Use

| User prompt | Use |
|---|---|
| User asks for new .NET unit tests | Use this skill |
| User asks to extend tests after a code change | Use this skill |
| User asks to align tests with repository conventions | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for an audit only | Use `test-anti-patterns`, `grade-tests`, or `assertion-quality` |
| User asks to run tests | Use `run-tests` |
| User uses xUnit or NUnit intentionally | Use a framework-matched skill or preserve local framework conventions |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Production class or method | Yes | Agent needs the target behavior. |
| Existing tests | No | Agent reads them to match local patterns. |
| Dependency graph | No | Agent reads constructor or factory dependencies when isolation is unclear. |

## Repository Conventions

| Area | Agent action | Pass |
|---|---|---|
| Test project | Agent places tests in the mirrored `Tests/**` project structure. | Namespace and path align to the production class. |
| Test class | Agent uses `{ProductionClass}Tests`. | File and class naming stay predictable. |
| Test method | Agent uses `Method_Scenario_Result`. | Method name communicates behavior. |
| Private fields | Agent uses local camelCase patterns and avoids underscore prefixes when neighboring tests do. | Fields match repository style. |
| Assertions | Agent prefers specific MSTest assertions. | Failure messages stay readable. |

## Coverage Pattern Table

| Category | Agent expectation | Pass |
|---|---|---|
| Happy path | Agent covers normal success behavior. | Main outcome is asserted. |
| Edge case | Agent covers null, empty, boundary, or alternate valid input when applicable. | Boundary behavior is explicit. |
| Failure path | Agent covers dependency failure or validation error when applicable. | Failure surface is asserted. |
| Data flow | Agent verifies important transformed values or collaborator inputs. | Captured values prove behavior. |
| Guard clauses | Agent covers argument validation for public APIs. | Invalid input throws or rejects as designed. |

## Dependency Pattern Table

| Dependency shape | Agent default | Agent avoids |
|---|---|---|
| External boundary | Agent mocks DB, HTTP, queue, or remote service dependencies. | Agent avoids live infrastructure in unit tests. |
| Stable framework helper | Agent uses real helpers such as `Options.Create(...)` or `NullLogger<T>.Instance` when sufficient. | Agent avoids unnecessary mocks. |
| Pure value object | Agent instantiates the real object. | Agent avoids mocking DTOs or records. |
| Sealed helper without interface | Agent constructs the real helper with mocked inner boundaries when practical. | Agent avoids brittle wrappers solely for tests. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads the target production code and sibling tests. | Review source and nearby tests. | New tests match repository patterns. |
| 2 | Agent maps direct dependencies and isolates only the needed boundaries. | Review constructor and collaborators. | Test setup contains only relevant mocks. |
| 3 | Agent writes focused tests for happy, edge, and failure behavior. | Review test list. | Public behavior coverage is balanced. |
| 4 | Agent uses specific assertions and data capture where behavior needs proof. | Review assertions and verifies. | Assertions prove outcomes, not implementation trivia. |
| 5 | Agent references `references/test-patterns.md` or `references/workflow-testing.md` when the target matches those scenarios. | Review supporting guidance. | Pattern choice matches repository examples. |

## Output Requirements

| Output | Requirement |
|---|---|
| Generated tests | Agent returns compilable MSTest code. |
| Rationale | Agent names only the repository conventions that affected the result. |
| Validation plan | Agent identifies the smallest relevant build or test command when execution is in scope. |

## Verification Checklist

- [ ] Agent mirrored repository naming and placement conventions.
- [ ] Agent covered happy, edge, and failure behavior where applicable.
- [ ] Agent mocked only direct external boundaries.
- [ ] Agent used specific MSTest assertions.
- [ ] Agent kept the skill under the token budget.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent copies a generic template blindly | Agent reads sibling tests first. |
| Agent mocks every collaborator | Agent keeps stable helpers real when they do not hide behavior. |
| Agent writes tests against internals only | Agent anchors assertions on observable behavior. |
| Agent omits failure-path coverage | Agent adds one meaningful negative path for public APIs. |

## References

- [references/test-patterns.md](references/test-patterns.md)
- [references/workflow-testing.md](references/workflow-testing.md)
