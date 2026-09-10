---
name: angular-vue-migration-orchestrator
title: Angular to Vue Migration Orchestrator
description: Coordinate Angular cleanup, Vue foundation work, feature migration, validation, and cutover with one program-level workflow.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1360
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - angular-anti-patterns
  - angular-to-vue3-migration
  - vue3-webapp-setup
  - webapp-cutover-strategy
related_skills:
  - vue3-component-library-design
  - vue3-e2e-testing-playwright
  - vue3-msal-authentication
  - vue3-pinia-state-management
appliesTo: '**/*.{ts,vue,html,json}'
tags:
  - angular
  - vue
  - migration
  - orchestration
---
# Angular to Vue Migration Orchestrator

Agent coordinates large Angular to Vue programs through one verified plan.

## When to Use

| Condition | Route |
|---|---|
| Many Angular features move to one Vue target | Agent uses this skill |
| One component or one feature needs direct porting | Agent uses `angular-to-vue3-migration` |
| Team needs only Vue setup | Agent uses `vue3-webapp-setup` |
| Team needs only Angular cleanup | Agent uses `angular-anti-patterns` |

## Program Workflow

| Phase | Agent action | Test | Pass |
|---|---|---|---|
| 0. Discover | Agent inventories apps, routes, dependencies, and risk | Review inventory | Each app and feature is listed |
| 1. Clean Angular | Agent fixes security and leak blockers | Run Angular tests | P0 and P1 blockers are closed |
| 2. Build Vue base | Agent creates routing, state, auth, and shared UI foundations | Run Vue setup validation | Foundation build and tests pass |
| 3. Migrate features | Agent ports features in priority order | Run parity suite per feature | Each migrated feature passes parity |
| 4. Consolidate | Agent removes duplicate code and standardizes boundaries | Review dependency graph | Shared code lives in one location |
| 5. Cut over | Agent rolls traffic to Vue and records rollback gates | Run rollout checks | Target metrics stay within threshold |

## Feature Order Matrix

| Feature type | Priority | Test | Pass |
|---|---|---|---|
| Static page | 1 | Route smoke test | Route content matches baseline |
| Read-only view | 2 | API and render test | Data and empty state match baseline |
| Form workflow | 3 | Submit invalid and valid data | Messages and save result match baseline |
| Multi-step workflow | 4 | End-to-end regression | All steps complete |

## Program Validation

| Check | Test | Pass |
|---|---|---|
| Inventory completeness | Review migration board | Each feature has owner and target phase |
| Angular baseline | Run Angular build and tests | Exit code = 0 |
| Vue baseline | Run Vue build and tests | Exit code = 0 |
| Feature parity | Run Playwright parity suite | Critical journeys pass |
| Duplication control | Review shared module map | Duplicate shared modules are removed |
| Cutover readiness | Review rollout checklist | Rollback trigger and owner exist |

## Output

Agent returns the phase plan, feature order, validation status, and active blockers.

