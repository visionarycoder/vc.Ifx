---
name: cwe-resource-management-fixes
title: CWE Resource & Resource-Management Fixes
description: Consolidated guidance for resource-related CWEs (CWE-117, CWE-190, CWE-269, CWE-276, CWE-284, CWE-400, CWE-416) with detection patterns, fix patterns, and measurable verification.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 1700
prerequisites:
  - Code search and static analysis tools
  - Basic familiarity with target languages and CI
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - cwe-input-validation-fixes
  - cwe-authentication-authz-fixes
  - security-controller
appliesTo: "**/*.{cs,cpp,c,h,rs,java,js,ts,tf,ps1,psm1,yaml,json,md}"
tags:
  - cwe
  - security
  - resource-management
---

# CWE Resource & Resource-Management Fixes

Agent uses this bundle for logging injection, arithmetic overflow, privilege defects, permission defects, resource exhaustion, and use-after-free defects.

## Activation

**USE FOR:** CWE-117, CWE-190, CWE-269, CWE-276, CWE-284, CWE-400, CWE-416.  
**DO NOT USE FOR:** General input verification or unrelated protocol redesign.

## Critical Rules

| Rule | Test | Pass |
|---|---|---|
| Agent uses structured logging and sanitizes hostile text before logging. | Run grep for interpolated logging in touched files. | Zero new interpolated log messages remain. |
| Agent verifies arithmetic on untrusted sizes in checked or guarded form. | Run analyzers and boundary tests. | Zero new overflow findings appear. |
| Agent applies least privilege to service identity, role, and file access. | Run existing config scans or integration checks. | Touched service paths use documented low-privilege settings. |
| Agent adds explicit caps for request size, rate, concurrency, depth, and timeout. | Run load or rate-limit tests already in repo. | Requests above cap are rejected predictably. |
| Agent removes unsafe ownership lifetimes and dispose races. | Run analyzers, sanitizers, or concurrency tests already in repo. | Zero new use-after-free or dispose-race findings appear. |

## CWE Coverage

| CWE | Concern | Trigger Pattern | Fix Pattern | Verification |
|---|---|---|---|---|
| 117 | Log injection | Interpolated log text, raw exception text, unescaped user content | Use structured logging and sanitizer helper | Run grep and log tests. Pass: Log output is structured and sanitized. |
| 190 | Integer overflow | `len * itemSize`, `offset + len`, unchecked cast | Use checked arithmetic or pre-guards | Run boundary tests. Pass: Overflow path returns reject result. |
| 269 | Improper privilege management | Root or LocalSystem service, broad impersonation | Use low-privilege identity and scoped elevation | Run config checks. Pass: Elevated identity is absent or scoped. |
| 276 | Incorrect default permissions | 0777, public bucket, permissive ACL | Use restrictive ACL or file mode and private storage defaults | Run IaC or runtime checks. Pass: Public or world-write defaults are absent. |
| 284 | Improper access control | Sensitive endpoint lacks policy | Use default-deny policy and ownership gate | Run access tests. Pass: Unauthorized requests fail. |
| 400 | Uncontrolled resource consumption | Unbounded read, queue, regex, task fan-out | Add caps, paging, timeouts, and rate limit | Run load tests. Pass: Caps trigger before service instability. |
| 416 | Use-after-free | `free` then reuse, disposal race, stale pointer | Use ownership-safe lifetime pattern | Run sanitizer or concurrency tests. Pass: Zero targeted defects appear. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Inventory | Agent scans for log interpolation, arithmetic hotspots, privilege config, permission config, access gaps, unbounded reads, and free or dispose paths. | Run targeted grep and existing analyzers. | Hotspot list exists for every touched file. |
| 2. Classify | Agent maps each hotspot to one CWE row. | Review hotspot list. | Every hotspot has one CWE label or explicit out-of-scope note. |
| 3. Fix | Agent adds logging sanitizer use, arithmetic guards, low-privilege settings, caps, and lifetime fixes. | Run targeted build for touched component. | Build passes with zero new errors. |
| 4. Run tests | Agent runs unit tests, integration tests, load tests, and sanitizer jobs already in repo. | Run existing tests for touched scope. | All targeted tests pass. |
| 5. Re-scan | Agent runs grep and analyzers again. | Re-run inventory commands. | Zero unexplained targeted hits remain in touched files. |

## Decision Matrix

| Condition | Use |
|---|---|
| Log message contains user-controlled text | Use structured placeholder plus sanitizer helper |
| Arithmetic uses untrusted size or offset | Use checked math or pre-guard |
| Service or job runs with elevated identity | Use low-privilege identity and scoped elevation |
| Endpoint allocates or queues unbounded work | Use size, rate, and concurrency caps |
| Native or unsafe code owns memory | Use ownership-safe lifetime pattern and sanitizer coverage |

## Repo Notes

| Area | Agent Uses | Agent Avoids |
|---|---|---|
| Logging | `LogSanitizationHelper.SanitizeForLog` or `SanitizingLogger<T>` | Ad-hoc sanitizer logic |
| ASP.NET Core caps | Built-in rate limiter, request size limit, Kestrel caps | Unbounded body reads |
| Storage and files | Explicit ACL or file mode | Public or world-write defaults |

## Verification Checklist

Agent verifies:
- [ ] Touched files use structured logging for hostile text
- [ ] Touched arithmetic paths reject overflow inputs
- [ ] Elevated privilege defaults are absent from touched configs
- [ ] Resource caps reject oversized or over-rate input
- [ ] Sanitizer or concurrency checks report zero new lifetime defects

## Inputs

| Input | Required | Default |
|---|---|---|
| Repo or component path | Yes | - |
| CWE subset | No | All 7 |
| Load or sanitizer command | No | Existing repo default |

## Outputs

Agent generates:
- Hotspot list with CWE labels
- Patch set for logging, arithmetic, privilege, permission, cap, and lifetime fixes
- Verification notes with analyzer, test, and load results

## References

- MITRE CWE: https://cwe.mitre.org
- ASP.NET Core logging and rate limiting docs
- Static and runtime tooling: Roslyn, ASan, tfsec, Checkov
