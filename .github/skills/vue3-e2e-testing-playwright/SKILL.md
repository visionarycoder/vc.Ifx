---
name: vue3-e2e-testing-playwright
title: Vue 3 End-to-End Testing with Playwright
description: Build deterministic Playwright suites for Vue 3 applications with auth, API mocking, parity checks, and CI diagnostics.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1290
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - vue3-webapp-setup
related_skills:
  - vue3-component-library-design
  - vue3-msal-authentication
  - angular-to-vue3-migration
appliesTo: '**/*.{ts,vue,json,yaml,yml}'
tags:
  - vue3
  - e2e
  - testing
  - playwright
---
# Vue 3 End-to-End Testing with Playwright

Agent tests user-visible Vue behavior across browser, route, and HTTP boundaries.

## When to Use

| Condition | Route |
|---|---|
| Critical user journeys need browser coverage | Agent uses this skill |
| Team compares Angular and Vue feature parity | Agent uses this skill |
| Pure store or component logic needs tests | Agent uses Vitest-based skills |

## Suite Matrix

| Suite | Agent action | Test | Pass |
|---|---|---|---|
| Mocked E2E | Agent mocks API calls at the browser boundary | Run mocked suite | Suite stays deterministic |
| Integration E2E | Agent uses deployed test APIs and approved identity | Run integration suite | Auth and API wiring work |
| Smoke | Agent runs production-safe read journeys after deployment | Run smoke suite | Critical routes load |
| Accessibility | Agent adds keyboard and a11y assertions | Run approved checks | No new violation appears |
| Visual | Agent snapshots stable UI fragments | Run visual suite | Snapshot diff is intentional |

## Migration Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Select journeys | Agent lists critical journeys and parity targets | Review suite map | Each journey has one owner |
| 2. Configure runner | Agent sets browser, base URL, traces, and reports | Run `npx playwright test --list` | Runner lists tests |
| 3. Stabilize auth | Agent uses test auth, storage state, or approved tenant flow | Run auth setup path | Auth state loads without manual steps |
| 4. Add mocks | Agent defines API mocks and Problem Details failures | Run mocked suite | Success and failure paths pass |
| 5. Add parity tests | Agent compares Angular and Vue journeys | Run parity suite | Each paired journey passes |
| 6. Wire CI | Agent stores traces and reports on failure | Run CI job | Failed tests retain diagnostics |

## Validation

| Check | Test | Pass |
|---|---|---|
| Runner health | Run `npx playwright test --list` | Exit code = 0 |
| Mocked suite | Run `npx playwright test` for mocked tests | Exit code = 0 |
| Integration suite | Run integration-tagged tests | Exit code = 0 |
| Accessibility | Run approved accessibility checks | No new violation appears |
| Fixed sleeps | Search `waitForTimeout\(` | Zero matches in new tests |
| Diagnostics | Inspect test-results output | Trace or screenshot exists on failure |

## Output

Agent returns covered journeys, auth mode, test results, and retained diagnostics paths.

