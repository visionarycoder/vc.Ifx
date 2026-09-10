---
name: generate-testability-wrappers
license: MIT
title: Generate Testability Wrappers
description: Generate minimal abstractions and dependency injection registrations for hard-to-test static dependencies.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1022
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - detect-static-dependencies
  - migrate-static-to-wrapper
appliesTo: '**/*.{cs,csproj,xml,json,md}'
tags:
  - generate
  - testability
  - wrappers
  - dependency-injection
---
# Generate Testability Wrappers

Agent generates the first abstraction layer around a hard-to-test dependency.

## When to Use

| User prompt | Use |
|---|---|
| User already found static dependencies | Use this skill |
| User needs a new wrapper or built-in seam | Use this skill |
| User needs dependency injection registration for a new seam | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks only for detection | Use `detect-static-dependencies` |
| User already has the abstraction | Use `migrate-static-to-wrapper` |
| User asks for broad call-site replacement | Use `migrate-static-to-wrapper` |

## Required Inputs

| Input | Required | Description |
|---|---|---|
| Dependency category | Yes | Time, file system, environment, network, console, process, or similar |
| Target framework | Yes | Determines built-in seams |
| Injection model | No | Existing container pattern |
| Target location | No | Namespace and folder for the abstraction |

## Strategy Matrix

| Dependency | Use |
|---|---|
| Time | `TimeProvider` first |
| HTTP | `IHttpClientFactory` or typed client |
| File system | `System.IO.Abstractions` or small wrapper |
| Environment | Small custom provider |
| Console | Small custom provider or `ILogger` |
| Process | Small custom provider |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent verifies the codebase lacks a suitable abstraction. | Run `rg` for the target interface or built-in seam. | No suitable seam exists. |
| 2 | Agent picks a built-in seam when one exists. | Read the strategy decision. | Built-in seam is used for time or HTTP when supported. |
| 3 | Agent writes the smallest interface and implementation that covers current calls. | Read the generated files. | Surface includes only needed members. |
| 4 | Agent writes dependency injection registration matching the repo pattern. | Read composition root files. | Registration exists in the expected container location. |
| 5 | Agent writes environment-specific implementations only when behavior differs by environment. | Read the generated files. | Extra implementations exist only for a real behavior split. |
| 6 | Agent stops before broad call-site migration. | Read changed production files. | Unrelated classes remain unchanged. |

## Design Rules

| Topic | Rule |
|---|---|
| Surface size | Agent writes only needed members |
| Built-ins | Agent uses built-in seams before custom seams |
| Naming | Agent matches sibling naming and namespace patterns |
| Scope | Agent keeps generation local to the named dependency |
| Follow-up | Agent routes bulk replacement to `migrate-static-to-wrapper` |

## Verification Checklist

- [ ] Agent verified no suitable seam already exists.
- [ ] Agent used built-in seams when available.
- [ ] Agent kept the wrapper surface small.
- [ ] Agent wrote dependency injection registration.
- [ ] Agent limited environment-specific implementations to real differences.
- [ ] Agent left broad call-site migration for the migration skill.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent generates giant utility interfaces | Agent writes only needed members. |
| Agent ignores built-in seams | Agent uses `TimeProvider` or `IHttpClientFactory` first. |
| Agent mixes wrapper generation with bulk replacement | Agent routes replacement work to `migrate-static-to-wrapper`. |
| Agent writes multiple implementations with no behavior split | Agent keeps one implementation. |
