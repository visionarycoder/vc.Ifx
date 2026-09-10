---
title: Security Controller SAST Integration
doc_type: reference
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 1200
prerequisites:
  - security-controller
related_skills:
  - security-controller
  - cwe-input-validation-fixes
  - cwe-injection-attack-fixes
appliesTo: '**/*.{cs,csproj,json,yml,yaml,md}'
tags:
  - security
  - sast
  - analyzers
  - ci
---
# Security Controller SAST Integration

This reference supports `security-controller` scan and verify modes with analyzer setup, result handling, and CI evidence patterns.

## Analyzer Baseline

| Area | Configuration |
|---|---|
| Core command | `dotnet build /p:EnableNETAnalyzers=true /p:AnalysisLevel=latest` |
| Primary findings | `CA*` and `SCS*` warnings |
| Scope control | Run solution, project, or narrowed project graph that contains the target surface |
| Evidence source | Standard build output, binary log, or CI log artifact |
| Follow-up | Route each finding to one primary CWE family before remediation starts |

## Configuration Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent restores the solution or project graph when packages are required. | Restore completes. | Build inputs are present. |
| 2 | Agent runs the analyzer-enabled build on the selected scope. | Build output exists. | Security warnings are captured. |
| 3 | Agent extracts warning ID, file path, line, and message. | Review parsed findings. | Every security warning has evidence. |
| 4 | Agent maps each finding to a CWE family and severity. | Review classification table. | Every finding has one family and one priority. |
| 5 | Agent preserves the raw analyzer evidence for report mode. | Review artifact paths or log sections. | Report links to actual evidence. |

## Analyzer Mapping Hints

| Signal | Likely Family | Notes |
|---|---|---|
| SQL, command, or HTML sink warnings | Injection attacks | Treat string-concatenated sinks as Critical unless compensating controls are explicit. |
| Missing auth or access control warnings | Authentication and authorization | Raise to Critical for sensitive write paths or privileged operations. |
| Deserialization or secret-handling warnings | Data protection | Raise to Critical for untrusted payloads or source-controlled secrets. |
| Validation, bounds, or conversion warnings | Input validation | Escalate when the value reaches a sink without boundary checks. |
| Logging, resource, or allocation warnings | Resource management | Escalate when unbounded loops, payloads, or log forgery risks exist. |
| Path, URL, redirect, or SSRF warnings | Miscellaneous security | Escalate to High when network or file-system reach expands. |
| Search-path or format-control warnings | Format and external control | Treat deployment-time control gaps as High when executable resolution changes. |

## Custom Analyzer Pattern Guidance

| Pattern | Detection Idea | Evidence Target |
|---|---|---|
| Unsafe dynamic SQL | Search query builders and string interpolation into command text | Repository grep plus analyzer warnings |
| Shell execution with input flow | Search `ProcessStartInfo`, command wrappers, or script launchers | Analyzer output and sink review |
| Untrusted HTML rendering | Search `HtmlString`, raw HTML helpers, or Angular trust bypass APIs | Template or controller evidence |
| Weak authorization path | Search missing `[Authorize]`, custom filter gaps, or ownership-free updates | Endpoint or handler evidence |
| Hard-coded secret flow | Search connection strings, tokens, and credential literals | Source file and configuration evidence |
| Unsafe deserialization | Search open polymorphic settings or permissive binders | Serializer configuration evidence |

## Result Normalization

| Field | Purpose |
|---|---|
| Warning ID | Stable key for triage and deduplication |
| File path | File hotspot reporting |
| Line | Review precision |
| Message | Human triage context |
| CWE family | Remediation routing |
| Priority | Fix order |
| Status | Detected, remediated, verified, suppressed with rationale, or not applicable |

## Severity Rules

| Priority | Decision Rule |
|---|---|
| Critical | Untrusted input reaches code execution, SQL execution, auth bypass, or unsafe deserialization path |
| High | Sensitive data exposure, path traversal, SSRF, CSRF, or executable search-path control exists |
| Medium | Validation, redirect, upload, logging injection, or bounded resource misuse exists |
| Low | Cleanup, null safety, or defensive hardening issue exists with low exploitability |

## CI/CD Integration

| Stage | Agent Action | Test | Pass |
|---|---|---|---|
| Pull request | Agent runs analyzer build on changed projects. | CI job completes. | Security warnings are visible in logs or annotations. |
| Remediation gate | Agent blocks completion when new Critical or High findings exist in changed scope. | Compare baseline to current run. | No new blocking findings remain. |
| Verification gate | Agent reruns analyzers after fixes. | CI rerun completes. | Fixed findings no longer appear. |
| Reporting gate | Agent stores normalized findings for report mode. | Review artifact. | Compliance report has a current evidence source. |

## Suppression Rules

| Rule | Action |
|---|---|
| False positive | Record explicit rationale tied to file and warning ID. |
| Accepted risk | Record scope, owner, and expiration note in the report. |
| Framework limitation | Record the existing compensating control and residual risk. |
| Legacy hotspot | Record phased remediation status and next verification point. |

## Output Template

| Section | Content |
|---|---|
| Analyzer command | Exact build command used |
| Scope | Solution, project, or directory |
| Finding totals | Counts by priority and family |
| Hotspots | Top files by issue count |
| Blocking findings | Critical and High items with file paths |
| Next route | Family specialist skill for remediation |

## Common Failure Modes

| Failure Mode | Correction |
|---|---|
| Build log truncates findings | Re-run the narrowed scope and preserve the complete output artifact. |
| Findings mix with non-security warnings | Filter to `CA*` and `SCS*` security-relevant IDs in the working set. |
| Severity looks inflated | Recheck exploit path, sink reachability, and compensating controls. |
| Severity looks understated | Recheck write paths, privileged operations, and attacker-controlled data flow. |
| CI shows new warnings after fixes | Re-run verify mode and inspect adjacent files for regressions. |
