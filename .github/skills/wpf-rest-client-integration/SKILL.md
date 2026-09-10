---
name: wpf-rest-client-integration
title: WPF REST Client Integration
description: Integrate WPF view models with REST APIs through typed clients, centralized token flow, resilient transport policies, and user-safe error handling.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1182
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-api-client-resilience
  - jwt-authentication-dotnet
  - wpf-mvvm-implementation
appliesTo: '**/*.{cs,xaml,json,csproj}'
tags:
  - wpf
  - httpclient
  - refit
  - jwt
---
# WPF REST Client Integration

Agent connects WPF features to REST APIs through typed clients, delegated authentication, resilient policies, and desktop-friendly error translation.

## When to Use

| Condition | Use |
|---|---|
| Agent adds CRUD, search, or workflow calls from a WPF feature to an HTTP API. | Agent uses this skill. |
| Agent replaces raw `new HttpClient()` usage with DI-owned clients. | Agent uses this skill. |
| Agent needs Problem Details to appear as user-safe desktop messages. | Agent uses this skill. |

## When Not to Use

| Condition | Use |
|---|---|
| Agent works on gRPC, named pipes, or direct database access. | Agent uses a transport-specific skill. |
| Agent creates only the desktop shell. | Agent uses `wpf-project-setup`. |
| Agent focuses on API design instead of the desktop client. | Agent uses a web API skill. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| API base URL | Yes | Base URL identifies the upstream host. |
| Authentication model | Yes | Model identifies token acquisition and refresh ownership. |
| Client style | Yes | Style identifies typed interfaces, generated clients, or wrappers. |
| Error contract | No | Contract identifies Problem Details or equivalent failure payloads. |
| Resilience requirements | No | Requirements identify timeout, retry, and cancellation rules. |

## Workflow

| Step | Agent action | Output | Test | Pass |
|---|---|---|---|---|
| 1. Register HTTP infrastructure | Agent registers `IHttpClientFactory`, typed clients, and optional resilience packages in DI. | Client registration | Agent resolves the client from DI. | The client resolves without ad hoc construction. |
| 2. Define typed contracts | Agent models endpoints with explicit DTOs and one typed client boundary. | API contract | Agent reviews the client interface. | DTOs and routes reflect the intended API version. |
| 3. Centralize authentication | Agent adds a handler or wrapper that applies bearer tokens outside view models. | Auth flow | Agent sends a protected request. | Requests carry the expected authentication behavior. |
| 4. Translate transport errors | Agent maps HTTP, timeout, and Problem Details failures into desktop-safe messages. | Error translation | Agent forces a failing response. | The UI receives safe messages instead of raw exceptions. |
| 5. Expose async view-model flows | Agent calls the client from async commands that surface loading and cancellation state. | Command integration | Agent triggers slow or canceled work. | The UI remains responsive and state resets correctly. |
| 6. Add resilience and tracing | Agent applies bounded retry rules to safe calls and flows correlation identifiers through requests and logs. | Resilience and tracing plan | Agent observes retries and correlation data. | Safe calls retry predictably and logs correlate client and server activity. |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Client verification | Existing tests or targeted `dotnet test [test-project].csproj` | Typed client and view-model tests pass |
| Live-call verification | Manual or integration smoke path | One representative screen completes a real round trip |

## Verification Checklist

- [ ] Agent resolves HTTP clients from DI.
- [ ] Agent applies tokens centrally instead of inside view models.
- [ ] Agent maps Problem Details to user-safe feedback.
- [ ] Agent keeps network calls async, visible, and cancelable.
- [ ] Agent applies resilience only to approved call paths.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Agent creates `HttpClient` inside view models. | Agent uses `IHttpClientFactory` or a typed client registration. |
| Agent copies bearer-token code into every API method. | Agent uses a delegating handler or central transport wrapper. |
| Agent retries unsafe mutations blindly. | Agent limits retries to idempotent or explicitly safe operations. |
| Agent shows raw Problem Details JSON to users. | Agent translates payloads into concise desktop messages. |
