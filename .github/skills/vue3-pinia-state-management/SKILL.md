---
name: vue3-pinia-state-management
title: Vue 3 Pinia State Management
description: Design small typed Pinia stores with explicit actions, async state, persistence limits, and RxJS migration guidance.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1270
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - vue3-webapp-setup
related_skills:
  - vue3-component-library-design
  - vue3-e2e-testing-playwright
  - angular-to-vue3-migration
appliesTo: '**/*.{vue,ts,js}'
tags:
  - vue3
  - pinia
  - state
  - migration
---
# Vue 3 Pinia State Management

Agent creates predictable store boundaries for Vue 3 applications.

## When to Use

| Condition | Route |
|---|---|
| State crosses route or component boundaries | Agent uses this skill |
| Angular `BehaviorSubject` services move to Vue | Agent uses this skill |
| State lives in one component only | Agent uses local `ref` or `reactive` |
| Query cache owns server-state behavior | Agent uses the project query pattern |

## Store Matrix

| Area | Agent action | Test | Pass |
|---|---|---|---|
| Ownership | Agent assigns one feature or core owner per store | Review store path | Store lives under correct module |
| State model | Agent uses setup stores with typed refs and getters | Run unit tests | State and getters match test inputs |
| Async actions | Agent models loading, success, and error state | Run action tests | Each state transition is observable |
| Persistence | Agent stores allowlisted preferences only | Search storage writes | No sensitive record is persisted |
| Composition | Agent avoids cyclic store imports | Run build and tests | Imports resolve without cycle failure |
| Schedule data | Agent preserves cron and calendar-event definitions | Run schedule tests | Both schedule types round-trip |

## RxJS Migration Map

| Angular source | Pinia target | Test | Pass |
|---|---|---|---|
| `BehaviorSubject<T>` | `ref<T>` | Run state tests | Consumers update on write |
| `combineLatest` | `computed` | Run getter tests | Derived value matches inputs |
| `subject.next(value)` | named action | Call action | State changes once per action |
| `switchMap` | async action with abort or request id | Run concurrency test | Old response does not overwrite new data |

## Migration Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Place stores | Agent separates core and feature stores | Review file paths | Store scope matches feature scope |
| 2. Model state | Agent adds typed refs, getters, and reset paths | Run unit tests | Reset returns store to baseline |
| 3. Add async flow | Agent injects typed clients into actions | Run action tests | Success and failure states appear |
| 4. Add persistence | Agent stores allowlisted preferences with schema checks | Load stored data in tests | Invalid schema is rejected |
| 5. Port RxJS | Agent maps Angular streams to Pinia patterns | Run migration tests | Store behavior matches source service |
| 6. Verify boundaries | Agent reviews imports and sensitive fields | Search store code | No token or secret stays in store state |

## Validation

| Check | Test | Pass |
|---|---|---|
| Unit health | Run `npm run test` | Exit code = 0 |
| Type health | Run `npm run type-check` | Exit code = 0 |
| Sensitive storage | Search `localStorage|sessionStorage` | Only allowlisted preferences persist |
| Reset behavior | Run logout or reset tests | State returns to baseline |
| Schedule coverage | Run schedule-focused tests | Cron and calendar-event tests pass |

## Output

Agent returns store ownership, migration mappings, persistence scope, and test results.


