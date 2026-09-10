---
name: webapp-frontend-security
title: Web App Front-End Security
description: Apply defense-in-depth practices to the portal web app through MSAL-only auth flow, emulation-aware authorization gates, safe storage, and secure DOM and HTTP patterns.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1244
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - webapi-authz-hardening
  - vue3-msal-authentication
  - angular-security-hardening
appliesTo: 'AzureAPI/src/Client.Portal.WebApp/**'
tags:
  - webapp
  - frontend
  - security
---
# Web App Front-End Security

Agent enforces defense-in-depth rules in `Client.Portal.WebApp` so that MSAL remains the only auth path, emulation-aware UI gates stay consistent, and browser code avoids secret leakage and unsafe DOM patterns.

## When to Use

| Condition | Use |
|---|---|
| Agent adds or changes routes, pages, components, or menu items in `Client.Portal.WebApp`. | Agent uses this skill. |
| Agent touches identity, token flow, guards, interceptors, or storage. | Agent uses this skill. |
| Agent reviews UI gating for admin, reader, or anonymous experiences. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent works only on server-side authorization. | Agent uses `webapi-authz-hardening`. |
| Agent works outside `Client.Portal.WebApp`. | Agent applies the security rules for the relevant client. |
| Agent edits presentation-only styling with no identity, route, or data implications. | Agent performs a narrower review. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Changed route or component scope | Yes | Scope identifies every changed UI entry point. |
| Auth and role requirements | Yes | Requirements identify authenticated, admin, and emulated behavior. |
| Backend endpoint usage | Yes | Usage identifies new or changed protected resources. |
| Storage changes | No | Changes identify any new session or local storage keys. |
| DOM or HTML rendering changes | No | Changes identify any unsafe rendering risk. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Verify the auth boundary | Agent confirms that MSAL remains the only auth system and that no custom token exchange returns. | Auth-boundary review | Agent inspects changed auth code. | No custom auth service or manual token store bypass appears. |
| 2. Verify route guards | Agent applies `MsalGuard`, `authEmulationGuard`, and `adminRoleGuard` where the route semantics require them. | Route-guard map | Agent reviews changed routes. | Each private route has the correct guard set. |
| 3. Verify admin gating | Agent binds admin-only UI to emulation-aware state instead of direct claim or storage reads. | UI-gating review | Agent reviews changed components. | Admin visibility flows through the approved broadcast path. |
| 4. Verify HTTP and storage rules | Agent keeps HTTP calls under the configured interceptor path and documents any approved storage keys. | Transport and storage review | Agent inspects changed services and storage use. | No manual `Authorization` header or undocumented key appears. |
| 5. Verify DOM and logging safety | Agent rejects unsafe HTML rendering, unsafe trust bypasses, and token or PII logging. | Output-safety review | Agent inspects templates and logs. | Browser output stays escaped and logs avoid sensitive data. |
| 6. Verify server parity | Agent confirms that every client-side gate has matching server-side enforcement. | Parity note | Agent traces changed endpoint use. | Client gating remains UX only and server enforcement remains authoritative. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Build verification | Existing app build command | The web app build succeeds |
| Route verification | Manual or existing route tests | Private and admin routes enforce the expected guards |
| Security review verification | Changed-file review against this skill | Zero forbidden auth, storage, or DOM patterns remain |

## Verification Checklist

- [ ] Agent keeps MSAL as the only auth system.
- [ ] Agent uses emulation-aware route and UI gates.
- [ ] Agent avoids manual bearer-header code.
- [ ] Agent documents any approved storage keys.
- [ ] Agent confirms matching server-side authorization.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent reads admin state directly from storage or raw claims for template gates. | Agent reads admin state through the approved emulation-aware broadcast path. |
| Agent adds a second auth interceptor. | Agent uses the existing MSAL interceptor configuration. |
| Agent stores secrets or raw access tokens in browser-owned storage. | Agent keeps secrets out of the bundle and keeps token handling inside MSAL. |
| Agent uses `[innerHTML]` or trust-bypass APIs on untrusted content. | Agent uses escaped bindings or sanitized content only. |
