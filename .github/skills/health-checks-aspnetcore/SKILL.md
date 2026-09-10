---
name: health-checks-aspnetcore
title: ASP.NET Core Health Checks
description: Configure liveness and readiness probes with custom health checks for ASP.NET Core applications and dependencies.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1100
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - dotnet-webapi
  - api-infrastructure-bundle
  - configuring-opentelemetry-dotnet
  - efcore-dbcontext-design
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - aspnetcore
  - health-checks
  - kubernetes
  - reliability
---
# ASP.NET Core Health Checks

Agent configures health check endpoints with liveness and readiness semantics for ASP.NET Core applications.

## When to Use

| Condition | Agent Action |
|---|---|
| Application deploys to Kubernetes or container orchestrators | Use this skill |
| Application exposes health status to load balancers | Use this skill |
| Application dependencies need health verification | Use this skill |

## When Not to Use

| Condition | Agent Action |
|---|---|
| Simple status endpoint without dependency checks | Write custom controller action |
| Non-ASP.NET Core application | Use platform-specific health pattern |

## Core Patterns

| Pattern | Use When | Implementation |
|---|---|---|
| Liveness probe | Orchestrator needs process-alive signal | Map `/health/live` to basic check |
| Readiness probe | Orchestrator needs traffic-ready signal | Map `/health/ready` to dependency checks |
| Startup probe | Application has slow initialization | Map `/health/startup` to initialization checks |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Add package | Agent adds `Microsoft.Extensions.Diagnostics.HealthChecks` and UI packages when needed. | Check project references. | Package references exist. |
| 2. Register checks | Agent registers health checks in `Program.cs` with `AddHealthChecks()`. | Inspect service registration. | Health check services registered. |
| 3. Add custom checks | Agent creates `IHealthCheck` implementations for database, cache, external API dependencies. | Inspect custom check classes. | Each dependency has one health check. |
| 4. Map endpoints | Agent maps liveness, readiness, and startup endpoints with appropriate filtering. | Run application and call endpoints. | Endpoints return 200 (healthy) or 503 (unhealthy). |
| 5. Configure tags | Agent uses tags to separate liveness checks from readiness checks. | Inspect endpoint mappings. | Liveness excludes dependency checks. |
| 6. Verify behavior | Agent tests healthy and unhealthy states. | Simulate dependency failure. | Readiness returns 503 when dependency fails. |

## Health Check Implementation Patterns

### Database Health Check

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly DbContext _context;
    
    public DatabaseHealthCheck(DbContext context) => _context = context;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("Database connection successful.")
            : HealthCheckResult.Unhealthy("Database connection failed.");
    }
}
```

### Registration Pattern

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" })
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

## Endpoint Configuration Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| HC-001 | Liveness endpoint excludes external dependencies | Filter liveness by tag |
| HC-002 | Readiness endpoint includes all required dependencies | Tag dependency checks with "ready" |
| HC-003 | Health check timeout stays bounded | Set `Timeout` on registration |
| HC-004 | Degraded state returns 200 with warning payload | Use `HealthCheckResult.Degraded()` |
| HC-005 | Sensitive diagnostics stay server-side only | Customize response writer for external consumers |

## Built-In Health Checks

| Check Type | NuGet Package | Use For |
|---|---|---|
| DbContext | `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` | EF Core database connections |
| SQL Server | `AspNetCore.HealthChecks.SqlServer` | Direct SQL Server connections |
| Redis | `AspNetCore.HealthChecks.Redis` | Redis cache availability |
| Azure Service Bus | `AspNetCore.HealthChecks.AzureServiceBus` | Service Bus connectivity |
| Azure Storage | `AspNetCore.HealthChecks.AzureStorage` | Blob/Queue/Table storage |

## UI Integration

Agent adds health check UI when requested:

```csharp
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
```

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Liveness responds | Call `/health/live` endpoint. | Returns 200 with healthy status. |
| Readiness includes dependencies | Call `/health/ready` endpoint. | Response includes database and other dependency statuses. |
| Failure detection | Stop dependency service and call `/health/ready`. | Returns 503 with unhealthy status. |
| Tag filtering | Compare liveness and readiness responses. | Liveness excludes dependency checks. |
| Timeout protection | Register check with 5s timeout and simulate 10s delay. | Health check times out and returns degraded. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Liveness includes database checks | Agent moves database checks to readiness-only tags. |
| No timeout configured | Agent adds `Timeout = TimeSpan.FromSeconds(5)` to registrations. |
| Exposing exception details externally | Agent customizes response writer to exclude stack traces. |
| Missing tag filtering | Agent adds tag predicates to endpoint mappings. |
| Synchronous blocking in checks | Agent uses async database/HTTP calls throughout. |

## Outputs

- Configured health check endpoints (`/health/live`, `/health/ready`, `/health/startup`)
- Custom `IHealthCheck` implementations for application dependencies
- Tag-based filtering for liveness vs. readiness separation
- Kubernetes-ready probe configuration examples
