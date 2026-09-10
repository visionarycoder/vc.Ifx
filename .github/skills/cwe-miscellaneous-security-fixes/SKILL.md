---
name: cwe-miscellaneous-security-fixes
title: CWE Miscellaneous Security Fixes
description: Consolidated guidance for path traversal, privilege, upload, null, redirect, permission, resource-limit, SSRF, and model-state CWEs with fix patterns and measurable verification.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 1760
prerequisites:
  - Familiarity with C# and .NET web app patterns and platform security
  - CI tooling for static analysis and tests
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - cwe-input-validation-fixes
  - cwe-authentication-authz-fixes
  - cwe-injection-attack-fixes
  - cwe-data-protection-fixes
  - cwe-resource-management-fixes
  - security-controller
appliesTo: '**/*.{cs,csproj,ps1,yaml,yml,tf,json,md}'
tags:
  - security
  - cwe
  - ssrf
  - path-traversal
---

# CWE Miscellaneous Security Fixes

Agent uses this bundle for path traversal, upload, null, redirect, permission, resource-limit, SSRF, privilege, and model-state defects.

## Activation

**USE FOR:** CWE-22, CWE-266, CWE-434, CWE-476, CWE-601, CWE-732, CWE-770, CWE-918, CWE-1174.  
**DO NOT USE FOR:** Broad architecture threat modeling or unrelated business-rule verification.

## Critical Rules

| Rule | Test | Pass |
|---|---|---|
| Agent normalizes paths under an explicit root before file access. | Run traversal tests with `..`, encoded separators, and zip-slip names. | Access outside root is rejected. |
| Agent verifies redirect and outbound URL targets against explicit allow-lists. | Run SSRF and redirect tests with private and external targets. | Disallowed targets are rejected. |
| Agent verifies upload extension, magic bytes, and size before persistence. | Run upload tests with spoofed extension and oversize file. | Invalid upload is rejected. |
| Agent keeps secrets and sensitive files under restrictive permissions. | Run config or filesystem checks already in repo. | Touched secret files are not public or world-write. |
| Agent keeps automatic model-state rejection active for API controllers. | Run invalid model tests. | Invalid model returns 400 with Problem Details. |

## CWE Coverage

| CWE | Concern | Trigger Pattern | Fix Pattern | Verification |
|---|---|---|---|---|
| 22 | Path traversal | `Path.Combine` with user path, archive extraction without root guard | Use normalized root guard or id-to-path mapping | Run traversal tests. Pass: Outside-root path is rejected. |
| 266 | Privilege | Broad admin role, `Owner`, `db_owner`, overpowered service account | Use narrow role and audited elevation | Run config tests. Pass: Broad role is absent from touched scope. |
| 434 | Unrestricted upload | `IFormFile` saved by user filename or webroot path | Use extension allow-list, magic-byte verification, size cap, random server filename | Run upload tests. Pass: Invalid or dangerous file is rejected. |
| 476 | Null dereference | Nullable warning ignored, unsafe `!`, missing null guard | Use nullable annotations and explicit null path | Run build and unit tests. Pass: Zero new null warnings in touched scope. |
| 601 | Open redirect | `Redirect(url)` from query or form | Use `LocalRedirect` or allow-listed hosts | Run redirect tests. Pass: External unapproved URL is rejected. |
| 732 | Incorrect permissions | 0777, `Everyone`, public secret storage | Use restrictive ACL or file mode and secret store | Run IaC or runtime checks. Pass: Touched sensitive paths are private. |
| 770 | Missing resource limits | Unbounded cache, channel, body size, parallel fan-out | Use explicit size, queue, and rate caps | Run load tests. Pass: Caps trigger before instability. |
| 918 | SSRF | Outbound `HttpClient` call from user URL | Use host allow-list, DNS private-range block, and hardened client | Run SSRF tests. Pass: Private and unapproved hosts are rejected. |
| 1174 | ASP.NET model-state bypass | Missing `[ApiController]` or suppressed invalid-model filter | Use `[ApiController]` or explicit invalid-model response factory | Run invalid model tests. Pass: Invalid model returns 400. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Inventory | Agent scans for file paths, uploads, redirects, outbound URLs, permissions, nullable warnings, and API controller config. | Run targeted grep and existing analyzers. | Hotspot list exists for every touched file. |
| 2. Classify | Agent maps each hotspot to one CWE row. | Review hotspot list. | Every hotspot has one CWE label or explicit out-of-scope note. |
| 3. Fix | Agent adds root guards, allow-lists, upload checks, permission fixes, null guards, and model-state enforcement. | Run targeted build for touched project. | Build passes with zero new errors. |
| 4. Run tests | Agent runs unit tests, integration tests, upload tests, redirect tests, and SSRF tests already in repo. | Run existing tests for touched scope. | All targeted tests pass. |
| 5. Re-scan | Agent runs grep and analyzers again. | Re-run inventory commands. | Zero unexplained targeted hits remain in touched files. |

## Decision Matrix

| Condition | Use |
|---|---|
| User input selects file path | Use normalized root guard or id-to-path mapping |
| Request redirects browser | Use `LocalRedirect` or host allow-list |
| Request uploads file | Use extension, magic-byte, and size checks |
| Request selects outbound URL | Use hardened named client and allow-list |
| Controller binds model | Use automatic invalid-model rejection |

## Repo Notes

| Area | Agent Uses | Agent Avoids |
|---|---|---|
| URL-encoded traversal | `UrlEncodingValidationFilter` when repo provides it | Ad-hoc decoding rules |
| Outbound HTTP | Named `IHttpClientFactory` client with redirect and cookie controls | Default client with user URL |
| API errors | `application/problem+json` for invalid model | Raw text error bodies |

## Verification Checklist

Agent verifies:
- [ ] Traversal probes cannot escape allowed root
- [ ] Invalid uploads are rejected before persistence
- [ ] Unapproved redirects and SSRF targets are rejected
- [ ] Sensitive paths use private permissions
- [ ] Invalid model state returns 400 with Problem Details

## Inputs

| Input | Required | Default |
|---|---|---|
| Repo or component path | Yes | - |
| CWE subset | No | All 9 |
| Allow-list inputs | No | Existing repo defaults |

## Outputs

Agent generates:
- Hotspot list with CWE labels
- Patch set for path, upload, redirect, SSRF, permission, null, and model-state fixes
- Verification notes with test and analyzer results

## References

- OWASP file upload guidance
- ASP.NET Core `LocalRedirect` guidance
- ASP.NET Core `IHttpClientFactory` guidance
- Azure IMDS guidance
