---
name: vue3-webapp-setup
title: Vue 3 WebApp Setup
description: Bootstrap Vue 3 web applications with Vite, TypeScript, routing, Pinia, testing, and enterprise folder boundaries.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1310
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - vue3-component-library-design
  - vue3-e2e-testing-playwright
  - vue3-pinia-state-management
  - vue3-msal-authentication
appliesTo: '**/*.{vue,ts,js,json}'
tags:
  - vue3
  - webapp
  - setup
  - vite
---
# Vue 3 WebApp Setup

Agent creates a Vue 3 application foundation that supports typed development, testing, and later migration work.

## When to Use

| Condition | Route |
|---|---|
| Team starts a new Vue 3 SPA | Agent uses this skill |
| Team builds the target app for Angular migration | Agent uses this skill |
| Team updates one existing Vue module only | Agent uses the module-specific skill |
| Team needs SSR-first architecture | Agent uses an SSR framework path |

## Setup Matrix

| Area | Agent action | Test | Pass |
|---|---|---|---|
| Scaffold | Agent creates the app with official Vue tooling | Run initial install and build | Scaffold completes |
| TypeScript | Agent enables strict TypeScript settings | Run type-check | Exit code = 0 |
| Routing | Agent registers Vue Router with base routes | Open root and not-found routes | Both routes resolve |
| State | Agent registers Pinia | Load app and store | Store initializes |
| Testing | Agent wires Vitest and Playwright | Run unit and E2E commands | Both commands start and finish |
| Structure | Agent creates core, shared, feature, asset, and config folders | Review tree | Folder layout matches target pattern |
| Config | Agent defines env keys under `VITE_*` | Review env files | Required keys exist |

## Migration Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Scaffold target | Agent creates the Vue app with selected tooling | Run install and build | Base app builds |
| 2. Lock typing | Agent enables strict TypeScript and path aliases | Run type-check | Alias and strict checks pass |
| 3. Add runtime layers | Agent registers router, Pinia, and shared config | Start app | App boots |
| 4. Add test layers | Agent configures Vitest and Playwright | Run unit and E2E dry run | Both runners load |
| 5. Create folders | Agent creates core, shared, and feature boundaries | Review tree | Each boundary exists |
| 6. Verify baseline | Agent runs validation commands | Run validate set | All baseline commands pass |

## Validation

| Check | Test | Pass |
|---|---|---|
| Type health | Run `npm run type-check` | Exit code = 0 |
| Lint health | Run `npm run lint` | Exit code = 0 |
| Unit health | Run `npm run test` | Exit code = 0 |
| Build health | Run `npm run build` | Exit code = 0 |
| E2E health | Run `npm run test:e2e` or runner dry run | Runner starts without config error |
| Folder layout | Review `src` tree | Core, shared, features, assets, and config exist |

## Output

Agent returns the scaffold command set, created boundaries, env keys, and baseline validation results.

