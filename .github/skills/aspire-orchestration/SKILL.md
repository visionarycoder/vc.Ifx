---
name: aspire-orchestration
title: .NET Aspire Orchestration
description: Model a .NET Aspire AppHost for multi-service .NET solutions during local orchestration, service discovery, dashboard, health, and deployment-readiness setup.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1829
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-aspire-vbd-development
  - configuring-opentelemetry-dotnet
  - dotnet-resilience-standards
  - aspire-components
appliesTo: '**/*.{cs,csproj,json,yml,yaml,md}'
tags:
  - aspire
  - apphost
  - orchestration
  - service-discovery
  - dashboard
  - deployment
---
# .NET Aspire Orchestration

Model a .NET Aspire AppHost that runs distributed services locally with stable resource names, shared defaults, health endpoints, dashboard visibility, and environment-aware configuration.

## When to Use

| Condition | Use |
|---|---|
| Solution runs multiple executables, containers, or backing services together | Use this skill |
| Local development needs service discovery instead of hardcoded localhost URLs | Use this skill |
| Team needs shared health checks, logs, traces, and dashboard visibility | Use this skill |
| AppHost coordinates environment variables, references, and dependency startup order | Use this skill |
| Deployment readiness needs one local topology that mirrors service relationships | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work touches one executable with no distributed dependency | Use a direct project run flow |
| Request focuses on one client integration package only | Use `aspire-components` |
| Request designs production deployment topology only | Use the matching infrastructure or deployment skill |
| Request changes business boundaries or service ownership | Use the matching architecture skill first |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Executable inventory | Yes | List every web app, worker, function, or desktop process. |
| Dependency inventory | Yes | List every cache, database, broker, and external service. |
| Startup contract | Yes | Identify which services depend on which resources. |
| Environment model | Yes | Define local, test, and cloud-specific settings. |
| Observability goals | Yes | Define required logs, traces, metrics, and health endpoints. |

## Workflow

| Step | Action | Pass |
|---|---|---|
| 1 | Create or update AppHost and ServiceDefaults. | Required orchestration entry points exist. |
| 2 | Model projects, containers, and backing services with stable logical names. | Names avoid machine-specific ports and ad hoc suffixes. |
| 3 | Wire dependencies with Aspire references and service discovery, not hardcoded URLs. | Consuming services resolve resources through the AppHost. |
| 4 | Centralize health, telemetry, resilience, and environment-specific config. | Cross-cutting defaults live in one shared location. |
| 5 | Run the AppHost and verify startup, dependency reachability, dashboard signals, and deployment-readiness checks. | Services start, dependencies resolve, and health data appears. |


## AppHost Pattern Matrix

| Concern | Pattern | Avoid |
|---|---|---|
| Entry point | One AppHost per runnable topology | Multiple competing AppHosts for one inner-loop path |
| Shared defaults | One ServiceDefaults project for telemetry, health, service discovery, and resilience | Repeating default registration in every service |
| Resource naming | Stable logical names such as `api`, `cache`, `postgres`, `messaging` | Port-based or developer-specific names |
| Dependency wiring | `WithReference` between projects and resources | Manual localhost endpoints in consuming services |
| Local containers | Container-backed dependencies for repeatable local runs | Manual sidecar startup outside the AppHost path |
| Environment data | Configuration providers and injected resource values | Code branches with embedded secrets or endpoints |

## AppHost Example

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var postgres = builder.AddPostgres("postgres").AddDatabase("appdb");

builder.AddProject<Projects.Sample_Api>("api")
    .WithReference(cache)
    .WithReference(postgres);

builder.AddProject<Projects.Sample_Worker>("worker")
    .WithReference(postgres);

builder.Build().Run();
```


## Service Defaults Pattern

```csharp
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
        builder.Services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
        builder.Services.AddOpenTelemetry();
        return builder;
    }
}
```


## Service Discovery and Configuration Matrix

| Scenario | Pattern |
|---|---|
| Service-to-service HTTP | ServiceDefaults plus named or typed `HttpClient` with discovery |
| Resource connection | Consumer `connectionName` matches the AppHost resource name |
| Environment variables | Read injected URIs only when no client integration package exists |
| Cloud-specific settings | Configuration providers, `ConnectionStrings`, and settings sections |
| Health endpoints | `/health` and `/alive` in development |
| CLI orchestration bundle | `AspireUseCliBundle` set to `true` in the AppHost project |


## MCP Hooks

| Task | Aspire MCP tool |
|---|---|
| Find local AppHosts | `aspire-list_apphosts` |
| Inspect the resource graph | `aspire-list_resources` |
| Review console or structured logs | `aspire-list_console_logs` or `aspire-list_structured_logs` |
| Inspect traces | `aspire-list_traces` and `aspire-list_trace_structured_logs` |
| Read orchestration docs | `aspire-list_docs` and `aspire-get_doc` |


## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| AppHost structure and naming | Review the AppHost project and resource graph. | Projects and dependencies appear once with stable logical names. |
| Service discovery and config | Search for localhost URLs and review connection settings. | Cross-service URLs are discovery-driven and no secrets or machine-specific endpoints live in code. |
| Health, dashboard, and dependencies | Run the AppHost and inspect health endpoints, dashboard signals, and container-backed resources. | Services start, dependencies resolve, and telemetry surfaces appear. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| AppHost names include ephemeral port or developer labels | Rewrite names to stable logical resource names. |
| Services keep hardcoded localhost URLs after Aspire adoption | Agent replaces direct addresses with service discovery and references. |
| Telemetry setup is repeated across services | Agent moves shared setup to ServiceDefaults. |
| Health checks exist but endpoints stay unmapped | Map readiness and liveness endpoints in the web host. |
| Local run flow depends on manual container startup | Add the dependency resource to the AppHost. |
