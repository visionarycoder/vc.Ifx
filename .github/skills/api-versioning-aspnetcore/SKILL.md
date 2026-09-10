---
name: api-versioning-aspnetcore
title: ASP.NET Core API Versioning
description: Configure ASP.NET Core API versioning with URL, header, or query readers plus deprecation and Sunset signaling.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1992
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - api-infrastructure-bundle
  - dotnet-webapi
  - webapi-compatibility-migration
  - middleware-authoring-aspnetcore
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - aspnetcore
  - api-versioning
  - compatibility
  - webapi
---
# ASP.NET Core API Versioning

Configure explicit API version negotiation with `Asp.Versioning.Http` and compatibility signaling for ASP.NET Core endpoints.

## When to Use

| Condition | Agent Action |
|---|---|
| Public or partner API introduces a breaking contract change | Use this skill |
| API needs URL, header, or query string version negotiation | Use this skill |
| API lifecycle needs deprecation and Sunset signaling | Use this skill |
| Endpoint set needs parallel v1 and v2 behavior during migration | Use this skill |

## When Not to Use

| Condition | Agent Action |
|---|---|
| API change is additive and contract-compatible | Keep the existing version and evolve the contract in place |
| System is GraphQL, gRPC, or non-HTTP transport | Use the transport-specific compatibility pattern |
| Internal endpoint has no version contract and no external callers | Keep routing simple and document the boundary |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Version reader strategy | Yes | Select URL segment, header, query string, or a combined reader. |
| Baseline version | Yes | Define the current stable version and the default behavior. |
| Compatibility scope | Yes | Record which endpoints preserve behavior and which endpoints diverge by version. |
| Deprecation schedule | No | Record retirement date, migration link, and Sunset policy when prompt scope includes lifecycle signaling. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Select one negotiation strategy or one combined reader and records the default-version rule. | Read service registration. | Version readers and default-version behavior are explicit. |
| 2 | Agent registers `AddApiVersioning()` and version metadata on controllers or minimal API endpoint sets. | Run `rg -n "AddApiVersioning|ApiVersion|MapToApiVersion|WithApiVersionSet" <scope>`. | Service registration and endpoint version metadata both exist. |
| 3 | Separate behavior by version only where the contract breaks. | Read handlers or controllers for changed endpoints. | Breaking behavior maps to a distinct version. |
| 4 | Agent reports supported and deprecated versions in response headers. | Call versioned endpoints and inspect headers. | Response headers expose supported and deprecated versions. |
| 5 | Add deprecation and Sunset signaling for retiring versions. | Call a deprecated version and inspect headers. | Deprecated responses include deprecation and Sunset metadata. |
| 6 | Agent runs targeted compatibility checks across active versions. | Run existing API tests or scripted HTTP calls for each active version. | Each active version returns the intended contract and route mapping. |

## Decision Matrix

| Strategy | Use When | Strength | Tradeoff |
|---|---|---|---|
| URL segment versioning | Route shape is public and explicit version visibility matters | Clear cache and link behavior | Route churn on version changes |
| Header versioning | Clean URLs matter and clients control headers easily | Stable route shape | Harder manual browser testing |
| Query string versioning | Existing clients already pass query parameters | Easy incremental rollout | Less explicit than URL segments |
| Combined readers | API supports more than one caller style during migration | Flexible compatibility window | Reader order and documentation need precision |

## Implementation Patterns

```csharp
using Asp.Versioning;

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = false;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("api-version"),
        new QueryStringApiVersionReader("api-version"));
});

var ordersVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();

app.MapGroup("/api/v{version:apiVersion}/orders")
    .WithApiVersionSet(ordersVersionSet)
    .MapGet("/", GetOrdersV1).MapToApiVersion(new ApiVersion(1, 0));
```

```csharp
app.Use(async (context, next) =>
{
    await next();
    if (context.GetRequestedApiVersion() == new ApiVersion(1, 0))
    {
        context.Response.Headers.Append("Deprecation", "true");
        context.Response.Headers.Append("Sunset", "Wed, 31 Dec 2026 23:59:59 GMT");
    }
});
```


## Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| AV-001 | One breaking contract change maps to one explicit version boundary. | Split divergent behavior into distinct versioned handlers. |
| AV-002 | Version negotiation logic stays explicit and documented in code. | Configure one reader or one combined reader in `AddApiVersioning()`. |
| AV-003 | Default version behavior matches the compatibility policy. | Set `AssumeDefaultVersionWhenUnspecified` and `DefaultApiVersion` intentionally. |
| AV-004 | Deprecated versions emit both report headers and lifecycle metadata. | Enable `ReportApiVersions` and append deprecation or Sunset headers. |
| AV-005 | Active versions keep independent route or handler mapping. | Add `MapToApiVersion()` or controller attributes per version. |
| AV-006 | OpenAPI grouping matches the active version set when API docs are in scope. | Add version-aware API explorer configuration. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| URL, header, and query negotiation | Call the same endpoint through each configured reader. | Each request resolves to the intended version. |
| Version reporting and Sunset | Inspect headers on active and deprecated versions. | Supported, deprecated, and Sunset metadata appears where expected. |
| Compatibility mapping | Review handlers or run existing versioned tests. | Breaking behavior stays isolated to the mapped version. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Breaking response change lands in an existing version | Agent creates a new version and isolates the divergent contract. |
| Reader strategy exists in docs only | Configure the reader in `AddApiVersioning()`. |
| Default version hides caller mistakes | Agent disables default-version assumption unless the compatibility policy requires it. |
| Deprecated version lacks migration signal | Add deprecation and Sunset headers with a migration link. |
| Route versioning and header versioning both exist with conflicting docs | Agent documents the combined reader order and tests each reader path. |

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find existing version negotiation patterns | `search_code` | Search for `AddApiVersioning`, `ApiVersion`, version readers, and deprecation headers before editing. |
| Review versioning PR changes | `pull_request_read` | Read changed routes, controllers, and migration notes before extending a versioning PR. |
| Verify rollout coverage | `search_code` | Search for versioned tests, OpenAPI grouping, and header assertions so compatibility work stays consistent. |

## Outputs

- Explicit API version reader configuration
- Versioned controller or minimal API endpoint mappings
- Deprecation and Sunset response signaling for retiring versions
- Compatibility verification across every active version
