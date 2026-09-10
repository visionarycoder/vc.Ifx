---
name: aspire-components
title: .NET Aspire Components
description: Wire .NET Aspire integration components for Redis, PostgreSQL, Service Bus, telemetry, health checks, configuration, and resilience in consuming .NET services.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1863
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - configuring-opentelemetry-dotnet
  - azure-service-bus-patterns
  - redis-patterns-dotnet
  - dotnet-resilience-standards
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - aspire
  - integrations
  - redis
  - postgresql
  - service-bus
  - telemetry
  - health-checks
---
# .NET Aspire Components

Agent wires Aspire integrations so consuming .NET services receive typed clients, health checks, telemetry, configuration providers, and resilience defaults.

## When to Use

| Condition | Use |
|---|---|
| Solution uses Aspire resources and consuming .NET services need typed client registration | Use this skill |
| Work adds Redis, PostgreSQL, SQL Server, RabbitMQ, or Azure Service Bus through Aspire | Use this skill |
| Configuration enters the app through Azure App Configuration or Aspire settings sections | Use this skill |
| Team needs health checks and OpenTelemetry from integration packages | Use this skill |
| Multiple resource instances need keyed registrations | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Request builds the AppHost topology itself | Use `aspire-orchestration` |
| Request focuses on non-Aspire raw client setup only | Use the matching technology-specific skill |
| Work changes broker semantics, cache policy, or database design beyond registration | Use the matching domain skill after integration setup |
| Consuming app is not a .NET service | Use language-specific guidance outside this skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| AppHost resource names | Yes | Client `connectionName` values match these names |
| Consuming project list | Yes | List each app that resolves typed clients or providers. |
| Integration package choice | Yes | Pick one package per resource type. |
| Configuration source | Yes | Choose `ConnectionStrings`, Aspire settings, or inline delegates. |
| Resilience and telemetry needs | Yes | State any explicit disable or override requirement. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent matches each AppHost resource to the correct Aspire client integration package. | Read the resource list and project packages. | Each resource type has one matching client package. |
| 2 | Agent registers the typed client or provider with a `connectionName` that matches the AppHost resource. | Read consuming project startup code. | Registration names match AppHost resource names exactly. |
| 3 | Agent applies configuration through supported Aspire settings, `ConnectionStrings`, or inline delegates. | Read appsettings and registration code. | Configuration path is explicit and environment-safe. |
| 4 | Agent keeps health checks and telemetry enabled unless an explicit exception exists. | Read registration options. | Default health and telemetry behavior remains active or documented. |
| 5 | Agent adds keyed registrations where multiple instances of one component exist. | Read startup code. | Named resources map to distinct typed clients. |
| 6 | Agent verifies client resolution, dependency reachability, and health status. | Run targeted app startup or tests. | App resolves the integration client and dependency access succeeds. |

## Component Matrix

| Component | Package | Registration | Injected Type | Default Signals |
|---|---|---|---|---|
| Redis | `Aspire.StackExchange.Redis` | `builder.AddRedisClient("cache")` | `IConnectionMultiplexer` | Health checks, tracing, metrics |
| PostgreSQL | `Aspire.Npgsql` | `builder.AddNpgsqlDataSource("postgresdb")` | `NpgsqlDataSource` | Health checks, tracing, metrics |
| SQL Server | `Aspire.Microsoft.Data.SqlClient` | `builder.AddSqlServerClient("sqldb")` | `SqlConnection` | Health checks, tracing, metrics |
| RabbitMQ | `Aspire.RabbitMQ.Client` | `builder.AddRabbitMQClient("rabbitmq")` | `IConnection` | Health checks, tracing |
| Azure Service Bus | `Aspire.Azure.Messaging.ServiceBus` | `builder.AddAzureServiceBusClient("servicebus")` | `ServiceBusClient` | Health checks, tracing |
| Azure App Configuration | `Aspire.Microsoft.Extensions.Configuration.AzureAppConfiguration` | `builder.AddAzureAppConfiguration("config")` | `IConfiguration` provider | Provider registration |

## Integration Example

```csharp
var builder = DistributedApplication.CreateBuilder(args);
var cache = builder.AddRedis("cache");
var postgresdb = builder.AddPostgres("postgres").AddDatabase("postgresdb");
var servicebus = builder.AddAzureServiceBus("servicebus");

builder.AddProject<Projects.Sample_Api>("api")
    .WithReference(cache)
    .WithReference(postgresdb)
    .WithReference(servicebus);

builder.AddRedisClient("cache");
builder.AddNpgsqlDataSource("postgresdb");
builder.AddAzureServiceBusClient("servicebus");
```

## Service Discovery and Configuration Matrix

| Scenario | Pattern | Result |
|---|---|---|
| Single resource instance | `connectionName` equals AppHost resource name | Aspire injects matching connection data |
| Multiple resource instances | Use keyed registrations such as `AddKeyedRedisClient` or `AddKeyedAzureServiceBusClient` | Consumers resolve one resource by name |
| Connection string binding | Place values under `ConnectionStrings` | Integration resolves standard named connection data |
| Settings binding | Place values under the package-specific `Aspire:*` section | Package toggles stay declarative |
| Inline override | Pass an options delegate to the registration method | One service overrides health, tracing, metrics, or client options without changing the AppHost |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Package mapping | Review `.csproj` package references | Each dependency type uses the correct Aspire integration package |
| Registration mapping | Review startup code | Each registration name matches the AppHost resource name |
| Client resolution | Start the consuming app or targeted tests | DI resolves every integration type successfully |
| Health checks | Query `/health` after startup | Dependency checks report healthy status |
| Telemetry | Open dashboard or inspect OTLP destination | Resource activity appears with service correlation |
| Configuration path | Review `ConnectionStrings`, `Aspire:*` sections, and inline delegates | Overrides are explicit and secrets stay outside code |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| `connectionName` differs from the AppHost resource name | Agent aligns the registration name to the AppHost resource. |
| Raw client package replaces the Aspire integration without a reason | Agent restores the Aspire integration or documents the direct environment-variable path. |
| Health checks or tracing are disabled by copy-paste | Agent removes the override or records the explicit exception. |
| One app uses the same resource type twice without keyed registration | Agent adds keyed registrations and keyed injection. |
| Broker and database packages are added but AppHost references are missing | Agent adds `WithReference` in the AppHost graph. |
