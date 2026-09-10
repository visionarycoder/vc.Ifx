---
name: global-exception-handling
title: Global Exception Handling
description: Implement global ASP.NET Core exception handling with RFC 9457 Problem Details, stable status mapping, correlation-aware logging, and streaming-safe failure behavior.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1279
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - rfc-9457-compliance
  - webapi-error-contract-hardening
  - structured-logging-serilog
appliesTo: '**/*.{cs,json}'
tags:
  - exceptions
  - aspnetcore
  - problem-details
  - webapi
---
# Global Exception Handling

Agent creates one global exception boundary for ASP.NET Core so that clients receive stable Problem Details contracts and operators receive correlated diagnostics.

## When to Use

| Condition | Use |
|---|---|
| Agent adds or refines global error handling in an ASP.NET Core API. | Agent uses this skill. |
| Agent replaces inconsistent payloads or HTML error pages with Problem Details. | Agent uses this skill. |
| Agent needs centralized exception-to-status mapping and correlation-aware logging. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent handles normal validation outcomes that are not exceptional. | Agent returns direct 4xx responses instead. |
| Agent works in a desktop-only or worker-only host with no HTTP response contract. | Agent uses host-specific exception handling. |
| Agent focuses only on per-endpoint refinement while a correct global handler already exists. | Agent performs local cleanup only. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Host type | Yes | Host identifies minimal API or controller-based ASP.NET Core. |
| Exception categories | Yes | Categories identify validation, not-found, dependency, auth, and unexpected failures. |
| Problem type URIs | No | URIs identify stable recurring error categories. |
| Correlation strategy | No | Strategy identifies trace IDs or instance identifiers. |
| Environment detail policy | Yes | Policy identifies development and production detail levels. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Define the exception map | Agent records which exception families map to which HTTP status codes and titles. | Exception map | Agent reviews the map. | The map avoids ambiguous or inconsistent status handling. |
| 2. Register one global boundary | Agent registers `AddProblemDetails`, `UseExceptionHandler`, and `IExceptionHandler` or equivalent middleware in one host boundary. | Host registration | Agent builds and starts the host. | The pipeline contains one active global exception boundary. |
| 3. Write stable Problem Details | Agent writes `type`, `title`, `status`, and safe `detail` or `instance` values consistently. | Problem Details policy | Agent triggers representative failures. | Failure responses use `application/problem+json` with safe fields. |
| 4. Log with correlation | Agent logs exceptions with path, method, and correlation identifiers that match the response contract. | Logging policy | Agent traces one failing request. | Operators correlate the response with server diagnostics. |
| 5. Separate expected validation flow | Agent returns direct validation results for normal client errors instead of throwing exceptions for those paths. | Validation split | Agent triggers invalid input. | Normal client mistakes return direct 4xx responses. |
| 6. Respect streaming rules | Agent validates before the first write and stops started streams safely without injecting Problem Details into a started stream. | Streaming policy | Agent tests a started-stream failure path. | Streaming endpoints fail safely and log correlation metadata. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Build verification | `dotnet build [project].csproj` | Zero compile errors |
| HTTP verification | Existing integration tests or targeted `dotnet test [test-project].csproj` | Error responses return expected status and content type |
| Log correlation verification | Manual or automated request trace | Response identifiers match server diagnostics |

## Verification Checklist

- [ ] Agent centralizes exception handling.
- [ ] Agent returns `application/problem+json` for unhandled HTTP failures.
- [ ] Agent keeps `detail` safe for the active environment.
- [ ] Agent separates expected validation flow from exceptional flow.
- [ ] Agent respects pre-stream and post-stream-start error semantics.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent catches exceptions in every controller action. | Agent centralizes exception handling in middleware or `IExceptionHandler`. |
| Agent returns 200 with an error payload. | Agent maps failures to the correct 4xx or 5xx response. |
| Agent writes stack traces or secrets into Problem Details. | Agent logs sensitive diagnostics server-side only. |
| Agent tries to inject Problem Details after streaming starts. | Agent stops the stream and logs the correlation data instead. |
