---
name: angular-vue-migration-plan
title: Generate Angular to Vue.js Migration Plan
description: Generate phased migration plan for Portal Angular WebApp and Scheduler Angular WebApp into one Vue.js WebApp.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1000
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - technology-stack-dictionary
  - angular-anti-patterns
  - angular-vue-migration-orchestrator
  - webapp-consolidation-strategy
related_prompts:
  - vbd-modernization-planning.prompt
appliesTo: '**/*.{md,ts,vue}'
tags:
  - angular
  - vue
  - migration
  - planning
  - ste
related_skills: []
---
# Generate Angular to Vue.js Migration Plan
Agent generates one phased plan for Portal Angular WebApp and Scheduler Angular WebApp consolidation into one Vue.js WebApp.
## Scope
| Item | Value |
|---|---|
| Sources | Portal Angular WebApp; Scheduler Angular WebApp |
| Target | Unified Vue.js WebApp |
| Goal | Feature parity on one shared foundation |
| Output | One migration plan with phases, gates, tests, and cutover workflow |
## Workflow
| Step | Agent action | Skill | Test | Pass |
|---|---|---|---|---|
| 1 | Agent audits both Angular WebApps for security, memory, performance, and maintainability defects. | `angular-anti-patterns` | Read plan. | Plan contains one audit table per WebApp. |
| 2 | Agent generates target Vue.js WebApp routes, layers, shared state, and shared components. | `webapp-consolidation-strategy` | Read plan. | Plan lists routes, layers, state, and shared components. |
| 3 | Agent lists blocking Angular cleanup tasks in priority order. | `angular-anti-patterns` | Read plan. | Plan lists all P0 tasks before all P1 tasks. |
| 4 | Agent lists Vue.js foundation tasks for auth, routing, state, components, and test tooling. | `angular-vue-migration-orchestrator` | Read plan. | Plan lists each foundation area once. |
| 5 | Agent routes features into migration waves with dependencies and feature flags. | `angular-to-vue3-migration` | Read plan. | Every source feature appears once. |
| 6 | Agent generates rollout, monitoring, and rollback workflow. | `webapp-cutover-strategy` | Read plan. | Plan defines 5%, 25%, and 100% rollout stages. |
## Feature Wave Order
| Wave | Source WebApp | Feature | Entry gate | Exit gate |
|---|---|---|---|---|
| 1 | Portal | Admin | P0 and P1 fixes pass. | Routes, state, and run tests pass. |
| 2 | Portal | Crosswalk | Wave 1 pass. | Feature parity and run tests pass. |
| 3 | Portal | Journal | Wave 2 pass. | Form workflow and run tests pass. |
| 4 | Scheduler | Runs | Wave 3 pass. | Read workflow and run tests pass. |
| 5 | Scheduler | Calendars | Wave 4 pass. | Calendar workflow and run tests pass. |
| 6 | Scheduler | Schedules | Wave 5 pass. | High-risk workflow and run tests pass. |
## Decision Rules
| Condition | Agent action |
|---|---|
| P0 defect exists | Agent fixes or schedules fix before Vue.js migration starts. |
| Shared contract changes | Agent lists backward-compatibility task. |
| Duplicate component exists | Agent routes both features to one shared Vue.js component. |
| Rollback signal crosses threshold | Agent routes traffic to Angular and logs cause. |
## Test Matrix
| Scope | Test | Pass |
|---|---|---|
| Coverage | Read plan. | Portal and Scheduler features appear once and only once. |
| Foundation | Read plan. | Auth, routing, state, components, and test tooling appear once. |
| Cutover | Read plan. | Rollout signals, thresholds, and rollback trigger appear once. |
| STE compliance | Search for banned verbs. | Zero banned-verb matches. |
| Token budget | Count estimated tokens. | Estimated tokens stay below 1500. |
## Invocation
Agent generates one compact migration plan with tables, explicit gates, and measurable pass criteria.
