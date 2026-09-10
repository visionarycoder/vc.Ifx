---
title: Test Gap Analysis Mutation Catalog
description: Extended pseudo-mutation examples for the test-gap-analysis skill.
doc_type: reference
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 376
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - test-gap-analysis
appliesTo: '**/*.md'
tags:
  - mutation
  - testing
  - references
---
# Test Gap Analysis Mutation Catalog

Agent uses this file for extended mutation examples.

## Boundary Examples

| Original | Mutation |
|---|---|
| `<` | `<=` |
| `>` | `>=` |
| `== 0` | `<= 0` |
| `index + 1` | `index` |

## Logic Examples

| Original | Mutation |
|---|---|
| `&&` | `||` |
| `||` | `&&` |
| `!condition` | `condition` |
| `if (x)` | `if (!x)` |

## Return Examples

| Original | Mutation |
|---|---|
| `return result` | `return null` |
| `return result` | `return default` |
| `return true` | `return false` |
| `return list` | `return []` |

## Exception Examples

| Original | Mutation |
|---|---|
| Guard clause throws | Remove the throw |
| `if (x == null) throw` | Remove the guard |
| `return err` | Swallow the error |

## Arithmetic Examples

| Original | Mutation |
|---|---|
| `a + b` | `a - b` |
| `a * b` | `a / b` |
| `x++` | `x--` |
| `-value` | `value` |

## Null Examples

| Original | Mutation |
|---|---|
| `x ?? fallback` | `x` |
| `x?.Method()` | `x.Method()` |
| `if (x == null) return` | Remove the guard |
| `unwrap_or` | `unwrap` |
