---
name: property-patterns
description: Applies MSBuild property patterns for defaults, appends, quoting, paths, and evaluation-order safety.
license: MIT
title: MSBuild Property Patterns
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1160
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - item-management
  - directory-build-organization
appliesTo: '**/*.{csproj,props,targets}'
tags:
  - msbuild
  - properties
  - patterns
  - ste
---
# MSBuild Property Patterns

This skill applies safe MSBuild property-definition patterns.
This skill keeps overrides possible and evaluation order intentional.

## Pattern Table

| Intent | Preferred pattern | Avoid |
|---|---|---|
| Default value | `Condition="'$(Prop)' == ''"` | Unconditional overwrite of consumer-set values |
| Append value | `$(Existing);NewValue` with empty-safe composition | Replacing composite values such as `NoWarn` or `DefineConstants` |
| Condition quoting | Quote both sides of string comparisons | Bare property comparisons that break on empty values |
| Paths | Normalize and use canonical MSBuild properties | Hardcoded relative `obj\` or machine paths |
| TFM-aware logic | Use the right evaluation surface for the chosen property timing | Assuming `$(TargetFramework)` is populated everywhere |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Inspect | Agent identifies where each property is defined and overwritten. | Evaluation map |
| 2. Normalize | Agent rewrites defaults, appends, and path values to the safe pattern. | Stable property set |
| 3. Scope | Agent moves timing-sensitive logic when the current file evaluates too early or too late. | Correct evaluation surface |
| 4. Validate | Agent builds with default settings and with an override scenario. | Verified property behavior |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Override safety | Build once with defaults and once with an explicit override. | Consumer overrides still win when intended. |
| Composition | Review composite properties such as `NoWarn` or `DefineConstants`. | New values append without erasing prior values. |
| Path hygiene | Review changed path properties. | Paths use canonical properties and valid quoting. |
| Evaluation timing | Review file placement and conditions. | Timing-sensitive logic executes where the needed properties are available. |
