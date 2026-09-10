---
name: efcore-migrations
description: Create and apply EF Core migrations with deterministic naming, rollback planning, and measurable deployment checks.
title: EF Core Migrations
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1250
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - efcore-dbcontext-design
related_skills:
  - efcore-testing-patterns
  - optimizing-ef-core-queries
appliesTo: '**/*.{cs,csproj,json,sql,md}'
tags:
  - efcore
  - migrations
---
# EF Core Migrations

Agent manages schema evolution with deterministic migrations, reversible changes where feasible, and explicit deployment verification.

## When to Use

| Condition | Use |
|---|---|
| Agent changes EF Core model shape | Use this skill |
| Agent prepares schema deployment or rollback steps | Use this skill |
| Agent seeds stable reference data through migrations | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Database is managed outside EF Core migrations | Use the team deployment process |
| Work is only `DbContext` design | Use `efcore-dbcontext-design` |
| Work is only test isolation or provider choice | Use `efcore-testing-patterns` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Target `DbContext` | Yes | Name context when the solution contains multiple contexts |
| Change scope | Yes | Tables, columns, indexes, constraints, or seed data |
| Deployment path | Yes | Local update, CI SQL script, or runtime migration |
| Rollback expectation | No | Required for destructive or high-risk changes |

## Migration Workflow

| Step | Agent action | Output |
|---|---|---|
| 1 | Agent builds the project and confirms the model compiles cleanly | Baseline build status |
| 2 | Agent creates one descriptive migration for one logical change set | New migration files |
| 3 | Agent reviews `Up` and `Down` for naming, data loss, and deterministic seed data | Edited migration when needed |
| 4 | Agent applies the migration to a local or test database path | Updated schema |
| 5 | Agent generates a deployment script when the target environment requires reviewable SQL | SQL artifact or command |
| 6 | Agent verifies rollback or downgrade path when the change is reversible | Rollback evidence |

Test: Agent runs `dotnet build [project].csproj`, `dotnet ef migrations add [Name]`, and `dotnet ef database update` against the approved non-production target.
Pass: Agent observes zero compile errors. Agent applies the migration successfully. Agent sees schema state that matches the intended model change.

## Decision Matrix

| Scenario | Agent uses | Agent avoids |
|---|---|---|
| Pure schema change | One focused migration | Bundling unrelated features |
| Reference seed data | Deterministic constants | `DateTime.Now`, `Guid.NewGuid()` in seeding |
| Production deployment | Reviewed SQL script or approved pipeline | Blind runtime migration without environment approval |
| Destructive change | Explicit backup and rollback note | Silent column/table drops |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Migration creation | `dotnet ef migrations add [Name]` | Files generated once with expected diff |
| Apply verification | `dotnet ef database update` | Zero apply errors |
| Rollback verification | `dotnet ef database update [PreviousMigration]` when supported | Database returns to expected prior state |

## Verification Checklist

Agent verifies:
- [ ] Migration name describes one logical change set
- [ ] `Up` and `Down` contain deterministic operations
- [ ] Seed data uses stable constants
- [ ] Apply path succeeds on a non-production target
- [ ] Deployment SQL exists when the environment requires script review

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Migration includes unrelated model drift | Revert drift or split the change |
| Seed data changes on every scaffold | Replace dynamic values with constants |
| Rollback path drops needed data | Document irreversibility and back up first |
| Production update depends on ad hoc local commands | Generate reviewed SQL or use the approved pipeline |
