---
name: webapp-consolidation-strategy
title: Web Application Consolidation Strategy
description: Plan consolidation of multiple web applications into one modular web app with shared infrastructure, bounded feature modules, staged migration, and measurable cutover readiness.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1296
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - frontend-module-consolidation
  - webapp-cutover-strategy
  - vue3-pinia-state-management
  - vue3-msal-authentication
appliesTo: '**/*.{vue,ts,json,yml,bicep}'
tags:
  - consolidation
  - architecture
  - module-design
  - portal
  - scheduler
---
# Web Application Consolidation Strategy

Agent consolidates multiple web applications into one modular web app by defining shared infrastructure, bounded feature ownership, phased migration, and operationally safe release criteria.

## When to Use

| Condition | Use |
|---|---|
| Agent merges Portal and Scheduler into one application shell. | Agent uses this skill. |
| Agent needs a phased plan that preserves feature isolation during migration. | Agent uses this skill. |
| Agent evaluates shared infrastructure, deployment shape, and cutover readiness together. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent migrates one application without consolidating multiple application boundaries. | Agent uses `angular-vue-migration-orchestrator`. |
| Agent reorganizes backend modules instead of the web client. | Agent uses `webapi-module-consolidation`. |
| Agent extracts only shared UI components. | Agent uses `vue3-component-library-design`. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Source applications | Yes | Sources identify the current apps, routes, and feature ownership. |
| Shared infrastructure targets | Yes | Targets identify auth, API, config, telemetry, and design-system ownership. |
| Release constraints | Yes | Constraints identify deployment, rollback, and environment boundaries. |
| Migration order | No | Order identifies Portal-first, Scheduler-first, or parallel slice sequencing. |
| Success measures | Yes | Measures identify parity, performance, and maintainability targets. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Baseline the current applications | Agent inventories routes, features, shared dependencies, and release boundaries across the source apps. | Consolidation inventory | Agent reviews the inventory. | Each source feature has one identified owner and migration target. |
| 2. Define the target module map | Agent groups capabilities into bounded feature modules and one shared core. | Target architecture | Agent reviews the module map. | Every feature imports through an allowed boundary only. |
| 3. Define shared infrastructure | Agent assigns ownership for auth, API transport, config, telemetry, and shared UI. | Shared-infrastructure plan | Agent reviews core ownership. | Shared code stays business-neutral and singularly owned. |
| 4. Define the migration phases | Agent sequences infrastructure setup, feature migration, consolidation cleanup, and cutover preparation. | Phase plan | Agent reviews dependencies across phases. | Each phase produces a shippable intermediate state. |
| 5. Define deployment and rollback shape | Agent selects the deployment model and records rollback rules before migration begins. | Release model | Agent reviews release constraints. | The plan supports staged release and reversal without emergency redesign. |
| 6. Define measurable success criteria | Agent records parity, bundle, performance, and maintainability targets that decide completion. | Success matrix | Agent compares criteria to the plan. | The program has explicit technical and business completion signals. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Architecture verification | Design review against the module map | Every feature has bounded ownership |
| Migration readiness verification | Phase review against dependencies | Every phase leaves the app releasable |
| Cutover readiness verification | Review with `webapp-cutover-strategy` | Rollout and rollback paths are defined |

## Verification Checklist

- [ ] Agent defines one target app with bounded feature modules.
- [ ] Agent assigns shared infrastructure to one neutral core.
- [ ] Agent sequences the migration in releasable phases.
- [ ] Agent defines deployment and rollback rules before cutover.
- [ ] Agent records measurable parity and performance targets.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent merges applications before defining feature ownership. | Agent defines the module map first. |
| Agent promotes feature-specific code into shared infrastructure. | Agent keeps shared code business-neutral and multi-consumer. |
| Agent creates a phase plan that ends in long-lived broken intermediate states. | Agent requires every phase to stay shippable. |
| Agent treats deployment shape as an afterthought. | Agent records rollout and rollback rules during strategy work. |
