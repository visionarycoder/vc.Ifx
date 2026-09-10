---
name: security-controller
title: Security Controller
description: >
  Manages security workflows with modes for scan, remediate, verify, and report. Executes SAST analysis, CWE remediation, and compliance reporting across injection, authentication, data protection, and resource management vulnerabilities.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: high
estimated_tokens: 2800
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - cwe-input-validation-fixes
  - cwe-authentication-authz-fixes
  - cwe-injection-attack-fixes
  - cwe-data-protection-fixes
  - cwe-resource-management-fixes
  - cwe-miscellaneous-security-fixes
  - cwe-format-external-fixes
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - security
  - cwe
  - controller
  - sast
  - compliance
---
# Security Controller

Agent uses this controller for broad security workflows that need scanning, prioritized remediation, verification, or compliance reporting across multiple CWE families.

## When to Use

| Condition | Mode |
|---|---|
| User asks to scan for security issues | `scan` |
| User asks to fix security vulnerabilities | `remediate` |
| User asks to verify security posture | `verify` |
| User asks to generate security report | `report` |
| Request spans multiple CWE families or needs one security baseline | `scan` or `remediate`, then `verify` and `report` |

## When Not to Use

| Condition | Route |
|---|---|
| One narrow vulnerability family is already named and no cross-family workflow is needed | Use the direct CWE specialist |
| Penetration testing or external scanner orchestration is requested | Use the named external workflow skill or direct repository workflow |
| Work is unrelated to security and only mentions logging, validation, or configuration incidentally | Use the domain skill that owns the change |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Scope | Yes | Repo, app area, project, file set, or endpoint surface |
| User intent | Yes | Scan, remediate, verify, or report |
| Threat focus | No | Specific CWEs, OWASP category, or broad sweep |
| Verification expectations | No | Build, tests, analyzers, endpoint checks, or report depth |
| Compliance target | No | OWASP Top 10, MITRE CWE Top 25, internal policy, or release gate |

## Mode Matrix

| User Intent | Mode | Agent Action | Output |
|---|---|---|---|
| "Scan for security issues" | `scan` | Agent runs analyzers, classifies findings by CWE family, ranks severity, and summarizes hotspots. | Security scan report |
| "Fix security vulnerabilities" | `remediate` | Agent fixes vulnerabilities in priority order, delegates by CWE family, groups edits by file, and verifies each family. | Remediation summary |
| "Verify security posture" | `verify` | Agent runs focused build, test, analyzer, and regression checks for changed scope. | Verification summary |
| "Generate security report" | `report` | Agent maps findings and fixes to OWASP Top 10 and MITRE CWE Top 25 coverage. | Compliance report |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Detect mode | Agent reads user intent and selects `scan`, `remediate`, `verify`, or `report`. | Review mode mapping. | One mode is selected. |
| 2. Execute scan | Agent runs analyzers and categorizes findings when mode is `scan` or when remediation starts without current evidence. | Analyzer run completes. | Findings are grouped by CWE family and severity. |
| 3. Execute remediation | Agent fixes vulnerabilities by priority and delegates by CWE family when mode is `remediate`. | Build and targeted tests pass after each family. | Zero regressions enter the changed scope. |
| 4. Execute verification | Agent runs build, tests, analyzers, and focused security checks for the changed scope. | Review command results. | Changed scope is clean or remaining risk is explicit. |
| 5. Generate report | Agent writes mode-specific output with severity, coverage, status, and remaining follow-up. | Review report sections. | Report contains required sections and current counts. |

## Scan Execution Matrix

| Step | Agent Action | Command or Evidence | Pass |
|---|---|---|---|
| Analyzer run | Agent runs Roslyn and .NET analyzers for the selected scope. | `dotnet build /p:EnableNETAnalyzers=true /p:AnalysisLevel=latest` | Analyzer output is captured. |
| Warning capture | Agent records all `CA*` and `SCS*` security warnings. | Build log or analyzer report | Every security finding enters the working set. |
| Family categorization | Agent maps each finding to a CWE family. | Family matrix below | Each finding has one primary family. |
| Priority ranking | Agent classifies Critical, High, Medium, or Low. | Severity rules below | Findings are ordered for action. |
| Scan report | Agent summarizes counts and hotspots. | Report template below | Report is concise and file-oriented. |

## CWE Family Matrix

| CWE Family | Trigger Patterns | Priority Bias | Remediate Route |
|---|---|---|---|
| Input validation | Bounds, overflow, model validation, trust-boundary validation gaps | Medium | `cwe-input-validation-fixes` |
| Authentication and authorization | CSRF, access control, auth bypass, missing authorization, IDOR | Critical or High | `cwe-authentication-authz-fixes` |
| Injection attacks | SQL injection, command injection, XSS, code execution, unsafe HTML sinks | Critical | `cwe-injection-attack-fixes` |
| Data protection | Secrets exposure, unsafe deserialization, information disclosure, credential storage | Critical or High | `cwe-data-protection-fixes` |
| Resource management | DoS, allocation limits, logging injection, concurrency exhaustion, lifecycle misuse | Medium or Low | `cwe-resource-management-fixes` |
| Miscellaneous security | Path traversal, SSRF, open redirect, null dereference, upload control gaps | High or Medium | `cwe-miscellaneous-security-fixes` |
| Format and external control | Search-path control, DLL hijacking, format-string or external resolution flaws | High or Medium | `cwe-format-external-fixes` |

## Priority Matrix

| Priority | Finding Types |
|---|---|
| Critical | Injection, deserialization of untrusted data, authentication bypass, missing authorization on sensitive operations |
| High | CSRF, hard-coded credentials, secrets exposure, path traversal, uncontrolled search path, SSRF |
| Medium | Input validation defects, integer overflow, unrestricted upload, logging injection, open redirect |
| Low | Resource cleanup gaps, null safety, defensive hardening findings with low exploitability |

