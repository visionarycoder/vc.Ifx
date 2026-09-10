---
name: rate-limiting-aspnetcore
title: ASP.NET Core Rate Limiting
description: Configure ASP.NET Core rate limiting with fixed or sliding windows, token buckets, concurrency limits, and custom policies.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1998
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - api-infrastructure-bundle
  - dotnet-webapi
  - middleware-authoring-aspnetcore
  - webapi-authz-hardening
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - aspnetcore
  - rate-limiting
  - middleware
  - api
---
# ASP.NET Core Rate Limiting

Configure ASP.NET Core request throttling with the built-in .NET 7+ rate limiting middleware.

## When to Use

| Condition | Agent Action |
|---|---|
| API needs per-client or per-user request quotas | Use this skill |
| Expensive endpoints need burst control | Use this skill |
| Application needs concurrency protection for downstream dependencies | Use this skill |
| Application needs endpoint-specific throttling rules | Use this skill |

## When Not to Use

| Condition | Agent Action |
|---|---|
| Reverse proxy or API gateway owns all throttling rules and app adds no per-endpoint behavior | Configure the gateway layer |
| Workload is background processing instead of HTTP request admission control | Use queue or channel backpressure patterns |
| Platform is not ASP.NET Core on .NET 7 or later | Use the platform-specific limiter |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Partition key | Yes | Select client IP, API key, user ID, tenant ID, or route group. |
| Limiter type | Yes | Select fixed window, sliding window, token bucket, concurrency, or custom policy. |
| Rejection contract | Yes | Define status code, body shape, and retry headers. |
| Endpoint scope | No | Record global, route-group, or endpoint-level application. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Select the partition key and limiter type from traffic shape and fairness goals. | Read the startup registration and endpoint scope. | One explicit partition key and limiter type exist for each protected path. |
| 2 | Agent registers `AddRateLimiter()` with named policies. | Run `rg -n "AddRateLimiter|AddFixedWindowLimiter|AddSlidingWindowLimiter|AddTokenBucketLimiter|AddConcurrencyLimiter" <scope>`. | Service registration contains the intended named policies. |
| 3 | Agent wires `UseRateLimiter()` into the pipeline and maps policies to endpoints or groups. | Read `Program.cs`. | Middleware registration and endpoint policy mapping both exist. |
| 4 | Return a stable rejection response with `429` and retry metadata when applicable. | Inspect `OnRejected` and call a throttled endpoint. | Rejected requests return the intended contract. |
| 5 | Agent excludes health or infrastructure endpoints when prompt scope calls for exemption. | Read endpoint metadata. | Exempt endpoints do not use the throttling policy. |
| 6 | Agent runs targeted request bursts and concurrency checks. | Run existing API tests or scripted HTTP calls. | Allowed requests pass, excess requests fail with the intended result. |

## Decision Matrix

| Limiter | Use When | Strength | Tradeoff |
|---|---|---|---|
| Fixed window | Traffic tolerates boundary bursts and rule simplicity matters | Simple configuration and predictable counters | Boundary bursts double at window boundaries |
| Sliding window | Traffic needs smoother enforcement across window edges | Better fairness than fixed windows | Higher tracking cost |
| Token bucket | Traffic needs burst allowance with steady refill | Supports short bursts with sustained rate control | Refill tuning needs traffic knowledge |
| Concurrency limiter | Workload cost depends on active in-flight requests | Protects scarce downstream resources | No time-based quota |
| Custom partitioned policy | Rule changes by user, tenant, API key, or route metadata | Fine-grained fairness | Partition selection errors reduce protection |

## Implementation Patterns

```csharp
using System.Globalization;
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new { title = "Too Many Requests", status = 429 }, cancellationToken);
    };

    options.AddPolicy("per-user", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 50,
                TokensPerPeriod = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(30),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

app.UseRateLimiter();
app.MapGet("/health/live", () => Results.Ok()).DisableRateLimiting();
app.MapGroup("/api/orders").RequireRateLimiting("per-user");
```

| Need | Pattern |
|---|---|
| Fixed quota | `AddFixedWindowLimiter` or `AddSlidingWindowLimiter` |
| Burst with refill | `AddTokenBucketLimiter` |
| In-flight protection | `AddConcurrencyLimiter` |
| Tenant or route fairness | Custom partitioned policy |


## Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| RL-001 | Partition key maps to the fairness boundary the prompt describes. | Change the partition to user, tenant, API key, or route group. |
| RL-002 | Authenticated quotas do not collapse all callers into one anonymous bucket after sign-in. | Move authenticated policies after authentication and use user or tenant identity. |
| RL-003 | IP-based limits behind proxies read the forwarded client IP safely. | Configure forwarded headers before IP-based partition logic. |
| RL-004 | Rejected requests return `429` with stable body shape and retry metadata when available. | Add `OnRejected` and `Retry-After` handling. |
| RL-005 | Queue limits stay bounded for latency-sensitive endpoints. | Set `QueueLimit` to zero or a small explicit value. |
| RL-006 | Health and infrastructure endpoints stay exempt when the scope calls for it. | Add `DisableRateLimiting()` or map a separate policy. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Quota enforcement | Send requests past the configured fixed, sliding, or token limits. | Excess requests return `429` and allowed requests succeed. |
| Concurrency protection | Hold requests open past the permit limit. | Extra requests queue or reject per policy. |
| Retry metadata and scope | Inspect rejected responses and call exempt endpoints. | `Retry-After` appears when available and exempt endpoints bypass limits. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Global anonymous bucket throttles every signed-in caller together | Agent partitions by authenticated identity after authentication runs. |
| IP throttling behind reverse proxy uses proxy IP only | Configure forwarded headers before partition lookup. |
| Queue limit hides overload with long waits | Agent bounds or removes the queue for latency-sensitive paths. |
| Policy lives in services but no endpoint uses it | Add `RequireRateLimiting()` or global middleware scope. |
| Health checks return `429` during incident response | Agent disables rate limiting for health endpoints. |

## Outputs

- Named policies for the requested traffic shape
- Ordered middleware registration with endpoint mappings
- Stable `429` rejection contract with retry metadata
- Verification evidence for quota, burst, and concurrency behavior

