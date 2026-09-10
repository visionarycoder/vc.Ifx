---
name: jwt-authentication-dotnet
title: JWT Authentication for .NET
description: Implement secure JWT authentication for .NET APIs with strict token verification, refresh handling, and safe claim design.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1223
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - webapi-authz-hardening
  - dotnet-webapi
  - configuration-options-pattern
appliesTo: '**/*.{cs,csproj,sln,slnx,props,targets,json,config}'
tags:
  - jwt
  - authentication
  - dotnet
  - api
  - bearer
---

# JWT Authentication for .NET

Agent implements JWT authentication with strict token verification, short-lived access tokens, and explicit refresh rules.

## When to Use

| Condition | Use |
|---|---|
| Agent builds stateless API authentication | Use this skill |
| Agent adds bearer token verification | Use this skill |
| Agent defines refresh-token workflow | Use this skill |
| Agent fixes claim scope or signing configuration | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Server-rendered app uses cookie auth | Use cookie guidance |
| Requirement depends on immediate server-side revocation for every request | Use stateful session design |
| Caller is machine-only and OAuth client credentials already exist | Use access-token client guidance |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Signing key or certificate | Yes | Strong secret or asymmetric key material |
| Issuer and audience | Yes | Values used for strict token verification |
| Access-token lifetime | Yes | Minutes or seconds |
| Claim model | Yes | Subject, roles, permissions, and tenant scope |
| Refresh strategy | No | Rotation, storage, revocation, and expiry |

## Workflow

1. Agent binds JWT settings through typed configuration.
2. Agent configures bearer authentication with strict issuer, audience, lifetime, and signing verification.
3. Agent centralizes token issuance and refresh-token handling.
4. Agent verifies unauthorized, expired, and refreshed paths.

Test: Run `dotnet build [project].csproj`.
Pass: Zero compile errors. Zero hard-coded secrets in changed scope.

## Rule Matrix

| Rule | Agent verifies | Detection pattern | Fix |
|---|---|---|---|
| JWT-001 | Signing material stays outside source code | Hard-coded secrets or sample fallback values | Read from secure configuration or key store |
| JWT-002 | Token verification is strict | Missing issuer, audience, lifetime, or signing-key verification | Enable all required verification options |
| JWT-003 | Access tokens stay short-lived | Multi-day access token lifetimes | Reduce lifetime and use refresh path |
| JWT-004 | Refresh tokens use separate policy | Access and refresh tokens share same handling | Store and rotate refresh tokens separately |
| JWT-005 | Claims stay minimal and authorization-focused | Payload stores large profiles or mutable app state | Keep subject and authorization claims only |
| JWT-006 | Algorithms are explicit | Runtime accepts unintended signing algorithm | Pin allowed algorithm and key type |
| JWT-007 | Error responses stay safe | Token verification details leak to caller | Return safe 401 or 403 response |
| JWT-008 | Authorization uses claims or policies consistently | Manual role parsing in feature code | Use policy or claim-based authorization |

## Pattern Matrix

| Scenario | Use | Avoid |
|---|---|---|
| SPA or mobile API | Short-lived bearer token plus refresh token | Long-lived access token only |
| Machine-to-machine API | Access token with explicit audience and scope | User refresh-token workflow |
| Role-based authorization | Policy and claim mapping | String parsing in controllers |
| Key rotation | Configuration-backed key source | Rebuild code for every key change |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Secret verification | Search changed files for inline JWT secrets or sample keys | Zero unauthorized matches |
| Auth verification | Run `dotnet test [test-project].csproj --filter Auth` when tests exist | Zero failing tests |

## Verification Checklist

Agent verifies:
- [ ] Signing material stays outside source code
- [ ] Issuer, audience, lifetime, and signing verification are explicit
- [ ] Access tokens stay short-lived
- [ ] Refresh tokens use separate storage and rotation rules
- [ ] Claims stay minimal and authorization-focused

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Weak sample secret remains in code | Move secret to secure configuration |
| Refresh token is treated like access token | Split storage and verification policy |
| Default clock tolerance is left unexplained | Set explicit tolerance |
| Claims carry mutable app state | Keep claims minimal |
