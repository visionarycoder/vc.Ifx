---
mode: agent
title: Unit Test Generation
description: Generate buildable unit tests that pin behavior and meet explicit coverage targets.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1200
invokes_skills:
  - code-testing-agent
  - assertion-quality
  - test-gap-analysis
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills: []
appliesTo: '**/*'
tags:
  - prompts
  - prompt
  - ste
---
# Unit Test Generation

Agent generates complete unit tests for the requested scope.

## Discovery

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads implementation and existing tests before writing code. | Review workflow log. | Source files and nearby tests were read. |
| 2 | Agent records framework, naming, lifecycle, assertion, and mocking conventions. | Read notes. | Convention fields are populated. |
| 3 | Agent records public signatures, observable outcomes, side effects, failures, cancellation, and concurrency. | Read notes. | All observed behavior fields are populated. |
| 4 | Agent records current coverage and target coverage. | Read coverage note. | Current and target values exist. |

## Test Design

| Requirement | Agent Action | Test | Pass |
|---|---|---|---|
| Requested behavior | Agent maps each requested scenario to one test. | Compare prompt and tests. | Every requested scenario appears once. |
| Main behavior | Agent writes happy-path tests first. | Read tests. | Happy-path coverage exists per target symbol. |
| Boundary values | Agent writes null, empty, limit, and transition tests when inputs support them. | Read tests. | Boundary coverage exists or a reason exists. |
| Failures | Agent verifies exception type or error result and visible consequences. | Read tests. | Failure assertions exist. |
| Interactions | Agent verifies important calls, arguments, order, and forbidden calls. | Read tests. | Interaction assertions exist when dependencies are mocked. |
| Cancellation | Agent verifies cancellation propagation. | Read tests. | Cancellation token behavior is asserted. |

## Assertion Depth

| Defect Pattern | Agent Action | Test | Pass |
|---|---|---|---|
| Changed comparison or branch | Agent writes value-specific assertions. | Read tests. | Result assertions use concrete values. |
| Removed validation | Agent writes failure assertions for invalid inputs. | Read tests. | Invalid-input tests exist. |
| Off-by-one result | Agent writes boundary result assertions. | Read tests. | Boundary outputs are pinned. |
| Dropped side effect | Agent writes secondary observable assertions. | Read tests. | Side effect is asserted. |
| Wrong dependency argument | Agent verifies dependency arguments. | Read tests. | Argument assertions exist. |
| Empty method body | Agent verifies visible result and side effect. | Read tests. | Empty body defect fails a test. |

## Execution

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent runs the smallest affected test project build. | Run build command. | Exit code = 0. |
| 2 | Agent runs related tests with the existing runner. | Run test command. | Exit code = 0. |
| 3 | Agent measures coverage when tooling already exists. | Run coverage command. | Coverage result exists. |
| 4 | Agent fixes test defects immediately. | Re-run tests. | Fresh test defects disappear. |
| 5 | Agent runs broader verification for shared contracts. | Run broader command. | Exit code = 0. |

## Output Contract

| Section | Test | Pass |
|---|---|---|
| Test files | Review diff. | Tests follow repository placement and naming. |
| Coverage summary | Read summary. | Covered behaviors are listed. |
| Commands and results | Read summary. | Build and test commands with outcomes exist. |
| Coverage result | Read summary. | Coverage value or skip reason exists. |
| Remaining gaps | Read summary. | Gaps or exclusions are explicit. |
