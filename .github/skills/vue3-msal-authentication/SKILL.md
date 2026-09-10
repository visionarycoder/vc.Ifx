---
name: vue3-msal-authentication
title: Vue 3 MSAL Authentication
description: Configure Microsoft Entra ID authentication in Vue 3 with MSAL, router guards, token acquisition, and secure storage boundaries.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1230
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - vue3-webapp-setup
  - entra-app-registration
related_skills:
  - vue3-pinia-state-management
  - webapp-frontend-security
appliesTo: '**/*.{vue,ts,js}'
tags:
  - vue3
  - msal
  - auth
  - entra
---
# Vue 3 MSAL Authentication

Agent wires secure browser authentication for Vue 3 SPAs that use Microsoft Entra ID.

## When to Use

| Condition | Route |
|---|---|
| Browser SPA uses Microsoft Entra ID | Agent uses this skill |
| API calls need user tokens | Agent uses this skill |
| App uses non-Microsoft identity | Agent uses another auth skill |
| App uses server session auth only | Agent uses a server auth skill |

## Auth Control Matrix

| Control | Agent action | Test | Pass |
|---|---|---|---|
| Flow | Agent uses Authorization Code with PKCE | Review MSAL config | PKCE flow is enabled |
| Cache | Agent keeps tokens in session storage or MSAL-managed cache | Search storage code | No token write appears in `localStorage` |
| Scopes | Agent requests only required scopes | Review scope list | Scope list matches API contract |
| Guards | Agent protects routes with router guards | Open protected route without auth | Route redirects to login |
| API calls | Agent acquires tokens silently before request send | Run authenticated API call | Request returns 200 or approved status |
| Failure handling | Agent clears auth state on invalid session | Simulate 401 | App clears session and redirects |

## Migration Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Register inputs | Agent maps client ID, authority, redirect URIs, and scopes | Review env keys | All required keys exist |
| 2. Configure MSAL | Agent creates browser config and client instance | Run app bootstrap | Client instance initializes |
| 3. Register plugin | Agent wires MSAL into Vue and Pinia boundaries | Run app startup | App loads without auth runtime error |
| 4. Protect routes | Agent adds route guards and intended-route restore | Run guarded route flow | User returns to intended route after login |
| 5. Protect API calls | Agent adds token acquisition in HTTP layer | Run API smoke test | Auth header reaches protected API |
| 6. Verify logout | Agent clears state and logout redirect paths | Run logout flow | Session ends and redirect completes |

## Validation

| Check | Test | Pass |
|---|---|---|
| Env completeness | Review `VITE_MSAL_*` keys | Required keys exist |
| Startup health | Run `npm run dev` or equivalent startup | App boots without MSAL error |
| Auth flow | Run login redirect flow | Account is present after redirect |
| API flow | Call protected API | Response succeeds with expected scope |
| Logout flow | Run logout path | Cached account and app state clear |
| Token storage | Search `localStorage` token writes | Zero token writes remain |

## Output

Agent returns config keys, guard paths, API token behavior, and test results.

