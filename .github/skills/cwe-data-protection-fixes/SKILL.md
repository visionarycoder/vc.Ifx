---
name: cwe-data-protection-fixes
title: CWE Data Protection Fixes
description: Consolidated guidance for data-protection CWEs (CWE-200, CWE-502, CWE-798, CWE-639, CWE-401, CWE-363) with detection patterns, fix patterns, and measurable verification.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 1680
prerequisites:
  - Secure-coding basics and access to repo and CI
  - Ability to run grep, gitleaks, and build and test suites
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - cwe-input-validation-fixes
  - cwe-authentication-authz-fixes
  - cwe-injection-attack-fixes
  - cwe-resource-management-fixes
  - cwe-miscellaneous-security-fixes
  - security-controller
appliesTo: '**/*.*'
tags:
  - security
  - cwe
  - data-protection
---

# CWE Data Protection Fixes

Agent uses this bundle for disclosure, unsafe deserialization, secret exposure, IDOR, resource leak, and file race defects.

## Activation

**USE FOR:** CWE-200, CWE-502, CWE-798, CWE-639, CWE-401, CWE-363.  
**DO NOT USE FOR:** General authorization routing, input bounds defects, or style-only refactors.

## Critical Rules

| Rule | Test | Pass |
|---|---|---|
| Agent keeps secrets out of source, config literals, and image layers. | Run secret scan on touched files. | Zero active secret hits remain. |
| Agent replaces unsafe type-open deserialization on untrusted input. | Run grep for banned serializers. | Zero banned serializer hits remain in touched files. |
| Agent returns safe error payloads to callers. | Run exception-path tests. | External responses exclude stack traces, SQL, and environment values. |
| Agent verifies ownership for user-supplied identifiers. | Run owner and non-owner tests. | Non-owner returns 404 or 403. |
| Agent closes disposable resources with scope-based disposal. | Run analyzers and leak-focused tests already in repo. | Zero new disposal findings appear. |

## CWE Coverage

| CWE | Concern | Trigger Pattern | Fix Pattern | Verification |
|---|---|---|---|---|
| 200 | Sensitive information exposure | `ex.ToString()`, developer page, entity serialization, verbose headers | Use DTOs, central exception handler, safe Problem Details | Run error-path tests. Pass: No sensitive fields in response. |
| 502 | Unsafe deserialization | `BinaryFormatter`, `NetDataContractSerializer`, `TypeNameHandling` | Use `System.Text.Json` typed DTOs and explicit discriminators | Run grep and deserialization tests. Pass: Invalid polymorphic payloads fail. |
| 798 | Hard-coded credentials | Secrets in code, Dockerfile, config, scripts | Move secrets to Key Vault or secret store and rotate exposed values | Run secret scan. Pass: Zero active secret hits remain. |
| 639 | IDOR | Query by route id without ownership predicate | Scope query by authenticated principal | Run access tests. Pass: Non-owner cannot read or write data. |
| 401 | Resource leak | Missing `using`, new `HttpClient` per request, unclosed stream | Use `using`, `await using`, or factory pattern | Run analyzers or stability tests. Pass: Zero new leak findings. |
| 363 | TOCTOU race | `File.Exists` before later open or delete | Open stable handle first and verify handle state | Run race or symlink tests when present. Pass: Swap attack fails. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Inventory | Agent runs scans for secrets, serializers, error leaks, IDOR, disposables, and pre-open file race logic. | Run repo grep and secret scan commands already used by the repo. | Hotspot list exists for every touched file. |
| 2. Classify | Agent maps each hotspot to one CWE row. | Review hotspot list. | Every hotspot has one CWE label or explicit out-of-scope note. |
| 3. Fix | Agent moves secrets, replaces serializers, adds ownership gates, and closes resource scope. | Run targeted build for touched project. | Build passes with zero new errors. |
| 4. Run tests | Agent runs unit tests, integration tests, and scan jobs for touched scope. | Run existing tests and secret scans for touched scope. | All targeted tests pass. Zero secret hits remain. |
| 5. Re-scan | Agent runs grep and secret scans again. | Re-run inventory commands. | Zero unexplained banned patterns remain in touched files. |

## Decision Matrix

| Condition | Use |
|---|---|
| Input deserializes from network or queue | Use typed DTOs and explicit discriminators |
| Request accesses user-owned resource | Use ownership predicate in query or handler |
| Secret appears in code or config | Move to secret store and rotate value |
| File workflow uses pre-open existence logic | Use handle-first logic |
| Disposable object has request scope | Use `using`, `await using`, or factory-managed scope |

## Detection Commands

| Concern | Command | Pass |
|---|---|---|
| Unsafe serializers | `rg -n "BinaryFormatter|SoapFormatter|NetDataContractSerializer|TypeNameHandling\." -g "**/*.cs"` | Zero targeted hits in touched files |
| Error leaks | `rg -n "UseDeveloperExceptionPage|ex\.ToString\(|X-Powered-By|X-AspNet-Version" -g "**/*.cs"` | Zero targeted hits in touched files |
| Secret literals | Use repo secret scan or `gitleaks` command already present in CI | Zero active secret hits |
| IDOR entry points | `rg -n "\{id\}|\{userId\}|\{orderId\}" -g "**/*.cs"` | Every route has ownership review |

## Verification Checklist

Agent verifies:
- [ ] Secret scans report zero active secrets in touched files
- [ ] Banned serializers are absent from touched files
- [ ] Error responses are safe for external users
- [ ] Ownership tests pass for positive and negative cases
- [ ] Disposal and resource-scope defects do not increase

## Inputs

| Input | Required | Default |
|---|---|---|
| Repo or project path | Yes | - |
| CWE subset | No | All 6 |
| Secret scan command | No | Existing repo default |

## Outputs

Agent generates:
- Hotspot list with CWE labels
- Patch set for disclosure, serializer, secret, IDOR, and leak fixes
- Verification notes with scan and test results
- Rotation note when a real secret is exposed

## References

- RFC 9457 Problem Details
- OWASP IDOR guidance
- Microsoft BinaryFormatter migration guidance
- GitHub secret scanning and gitleaks guidance
