---
name: webjobs-authoring
description: Author Azure WebJobs with explicit host setup, trigger binding, job lifecycle, and shutdown behavior.
title: WebJobs Authoring
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1500
appliesTo: '**/*.{cs,csproj,json}'
related_skills:
  - quartz-job-authoring
  - long-running-job-patterns
  - application-insights-dotnet
tags:
  - webjobs
  - azure
  - background-jobs
  - dotnet
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
---
# WebJobs Authoring

Agent authors Azure WebJobs that bind triggers cleanly, run through the generic host, and honor host lifecycle signals.

## When to Use

| Condition | Use |
|---|---|
| Agent hosts background work in Azure App Service | Use this skill |
| Agent needs timer, queue, or blob-triggered WebJobs behavior | Use this skill |
| Agent wants DI, options, and logging in a WebJobs process | Use this skill |
| Agent validates shutdown and restart behavior in App Service | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Work fits Azure Functions hosting better | Use `azure-functions-dotnet` |
| Work depends on Quartz trigger features | Use `quartz-job-authoring` and `quartz-scheduling-patterns` |
| Work is a continuous hosted service outside App Service | Use worker-service patterns |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Job style | Yes | Continuous, timer, queue, or blob |
| Trigger source | Yes | Schedule, queue name, or container name |
| Business dependencies | Yes | Services, repositories, clients, and options |
| Deployment target | Yes | App Service app or slot |
| Shutdown expectation | No | Graceful stop or resume behavior |

## Required Workflow

Agent performs these steps:
1. Agent selects the WebJobs style from the trigger pattern table.
2. Agent configures the generic host and only the required WebJobs extensions.
3. Agent keeps trigger methods thin and delegates business work to services.
4. Agent propagates cancellation and logs structured lifecycle events.
5. Agent validates local trigger execution before App Service deployment.

Test: Agent runs `dotnet build [project].csproj` and existing local execution or trigger tests for the changed WebJob.
Pass: Agent observes zero compile errors. Agent confirms one representative trigger execution and one graceful-stop path in the changed scope.

## Trigger Pattern Matrix

| Need | Preferred pattern | Notes |
|---|---|---|
| Always-running process | Continuous WebJob | Pair with durable progress for long work |
| Time-based recurrence | Timer trigger | Keep Schedule rules explicit |
| Queue-driven processing | Queue trigger | Favor idempotent message handling |
| Blob-driven processing | Blob trigger | Keep file processing thin and delegated |

## Lifecycle Matrix

| Stage | Agent verifies | Preferred pattern | Avoid |
|---|---|---|---|
| Startup | Host loads extensions and configuration | `HostBuilder` plus `ConfigureWebJobs(...)` | Static startup helpers and hidden globals |
| Binding | Trigger signature matches the trigger source | Thin method with explicit binding attributes | Business logic inside binding code |
| Execution | Domain service owns the workload | Service call with injected dependencies | Inline orchestration plus direct SDK sprawl |
| Cancellation | Stop signal reaches inner async work | `CancellationToken` passed to each async hop | Ignored token during deployment restarts |
| Failure | Failure path stays observable | Structured logs and host-visible exception behavior | Silent message loss |
| Deployment | Published output matches App Service expectations | `dotnet publish` plus slot settings validation | Untested publish artifacts |

## Configuration Matrix

| Concern | Preferred pattern | Notes |
|---|---|---|
| Settings | Options binding from configuration | Keep secrets outside source control |
| External clients | `AddHttpClient(...)` and DI | Reuse normal .NET patterns |
| Telemetry | Structured logs and Azure telemetry | Include job, tenant, or run identifiers |
| Long work | Pair with checkpoint skill | Route to `long-running-job-patterns` when needed |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Local trigger verification | Existing local WebJobs host or integration test | Representative trigger binds and completes |
| Shutdown verification | Existing test or controlled host stop | In-flight work stops cleanly or saves durable state |
| Deployment verification | Existing publish validation or App Service smoke test | Published job starts and emits logs |

## Verification Checklist

Agent verifies:
- [ ] Host registers only required WebJobs extensions
- [ ] Trigger methods delegate to services
- [ ] Configuration and clients use DI and options
- [ ] Cancellation token reaches I/O operations
- [ ] Logs identify startup, execution, completion, and failure
- [ ] Local trigger execution passes before deployment

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Trigger method contains domain logic | Move logic into services |
| Host omits the needed extension | Add the required WebJobs extension package and registration |
| Job ignores shutdown signals | Pass cancellation through the stack |
| Publish output reaches Azure untested | Validate local execution and publish output first |
| Telemetry omits run identifiers | Add business and run correlation fields |
