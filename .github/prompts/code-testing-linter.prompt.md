---
mode: agent
title: Linter Agent
description: Run scoped format or lint fix commands and report results.
doc_type: prompt
status: active
last_invoked_by: code-testing-implementer
last_updated: 2026-07-29
target_audience: ai
complexity: low
estimated_tokens: 650
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
# Linter Agent

Agent runs scoped format or lint fix commands.

## Command Discovery

| Input State | Agent Action | Test | Pass |
|---|---|---|---|
| Command provided | Agent uses the provided command. | Read input. | One lint command exists. |
| `.testagent/research.md` exists | Agent reads the Commands section. | Read `.testagent/research.md`. | One lint command exists. |
| `.testagent/plan.md` exists | Agent reads the Commands section. | Read `.testagent/plan.md`. | One lint command exists. |
| No command exists | Agent maps project files to a fix command. | Read project files. | One scoped fix command exists. |

## Command Map

| Marker | Fix Command | Test | Pass |
|---|---|---|---|
| `*.csproj` or `*.sln` | `dotnet format --include <path>` | Run command. | Exit code = 0. |
| `package.json` | `npm run lint:fix` or `npm run format` | Run command. | Exit code = 0. |
| `pyproject.toml` | `black <path>` or `ruff format <path>` | Run command. | Exit code = 0. |
| `go.mod` | `go fmt ./...` | Run command. | Exit code = 0. |
| `Cargo.toml` | `cargo fmt` | Run command. | Exit code = 0. |
| `.prettierrc` | `npx prettier --write <path>` | Run command. | Exit code = 0. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects a fix command, not a verify-only command. | Read command. | Command writes fixes. |
| 2 | Agent runs the smallest scoped command. | Run command. | Command starts and returns output. |
| 3 | Agent reports changed files or fail details. | Read report. | Report includes command and outcome. |

## Report Format

**Pass**

```text
LINT: PASS
Command: <cmd>
Changes: <files> or No changes
```

**Fail**

```text
LINT: FAIL
Command: <cmd>
Error: <message>
```

## Rules

| Rule | Test | Pass |
|---|---|---|
| Agent uses fix commands only. | Read command. | Command writes changes. |
| Agent scopes the command to changed files when tooling supports it. | Read command. | Command target excludes unrelated files. |
| Agent reports only actual errors. | Read pass report. | Pass report excludes warning text. |
