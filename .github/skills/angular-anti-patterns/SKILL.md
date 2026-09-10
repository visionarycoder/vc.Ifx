---
name: angular-anti-patterns
title: Angular Anti-Patterns
description: Detect and remove Angular anti-patterns that block performance, security, testability, and Vue migration.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1180
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - Angular 12+ knowledge
  - TypeScript proficiency
  - RxJS basics
related_skills:
  - angular-modernization
  - angular-security-hardening
  - angular-to-vue3-migration
appliesTo: '**/*.{ts,html,scss,json}'
tags:
  - angular
  - anti-patterns
  - refactoring
  - migration
---
# Angular Anti-Patterns

Agent removes Angular anti-patterns before modernization or Vue migration starts.

## When to Use

| Condition | Route |
|---|---|
| Angular code shows leaks, slow views, or large components | Agent uses this skill |
| Angular migration needs cleanup before conversion | Agent uses this skill |
| New Vue code needs setup guidance | Agent uses `vue3-webapp-setup` |
| Backend API issues drive the defect | Agent uses a webapi skill |

## Detection Matrix

| Area | Agent action | Test | Pass |
|---|---|---|---|
| Change detection | Agent adds `OnPush`, `trackBy`, and immutable updates | Run `ng build` | Build succeeds |
| Memory leaks | Agent removes unmanaged subscriptions, timers, and listeners | Run leak-focused unit tests | Zero leak regressions |
| RxJS misuse | Agent replaces nested subscriptions with operators | Run feature tests | All async flows pass |
| Component design | Agent moves business rules to services or composables | Run unit tests | Component tests stay green |
| Template cost | Agent removes heavy method calls and unsafe HTML | Run `ng test` | Zero template failures |
| Forms | Agent moves complex forms to Reactive Forms | Submit invalid and valid inputs | Error and success flows match spec |
| HTTP and state | Agent moves HTTP calls out of components | Run integration tests | Requests still succeed |
| Security | Agent removes `innerHTML` and `bypassSecurityTrust*` misuse | Search for unsafe APIs | Zero unsafe hits remain |

## Migration Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Inventory | Agent lists components, services, forms, and routes with defects | Review issue list | List covers changed scope |
| 2. Secure | Agent fixes XSS, secret, and auth storage defects first | Search `innerHTML|bypassSecurity|localStorage` | Zero unsafe findings in changed scope |
| 3. Stabilize | Agent removes leaks and nested subscriptions | Run targeted browser and unit tests | No leak symptom remains |
| 4. Simplify | Agent extracts business logic from components | Run component tests | Tests pass after extraction |
| 5. Optimize | Agent adds `OnPush`, `trackBy`, and lazy boundaries | Run `ng build` and profile target view | Build succeeds and target view stays stable |
| 6. Prepare migration | Agent records remaining Angular-only patterns | Review conversion list | List maps each pattern to a Vue target |

## Validation

| Check | Test | Pass |
|---|---|---|
| Unsafe HTML removed | Search `innerHTML|bypassSecurityTrust` | Zero matches in changed files |
| Subscription cleanup added | Search `subscribe\(` and inspect each hit | Each hit uses managed cleanup or async pipe |
| Component scope reduced | Review changed components | Business rules live outside route components |
| Build health | Run `ng build` | Exit code = 0 |
| Test health | Run `ng test --watch=false` | Exit code = 0 |
| Migration readiness | Compare Angular patterns to Vue plan | Each changed feature has a conversion path |

## Output

Agent returns a defect list, changed files, test results, and remaining migration blockers.

