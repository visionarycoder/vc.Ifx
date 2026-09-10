---
name: api-infrastructure-bundle
title: API Infrastructure Bundle
description: Bundle routing skill for ASP.NET Core infrastructure concerns including health checks, rate limiting, CORS, versioning, and middleware.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 900
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - health-checks-aspnetcore
  - rate-limiting-aspnetcore
  - cors-configuration-aspnetcore
  - api-versioning-aspnetcore
  - middleware-authoring-aspnetcore
  - dotnet-webapi
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - aspnetcore
  - bundle
  - infrastructure
  - api
---
# API Infrastructure Bundle

Agent uses this bundle for ASP.NET Core infrastructure setup including health checks, rate limiting, CORS, API versioning, and custom middleware.

## Activation

| Prompt Scope | Use This Bundle | Route Detail |
|---|---|---|
| Configure health checks for Kubernetes | Yes | Agent uses health-checks-aspnetcore |
| Add rate limiting to API | Yes | Agent uses rate-limiting-aspnetcore |
| Configure CORS policies | Yes | Agent uses cors-configuration-aspnetcore |
| Add API versioning | Yes | Agent uses api-versioning-aspnetcore |
| Create custom middleware | Yes | Agent uses middleware-authoring-aspnetcore |
| Implement business logic | No | Agent routes to appropriate domain skill |

## Coverage Matrix

| Track | Specialist Skill | Agent Uses When |
|---|---|---|
| Health checks | `health-checks-aspnetcore` | Liveness/readiness probes, dependency checks in scope |
| Rate limiting | `rate-limiting-aspnetcore` | Request throttling, quota management in scope |
| CORS | `cors-configuration-aspnetcore` | Cross-origin resource sharing in scope |
| API versioning | `api-versioning-aspnetcore` | Endpoint versioning, deprecation in scope |
| Middleware | `middleware-authoring-aspnetcore` | Custom request/response pipeline logic in scope |

## Decision Order

| Decision | Agent Action |
|---|---|
| Multiple infrastructure concerns in one prompt | Agent applies all relevant specialist skills |
| Health checks + rate limiting requested | Agent uses health-checks-aspnetcore and rate-limiting-aspnetcore |
| Unclear which infrastructure component | Agent clarifies with user before proceeding |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Classify scope | Agent maps prompt to one or more infrastructure tracks. | Agent lists infrastructure components in scope. | Every component maps to one specialist skill. |
| 2. Apply specialists | Agent applies matching specialist skills in dependency order. | Agent records applied specialist skills. | Every infrastructure concern has one specialist application. |
| 3. Verify integration | Agent verifies middleware ordering and startup configuration. | Inspect `Program.cs` middleware pipeline. | Middleware appears in correct order. |
| 4. Run tests | Agent runs existing API and integration tests. | Run `dotnet test` for affected projects. | Zero test failures. |

## Middleware Ordering Guidelines

| Middleware Type | Typical Order | Reason |
|---|---|---|
| Exception handling | 1 (first) | Catches exceptions from all downstream middleware |
| CORS | 2 | Before auth, allows preflight requests |
| Authentication | 3 | Before authorization |
| Authorization | 4 | After authentication |
| Rate limiting | 5 | After auth, limit authenticated users |
| Custom middleware | 6 | After infrastructure, before routing |
| Routing | 7 | Maps requests to endpoints |
| Endpoints | 8 (last) | Executes business logic |

## Verification Matrix

| Track | Test | Pass |
|---|---|---|
| Health checks | Call `/health/live` and `/health/ready`. | Returns 200 with appropriate status. |
| Rate limiting | Send requests exceeding limit. | Returns 429 Too Many Requests. |
| CORS | Send preflight OPTIONS request. | Returns appropriate CORS headers. |
| API versioning | Call versioned endpoint. | Routes to correct version. |
| Middleware | Inspect pipeline execution order. | Middleware executes in documented order. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| CORS after authentication | Agent moves CORS registration before auth. |
| Rate limiting before authentication | Agent moves rate limiting after auth when user-based. |
| Missing health check tags | Agent adds liveness/readiness tag filtering. |
| Middleware registered in wrong order | Agent reorganizes pipeline per ordering guidelines. |

## Outputs

- Configured infrastructure components per specialist skills
- Properly ordered middleware pipeline
- Verified integration test results
- Infrastructure configuration documentation
