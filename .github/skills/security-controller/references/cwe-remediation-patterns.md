---
title: Security Controller CWE Remediation Patterns
doc_type: reference
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: high
estimated_tokens: 1500
prerequisites:
  - security-controller
related_skills:
  - security-controller
  - cwe-authentication-authz-fixes
  - cwe-data-protection-fixes
  - cwe-format-external-fixes
  - cwe-injection-attack-fixes
  - cwe-input-validation-fixes
  - cwe-miscellaneous-security-fixes
  - cwe-resource-management-fixes
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - security
  - cwe
  - remediation
  - testing
---
# Security Controller CWE Remediation Patterns

This reference supports `security-controller` remediate and verify modes with family-level fix patterns, compact examples, and focused tests.

## Remediation Order

| Order | Family | Why |
|---|---|---|
| 1 | Injection attacks | Direct code, SQL, shell, or browser execution path |
| 2 | Authentication and authorization | Privilege and identity boundary protection |
| 3 | Data protection | Secret exposure and unsafe object materialization |
| 4 | Miscellaneous security | Path, network, redirect, and upload control |
| 5 | Format and external control | Executable resolution and format-driven execution risk |
| 6 | Input validation | Trust-boundary stability and sink protection |
| 7 | Resource management | Abuse resistance and resilience hardening |

## Input Validation Patterns

| Problem | Weak Pattern | Strong Pattern | Verification |
|---|---|---|---|
| Missing model validation | Values flow directly from request to business logic | Validate DTOs at the boundary and reject invalid state early | Send invalid payload and confirm rejection path |
| Numeric bounds missing | Arithmetic or indexing trusts caller input | Clamp or reject out-of-range values before use | Edge-case tests cover min, max, and overflow candidates |
| Enum or discriminator trust | Caller-controlled strings select unsafe behavior | Use allow-lists or explicit parsing with rejection | Invalid selector does not reach handler |

## Authentication and Authorization Patterns

| Problem | Weak Pattern | Strong Pattern | Verification |
|---|---|---|---|
| Missing authorization | Sensitive endpoint lacks auth attribute or policy check | Add endpoint and resource-level authorization | Unauthenticated or unauthorized request is blocked |
| IDOR | Caller selects arbitrary object ID without ownership check | Bind action to caller identity and resource ownership | Cross-tenant access fails |
| CSRF gap | State-changing browser request lacks anti-forgery pairing | Use framework anti-forgery and same-site protections | Forged request fails |

## Injection Attack Patterns

| Problem | Weak Pattern | Strong Pattern | Verification |
|---|---|---|---|
| SQL injection | String-concatenated query text | Parameterized commands or query composition APIs | Payload with quote metacharacters does not change query shape |
| Command injection | User input joins shell command text | Use direct process arguments or reject unsafe input | Metacharacters remain data, not control |
| XSS | Raw HTML or script-capable content reaches response | Encode by default and allow only sanitized rich text | Script payload renders inert |

## Data Protection Patterns

| Problem | Weak Pattern | Strong Pattern | Verification |
|---|---|---|---|
| Hard-coded secret | Secret lives in source or static config | Use environment-backed secret provider or vault integration | Search finds zero secret literals in changed files |
| Unsafe deserialization | Untrusted data materializes permissive types | Use safe serializer settings and bounded contracts | Malicious payload is rejected |
| Sensitive data exposure | Internal exception or secret reaches client payload | Return safe error contract and log detail server-side | Response omits sensitive detail |

## Miscellaneous Security Patterns

| Problem | Weak Pattern | Strong Pattern | Verification |
|---|---|---|---|
| Path traversal | User input joins file-system path directly | Normalize path, anchor to allow-listed root, reject escape attempts | `..` payload fails |
| SSRF | Arbitrary URL reaches backend fetch | Use allow-listed hosts or route keys | External or private-network probes fail |
| Open redirect | Redirect target trusts caller input | Restrict to local or allow-listed destinations | External redirect payload fails |
| Upload control gap | File upload trusts extension or client metadata | Validate type, size, storage path, and scanning flow | Disallowed file is rejected |

## Format and External Control Patterns

| Problem | Weak Pattern | Strong Pattern | Verification |
|---|---|---|---|
| Search-path control | Runtime resolves tools or libraries from ambient path | Use absolute trusted paths and fixed resolution order | Ambient path changes do not alter target |
| DLL hijacking | Load behavior trusts mutable directory layout | Restrict load path and deployment layout | Untrusted adjacent file is ignored |
| Format control | Untrusted format string or template reaches interpreter sink | Treat template data as data, not instructions | Crafted tokens remain inert |

## Resource Management Patterns

| Problem | Weak Pattern | Strong Pattern | Verification |
|---|---|---|---|
| Unbounded request work | Pagination, upload, or loop lacks limits | Add caps, timeout, and cancellation | Oversized request fails fast |
| Logging injection | Raw user input writes directly into logs | Use structured logging and sanitize control characters | Forged line breaks do not corrupt logs |
| Cleanup gap | Disposable or stream lifetime is implicit and leaky | Use scoped lifetime or deterministic disposal | Stress test shows stable resource use |

## Before and After Mini Examples

| Family | Before | After |
|---|---|---|
| SQL injection | `var sql = "select * from Users where Name = '" + name + "'";` | `var command = new SqlCommand("select * from Users where Name = @name"); command.Parameters.AddWithValue("@name", name);` |
| Authorization | `return await repository.GetOrder(id);` | `return await repository.GetOrderForOwner(id, userId);` |
| Path traversal | `var path = Path.Combine(root, input);` | `var fullPath = ValidateChildPath(root, input);` |
| Secret handling | `var apiKey = "secret-value";` | `var apiKey = configuration["ApiKey"];` |
| Logging injection | `logger.LogInformation("User " + userInput);` | `logger.LogInformation("User {UserInput}", SanitizeForLog(userInput));` |

## Testing Strategies

| Family | Test Focus | Examples |
|---|---|---|
| Injection | Attacker payload stays inert | SQL quotes, shell metacharacters, script tags |
| Auth/authz | Access boundary holds | Anonymous, wrong-user, wrong-role, cross-tenant |
| Data protection | Secret and object boundaries hold | Secret grep, serializer rejection, safe error payload |
| Miscellaneous | Path and URL boundaries hold | `..`, private IP SSRF, external redirect, unsafe upload |
| Resource management | Abuse limits hold | Oversized payload, repeated requests, cancellation |

## Verification Loop

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent fixes one family or one hotspot file batch. | Build the smallest affected scope. | Build succeeds. |
| 2 | Agent runs targeted tests for the changed behavior. | Existing test runner | Tests succeed. |
| 3 | Agent re-runs analyzer checks for the same scope. | Analyzer build | Fixed findings are gone. |
| 4 | Agent records remaining findings and next family. | Review notes | Remediation order stays explicit. |

## Grouping Rules

| Rule | Use |
|---|---|
| One file, many same-family findings | Fix together |
| One file, many family findings with one trust boundary | Fix in one pass and record primary family |
| Many files, one shared helper change | Edit helper first, then adjust call sites |
| Framework-wide control | Use centralized middleware, filter, or serializer configuration |

## Common Review Questions

| Question | Expected Evidence |
|---|---|
| Does untrusted input reach a dangerous sink? | Boundary validation plus safe sink usage |
| Does identity bind to data access? | Policy and ownership enforcement |
| Does a client receive sensitive detail? | Safe response contract and server-side logging |
| Do limits resist abuse? | Explicit caps, timeout, and cancellation |
| Did fixes stay surgical? | Grouped file-local edits and targeted verification |
