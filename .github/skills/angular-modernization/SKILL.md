---
name: angular-modernization
title: Angular Modernization
description: Upgrade Angular applications to current patterns with standalone components, signals, modern control flow, and measured validation.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1190
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - Angular 12+ current version
  - angular-anti-patterns
related_skills:
  - angular-anti-patterns
  - angular-to-vue3-migration
appliesTo: '**/*.{ts,html,json}'
tags:
  - angular
  - modernization
  - standalone
  - signals
---
# Angular Modernization

Agent upgrades Angular applications in small verified steps.

## When to Use

| Condition | Route |
|---|---|
| Angular 12-17 application needs current Angular patterns | Agent uses this skill |
| Team needs standalone components or signals | Agent uses this skill |
| Team needs framework migration to Vue | Agent uses `angular-to-vue3-migration` |
| Team needs AngularJS migration | Agent uses a dedicated AngularJS path |

## Upgrade Matrix

| Target | Agent action | Test | Pass |
|---|---|---|---|
| Framework version | Agent upgrades Angular packages with the update guide | Run `ng version` and `ng build` | Target version appears and build succeeds |
| Standalone components | Agent removes NgModule-only patterns | Run component tests | Standalone imports resolve |
| Signals | Agent moves local UI state to signals | Run unit tests | Derived state updates without manual sync |
| Control flow | Agent replaces legacy structural directives in changed views | Run template tests | Rendered output matches baseline |
| Defer blocks | Agent lazy-loads heavy UI segments | Run route smoke tests | Deferred content loads on trigger |
| Destroy cleanup | Agent uses `DestroyRef` or `takeUntilDestroyed` | Run leak tests | Cleanup executes on destroy |

## Migration Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Baseline | Agent records Angular version, warnings, and test status | Run `ng build` and `ng test --watch=false` | Baseline output is captured |
| 2. Upgrade packages | Agent upgrades core packages one release band at a time | Run `ng update` and `ng build` | No package conflict blocks the build |
| 3. Convert structure | Agent moves selected features to standalone components | Run feature tests | Routes and imports resolve |
| 4. Modernize reactivity | Agent replaces local mutable state with signals | Run unit tests | Signal state matches old behavior |
| 5. Modernize templates | Agent replaces legacy control flow in changed templates | Run UI regression tests | UI output stays stable |
| 6. Optimize | Agent adds defer boundaries and cleanup APIs | Run profile and leak checks | Load and destroy paths stay stable |

## Validation

| Check | Test | Pass |
|---|---|---|
| Upgrade health | Run `ng build` | Exit code = 0 |
| Test health | Run `ng test --watch=false` | Exit code = 0 |
| Template parity | Compare changed routes before and after upgrade | No route loses content or actions |
| Signal stability | Run state-focused tests | Computed values update after source changes |
| Cleanup stability | Run destroy-path tests | No leak regression appears |
| Warning reduction | Review build output | No new deprecation warning appears |

## Output

Agent returns the version change, migrated features, validation results, and remaining upgrade blockers.

