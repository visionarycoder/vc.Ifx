---
name: mtp-hot-reload
description: Sets up and uses Microsoft.Testing.Platform hot reload for rapid iterative test-fix loops.
license: MIT
title: MTP Hot Reload for Iterative Test Fixing
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1000
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - run-tests
  - filter-syntax
  - platform-detection
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - mtp
  - hot-reload
  - testing
  - ste
---
# MTP Hot Reload for Iterative Test Fixing

This skill sets up Microsoft.Testing.Platform hot reload for rapid local fix loops.
This skill activates only when the project already uses, or is intentionally moving to, MTP.

## Setup Matrix

| Concern | Agent action |
|---|---|
| Platform detection | Agent confirms MTP with `platform-detection` before changing commands or configuration. |
| Package setup | Agent adds only the required MTP hot-reload package or setting for the existing framework stack. |
| Filter usage | Agent reuses `filter-syntax` for runner-correct selection syntax. |
| Loop control | Agent scopes hot reload to the smallest failing test set. |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Detect | Agent confirms the project uses Microsoft.Testing.Platform. | Platform decision |
| 2. Configure | Agent applies the minimal package, environment variable, or launch profile change required for hot reload. | Ready project |
| 3. Run | Agent starts the hot-reload loop against the narrowest failing test slice. | Fast feedback loop |
| 4. Fix | Agent edits production or test code in response to live failures. | Iterative repair |
| 5. Reset | Agent documents or removes temporary setup when the setup was task-specific. | Stable repo state |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Platform fit | Compare commands to project detection. | Hot reload uses MTP only on MTP projects. |
| Narrow scope | Review the selected test filters. | The loop targets only the requested failing tests or category. |
| Iteration value | Compare loop behavior to normal reruns. | The setup reduces rebuild or rerun overhead for the active fix cycle. |
| Cleanup | Review temporary setup after the task. | Task-specific toggles do not linger unintentionally. |
