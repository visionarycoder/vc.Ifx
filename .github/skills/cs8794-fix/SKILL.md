---
name: cs8794-fix
description: Remediates CS8794 diagnostics by removing tautological type patterns and preserving intent.
title: CS8794 Fix
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: low
estimated_tokens: 760
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - property-patterns
appliesTo: '**/*.{cs,csproj}'
tags:
  - csharp
  - diagnostics
  - cs8794
  - ste
---
# CS8794 Fix

This skill removes tautological type patterns that always match the static type.
This skill preserves behavior while simplifying pattern logic.

## Pattern Table

| Intent | Avoid | Preferred |
|---|---|---|
| Bind existing string | `if (value is string s)` when `value` is already `string` | `var s = value;` or direct use |
| Switch on known type | `value switch { string s => F(s) }` when `value` is `string` | `F(value)` |
| Guard null only | `if (value is string s)` for nullable string | `if (value is not null)` then use `value` |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Locate | Agent finds each CS8794 diagnostic site. | Diagnostic list |
| 2. Verify | Agent checks the static type and the real branch intent. | Safe rewrite plan |
| 3. Rewrite | Agent replaces tautological patterns with direct use, null checks, or meaningful patterns. | Updated source |
| 4. Validate | Agent rebuilds the affected project. | Diagnostic-free result |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Diagnostic removal | Run `dotnet build` on the affected project. | Zero CS8794 diagnostics remain in changed scope. |
| Behavior preservation | Review the rewritten branch semantics. | Logic matches the original intent. |
| Pattern necessity | Review retained patterns. | Every retained pattern performs real runtime narrowing or shape testing. |
