---
name: validation-mvvm
title: MVVM Validation Patterns
description: Apply MVVM validation patterns for observable view models, async rules, UI error presentation, and .NET 10 validation flows in desktop applications.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1435
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - wpf-mvvm-implementation
  - dotnet-validation-standards
  - writing-mstest-tests
appliesTo: '**/*.{cs,csproj,xaml}'
tags:
  - mvvm
  - validation
  - wpf
  - communitytoolkit
  - fluentvalidation
---
# MVVM Validation Patterns

Agent implements MVVM validation with observable state, deterministic rule ownership, and UI feedback that stays testable and binding-friendly.

## When to Use

| Condition | Use |
|---|---|
| Work adds editable fields, save flows, or form state to an MVVM screen | Use this skill |
| Work needs `INotifyDataErrorInfo`, `ObservableValidator`, or DataAnnotations in a view model | Use this skill |
| Work integrates FluentValidation with property-change or submit-time workflows | Use this skill |
| Work needs cross-property, async, or form-level validation state | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work validates HTTP requests, DTOs, or Web API contracts | Use `dotnet-validation-standards` |
| Work changes view layout only with no rule or state changes | Edit the view directly |
| Work enforces domain invariants inside entities or value objects | Keep rules in domain code |
| Work adds UI automation for rendering-only defects | Use a UI-focused testing skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Target view model | Yes | Identify editable state and command surface. |
| Validation scope | Yes | Identify property, cross-property, async, and form-level rules. |
| UI feedback path | Yes | Identify inline messages, summary areas, and error templates. |
| Validation trigger | No | Identify property-change, submit-time, or hybrid execution. |
| External lookup rule | No | Identify async uniqueness or availability checks. |

## Pattern Matrix

| Scenario | Preferred Pattern | Guardrail | Pass Target |
|---|---|---|---|
| Property rule | `ValidationAttribute`, DataAnnotations, or `ValidateProperty` | Keep one deterministic message source | Invalid value reports one stable property error |
| Cross-property rule | Revalidate dependent members on each contributing change | Keep related properties synchronized | Combined state reports and clears correctly |
| Async rule | Track pending state, cancellation, and stale-result rejection | Latest request owns the result | Overlapping checks settle on the newest value |
| On-change workflow | Validate after setter completion | Avoid expensive remote checks on every keystroke unless intentional | UI feedback appears during edit flow |
| Submit-time workflow | Run `ValidateAllProperties` or equivalent before the command side effect | Block mutation on invalid state | Invalid form blocks submit |
| Hybrid workflow | Use light on-change checks and full submit-time aggregation | Keep one shared error store | Screen stays responsive and final gate stays complete |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent maps each rule to property, cross-property, async, or form scope. | Review the rule map. | Each rule has one owner. |
| 2 | Agent selects one validation engine per view model path. | Review the changed view model and registrations. | Validation flow stays consistent inside the screen. |
| 3 | Agent wires validation triggers to property change, submit-time, or both. | Exercise field edits and submit actions. | Trigger timing matches the workflow. |
| 4 | Agent aggregates errors through `INotifyDataErrorInfo`. | Bind errors and inspect command readiness. | UI and commands read one shared validation state. |
| 5 | Agent adds async validation state for in-flight checks and stale-result protection when external lookup rules exist. | Force delayed overlapping requests. | Busy and error state stay deterministic. |
| 6 | Agent verifies inline presentation, templates, summaries, and command gating. | Run the screen or binding tests. | Users see property and form-level errors. |

## Rules

| Topic | Rule |
|---|---|
| Rule ownership | Keep one primary validation engine or source per screen path. |
| Aggregation | Keep `HasErrors`, summaries, and command enablement tied to the same error store. |
| Async safety | Track request ownership and ignore stale completions. |
| Readability | Surface readable property and summary messages, not color-only feedback. |
| Domain boundaries | Keep boundary validation in the view model and invariants in domain code. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Rule ownership | Review changed files. | Each validation rule has one owner. |
| Property notifications | Edit invalid and valid values. | Errors appear and clear after the intended trigger. |
| Cross-property coverage | Change each dependent property. | Related errors refresh on every dependency change. |
| Async safety | Run overlapping lookup tests. | Stale responses do not overwrite current state. |
| Form aggregation | Trigger full-form validation. | `HasErrors`, summaries, and commands stay synchronized. |
| UI presentation | Run the screen or binding checks. | Inline and form-level errors remain visible. |

## Outputs

- Validation design map with rule ownership and triggers
- View-model validation implementation plan
- UI binding guidance for inline errors and summaries
- Targeted tests for property, cross-property, and async validation behavior
