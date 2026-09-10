---
name: configuration-options-pattern
title: Configuration Options Pattern
description: Use the .NET options pattern for typed configuration binding, verification, named options, and reload-aware consumers.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1189
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-app-configuration-dotnet
  - dependency-injection-patterns
  - webjobs-authoring
appliesTo: '**/*.{cs,json}'
tags:
  - options
  - configuration
  - dotnet
  - named-options
  - reload
---

# Configuration Options Pattern

Agent models configuration as typed options with explicit verification and reload semantics.

## When to Use

| Condition | Use |
|---|---|
| Agent binds structured configuration into POCOs | Use this skill |
| Agent adds startup verification for required settings | Use this skill |
| Agent manages multiple instances of same options shape | Use this skill |
| Agent integrates Azure App Configuration refresh | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Setting is one trivial scalar with no reuse | Keep local scalar read |
| Secret stays in dedicated secret store and no typed object adds value | Use secret-specific pattern |
| Consumer cannot tolerate runtime reload | Use non-reload options path |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Section name | Yes | Bound configuration section |
| Options type | Yes | Typed settings class |
| Verification rules | Yes | Required fields, URI rules, ranges, or consistency rules |
| Reload scope | No | Static, per-request, or live-reload behavior |
| External sources | No | App Configuration, Key Vault, env vars, or JSON |

## Workflow

1. Agent groups configuration into one coherent options type per concern.
2. Agent binds through the options pipeline and adds startup verification.
3. Agent selects `IOptions`, `IOptionsSnapshot`, or `IOptionsMonitor` by reload scope.
4. Agent verifies named options, post-configuration, and live-reload behavior.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero ad hoc string-key reads remain in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| OPT-001 | Options type models one coherent concern | One giant settings class mixes unrelated sections | Split by concern |
| OPT-002 | Binding uses options pipeline | Consumers read raw string keys repeatedly | Bind once through `AddOptions` |
| OPT-003 | Required settings use startup verification | Invalid URI or missing value fails at runtime only | Add verification and `ValidateOnStart()` |
| OPT-004 | Consumer uses correct options interface | Live-reload requirement uses `IOptions` only | Use `IOptionsSnapshot` or `IOptionsMonitor` |
| OPT-005 | Named options cover repeated shape | Multiple near-identical POCOs exist | Use named options |
| OPT-006 | Derived defaults run once | Consumers clamp or normalize values repeatedly | Use post-configuration |
| OPT-007 | Secrets stay outside code | Secret defaults or inline secrets appear in options class or JSON sample | Move secret to secure source |
| OPT-008 | External refresh aligns with consumer behavior | App Configuration refresh exists and consumers cache stale values | Use monitor-aware consumers |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| Static app setting | `IOptions<T>` | Raw string lookups everywhere |
| Per-request web scope | `IOptionsSnapshot<T>` | Global mutable static field |
| Live refresh service | `IOptionsMonitor<T>` | `IOptions<T>` with manual polling |
| Multiple partner endpoints | Named options | One POCO type per partner only |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Scope verification | Search changed files for repeated `configuration["..."]` lookups in consumers | Zero unauthorized matches |
| Behavior verification | Run `dotnet test [test-project].csproj --filter Options` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] Options types model one coherent concern
- [ ] Binding and verification are centralized
- [ ] Consumer interface matches reload scope
- [ ] Named options cover repeated shapes
- [ ] Secrets stay outside source code

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Giant settings class mixes concerns | Split options type |
| Consumer reads raw string keys repeatedly | Inject typed options |
| Reload requirement uses `IOptions<T>` | Switch to monitor-aware interface |
| Secret default sits in code or JSON | Move secret to secure source |
