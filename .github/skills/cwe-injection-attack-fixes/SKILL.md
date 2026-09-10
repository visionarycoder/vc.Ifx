---
name: cwe-injection-attack-fixes
title: CWE Injection & Code-Execution Attack Fixes
description: Consolidated guidance for injection and code-execution CWEs (CWE-77, CWE-78, CWE-79, CWE-89, CWE-94, CWE-98) with sink patterns, fix patterns, and measurable verification.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 1760
prerequisites:
  - Familiarity with the codebase primary languages (C#, JS/TS, PHP, Java, Python)
  - Access to CI and a test harness for integration tests
  - Ability to run grep, build, and test tooling
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - cwe-input-validation-fixes
  - cwe-authentication-authz-fixes
  - cwe-data-protection-fixes
  - cwe-resource-management-fixes
  - cwe-miscellaneous-security-fixes
  - security-controller
appliesTo: "**/*.{cs,js,ts,php,py,java,ps1,sh}"
tags:
  - injection
  - sql
  - xss
  - command-injection
  - rce
  - php
  - dotnet
  - javascript
---

# CWE Injection & Code-Execution Attack Fixes

Agent uses this bundle for untrusted input that reaches SQL, shell, HTML, eval, or include sinks.

## Activation

**USE FOR:** CWE-77, CWE-78, CWE-79, CWE-89, CWE-94, CWE-98.  
**DO NOT USE FOR:** Authentication defects, pure data exposure defects, or broad architecture redesign.

## Critical Rules

| Rule | Test | Pass |
|---|---|---|
| Agent never concatenates untrusted input into SQL, shell, HTML, eval, or include sinks. | Run grep for known sink patterns. | Zero unsafe sink concatenations remain in touched files. |
| Agent uses parameterized APIs or explicit allow-lists. | Run unit and integration tests with attack payloads. | Attack payloads do not alter command, query, or render flow. |
| Agent uses contextual encoding for HTML, URL, and JavaScript output. | Run XSS tests with script payloads. | Rendered output is encoded or rejected. |
| Agent avoids eval and dynamic file include on untrusted input. | Run grep for eval and include patterns. | Zero untrusted eval or include paths remain. |
| Agent runs services and database accounts with least privilege. | Run deployment or config checks already present in repo. | Touched service and DB paths use documented low-privilege identities. |

## CWE Coverage

| CWE | Concern | Trigger Pattern | Fix Pattern | Verification |
|---|---|---|---|---|
| 77 | Interpreter injection | XPath, LDAP, or shell fragments built with user input | Use typed APIs, argument arrays, or allow-lists | Run attack payload tests. Pass: Payload is treated as data. |
| 78 | OS command injection | `Process.Start`, `exec`, `subprocess(..., shell=True)` | Use `ArgumentList`, `spawn`, `execFile`, or `shell=False` | Run command payload tests. Pass: No shell expansion occurs. |
| 79 | Cross-site scripting | `Html.Raw`, `innerHTML`, `document.write` with user content | Use framework encoding, `textContent`, or sanitizer | Run browser or rendering tests. Pass: Script payload does not run. |
| 89 | SQL injection | Raw SQL string concatenation, `FromSqlRaw`, `CommandText = ... + input` | Use parameters, LINQ, or interpolated safe APIs | Run SQL payload tests. Pass: Query returns safe reject or expected rows only. |
| 94 | Code injection | `eval`, `new Function`, script evaluation, dynamic compiler input | Replace with DSL, allow-list, or sandboxed admin-only path | Run grep and attack tests. Pass: Untrusted input is not executed. |
| 98 | PHP file inclusion | `include` or `require` from request input | Map logical names to fixed files and fixed directories | Run path payload tests. Pass: Only allow-listed files load. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Inventory | Agent runs sink search for SQL, shell, XSS, eval, and include patterns. | Run `rg -n "Process\.Start|exec\(|innerHTML|Html\.Raw|FromSqlRaw|CommandText\s*=|eval\(|new Function\(|include|require"`. | Hotspot list exists for every touched file. |
| 2. Classify | Agent maps each hotspot to one CWE and one sink type. | Review hotspot list. | Every hotspot has one row in the coverage matrix. |
| 3. Fix | Agent replaces unsafe sinks with parameters, encoders, argument arrays, or allow-lists. | Run targeted build for touched component. | Build passes with zero new errors. |
| 4. Run tests | Agent runs unit tests and integration tests with attack payloads. | Run existing tests for touched component. | All targeted tests pass. |
| 5. Re-scan | Agent runs grep again for targeted sink patterns. | Re-run inventory command. | Zero unexplained sink hits remain in touched files. |

## Decision Matrix

| Condition | Use |
|---|---|
| SQL string contains untrusted input | Use parameterized query or LINQ |
| Shell call contains user-controlled argument | Use argument array and `UseShellExecute = false` |
| HTML output contains user-controlled text | Use framework encoding or sanitizer |
| Feature requests dynamic rules | Use fixed DSL or allow-list |
| PHP route selects view or file | Use logical-name mapping to fixed path |

## Language Patterns

| Language | Agent Uses | Agent Avoids |
|---|---|---|
| C# | `DbParameter`, LINQ, `ProcessStartInfo.ArgumentList`, Razor encoding | Raw SQL concatenation, `Html.Raw` on user input |
| JavaScript | `spawn`, `execFile`, `textContent`, sanitizer | `exec`, `innerHTML`, `eval` |
| Python | `subprocess.run(list, shell=False)` | `shell=True` with user input |
| PHP | Allow-listed includes, fixed directories | Request-driven `include` or `require` |

## Verification Checklist

Agent verifies:
- [ ] SQL payloads do not alter query shape
- [ ] Shell payloads do not trigger command chaining
- [ ] Script payloads do not run in rendered output
- [ ] Touched files contain zero untrusted eval paths
- [ ] Touched include paths use allow-listed file mapping

## Inputs

| Input | Required | Default |
|---|---|---|
| Repo or component path | Yes | - |
| Language scope | No | All matching files |
| CWE subset | No | All 6 |

## Outputs

Agent generates:
- Sink inventory with CWE labels
- Patch set that replaces unsafe sink usage
- Test notes with attack payload coverage
- Verification notes with grep and test results

## References

- OWASP Injection guidance: https://owasp.org/www-community/attacks/Injection
- Microsoft SQL injection guidance: https://learn.microsoft.com/dotnet/secure/sql-injection
- MDN XSS guidance: https://developer.mozilla.org/en-US/docs/Glossary/Cross-site_scripting
- PHP include guidance: https://www.php.net/manual/en/
