---
name: result-types-dotnet
title: Result Types for .NET
description: Model success, failure, absence, and decision unions in .NET with hand-rolled result types and railway-oriented flow instead of exception-driven control.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1973
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - cqrs-patterns-dotnet
  - domain-events-dotnet
  - dotnet-exception-handling-standards
  - dotnet-validation-standards
appliesTo: '**/*.{cs,csproj,md}'
tags:
  - dotnet
  - ddd
  - result
  - error-handling
  - functional
---
# Result Types for .NET

Model expected outcomes with explicit result types, option types, and union shapes instead of exception-driven branching.

## When to Use

| Prompt or Code Shape | Use |
|---|---|
| Business validation failures are common and expected | Use this skill |
| Command or query flow needs explicit success and failure states | Use this skill |
| Caller logic needs absence handling without null-driven ambiguity | Use this skill |
| Team asks for railway-oriented composition without third-party libraries | Use this skill |

## When Not to Use

| Prompt or Code Shape | Route |
|---|---|
| Runtime fault is exceptional and aborts the operation | Keep exception flow for the fault path |
| API contract already standardizes RFC problem details at the edge only | Map internal results to that edge contract |
| Work item asks only for domain event design | Use `domain-events-dotnet` |
| Work item asks only for command-query split | Use `cqrs-patterns-dotnet` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Outcome categories | Yes | Agent distinguishes success, validation failure, not found, conflict, and fault. |
| Consumer expectations | Yes | Record what each caller needs to branch on. |
| Composition points | Yes | Record where flows chain through map and bind steps. |
| Nullability rules | Yes | Agent avoids mixed null and result signaling. |
| Boundary mapping | No | Record HTTP, message, or UI mapping at the outer edge. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Classify outcomes into explicit categories. | Read use cases and current exception flow. | Expected outcomes have named result states. |
| 2 | Define `Result`, `Result<T>`, `Option<T>`, and union shapes where needed. | Read shared outcome types. | Shared types cover success, failure, and absence without null ambiguity. |
| 3 | Rewrite orchestration to compose with `Map`, `Bind`, and `Match`. | Read changed service or handler flow. | Control flow expresses outcome transitions without nested exception branching. |
| 4 | Reserve exceptions for faults, not caller-correctable outcomes. | Read throws and catch blocks. | Expected business outcomes avoid exception control flow. |
| 5 | Map outer-edge contracts from results explicitly. | Read controllers, endpoints, or UI adapters. | Boundary mapping stays explicit and local. |
| 6 | Agent runs targeted build and tests. | Run `dotnet build <project>` and targeted tests. | Build succeeds and changed flows pass tests. |

## Decision Matrix

| Outcome Shape | Use When | Agent Action |
|---|---|---|
| `Option<T>` | Value absence is normal and non-error | Model `Some` or `None` only. |
| `Result` | Operation has success or failure without payload | Return state plus error details. |
| `Result<T>` | Operation returns payload on success | Return typed value or error. |
| Union record hierarchy | Caller branches on more than success or failure | Model named cases such as `Approved`, `Rejected`, and `Deferred`. |

## Implementation Patterns

```csharp
public sealed record Error(string Code, string Message);

public readonly record struct Result(bool IsSuccess, Error? Error)
{
    public static Result Success() => new(true, null);
    public static Result Failure(string code, string message) => new(false, new(code, message));
}

public readonly record struct Result<T>(bool IsSuccess, T? Value, Error? Error)
{
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string code, string message) => new(false, default, new(code, message));
    public Result<TNext> Map<TNext>(Func<T, TNext> map) => IsSuccess ? Result<TNext>.Success(map(Value!)) : Result<TNext>.Failure(Error!.Code, Error.Message);
    public Task<Result<TNext>> BindAsync<TNext>(Func<T, Task<Result<TNext>>> bindAsync) => IsSuccess ? bindAsync(Value!) : Task.FromResult(Result<TNext>.Failure(Error!.Code, Error.Message));
}

public readonly record struct Option<T>(bool HasValue, T? Value)
{
    public static Option<T> Some(T value) => new(true, value);
    public static Option<T> None() => new(false, default);
}

public abstract record PaymentDecision
{
    public sealed record Approved(decimal Amount) : PaymentDecision;
    public sealed record Rejected(string ReasonCode) : PaymentDecision;
    public sealed record Deferred(DateOnly ReviewDate) : PaymentDecision;
}
```

```csharp
return await repository.GetByIdAsync(command.TimesheetId, cancellationToken)
    .ToResult("timesheet.not_found", "Timesheet was not found.")
    .BindAsync(timesheet => Validate(timesheet, command))
    .BindAsync(timesheet => Approve(timesheet, clock, cancellationToken));
```


## Rules

| Rule | Agent Action |
|---|---|
| Exceptions | Use exceptions for faults and broken assumptions, not expected validation outcomes. |
| Nullability | Agent avoids returning `null` when `Option<T>` or `Result<T>` represents the outcome. |
| Error vocabulary | Use stable codes that align with domain language and boundary mapping. |
| Result nesting | Agent avoids `Result<Option<T>>` unless absence and failure are both required and distinct. |
| Mapping | Keep result-to-HTTP or result-to-UI mapping at the outer edge. |
| Libraries | Agent avoids FluentResults, OneOf, LanguageExt, and similar dependencies. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Exception and null reduction | Search throws, catches, null returns, and changed return types. | Expected outcomes use `Result`, `Result<T>`, `Option<T>`, or named unions. |
| Error and composition shape | Review error codes and orchestration methods. | Codes are stable and `Map` or `Bind` expresses the flow. |
| Library absence | Search for FluentResults, OneOf, LanguageExt, or Optional. | No third-party result package appears in the changed scope. |
| Build and tests | Run targeted build and tests. | Build succeeds and changed tests pass. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Result type wraps exceptions wholesale | Separate expected errors from true faults. |
| Both `null` and `Result<T>` signal absence | Keep one absence pattern only. |
| Error messages replace stable error codes | Add a code and keeps message text secondary. |
| Union cases mirror HTTP status codes directly | Agent renames cases in domain vocabulary and maps at the boundary. |
| Helper methods return nested result pyramids | Agent flattens flow with bind operations and focused helpers. |

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find existing outcome models | `search_code` | Search for `Result`, `Option`, error codes, and boundary mapping helpers before editing. |
| Review result-type PR changes | `pull_request_read` | Read changed handlers, controllers, and validation flow before extending explicit outcome modeling. |
| Verify exception-to-result boundaries | `search_code` | Search for expected throws, null returns, and mapping code so result adoption stays consistent. |

## Outputs

- Shared result and option types
- Domain error vocabulary
- Railway-oriented handler examples
- Union-shape decision examples
- Boundary mapping guidance
- Verification steps for outcome flow
