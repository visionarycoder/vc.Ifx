---
name: azure-ad-authentication-dotnet
description: Integrate Microsoft Entra ID authentication in .NET with Microsoft.Identity.Web or MSAL.NET, tenant selection, downstream API access, role enforcement, and testable validation steps.
title: Azure AD Authentication for .NET
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1560
prerequisites:
  - dotnet-webapi
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - aspnetcore-identity-setup
  - webapi-authz-hardening
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - dotnet
  - azure
  - entra
  - authentication
  - security
---
# Azure AD Authentication for .NET

Agent integrates Microsoft Entra ID authentication for .NET apps. Agent uses Microsoft.Identity.Web for ASP.NET Core apps and MSAL.NET for daemon or client-credential flows.

## When to Use

| User prompt | Use |
|---|---|
| User asks for Microsoft Entra ID sign-in | Use this skill |
| User asks for Microsoft Graph or downstream API tokens | Use this skill |
| User asks for app roles, policies, or multi-tenant validation | Use this skill |

## When Not to Use

| User prompt | Route |
|---|---|
| User asks for machine-to-machine access with no user context and platform identity is available | Use managed identity patterns |
| User asks for consumer-only identity | Use Azure AD B2C or generic OIDC guidance |
| User asks for local username and password storage | Use `aspnetcore-identity-setup` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Tenant model | Yes | Use single-tenant, multi-tenant, or `common`/`organizations` explicitly. |
| Client ID | Yes | Use the app registration client ID. |
| Redirect URI or API audience | Yes | Match the app type. |
| Secret or certificate path | For confidential clients | Store secrets outside source. |
| Required scopes or app roles | Yes | List Graph or downstream API permissions. |

## Flow Matrix

| App type | Preferred pattern |
|---|---|
| ASP.NET Core web app | `AddMicrosoftIdentityWebApp(...)` |
| ASP.NET Core Web API | `AddMicrosoftIdentityWebApi(...)` |
| Downstream API for signed-in user | `EnableTokenAcquisitionToCallDownstreamApi()` |
| Daemon or worker | `ConfidentialClientApplicationBuilder` plus client credentials |
| Authorization by role | App roles or named policies |
| Multi-tenant acceptance | Token-validated tenant allow list |

## Workflow

| Step | Agent action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects the auth flow from the app type. | Review startup code. | Web app, API, or daemon flow matches the host. |
| 2 | Agent wires Entra configuration from config and secret stores. | Inspect settings sources. | Client ID, tenant, callback, and secret paths are configuration-driven. |
| 3 | Agent registers authentication and authorization. | Inspect `Program.cs`. | Authentication runs before authorization. |
| 4 | Agent enables downstream token acquisition only when the app calls other APIs. | Review service registration. | Token acquisition exists only for actual downstream use. |
| 5 | Agent enforces scopes, roles, or policies. | Inspect controllers and policies. | Protected endpoints require explicit auth rules. |
| 6 | Agent validates tenant rules. | Review token validation events or config. | Multi-tenant apps restrict tenants intentionally. |
| 7 | Agent verifies sign-in, sign-out, or token acquisition. | Run targeted auth tests or manual flow. | Principal claims and downstream tokens resolve correctly. |

## Configuration Matrix

| Setting | Agent expectation |
|---|---|
| `AzureAd:Instance` | Use Microsoft login base URI. |
| `AzureAd:TenantId` | Use a specific tenant ID or explicit multi-tenant value. |
| `AzureAd:ClientId` | Use the registered application ID. |
| `AzureAd:ClientSecret` | Use Key Vault, user secrets, or secure config. |
| `AzureAd:CallbackPath` | Match the registered redirect URI path. |
| Graph or downstream scopes | Use least-privilege scopes only. |

## Test Matrix

| Test | Agent verifies |
|---|---|
| Sign-in | Valid user reaches the authenticated route. |
| Sign-out | Session cookie and OpenID Connect sign-out flow clear correctly. |
| Claims | Required claims or roles exist after sign-in. |
| Downstream API | Token acquisition succeeds for listed scopes. |
| Access control | Unauthorized or under-privileged calls fail. |
| Tenant filter | Disallowed tenants fail validation. |

## Validation Checklist

- [ ] Agent kept Entra settings in configuration, Key Vault, or user secrets.
- [ ] Agent used Microsoft.Identity.Web for ASP.NET Core hosts when applicable.
- [ ] Agent registered authentication before authorization middleware.
- [ ] Agent requested least-privilege scopes or app roles.
- [ ] Agent enforced roles or policies on protected endpoints.
- [ ] Agent validated multi-tenant access intentionally when the app accepts multiple tenants.

## References

Agent reads `references/auth-flow-patterns.md` for compact setup snippets for web apps, web APIs, daemon apps, roles, and OBO flows.
