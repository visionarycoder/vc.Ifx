---
name: cwe-authentication-authz-fixes
title: CWE Authentication & Authorization Fixes
description: Consolidated guidance for authentication and authorization CWEs (CWE-287, CWE-288, CWE-306, CWE-352, CWE-862, CWE-863) with detection patterns, fix patterns, and measurable verification.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 1610
prerequisites:
  - Knowledge of ASP.NET Core authentication, authorization, and antiforgery
  - Access to repo and CI to run grep and dotnet commands
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - cwe-input-validation-fixes
  - cwe-injection-attack-fixes
  - cwe-data-protection-fixes
  - cwe-resource-management-fixes
  - cwe-miscellaneous-security-fixes
  - security-controller
  - webapi-authz-hardening
appliesTo: "**/*.{cs,cshtml,razor,html}"
tags:
  - cwe
  - auth
  - authorization
  - csrf
  - aspnet-core
---

# CWE Authentication & Authorization Fixes

Agent uses this bundle for authentication, authorization, and CSRF defects in ASP.NET Core services and browser flows.

## Activation

**USE FOR:** CWE-287, CWE-288, CWE-306, CWE-352, CWE-862, CWE-863.  
**DO NOT USE FOR:** Crypto storage defects, secret leakage defects, or unrelated identity redesign.

## Critical Rules

| Rule | Test | Pass |
|---|---|---|
| Agent requires authentication on every state-changing endpoint unless the user documents an anonymous exception. | Run endpoint tests without credentials. | State-changing endpoints return 401 or documented anonymous result. |
| Agent verifies resource ownership on sensitive reads and writes. | Run owner and non-owner integration tests. | Owner passes. Non-owner returns 403 or 404. |
| Agent enables antiforgery on cookie-authenticated browser flows. | Run POST without antiforgery token. | Request returns 400 or framework rejection. |
| Agent keeps JWT signature, issuer, audience, and lifetime verification enabled. | Run token tests with invalid signature, issuer, audience, and expiration. | Every invalid token returns 401. |
| Agent documents every `[AllowAnonymous]` mutation path with inline rationale. | Run `rg -n "\[AllowAnonymous\]" -g "**/*.cs"`. | Every hit on a mutation path has rationale or is fixed. |

## CWE Coverage

| CWE | Concern | Trigger Pattern | Fix Pattern | Verification |
|---|---|---|---|---|
| 287 | Improper authentication | `ValidateSignature = false`, plaintext password compare, custom no-op token logic | Use framework token verification and secure KDFs | Run tampered-token tests. Pass: 401 for invalid tokens. |
| 288 | Alternate auth path | Debug, health, swagger, or legacy path bypasses auth pipeline | Route all exposed paths through one auth pipeline or private interface | Run route tests. Pass: Protected paths reject unauthenticated access. |
| 306 | Missing authentication | `MapPost`, `MapDelete`, or controller mutation without auth | Add `[Authorize]`, `.RequireAuthorization()`, or fallback policy | Run anonymous mutation tests. Pass: 401. |
| 352 | CSRF | Cookie-authenticated unsafe verb lacks antiforgery | Add antiforgery filters and header or form token flow | Run CSRF test. Pass: Missing token request fails. |
| 862 | Missing authorization | Authenticated caller reaches sensitive action without policy or ownership gate | Use policy or `IAuthorizationService` gate | Run role and ownership tests. Pass: Unauthorized caller fails. |
| 863 | Incorrect authorization | Decision uses headers, query fields, or wrong claim compare | Read claims from `User` and use explicit handlers | Run forged-header tests. Pass: Forged input does not grant access. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Inventory | Agent reads auth pipeline, mapped endpoints, and policy registration. | Run `rg -n "\[AllowAnonymous\]|UseAuthentication|UseAuthorization|AddJwtBearer|RequireAuthorization|FallbackPolicy" -g "**/*.cs"`. | Inventory covers startup and endpoint files. |
| 2. Find gaps | Agent finds mutation paths without auth, policy, or antiforgery. | Run `rg -n "Map(Post|Put|Patch|Delete)\(|\[Http(Post|Put|Patch|Delete)" -g "**/*.cs"`. | Every mutation path has auth decision recorded. |
| 3. Fix | Agent adds fallback policy, endpoint auth, ownership checks, and antiforgery where needed. | Run targeted build for touched project. | Build passes with zero new errors. |
| 4. Run tests | Agent runs unit tests for handlers and integration tests for 401, 403, 404, and CSRF. | Run existing test project for touched service. | All targeted tests pass. |
| 5. Re-scan | Agent runs grep again for missing auth patterns. | Re-run inventory commands. | Zero unexplained auth gaps remain. |

## Decision Matrix

| Condition | Use |
|---|---|
| Endpoint changes server state | Use `[Authorize]` or `.RequireAuthorization()` |
| Endpoint reads or writes user-owned resource | Use resource-based authorization |
| Browser flow uses cookies | Use antiforgery token verification |
| Service uses bearer tokens | Use strict token verification parameters |
| Background job replays user action | Use stored claims plus fresh authorization gate |

## ASP.NET Core Patterns

| Area | Agent Uses | Agent Avoids |
|---|---|---|
| Default policy | `FallbackPolicy = RequireAuthenticatedUser()` | Public-by-default pipelines |
| Resource gate | `IAuthorizationService.AuthorizeAsync(User, resource, requirement)` | Role switch logic in controller body |
| Antiforgery | `AutoValidateAntiforgeryToken` and `X-XSRF-TOKEN` | Cookie-authenticated POST without token |
| Minimal APIs | `.RequireAuthorization("PolicyName")` | Anonymous mutation handlers |

## Verification Checklist

Agent verifies:
- [ ] Anonymous mutation requests fail
- [ ] Non-owner requests fail
- [ ] Invalid bearer tokens fail
- [ ] Missing antiforgery tokens fail in cookie flows
- [ ] Touched auth handlers pass unit and integration tests

## Inputs

| Input | Required | Default |
|---|---|---|
| Repo or project path | Yes | - |
| Endpoint scope | No | All matching files |
| CWE subset | No | All 6 |

## Outputs

Agent generates:
- Endpoint inventory with auth gap labels
- Patch set for auth, authorization, and antiforgery fixes
- Unit tests or integration tests when the repo already contains them
- Verification notes with route, token, and CSRF results

## References

- OWASP Authentication Cheat Sheet
- OWASP Authorization Cheat Sheet
- ASP.NET Core authentication and authorization docs
- ASP.NET Core antiforgery docs
