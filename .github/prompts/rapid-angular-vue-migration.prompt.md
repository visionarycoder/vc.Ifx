---
name: rapid-angular-vue-migration
title: Rapid Angular to Vue.js Migration
description: Run compressed migration of Portal Angular WebApp and Scheduler Angular WebApp into one Vue.js WebApp in seven days.
doc_type: prompt
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1190
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - technology-stack-dictionary
  - angular-anti-patterns
  - angular-to-vue3-migration
  - webapp-consolidation-strategy
related_prompts:
  - angular-vue-migration-plan.prompt
tags:
  - angular
  - vue
  - migration
  - rapid
  - ste
related_skills: []
appliesTo: '**/*'
---
# Rapid Angular to Vue.js Migration
Agent runs a seven-day migration of Portal Angular WebApp and Scheduler Angular WebApp into one Vue.js WebApp.
## Scope
| Item | Value |
|---|---|
| Sources | Portal Angular WebApp; Scheduler Angular WebApp |
| Target | Unified Vue.js WebApp |
| Duration | 7 days |
| Strategy | Parallel work for discovery and feature migration. Sequential work for cutover. |
## Day Plan
| Day | Agent action | Skills | Test | Pass |
|---|---|---|---|---|
| 1 | Agent audits both Angular WebApps, fixes P0 defects, and writes Vue.js foundation tasks. | `angular-anti-patterns`, `webapp-consolidation-strategy` | Read plan. | Plan lists both audits, P0 backlog, auth, routing, state, and shared components. |
| 2 | Agent migrates Portal Admin, Crosswalk, and Journal in parallel lanes. | `angular-to-vue3-migration` | Read plan. | All Portal features appear once. |
| 3 | Agent migrates Scheduler Runs, Calendars, and Schedules in parallel lanes. | `angular-to-vue3-migration` | Read plan. | All Scheduler features appear once. |
| 4 | Agent consolidates duplicate code, writes shared Vue.js components, and tunes bundle split. | `webapp-consolidation-strategy` | Read plan. | Plan lists shared paths and bundle tasks. |
| 5 | Agent runs tests, writes deployment workflow, and writes rollback workflow. | `vue3-e2e-testing-playwright` | Read plan. | Plan lists unit test, component test, end-to-end test, deploy, and rollback tasks. |
| 6 | Agent routes 5% traffic, monitors signals, and records rollback trigger. | `webapp-cutover-strategy` | Read plan. | Plan lists 5% and 25% rollout gates. |
| 7 | Agent routes 100% traffic, monitors signals, and archives Angular paths after the observation gate passes. | `webapp-cutover-strategy` | Read plan. | Plan lists 100% gate and archive steps. |
## Parallel Lanes
| Day | Lane A | Lane B | Lane C | Lane D |
|---|---|---|---|---|
| 1 | Portal audit | Scheduler audit | Vue.js foundation | Component inventory |
| 2 | Portal Admin | Portal Crosswalk | Portal Journal | Portal test fixes |
| 3 | Scheduler Runs | Scheduler Calendars | Scheduler Schedules | Scheduler test fixes |
| 5 | Full run tests | Deployment workflow | Rollback workflow | Smoke test fixes |
## Quality Gates
| Gate | Test | Pass |
|---|---|---|
| Security | Read backlog. | P0 defects equal 0 before Day 2 ends. |
| Feature coverage | Read feature list. | Admin, Crosswalk, Journal, Runs, Calendars, and Schedules appear once. |
| Test workflow | Read day plan. | Unit test, component test, and end-to-end test tasks appear. |
| Performance | Read rollout plan. | Load time target and bundle target appear once. |
| Rollback | Read rollback workflow. | Rollback routes traffic to Angular in one path. |
## Metrics
| Metric | Target |
|---|---|
| Feature parity | 100% |
| Lighthouse score | 90 or greater |
| Bundle size | Less than or equal to combined Angular baseline |
| Load time | Less than 3 seconds |
| Production P0 defects | 0 |
| Rollback time | Less than 5 minutes |
## Decision Rules
| Condition | Agent action |
|---|---|
| P0 defect exists | Agent fixes defect before feature migration continues. |
| End-to-end test fails | Agent fixes the failing flow before cutover starts. |
| Bundle exceeds target | Agent writes extra split or lazy-route task. |
| Error rate crosses threshold | Agent routes traffic to Angular and logs cause. |
| Auth defect appears | Agent stops rollout and routes traffic to Angular. |
## Test Matrix
| Scope | Test | Pass |
|---|---|---|
| Prompt size | Count estimated tokens. | Estimated tokens stay below 1500. |
| STE compliance | Search for banned verbs. | Zero banned-verb matches. |
| Terminology | Read prompt. | Prompt uses Angular, Vue.js, and WebApp. |
| Workflow coverage | Read tables. | Every day row contains Test and Pass. |
## Invocation
Agent uses this prompt to generate one seven-day migration workflow with compact tables and explicit gates.
