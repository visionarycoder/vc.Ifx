---
name: roslyn-codefix-authoring
title: Roslyn Code Fix Authoring
description: Author Roslyn code fixes with deterministic edits, safe fix registration, C# 8-safe implementation, and MSTest verification for src/ifx projects.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 780
appliesTo: '**/*.{cs,csproj,props,targets}'
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - roslyn-analyzer-authoring
  - ca-code-quality-fixes
  - mcp-csharp-test
tags:
  - roslyn
  - codefix
  - analyzers
  - ifx
---
# Roslyn Code Fix Authoring

Agent authors `CodeFixProvider` implementations for repository analyzers. Agent keeps fixes deterministic, narrow, and safe for batch application.

## When to Use

| User prompt | Use |
|---|---|
| User asks for a one-click remediation for an analyzer | Use this skill |
| User asks for Fix All support | Use this skill |
| User asks for code-fix tests with exact before and after output | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for a diagnostic with no automatic edit | Use `roslyn-analyzer-authoring` |
| User asks for generated files at compile time | Use `roslyn-source-generator-authoring` |
| User asks for a broad refactor with human choice | Use a refactor workflow |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Diagnostic IDs | Yes | List every handled ID. |
| Remediation shape | Yes | State the exact edit. |
| Safety limits | Yes | List no-fix cases and semantic preconditions. |
| Fix All expectation | Recommended | State whether batch edits compose safely. |
| Alternate titles | No | Use when multiple valid edits exist. |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps each diagnostic to one deterministic edit. | Review rule-to-fix notes. | Each diagnostic has one clear remediation or one explicit no-fix rule. |
| 2 | Agent declares stable fixable IDs and equivalence keys. | Inspect provider shell. | IDs and titles are stable. |
| 3 | Agent registers code actions without heavy edits. | Inspect `RegisterCodeFixesAsync`. | Registration work stays cheap. |
| 4 | Agent applies structured edits with Roslyn editors or focused syntax replacement. | Inspect fix method. | Edit logic preserves trivia and compiles. |
| 5 | Agent enables Fix All only for composable edits. | Inspect `GetFixAllProvider`. | Batch support uses `BatchFixer` only when repeated application stays safe. |
| 6 | Agent adds MSTest coverage. | Run code-fix tests. | Positive, no-fix, and fixed-output tests pass. |

## Pattern Matrix

| Concern | Preferred pattern |
|---|---|
| Provider export | `ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(...))` |
| Edit engine | `DocumentEditor` or `SyntaxGenerator` for structured changes |
| Title stability | One clear title per action |
| Equivalence key | One stable key per code action family |
| Ambiguous cases | Return no fix |
| Batch edits | Use only for independent repeated edits |

## Test Matrix

| Test | Agent verifies |
|---|---|
| Positive fix | Diagnostic triggers and the fixed code matches exactly. |
| No-fix case | Invalid preconditions produce no action. |
| Already-fixed case | Compliant code stays unchanged. |
| Fix All case | Batch application produces deterministic output. |
| C# 8 case | `src/ifx` project code compiles without newer language syntax. |

## Validation Checklist

- [ ] Agent used C# 8-safe syntax in `src/ifx` projects.
- [ ] Agent kept `RegisterCodeFixesAsync` lightweight.
- [ ] Agent used stable titles and equivalence keys.
- [ ] Agent skipped ambiguous or risky edits.
- [ ] Agent enabled Fix All only for composable changes.
- [ ] Agent added exact fixed-output MSTest cases.

## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Agent offers a fix for a non-deterministic diagnostic | Agent narrows the diagnostic or removes the fix. |
| Agent rewrites too much syntax | Agent edits the smallest valid node set. |
| Agent ignores trivia or formatting | Agent preserves trivia and runs formatter-aware APIs when needed. |
| Agent enables Fix All by habit | Agent proves batch safety first. |
