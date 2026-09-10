---
name: middleware-authoring-aspnetcore
title: ASP.NET Core Middleware Authoring
description: Author ASP.NET Core middleware with correct pipeline ordering, terminal behavior, conventions, and factory-based composition.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1986
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - api-infrastructure-bundle
  - dotnet-webapi
  - global-exception-handling
  - rate-limiting-aspnetcore
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - aspnetcore
  - middleware
  - pipeline
  - webapi
---
# ASP.NET Core Middleware Authoring

Agent authors ASP.NET Core middleware with explicit pipeline placement and predictable request or response behavior.

## When to Use

| Condition | Agent Action |
|---|---|
| Cross-cutting request or response logic applies before endpoint execution | Use this skill |
| Application needs custom headers, correlation, tenant resolution, or short-circuit logic | Use this skill |
| Dependency scope or per-request services favor `IMiddleware` activation | Use this skill |
| Pipeline ordering changes affect endpoint behavior | Use this skill |

## When Not to Use

| Condition | Agent Action |
|---|---|
| Logic applies to one endpoint only | Use endpoint filters, action filters, or handler logic |
| Transformation belongs in reverse proxy or load balancer | Configure the upstream layer |
| Concern is exception-to-problem-details mapping only | Use the exception-handling workflow |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Pipeline concern | Yes | Record logging, auth context, correlation, tenancy, response shaping, or short-circuit behavior. |
| Placement | Yes | Record before routing, after routing, before auth, after auth, or near endpoints. |
| Lifetime model | Yes | Select conventional middleware or `IMiddleware` activation. |
| Terminal behavior | No | Record whether the middleware ends the pipeline for a subset of requests. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Select the lowest-cost extension point that matches the concern. | Read the changed code surface. | Middleware exists only where cross-cutting behavior justifies it. |
| 2 | Agent chooses conventional middleware or `IMiddleware` activation from lifetime and dependency needs. | Inspect type definition and service registration. | Activation model matches dependency lifetime requirements. |
| 3 | Agent places the middleware in the correct pipeline order. | Read `Program.cs`. | Middleware runs before or after adjacent components as intended. |
| 4 | Agent preserves one clear `next` flow: short-circuit once or call `next` once. | Inspect `InvokeAsync`. | No duplicate downstream execution path exists. |
| 5 | Verify request body, response body, and header behavior for the target paths. | Run existing API tests or scripted HTTP calls. | Middleware output matches the intended contract. |
| 6 | Verify scoped dependency behavior under request load. | Run targeted requests and inspect DI failures or scope leaks. | Per-request services resolve and dispose correctly. |

## Decision Matrix

| Choice | Use When | Strength | Tradeoff |
|---|---|---|---|
| Conventional middleware class | Dependency graph is simple and constructor state is singleton-safe | Minimal ceremony | Scoped services belong in `InvokeAsync` parameters, not constructor state |
| `IMiddleware` | Middleware needs scoped services in the constructor or factory-managed activation | Clean DI semantics | One extra service registration |
| Terminal middleware | Path ends the pipeline intentionally | Fast short-circuit | Downstream middleware and endpoints do not run |
| Non-terminal middleware | Request needs downstream endpoint execution | Flexible composition | `next` flow needs discipline |

## Implementation Patterns

```csharp
public sealed class CorrelationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationMiddleware> logger)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
        context.Response.Headers["X-Correlation-Id"] = correlationId;
        using (logger.BeginScope(new Dictionary<string, object?> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }
}

public sealed class TenantMiddleware(ITenantResolver tenantResolver) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Items["Tenant"] = await tenantResolver.ResolveAsync(context);
        await next(context);
    }
}

builder.Services.AddScoped<TenantMiddleware>();
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.MapControllers();
```

## Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| MW-001 | Middleware exists only for cross-cutting behavior. | Move endpoint-specific logic into filters or handlers. |
| MW-002 | Scoped services do not live in singleton middleware constructor state. | Inject scoped services into `InvokeAsync` or switch to `IMiddleware`. |
| MW-003 | Middleware calls `next` once or short-circuits once. | Remove duplicate `next` paths and return after terminal writes. |
| MW-004 | Headers append before response body starts when the middleware owns those headers. | Move header writes ahead of downstream body writes or guard with `HasStarted`. |
| MW-005 | Body reads respect buffering and stream position rules. | Enable buffering and rewind intentionally when multiple readers exist. |
| MW-006 | Pipeline order matches the adjacent infrastructure concern. | Reorder middleware around routing, CORS, auth, rate limiting, and endpoints. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Ordering and short-circuiting | Exercise a path that touches adjacent middleware and one terminal branch. | Observed behavior matches the documented order and downstream execution count. |
| Headers and body flow | Inspect middleware-owned headers and send a request body through middleware plus endpoint. | Headers arrive on the intended responses and downstream readers still succeed. |
| Scoped dependencies | Run requests that resolve scoped services. | No DI lifetime or disposal defects appear. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Scoped service injected into conventional middleware constructor | Agent moves the dependency into `InvokeAsync` or uses `IMiddleware`. |
| Middleware writes response and still calls `next` | Return immediately after terminal behavior. |
| Custom middleware order breaks CORS or auth | Agent reorders the pipeline around the relevant infrastructure middleware. |
| Request body read consumes the stream | Agent enables buffering and rewinds before downstream execution. |
| Header write happens after body start | Agent moves header mutation earlier or checks `Response.HasStarted`. |

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find pipeline precedents | `search_code` | Search for `UseMiddleware`, `IMiddleware`, `InvokeAsync`, and pipeline order before authoring middleware. |
| Review middleware PR changes | `pull_request_read` | Read changed `Program.cs`, middleware classes, and adjacent infrastructure before changing order or behavior. |
| Verify ordering impact | `search_code` | Search for exception handling, auth, CORS, and rate-limiter placement so the new middleware fits the pipeline. |

## Outputs

- Middleware or `IMiddleware` implementation aligned to DI lifetime needs
- Explicit pipeline ordering in `Program.cs`
- One clear downstream or short-circuit path
- Verification steps for ordering, headers, body handling, and DI scope behavior
