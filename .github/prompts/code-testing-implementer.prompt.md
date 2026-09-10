---
mode: agent
title: Test Implementer
description: Implement one planned phase, then verify build, run tests, and verify harness discovery.
doc_type: prompt
status: active
last_invoked_by: code-testing-generator
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1300
invokes_skills:
  - code-testing-extensions
  - code-testing-agent
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
# Test Implementer

Agent implements one phase from `.testagent/plan.md`.

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads `.testagent/plan.md` and `.testagent/research.md`. | Read both files. | Phase scope, commands, and targets are clear. |
| 2 | Agent reads every source file in the phase. | Compare plan and reads. | Every planned source file was read. |
| 3 | Agent records the harness baseline from repo root. | Run harness discovery command. | Baseline test count exists. |
| 4 | Agent verifies project references before writing tests. | Read manifest diff. | Required source references exist. |
| 5 | Agent registers a new test project when the phase needs one. | Run harness discovery command. | New project appears in discovery output. |
| 6 | Agent writes test files for planned scenarios. | Review diff. | New tests match planned files and scenarios. |
| 7 | Agent runs `code-testing-builder` for the scoped test project. | Run build command. | Exit code = 0. |
| 8 | Agent runs `code-testing-tester` for the scoped test project. | Run test command. | Exit code = 0. |
| 9 | Agent verifies harness discovery delta from repo root. | Run harness discovery command. | Delta = tests added. |
| 10 | Agent runs `code-testing-linter` when a fix command exists. | Run lint command. | Exit code = 0 or no-op. |
| 11 | Agent writes the phase report. | Read report. | Report lists status, counts, and files. |

## Edit Boundaries

| Boundary | Agent Action | Test | Pass |
|---|---|---|---|
| Existing test file | Agent appends new tests only. | Review diff. | No existing test line changed. |
| Existing production file | Agent leaves production code unchanged. | Review diff. | No production file diff exists. |
| Build manifest | Agent writes minimal registration or dependency changes. | Review diff. | Diff touches only required manifest lines. |
| Sparse workspace | Agent works with current files. | Review command log. | No version control cleanup command ran. |

## Test Depth

| Requirement | Agent Action | Test | Pass |
|---|---|---|---|
| Main behavior | Agent writes at least one happy-path test. | Read tests. | One happy-path test exists per target symbol. |
| Boundary values | Agent writes null, empty, or limit tests when inputs support them. | Read tests. | Boundary coverage exists or a reason exists. |
| Error paths | Agent writes observable failure tests. | Read tests. | Failure behavior is asserted. |
| Side effects | Agent asserts a second observable when behavior changes state or dependencies. | Read tests. | Test includes secondary observable. |

## Report Format

```text
PHASE: <n>
STATUS: PASS | PARTIAL | FAIL
TESTS_CREATED: <n>
TESTS_PASSING: <n>
HARNESS_DISCOVERY: <delta>
FILES:
- path (n tests)
ISSUES:
- <line>
```

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent completes the assigned phase. | Compare plan and report. | Every planned file has an outcome. |
| Agent retries build defects no more than 3 times. | Review workflow log. | Build retry count ≤ 3. |
| Agent retries test defects no more than 5 times. | Review workflow log. | Test retry count ≤ 5. |
| Agent fixes fresh test expectations by reading production code. | Review defect notes. | Notes describe expected and actual behavior. |
