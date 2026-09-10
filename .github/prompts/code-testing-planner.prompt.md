---
mode: agent
title: Test Planner
description: Convert research findings into a phased test plan with file-level scope and verification gates.
doc_type: prompt
status: active
last_invoked_by: code-testing-generator
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1000
invokes_skills:
  - code-testing-extensions
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
# Test Planner

Agent converts research findings into `.testagent/plan.md`.

## Strategy Selection

| Research State | Agent Action | Test | Pass |
|---|---|---|---|
| Most files untested or unknown | Agent uses Broad strategy. | Read coverage estimates. | Untested or unknown files are the majority. |
| Most files tested | Agent uses Targeted strategy. | Read coverage estimates. | Well-tested files are the majority. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads `.testagent/research.md`. | Read file. | Language, commands, graph, and coverage are present. |
| 2 | Agent selects Broad or Targeted strategy. | Read strategy note. | One strategy is recorded. |
| 3 | Agent groups work by layer, priority, dependency, and complexity. | Read phase table. | Every scoped file belongs to one phase. |
| 4 | Agent maps each source file to a test file and test class or module. | Read file plan. | Every scoped source file has a destination. |
| 5 | Agent lists planned scenarios for each target method or function. | Read scenario list. | Happy path, boundary values, and failures appear when applicable. |
| 6 | Agent writes `.testagent/plan.md`. | Read output file. | File matches the required sections. |

## Phase Rules

| Rule | Test | Pass |
|---|---|---|
| Agent places leaf components before upper layers. | Read phase order. | Leaf phase appears first. |
| Agent places untested files before partially tested files. | Read priority order. | Untested files appear earlier. |
| Agent limits each phase to independently useful work. | Count phase files. | Phase scope is readable and testable. |
| Agent reuses existing test projects when one covers the target code. | Read project mapping. | Existing test project is selected when available. |

## Output Contract

| Section | Required Content | Test | Pass |
|---|---|---|---|
| Overview | Scope and selected strategy | Read section. | Scope and strategy both exist. |
| Commands | Build, run tests, and lint commands | Read section. | All three command types exist or a reason exists. |
| Phase Summary | Phase, focus, files, estimated tests | Read table. | Summary covers every phase. |
| Phase Detail | Source path, test path, test type, scenarios | Read detail section. | Every file has detail lines. |
| Success Criteria | Measurable completion gates | Read section. | Every phase has pass criteria. |

## Success Criteria Template

| Gate | Test | Pass |
|---|---|---|
| File creation | Compare planned and written files. | Every planned test file exists. |
| Build | Run scoped build command. | Exit code = 0. |
| Test run | Run scoped test command. | Exit code = 0. |

## Output Skeleton

```markdown
# Test Implementation Plan

## Overview

## Commands

## Phase Summary
| Phase | Focus | Files | Est. Tests |
|---|---|---|---|

## Phase 1

### Files to Test

### Success Criteria
```