## Remediation Delegation Matrix

| User Request Contains | Route To | Covers |
|---|---|---|
| "validation" / "bounds" / "buffer" / "overflow" | `cwe-input-validation-fixes` | Validation, bounds, memory-safety style input issues |
| "authentication" / "authorization" / "CSRF" / "access control" | `cwe-authentication-authz-fixes` | Authentication, authorization, CSRF, access-control issues |
| "injection" / "SQL" / "command" / "XSS" / "code execution" | `cwe-injection-attack-fixes` | Command, code, HTML, SQL, related injection issues |
| "disclosure" / "deserialization" / "secret" / "leak" | `cwe-data-protection-fixes` | Information disclosure, deserialization, secret handling, ownership leaks |
| "logging injection" / "DoS" / "resource" / "privilege" | `cwe-resource-management-fixes` | Logging injection, integer/resource misuse, privilege, DoS, lifecycle issues |
| "path traversal" / "upload" / "redirect" / "SSRF" / "null pointer" | `cwe-miscellaneous-security-fixes` | Path traversal, upload, null-deref, open redirect, SSRF, filesystem permission issues |
| "search path" / "DLL hijacking" / "format string" | `cwe-format-external-fixes` | External search-path and format-specific issues |

## Remediation Execution Rules

| Rule | Agent Action |
|---|---|
| Priority first | Agent processes Critical findings before High, Medium, and Low. |
| Family ownership | Agent routes each finding to one primary CWE specialist. |
| File grouping | Agent batches file-local fixes together to reduce churn. |
| Verification loop | Agent runs verification after each family or file batch. |
| Evidence continuity | Agent keeps pre-fix finding, change, and post-fix verification together in the report. |

## Cross-Cutting Security Rules

| Rule | Pattern |
|---|---|
| Input handling | Verify at trust boundary with framework-native validation |
| Dangerous sinks | Never concatenate untrusted input into SQL, shell, HTML, URLs, paths, logs |
| Logging | Use structured logging. Sanitize hostile input. |
| Errors | Expose safe external messages. Keep sensitive detail server-side. |
| Access control | Require authn/authz, ownership checks for state-changing or sensitive operations |
| Deserialization | Avoid type-open or unsafe serializers for untrusted input |
| Secrets | Keep out of source, static config |
| Resource use | Enforce request, concurrency, timeout, pagination limits |

## Verify Mode Matrix

| Verification Area | Agent Action | Test | Pass |
|---|---|---|---|
| Applicability | Agent states which CWE families are in scope and why. | Review findings map. | Every in-scope family is explicit. |
| Build health | Agent runs the smallest existing build that covers changed files. | `dotnet build` for the affected solution or project | Zero new build errors |
| Test health | Agent runs the smallest existing test set that covers changed behavior. | `dotnet test` for the affected project or solution | Zero test failures |
| Analyzer regression | Agent re-runs security analyzers on the changed scope. | Analyzer run after fixes | No new security warnings enter the changed scope |
| Remaining risk | Agent records unresolved items and rationale. | Review summary | Remaining items are explicit, prioritized, and evidenced |

## Report Mode Output

| Section | Content |
|---|---|
| Security Scan Results | Critical, High, Medium, and Low counts plus file hotspots |
| OWASP Top 10 Coverage | Category-to-family mapping with detected or remediated status |
| MITRE CWE Top 25 Coverage | Covered, detected, remediated, or not-applicable status per CWE |
| Posture Summary | Total detected, remediated, remaining, and coverage percentages |
| Evidence Notes | Commands, analyzer sources, tests, and scope boundaries |

## Report Templates

| Template | Format |
|---|---|
| Scan summary | `Security Scan Results:`<br>`- Critical: X issues across Y files`<br>`- High: X issues across Y files`<br>`- Medium: X issues across Y files`<br>`- Low: X issues across Y files` |
| Hotspot summary | `Top 5 Issues by File:`<br>`[file path] - [issue count] - [CWE families]` |
| Posture summary | `Total vulnerabilities detected: X`<br>`Remediated: Y`<br>`Remaining: Z`<br>`Coverage: X% of OWASP Top 10, Y% of CWE Top 25` |

## References

| File | Use |
|---|---|
| `references/sast-integration.md` | Analyzer configuration, custom pattern guidance, CI integration |
| `references/cwe-remediation-patterns.md` | Common fixes, before/after patterns, testing strategies |
| `references/compliance-reporting.md` | OWASP and MITRE mapping, checklist, report templates |

## Verification Checklist

| Checkpoint | Pass Condition |
|---|---|
| Mode selected | Intent maps to one execution mode. |
| Seven CWE families covered | Input validation, auth/authz, injection, data protection, resource management, miscellaneous, and format/external are present. |
| Cross-cutting rules preserved | The rules table stays intact. |
| Remediation routing preserved | Every family maps to a specialist skill. |
| Scan workflow present | Analyzer command, categorization, priority, and report template exist. |
| Verify workflow present | Build, test, analyzer, and remaining-risk checks exist. |
| Report workflow present | OWASP, MITRE, and posture summary sections exist. |
| STE wording | Modal scan returns zero prohibited matches. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Starting remediation with stale findings | Agent runs `scan` first when current evidence is absent. |
| Mixing multiple families into one unstructured change set | Agent groups by file and records one primary family per finding. |
| Treating all findings as equal | Agent uses the priority matrix before editing. |
| Claiming closure without analyzer rerun | Agent runs `verify` after each family and again at the end. |
| Reporting coverage without evidence | Agent ties OWASP and MITRE status to actual findings or verified clean scope. |
