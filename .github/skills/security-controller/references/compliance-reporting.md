---
title: Security Controller Compliance Reporting
doc_type: reference
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 800
prerequisites:
  - security-controller
related_skills:
  - security-controller
  - cwe-data-protection-fixes
  - cwe-injection-attack-fixes
appliesTo: '**/*.{md,json,cs}'
tags:
  - security
  - compliance
  - owasp
  - mitre
---
# Security Controller Compliance Reporting

This reference supports `security-controller` report mode with OWASP Top 10 mapping, MITRE CWE Top 25 coverage, and compact report templates.

## OWASP Top 10 Mapping

| OWASP Category | Primary CWE Families |
|---|---|
| A01 Broken Access Control | Authentication and authorization, miscellaneous security |
| A02 Cryptographic Failures | Data protection |
| A03 Injection | Injection attacks, input validation |
| A04 Insecure Design | Input validation, authentication and authorization, resource management |
| A05 Security Misconfiguration | Data protection, format and external control, miscellaneous security |
| A06 Vulnerable and Outdated Components | Data protection, format and external control |
| A07 Identification and Authentication Failures | Authentication and authorization |
| A08 Software and Data Integrity Failures | Data protection, format and external control |
| A09 Security Logging and Monitoring Failures | Resource management, data protection |
| A10 Server-Side Request Forgery | Miscellaneous security |

## MITRE CWE Top 25 Checklist

| CWE | Title | Primary Family | Status Values |
|---:|---|---|---|
| 20 | Improper Input Validation | Input validation | Covered, Detected, Remediated, Not applicable |
| 22 | Path Traversal | Miscellaneous security | Covered, Detected, Remediated, Not applicable |
| 77 | Command Injection | Injection attacks | Covered, Detected, Remediated, Not applicable |
| 78 | OS Command Injection | Injection attacks | Covered, Detected, Remediated, Not applicable |
| 79 | Cross-Site Scripting | Injection attacks | Covered, Detected, Remediated, Not applicable |
| 89 | SQL Injection | Injection attacks | Covered, Detected, Remediated, Not applicable |
| 94 | Code Injection | Injection attacks | Covered, Detected, Remediated, Not applicable |
| 200 | Information Exposure | Data protection | Covered, Detected, Remediated, Not applicable |
| 287 | Improper Authentication | Authentication and authorization | Covered, Detected, Remediated, Not applicable |
| 352 | CSRF | Authentication and authorization | Covered, Detected, Remediated, Not applicable |
| 400 | Uncontrolled Resource Consumption | Resource management | Covered, Detected, Remediated, Not applicable |
| 434 | Unrestricted File Upload | Miscellaneous security | Covered, Detected, Remediated, Not applicable |
| 502 | Deserialization of Untrusted Data | Data protection | Covered, Detected, Remediated, Not applicable |
| 798 | Hard-coded Credentials | Data protection | Covered, Detected, Remediated, Not applicable |
| 862 | Missing Authorization | Authentication and authorization | Covered, Detected, Remediated, Not applicable |
| 918 | SSRF | Miscellaneous security | Covered, Detected, Remediated, Not applicable |

## Coverage Rules

| Rule | Agent Action |
|---|---|
| Covered | Agent marks a CWE as covered when the controller checks that family in scan or verify mode. |
| Detected | Agent marks detected when current evidence contains one or more findings. |
| Remediated | Agent marks remediated when fixes exist and verify mode confirms closure in changed scope. |
| Not applicable | Agent records stack or scope rationale. |

## Posture Summary Fields

| Field | Meaning |
|---|---|
| Total vulnerabilities detected | Count of current findings in the selected scope |
| Remediated | Count of verified fixed findings |
| Remaining | Count of unresolved or accepted-risk findings |
| OWASP coverage | Percent of mapped categories with scan or verify evidence |
| CWE coverage | Percent of tracked CWE entries with scan or verify evidence |

## Report Template

| Section | Template |
|---|---|
| Heading | `Security Compliance Report` |
| OWASP coverage | `A01 Broken Access Control -> Authentication and authorization, miscellaneous security -> [Status]` |
| MITRE coverage | `CWE-89 SQL Injection -> [Status]` |
| Posture | `Total vulnerabilities detected: X`<br>`Remediated: Y`<br>`Remaining: Z`<br>`Coverage: X% of OWASP Top 10, Y% of CWE Top 25` |
| Evidence | `Evidence: analyzer run, targeted build, targeted tests, file hotspot summary` |

## Reporting Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent collects current scan and verify evidence. | Review evidence list. | Report uses current data. |
| 2 | Agent maps findings and clean areas to OWASP and CWE rows. | Review mapping table. | Every reported status has a basis. |
| 3 | Agent calculates totals and coverage percentages. | Recheck counts. | Totals align with evidence. |
| 4 | Agent records remaining risk and rationale. | Review open items. | Residual risk is explicit. |

## Common Reporting Errors

| Error | Correction |
|---|---|
| Coverage exceeds actual evidence | Count only categories or CWEs touched by scan or verify evidence. |
| Remediated count includes unverified fixes | Move those items back to detected until verify mode passes. |
| Not-applicable lacks rationale | Add stack, framework, or scope basis. |
| OWASP mapping hides CWE detail | Include both category view and CWE row view. |
