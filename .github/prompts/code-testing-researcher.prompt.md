---
mode: agent
title: Test Researcher
description: Analyze the codebase and write `.testagent/research.md` with commands, structure, coverage estimates, and priorities.
doc_type: prompt
status: active
last_invoked_by: code-testing-generator
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1250
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
# Test Researcher

Agent writes `.testagent/research.md` for the requested scope.

## Discovery Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent reads the matching language extension. | Read workflow log. | One matching extension file was read. |
| 2 | Agent reads project structure markers. | Read file inventory. | Project files, test files, and config files are listed. |
| 3 | Agent identifies language, framework, and test framework. | Read overview data. | Language and framework fields are populated. |
| 4 | Agent limits research to user scope or full workspace. | Compare scope and file list. | Listed files stay in scope. |
| 5 | Agent reads source files and dependency relationships. | Read dependency notes. | Public symbols and dependencies are recorded. |
| 6 | Agent records scoped and harness-equivalent commands. | Read commands section. | Two test commands exist. |
| 7 | Agent estimates coverage by source file. | Read coverage table. | Every scoped source file has a coverage state. |
| 8 | Agent writes `.testagent/research.md`. | Read output file. | File contains all required sections. |

## Marker Map

| Marker | Agent Action | Test | Pass |
|---|---|---|---|
| `*.csproj` or `*.sln` | Agent records .NET project structure and test package references. | Read overview data. | Project and framework names exist. |
| `package.json` | Agent records JS or TS dependencies and test scripts. | Read overview data. | Package manager and test runner exist. |
| `pyproject.toml` or `pytest.ini` | Agent records Python packaging and test runner. | Read overview data. | Python test runner exists. |
| `go.mod` | Agent records Go package scope and `*_test.go` patterns. | Read overview data. | Go package scope exists. |
| `Cargo.toml` | Agent records Rust crate scope and test layout. | Read overview data. | Rust test layout exists. |
| `pom.xml` or `build.gradle*` | Agent records Java or Kotlin build and test runner. | Read overview data. | Build tool and runner exist. |
| `Gemfile` | Agent records Ruby runner and test layout. | Read overview data. | Ruby runner exists. |
| `Package.swift` | Agent records Swift package and XCTest layout. | Read overview data. | Swift package data exists. |
| `*.Tests.ps1` | Agent records Pester layout. | Read overview data. | PowerShell test layout exists. |
| `CMakeLists.txt` | Agent records C++ build and test runner. | Read overview data. | C++ test runner exists. |

## Output Contract

| Section | Required Content | Test | Pass |
|---|---|---|---|
| Project Overview | Path, language, framework, test framework | Read section. | All four fields exist. |
| Dependency Graph | Leaf, mid-layer, top-layer groups | Read section. | All groups exist or a reason exists. |
| Build and Test Commands | Build, scoped test, harness-equivalent test, lint | Read section. | All command slots exist or a reason exists. |
| Project Structure | Source paths and test paths | Read section. | Both path groups exist. |
| Files to Test | Priority table with API, testability, coverage, notes | Read section. | Every scoped source file appears once. |
| Existing Tests and Coverage | Source-to-test mapping | Read section. | Every mapped test file names a source file. |
| Recommendations | Priority order and blockers | Read section. | Order and blockers both exist. |

## Coverage States

| State | Test | Pass |
|---|---|---|
| Untested | Search mapped tests. | No matching test file exists. |
| Partial | Read mapped tests. | Some public behavior lacks coverage. |
| Well-tested | Read mapped tests. | Main behavior, boundary values, and failures are covered. |
