---
name: efcore-testing-patterns
description: Test EF Core code with provider-aware isolation, deterministic data setup, and measurable verification rules.
title: EF Core Testing Patterns
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1400
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - dotnet-unit-testing
related_skills:
  - efcore-dbcontext-design
  - efcore-migrations
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - efcore
  - testing
  - isolation
---
# EF Core Testing Patterns

Agent tests EF Core code with explicit provider choice, per-test isolation, and assertions that prove data behavior.

## When to Use

| Condition | Use |
|---|---|
| Agent tests repositories, handlers, or services backed by EF Core | Use this skill |
| Agent validates mappings, migrations, or relational behavior | Use this skill |
| Agent investigates flaky tests caused by shared database state | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Test does not need EF behavior | Use standard unit tests without EF |
| Work is only query tuning | Use `optimizing-ef-core-queries` |
| Work targets production load testing | Use performance tooling |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Changed data-access scope | Yes | Repository, service, or `DbContext` |
| Required fidelity | Yes | InMemory, SQLite in-memory, or real test database |
| Isolation boundary | Yes | Per-test database, transaction, or disposable schema |
| Existing test runner | No | MSTest, xUnit, or equivalent |

## Strategy Matrix

| Test goal | Agent uses | Agent avoids |
|---|---|---|
| Pure control-flow logic around repositories | Mock abstraction, not `DbSet` internals | Complex `DbSet` mocks |
| Relational query semantics | SQLite in-memory with open connection lifetime | EF InMemory provider |
| Simple non-relational state assertions | EF InMemory provider | Assuming provider matches SQL translation |
| End-to-end persistence and migration checks | Real test database or containerized database | Shared developer database |

## Required Workflow

Agent performs these steps:
1. Agent chooses the lowest-cost provider that still proves the required behavior.
2. Agent creates isolated data state per test by using a unique database name, dedicated connection, or transactional cleanup.
3. Agent seeds only the records the assertion needs.
4. Agent asserts observable outcomes, including row shape, tracking behavior, exceptions, or generated side effects.
5. Agent disposes connections, contexts, and fixtures deterministically.

Test: Agent runs the smallest existing test command that covers the changed data-access path.
Pass: Agent observes zero failing tests. Agent observes zero cross-test contamination. Agent reruns each changed test independently with the same passing result.

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Targeted test run | `dotnet test [test-project].csproj --filter [Scope]` when filters exist | Zero failures |
| Isolation verification | Re-run the same targeted command twice | Identical pass result both times |
| Migration verification | Existing migration/integration tests | Schema setup succeeds |
| Cleanup verification | Inspect fixture or teardown behavior | No persistent test residue |

## Verification Checklist

Agent verifies:
- [ ] Provider choice matches the behavior under test
- [ ] Each test owns its own state boundary
- [ ] Test data stays minimal and deterministic
- [ ] Assertions prove behavior, not only non-null results
- [ ] Contexts and connections are disposed explicitly

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Mocking `DbSet` query providers for EF behavior | Test against SQLite or a real database |
| Using shared database names across tests | Generate a unique name or isolated connection |
| Using `EnsureCreated` when migration behavior matters | Use `Migrate` in migration-sensitive tests |
| Writing assertions that only check count > 0 | Assert the exact record, value, or failure mode |
