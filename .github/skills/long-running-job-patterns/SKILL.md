---
name: long-running-job-patterns
description: Design resilient long-running .NET jobs with explicit lifecycle, checkpointing, cancellation, resumability, and recovery behavior.
title: Long-Running Job Patterns
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1550
appliesTo: '**/*.{cs,json}'
related_skills:
  - webjobs-authoring
  - quartz-job-authoring
  - application-insights-dotnet
tags:
  - background-jobs
  - resilience
  - checkpointing
  - dotnet
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
---
# Long-Running Job Patterns

Agent designs long-running jobs that resume deterministically, stop safely, and expose progress.

## When to Use

| Condition | Use |
|---|---|
| Agent designs work that runs for minutes or hours | Use this skill |
| Agent needs checkpoints, leases, resumability, or replay control | Use this skill |
| Agent needs observable cancellation and restart behavior | Use this skill |
| Agent needs durable progress outside a single process lifetime | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Work is short-lived and restart cost stays trivial | Use simpler job patterns |
| Primary work is trigger-host authoring | Use Quartz, WebJobs, or Functions host skills |
| Work is event-stream based and naturally partitioned elsewhere | Use event-driven patterns |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Work-unit boundary | Yes | Smallest idempotent or guarded unit |
| Checkpoint store | Yes | Durable state for resume |
| Cancellation source | Yes | Host shutdown, operator stop, or timeout |
| Duplicate-run guard | Yes | Lease, lock, or idempotency key |
| Progress audience | No | Logs, telemetry, dashboards, or APIs |

## Required Workflow

Agent performs these steps:
1. Agent defines the work unit and idempotency boundary first.
2. Agent records durable checkpoints at logical progress boundaries.
3. Agent propagates cancellation through loops and I/O.
4. Agent separates transient retry from terminal failure.
5. Agent drills restart, duplicate-run, and cancellation behavior before release.

Test: Agent runs `dotnet build [project].csproj` and existing tests or job drills that cover cancellation, resume, and duplicate-run behavior.
Pass: Agent observes zero compile errors. Agent confirms one successful run, one interrupted run, and one resumed run with no unintended duplicate work.

## Lifecycle Matrix

| Stage | Agent verifies | Preferred pattern | Avoid |
|---|---|---|---|
| Startup | Run identity and duplicate guard exist | Durable run ID plus lease or idempotency key | Untracked concurrent starts |
| Execution | Work splits into retryable units | Small batches or items with explicit boundaries | One giant monolithic transaction |
| Checkpoint | Resume state explains next step | Persisted position, counts, and timestamps | Opaque last-seen flags |
| Cancellation | Stop path saves recoverable state | Token checks near loops and I/O | Cancellation only at outer entry point |
| Failure | Retry policy distinguishes transient from fatal | Classified exceptions and limited retries | Blind retry of data defects |
| Resume | Replay rules stay deterministic | Skip completed units and continue from checkpoint | Guess-based resume logic |
| Completion | Final state closes the run | Completion record plus summary metrics | Run ends with no durable outcome |

## Pattern Matrix

| Concern | Preferred pattern | Notes |
|---|---|---|
| Work identity | Durable `RunId` and item IDs | Logs, checkpoints, and outputs align |
| Progress | Structured counts and stage names | Operators see slow versus stuck |
| Checkpoint frequency | Batch boundary or meaningful milestone | Balance recovery value against storage cost |
| Retry | Separate transient retry path | Pair with idempotent work units |
| Shutdown | Save state on host stop signal | Treat cancellation as normal lifecycle |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Cancellation drill | Existing test or targeted run cancelled mid-execution | Job stops promptly and saves usable checkpoint |
| Resume drill | Existing test or targeted rerun after interruption | Job resumes from checkpoint with deterministic counts |
| Duplicate-run drill | Existing test or targeted concurrent start | Second run is blocked or handled safely |
| Telemetry drill | Existing logs or monitoring output | Progress, failure, and completion events stay queryable |

## Verification Checklist

Agent verifies:
- [ ] Work units and idempotency boundaries are explicit
- [ ] Checkpoint data explains resume decisions
- [ ] Cancellation reaches loops and I/O
- [ ] Retry policy separates transient from terminal failure
- [ ] Duplicate execution is blocked or neutralized
- [ ] Progress and outcome data stay observable

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Job treats the whole run as one transaction | Split work into durable units |
| Checkpoint data lacks business identifiers | Store the identifiers needed for resume and audit |
| Retry path replays non-idempotent side effects | Add guards or move the boundary |
| Operators cannot see partial progress | Emit structured progress events |
| Validation covers only the happy path | Run interruption and duplicate-run drills |
