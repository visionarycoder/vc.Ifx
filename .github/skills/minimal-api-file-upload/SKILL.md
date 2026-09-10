---
name: minimal-api-file-upload
title: Implementing File Uploads in ASP.NET Core Minimal APIs
description: Implement secure file upload endpoints in ASP.NET minimal APIs with correct binding, dual size limits, antiforgery decisions, content validation, and streaming paths.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1289
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - webapi-input-validation
  - webapi-error-contract-hardening
  - cwe-input-validation-fixes
appliesTo: '**/*'
tags:
  - minimal
  - api
  - file
  - upload
---
# Implementing File Uploads in ASP.NET Core Minimal APIs

Agent implements minimal API file uploads by selecting the correct binding model, configuring both request-size limits, validating content safely, and choosing buffered or streamed handling intentionally.

## When to Use

| Condition | Use |
|---|---|
| Agent adds multipart file upload endpoints to an ASP.NET Core minimal API. | Agent uses this skill. |
| Agent needs file-type validation, size limits, or antiforgery decisions for uploads. | Agent uses this skill. |
| Agent needs a streaming path for large uploads. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent builds MVC controller upload actions only. | Agent uses MVC-specific patterns. |
| Agent accepts a simple JSON payload with no files. | Agent uses a normal JSON endpoint design. |
| Agent handles very large transfers that require a dedicated ingestion architecture. | Agent designs a specialized streaming flow first. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| File shape | Yes | Shape identifies single file, multiple files, or mixed form fields. |
| Size limits | Yes | Limits identify both request and multipart limits. |
| Allowed content | Yes | Content identifies accepted types and signature rules. |
| Auth and antiforgery model | Yes | Model identifies cookie, JWT, or anonymous upload semantics. |
| Storage target | No | Target identifies disk, cloud storage, or downstream processing. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Choose the binding model | Agent selects `IFormFile`, `IFormFileCollection`, or manual multipart streaming based on payload size and field mix. | Binding design | Agent reviews the endpoint signature. | The signature matches the multipart shape without hidden binding ambiguity. |
| 2. Configure both size limits | Agent configures the request-body limit and the multipart form limit together. | Size-limit configuration | Agent uploads a near-limit file. | The endpoint enforces the intended limit consistently. |
| 3. Decide antiforgery behavior | Agent keeps antiforgery enabled for cookie-authenticated form posts and disables it explicitly only for safe API-style upload paths. | Antiforgery decision | Agent sends a representative request. | The endpoint follows the intended CSRF posture. |
| 4. Validate file content safely | Agent validates declared content type, file signature, and safe file naming before persistence. | Validation flow | Agent uploads valid and invalid samples. | Invalid content fails before storage and safe files store without path traversal risk. |
| 5. Choose buffered or streamed persistence | Agent keeps `IFormFile` for moderate uploads and uses multipart streaming for large uploads or chunk processing. | Persistence path | Agent uploads a representative large file. | The chosen path avoids unnecessary buffering risk. |
| 6. Return stable results and safe errors | Agent returns normal success payloads and RFC-aligned error responses without leaking filesystem or stack details. | Response contract | Agent triggers validation failures. | Clients receive safe status codes and messages. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Build verification | `dotnet build [project].csproj` | Zero compile errors |
| Happy-path verification | Representative multipart request | Valid uploads succeed |
| Failure verification | Oversize, invalid type, and invalid signature requests | Unsafe uploads fail with the expected status codes |

## Verification Checklist

- [ ] Agent selects the correct binding model for the upload shape.
- [ ] Agent configures both request and multipart size limits.
- [ ] Agent makes the antiforgery decision explicit.
- [ ] Agent validates file signatures and safe storage names.
- [ ] Agent keeps large-file paths streamed where buffering adds risk.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent configures only one upload-size limit. | Agent configures both Kestrel and multipart form limits. |
| Agent trusts `file.FileName` or extension input. | Agent generates a safe storage name and derives the extension from validated content. |
| Agent trusts `Content-Type` alone. | Agent validates magic bytes or another trusted signature rule. |
| Agent disables antiforgery on a cookie-authenticated endpoint casually. | Agent keeps antiforgery unless the endpoint uses an API-safe alternative such as JWT or anonymous upload semantics. |
