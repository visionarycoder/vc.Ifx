---
name: scalar-openapi-integration
title: Scalar OpenAPI Integration
description: Integrate Scalar UI into ASP.NET Core applications with OpenAPI route setup, document customization, and production-safe API reference patterns.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1749
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - dotnet-webapi
related_skills:
  - rfc-7807-compliance
  - rfc-9457-compliance
  - dotnet-webapi
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - aspnetcore
  - openapi
  - scalar
  - api-docs
  - minimal-api
---
# Scalar OpenAPI Integration

Use this skill to add Scalar API Reference to ASP.NET Core apps and align the UI, OpenAPI documents, and environment exposure rules.

## When to Use

| User prompt | Use |
|---|---|
| User adds Scalar UI to an ASP.NET Core API. | Use this skill. |
| User replaces or supplements Swagger UI with a modern API reference. | Use this skill. |
| User customizes OpenAPI routes, metadata, multiple documents, or API reference branding. | Use this skill. |

## When Not to Use

| User prompt | Route |
|---|---|
| User needs OpenAPI generation only with no interactive documentation UI. | Use OpenAPI generation patterns only. |
| User builds a static documentation portal with narrative guides, tutorials, or changelogs. | Use documentation workflows. |
| User asks for client SDK generation from OpenAPI. | Use client-generation skills. |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| OpenAPI generator | Yes | Record `Microsoft.AspNetCore.OpenApi`, Swashbuckle, NSwag, or equivalent. |
| Document layout | Yes | Record single document, versioned documents, or internal and public split. |
| Exposure scope | Yes | Record development-only, authenticated internal, or public documentation exposure. |
| UI customization | No | Record route, title, theme, layout, servers, and preferred auth scheme behavior. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent registers or verifies OpenAPI document generation first. | Run `rg -n "AddOpenApi|AddSwaggerGen|AddOpenApiDocument" <scope>`. | The project emits at least one OpenAPI document. |
| 2 | Expose the OpenAPI JSON route explicitly. | Read `Program.cs`. | The app maps a predictable OpenAPI route pattern. |
| 3 | Map Scalar on the intended route and ties it to the correct OpenAPI route pattern. | Run `rg -n "MapScalarApiReference|WithOpenApiRoutePattern|AddDocument" <scope>`. | Scalar loads the intended document set. |
| 4 | Agent customizes titles, versions, tags, examples, servers, and auth metadata at the OpenAPI source. | Read endpoint metadata and OpenAPI configuration. | The JSON document carries the intended metadata and security schemes. |
| 5 | Apply environment or authorization gating for the docs surface. | Read `Program.cs` and endpoint policies. | Docs exposure matches the requested operational boundary. |
| 6 | Agent runs build and endpoint verification. | Run `dotnet build <project-or-solution>` and fetch the docs endpoints. | Build passes and both OpenAPI JSON and Scalar UI routes respond. |

## Decision Matrix

| Scenario | Agent action |
|---|---|
| Minimal API with `Microsoft.AspNetCore.OpenApi` | Use `AddOpenApi`, `MapOpenApi`, and `MapScalarApiReference`. |
| Existing Swashbuckle installation | Map Swagger JSON to `/openapi/{documentName}.json` and points Scalar at that route. |
| Multiple API versions | Agent registers each document and sets the default one explicitly. |
| Internal-only docs | Agent gates Scalar with environment checks or authorization. |
| Public docs behind an API gateway | Add external server URLs and proxy metadata intentionally. |

## Implementation Patterns

| Scenario | Pattern |
|---|---|
| Built-in generator | `AddOpenApi`, `MapOpenApi`, and `MapScalarApiReference` |
| Swashbuckle or NSwag | Keep the JSON route stable and point Scalar at that pattern |
| Multiple documents | Register each document and pick a default one explicitly |
| Internal docs | Gate Scalar with environment checks or authorization |

```csharp
using Scalar.AspNetCore;

builder.Services.AddOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");
    app.MapScalarApiReference("/docs", options =>
    {
        options.WithTitle("Journal Loader API")
            .WithOpenApiRoutePattern("/openapi/{documentName}.json")
            .AddDocument("v1", "Public API", "/openapi/v1.json", isDefault: true);
    });
}

app.MapPost("/journals/import", Handler)
    .WithSummary("Queue a journal import batch")
    .ProducesProblem(StatusCodes.Status400BadRequest);
```


## Rules

| Topic | Rule |
|---|---|
| Generator order | Configure the OpenAPI generator before Scalar. |
| Route stability | Expose a stable JSON route pattern such as `/openapi/{documentName}.json`. |
| UI exposure | Align docs visibility with environment and authorization boundaries. |
| Metadata source | Agent prefers endpoint and OpenAPI metadata over ad-hoc UI-only labels. |
| Multiple documents | Name documents consistently and sets a default selection explicitly. |
| Authentication display | Configure security schemes in OpenAPI before prefilling Scalar auth helpers. |
| Production safety | Agent avoids browser-visible real credentials in Scalar configuration. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| JSON generation and Scalar route | Search for `AddOpenApi`, `MapOpenApi`, and `MapScalarApiReference`, then fetch the routes. | OpenAPI JSON and Scalar UI both respond. |
| Route linkage and metadata | Inspect the configured route pattern and representative endpoint metadata. | Scalar loads the intended document and the document carries summaries, tags, and error contracts. |
| Exposure boundary and build | Review environment or auth gating and run the narrowest build. | Docs visibility matches policy and the integration compiles cleanly. |


## Common Pitfalls

| Pitfall | Agent fix |
|---|---|
| Map Scalar without a reachable OpenAPI JSON route. | Map or preserves the JSON route first and links Scalar to that pattern. |
| Agent customizes only the UI and leaves the OpenAPI document thin. | Agent enriches endpoint metadata and document generation at the source. |
| Expose internal docs on public environments accidentally. | Add environment or authorization gating. |
| Store real tokens or passwords in Scalar auth helpers. | Agent removes real secrets and keeps browser-visible auth configuration non-sensitive. |
| Agent mixes inconsistent document names and routes. | Agent standardizes document names and route patterns across generator and UI configuration. |

## Outputs

- Scalar route mapping and OpenAPI route design
- Document version and visibility plan
- OpenAPI metadata customization pattern
- Production-safe documentation exposure rules
- Verification commands for build and docs endpoints
