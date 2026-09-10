---
name: cors-configuration-aspnetcore
title: ASP.NET Core CORS Configuration
description: Configure ASP.NET Core CORS policies with explicit origins, preflight behavior, credentials handling, and secure wildcard avoidance.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1888
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - api-infrastructure-bundle
  - dotnet-webapi
  - middleware-authoring-aspnetcore
  - webapp-frontend-security
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - aspnetcore
  - cors
  - security
  - webapi
---
# ASP.NET Core CORS Configuration

Agent configures ASP.NET Core CORS with explicit origin policies, correct preflight handling, and safe credential behavior.

## When to Use

| Condition | Agent Action |
|---|---|
| Browser client calls an ASP.NET Core API from another origin | Use this skill |
| API needs named policies for public, partner, or internal browser callers | Use this skill |
| Credentialed cross-origin requests need cookie or auth header support | Use this skill |
| Preflight failures block browser integration | Use this skill |

## When Not to Use

| Condition | Agent Action |
|---|---|
| Caller is server-to-server and browser CORS enforcement is irrelevant | Skip CORS work and focus on authentication or network policy |
| Platform is not ASP.NET Core | Use the platform-specific CORS workflow |
| Requirement asks for unrestricted wildcard origin with credentials | Replace the requirement with explicit trusted origins |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Allowed origins | Yes | Exact scheme, host, and port values |
| Methods and headers | Yes | Allowed request surface for preflight and actual requests |
| Credential behavior | Yes | Cookie or auth-header use and matching origin rule |
| Scope | No | Global, controller, endpoint, or route-group policy application |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent records the browser callers and exact origins. | Read configuration and policy names. | Every trusted origin is explicit. |
| 2 | Agent defines named CORS policies in `AddCors()`. | Run `rg -n "AddCors|WithOrigins|AllowAnyOrigin|AllowCredentials|SetIsOriginAllowed" <scope>`. | Service registration contains the intended named policies. |
| 3 | Agent applies `UseCors()` or endpoint-level policy mapping in the correct pipeline position. | Read `Program.cs`. | CORS runs before endpoint execution and aligns to the chosen scope. |
| 4 | Agent verifies preflight behavior for methods, headers, and max age. | Send `OPTIONS` preflight requests. | Preflight responses contain the intended allow headers. |
| 5 | Agent verifies credential behavior against explicit origins. | Send credentialed cross-origin requests. | Responses use explicit origin echo and credential header behavior that matches the policy. |
| 6 | Agent validates denial paths for disallowed origins. | Send the same requests from an untrusted origin. | Browser-facing CORS headers stay absent for denied origins. |

## Decision Matrix

| Policy Shape | Use When | Strength | Tradeoff |
|---|---|---|---|
| Exact origins | Caller origin set is known | Strongest browser trust boundary | Origin inventory needs upkeep |
| Named public-read policy | Browser clients read unauthenticated public resources | Clear separation from credentialed flows | Public surface still needs method and header discipline |
| Credentialed SPA policy | Browser app sends cookies or auth headers | Supports authenticated browser flows | Wildcard origin is invalid and unsafe |
| Endpoint-specific policy | Only part of the API needs cross-origin access | Narrowest exposure | Policy mapping needs route discipline |

## Implementation Patterns

### Named Policy Registration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("spa-client", policy =>
    {
        policy.WithOrigins(
                "https://portal.contoso.com",
                "https://admin.contoso.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(30));
    });

    options.AddPolicy("public-read", policy =>
    {
        policy.WithOrigins("https://docs.contoso.com")
            .WithMethods("GET")
            .WithHeaders("Accept", "Content-Type");
    });
});
```

### Pipeline and Endpoint Scope

```csharp
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGroup("/partner")
    .RequireCors("spa-client")
    .MapGet("/status", () => Results.Ok());
```

## Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| CORS-001 | Policy lists exact trusted origins for credentialed requests. | Replace wildcard origin rules with `WithOrigins(...)`. |
| CORS-002 | `AllowAnyOrigin()` and `AllowCredentials()` never appear together. | Use explicit origins for credentialed flows. |
| CORS-003 | CORS middleware order matches routing and auth flow requirements. | Place `UseCors()` after routing setup and before auth or endpoint execution. |
| CORS-004 | Preflight allows only the methods and headers the caller needs. | Replace broad allow rules with explicit methods or headers where the prompt calls for restriction. |
| CORS-005 | Denied origins do not receive success-shaping CORS headers. | Narrow the policy scope or allowed origin list. |
| CORS-006 | Non-browser integrations do not drive unnecessary CORS expansion. | Keep CORS rules focused on browser callers only. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Allowed-origin preflight | Send `OPTIONS` with an allowed `Origin`, method, and headers. | Response includes `Access-Control-Allow-Origin` and matching allow metadata. |
| Disallowed-origin preflight | Send the same `OPTIONS` request from an untrusted origin. | Response omits the allow-origin header. |
| Credentialed request | Send a browser-equivalent request with cookies or auth headers from an allowed origin. | Response includes the explicit origin and `Access-Control-Allow-Credentials: true`. |
| Wildcard safety | Inspect policy registration. | No credentialed policy uses `AllowAnyOrigin()`. |
| Scope correctness | Call routes inside and outside endpoint-level policies. | Only the intended routes emit CORS headers. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| `AllowAnyOrigin()` appears with credentials | Agent replaces wildcard origin rules with explicit trusted origins. |
| Preflight fails because requested header is missing | Agent adds the specific header or aligns the client request shape. |
| `UseCors()` runs after endpoints | Agent moves CORS earlier in the pipeline. |
| Server-to-server callers drive broad browser policy | Agent narrows CORS to browser-facing routes only. |
| One global policy exposes internal routes | Agent moves sensitive routes to endpoint-level named policies. |

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find existing CORS policies | `search_code` | Agent searches for `AddCors`, `UseCors`, policy names, and `RequireCors` mappings before editing. |
| Review CORS PR changes | `pull_request_read` | Agent reads changed startup, endpoint, and frontend-caller context before correcting policy scope. |
| Verify browser surface coverage | `search_code` | Agent searches for `OPTIONS` tests, origin lists, and credential flows so policy updates stay aligned. |
