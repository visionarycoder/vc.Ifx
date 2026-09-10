---
name: vue3-component-library-design
title: Vue 3 Component Library Design
description: >
  Build reusable Vue 3 components with typed contracts, accessibility, tests, 
  and stable ownership boundaries.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: medium
estimated_tokens: 1170
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - vue3-webapp-setup
related_skills:
  - fluent2-design-comprehensive
  - vue3-e2e-testing-playwright
  - vue3-pinia-state-management
  - angular-to-vue3-migration
related_docs:
  - references/fluent2-implementation-patterns.md
appliesTo: '**/*.{vue,ts}'
tags:
  - vue3
  - fluent
  - fluent2
  - component
  - library
  - design
---
# Vue 3 Component Library Design

Agent builds reusable Vue 3 components with stable public contracts.

## When to Use

| Condition | Route |
|---|---|
| Team needs shared UI across features | Agent uses this skill |
| Team migrates duplicate Angular UI to one Vue library | Agent uses this skill |
| One page needs route or state setup | Agent uses `vue3-webapp-setup` or `vue3-pinia-state-management` |

## Component Contract Matrix

| Contract area | Agent action | Test | Pass |
|---|---|---|---|
| Props | Agent defines typed props with defaults | Run component tests | Invalid and valid props behave as defined |
| Events | Agent defines typed emits | Trigger events in tests | Payload shape matches contract |
| Slots | Agent documents required and optional slots | Mount slot scenarios | Render output stays stable |
| Accessibility | Agent adds roles, labels, and keyboard support | Run accessibility checks | No new violation appears |
| Styling | Agent scopes component styles and tokens | Review rendered variants | Variants stay isolated |
| Packaging | Agent exports components through stable barrels | Run type-check and build | Import paths resolve |

## Migration Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Audit | Agent lists duplicate or fragile components | Review inventory | Each candidate has owner and replacement plan |
| 2. Define primitives | Agent creates base controls and layout parts | Run unit tests | Base components compile and render |
| 3. Add accessibility | Agent adds keyboard and ARIA behavior | Run accessibility suite | Focus and labels work |
| 4. Add wrappers | Agent builds feature-facing wrappers on top of primitives | Run feature tests | Wrappers keep product behavior |
| 5. Publish exports | Agent updates barrel files and docs | Run `npm run type-check` | Imports resolve from shared entry points |
| 6. Remove duplicates | Agent replaces old copies with shared components | Run regression tests | No feature loses behavior |

## Validation

| Check | Test | Pass |
|---|---|---|
| Component tests | Run `npm run test` | Exit code = 0 |
| Type health | Run `npm run type-check` | Exit code = 0 |
| Accessibility | Run approved accessibility checks | No new violation appears |
| Variant stability | Review primary variants | Each documented variant renders |
| Export stability | Build a consumer import path | Shared barrel exports resolve |

## Output

Agent returns the shared component list, replaced duplicates, and validation results.

