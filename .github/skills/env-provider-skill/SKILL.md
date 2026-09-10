---
name: env-provider-skill
description: Generates environment provider abstractions for configuration access, validation, and testability.
title: Environment Provider Skill
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1180
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - configuration-options-pattern
  - dotnet-dependency-injection-standards
appliesTo: '**/*.{cs,csproj,json,yml,yaml}'
tags:
  - configuration
  - environment
  - testability
  - ste
---
# Environment Provider Skill

This skill creates testable configuration-access abstractions over environment and configuration sources.
This skill favors typed reads, clear validation, and dependency-injection registration.

## Pattern Table

| Concern | Preferred pattern | Example |
|---|---|---|
| Access surface | Interface-backed provider | `IEnvironmentProvider` |
| Required value | Throw descriptive startup exception | `GetRequiredString("PayrollApi:Url")` |
| Typed parsing | Parse in one location | `GetInt`, `GetBool`, `GetEnum<T>` |
| Tests | In-memory test provider | Dictionary-backed fake |
| Composition | Register via DI | `AddSingleton<IEnvironmentProvider, EnvironmentProvider>()` |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Classify | Agent inventories configuration reads and groups them by required, optional, and typed values. | Access map |
| 2. Define | Agent defines the interface and the smallest useful typed accessor set. | Provider contract |
| 3. Implement | Agent wraps `IConfiguration` or the relevant source with validation and logging. | Production provider |
| 4. Test | Agent adds a fake or stub provider for unit tests. | Test provider |
| 5. Register | Agent wires the provider into DI and updates consumers. | Integrated configuration surface |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Typed access | Review changed consumers. | Consumers read configuration through the provider instead of ad hoc parsing. |
| Validation | Run startup path or targeted tests. | Required keys fail fast with descriptive messages. |
| Testability | Review unit tests or fakes. | Tests supply configuration without mutating process-wide environment state. |
| Registration | Review service registration. | Exactly one canonical provider registration exists per composition root. |
