---
name: frontend-module-consolidation
title: Frontend Module Consolidation
description: Consolidate web frontends into bounded feature modules with lazy routes, public APIs, shared neutral infrastructure, and explicit cross-module communication rules.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1185
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - angular-to-vue3-migration
  - vue3-pinia-state-management
  - webapp-cutover-strategy
appliesTo: '**/*.{vue,ts,js,json}'
tags:
  - frontend
  - module
  - consolidation
---
# Frontend Module Consolidation

Agent merges frontend capabilities into one application shell without creating a coupled monolith by enforcing feature boundaries, lazy routes, and narrow public APIs.

## When to Use

| Condition | Use |
|---|---|
| Agent combines Portal and Scheduler frontends into one shell. | Agent uses this skill. |
| Agent migrates feature slices into a modular Vue application. | Agent uses this skill. |
| Agent needs rules for shared code, route chunks, or cross-feature communication. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent extracts a shared component library only. | Agent uses `vue3-component-library-design`. |
| Agent reorganizes backend modules instead of frontend features. | Agent uses `webapi-module-consolidation`. |
| Agent requires separate products with independent security or release boundaries. | Agent keeps separate applications. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Current feature inventory | Yes | Inventory identifies routes, stores, services, and ownership. |
| Target module map | Yes | Map identifies feature roots and shared infrastructure. |
| Shared dependency rules | Yes | Rules identify core and shared owners. |
| Communication needs | No | Needs identify props, stores, events, or navigation seams. |
| Bundle constraints | No | Constraints identify lazy loading and budget goals. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Inventory by capability | Agent groups current code by business capability instead of technical type. | Capability inventory | Agent reviews the inventory. | Each capability has one target module. |
| 2. Define public APIs | Agent exposes only routes, components, types, or helpers that other modules legitimately consume. | Public API map | Agent reviews module exports. | No feature depends on another feature's internal paths. |
| 3. Define lazy route seams | Agent makes route-level chunks the default integration seam for features. | Route-chunk plan | Agent inspects route definitions. | Heavy features load dynamically and independently. |
| 4. Separate shared from reused | Agent promotes code into `shared` only when the code is neutral, stable, and multi-consumer. | Shared-code plan | Agent reviews shared candidates. | Shared code has no hidden business policy. |
| 5. Define communication rules | Agent chooses props, composables, stores, navigation, or typed events based on the narrowest valid coupling. | Communication matrix | Agent reviews cross-module flows. | Cross-module dependencies stay explicit and typed. |
| 6. Enforce boundaries and budgets | Agent adds or verifies automated checks for restricted imports and route-chunk behavior. | Enforcement plan | Agent runs existing lint or build paths. | Boundary rules and lazy chunks remain observable. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Boundary verification | Existing lint path or restricted-import check | No forbidden feature-internal imports remain |
| Build verification | Existing web build path | Route chunks and public exports build successfully |
| Feature verification | Existing tests or targeted parity checks | Moved features load and behave correctly |

## Verification Checklist

- [ ] Agent assigns one owner to each feature module.
- [ ] Agent exposes cross-module access only through public APIs.
- [ ] Agent keeps shared code business-neutral.
- [ ] Agent lazy-loads feature route chunks.
- [ ] Agent documents explicit cross-module communication rules.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent turns `shared` into a dumping ground. | Agent promotes code only after neutral, stable, multi-consumer review. |
| Agent imports another feature's internal files directly. | Agent imports through the target feature public API. |
| Agent keeps all domain state in one global store. | Agent keeps state with feature ownership and lifts only true cross-app state. |
| Agent eager-loads every route. | Agent uses dynamic route imports and inspects the build output. |
