---
name: angular-security-hardening
title: Angular Security Hardening
description: Harden Angular applications against XSS, CSRF, token exposure, dependency risk, and insecure runtime configuration.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: high
estimated_tokens: 934
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - angular-anti-patterns
  - webapp-frontend-security
related_skills:
  - security-controller
  - webapp-frontend-security
  - webapi-authz-hardening
appliesTo: '**/*.{ts,html,json}'
tags:
  - angular
  - security
  - xss
  - csrf
---
# Angular Security Hardening

Agent removes frontend security defects before release or migration work starts.

## When to Use

| Condition | Route |
|---|---|
| Angular application handles identity, forms, or user HTML | Agent uses this skill |
| Audit lists XSS, CSRF, or token storage defects | Agent uses this skill |
| API authorization fails on the server | Agent uses `webapi-authz-hardening` |
| Infrastructure headers or secrets drive the risk | Agent uses an infrastructure or security skill |

## Control Matrix

| Control | Agent action | Test | Pass |
|---|---|---|---|
| XSS | Agent removes unsafe `innerHTML` and trust bypass calls | Search `innerHTML|bypassSecurityTrust` | Zero unsafe matches in changed files |
| CSRF | Agent routes state-changing calls through `HttpClient` and XSRF config | Inspect network trace | Header and cookie pair exist |
| Token storage | Agent moves tokens out of `localStorage` | Search `localStorage` | Zero token writes remain |
| Route protection | Agent adds guards to protected routes | Open protected route without auth | Route redirects or blocks access |
| Runtime config | Agent removes hardcoded secrets and HTTP endpoints | Search secret and `http://` patterns | Zero exposed secret hits remain |
| Dependencies | Agent updates or replaces vulnerable packages | Run `npm audit` | Zero unresolved high findings in changed app |
| CSP and headers | Agent documents required headers for deployment | Run `curl -I <app-url>` | Security headers appear |

## Migration Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1. Scan | Agent searches for unsafe APIs, storage, and config | Run repository grep set | Findings list is complete for changed scope |
| 2. Remove direct risk | Agent fixes XSS and secret exposure defects first | Re-run grep set | Zero direct risk hits remain |
| 3. Protect requests | Agent configures XSRF and guarded routes | Run browser auth flows | Protected calls include auth and XSRF controls |
| 4. Harden runtime | Agent disables debug-only production settings | Run production build | Source maps and debug flags follow config |
| 5. Audit packages | Agent resolves vulnerable frontend packages | Run `npm audit` | No unresolved high finding stays in changed app |
| 6. Verify cutover readiness | Agent records residual risks and controls | Review security summary | Each residual risk has owner and scope |

## Validation

| Check | Test | Pass |
|---|---|---|
| Unsafe HTML removed | Search `innerHTML|bypassSecurityTrust` | Zero matches in changed files |
| Token exposure removed | Search `localStorage|sessionStorage` for token writes | No token write remains outside approved cache |
| Protected route behavior | Open protected route without login | Route denies anonymous access |
| CSRF behavior | Inspect state-changing request | XSRF header is present |
| Dependency status | Run `npm audit` | No unresolved high finding in changed app |
| Build health | Run `ng build --configuration production` | Exit code = 0 |

## Output

Agent returns findings, fixes, tests, and unresolved deployment controls.
