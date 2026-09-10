---
name: specification-pattern-dotnet
title: Specification Pattern for .NET
description: Compose business rules and query filters in .NET with hand-rolled specifications, expression combinators, and VBD-aligned rule ownership.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1944
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - repository-unitofwork-efcore
  - cqrs-patterns-dotnet
  - vbd-system-design
  - optimizing-ef-core-queries
appliesTo: '**/*.{cs,csproj,md}'
tags:
  - dotnet
  - ddd
  - specification
  - efcore
  - vbd
---
# Specification Pattern for .NET

Agent composes reusable business rules and query predicates with hand-rolled specifications that align with VBD-owned policy.

## When to Use

| Prompt or Code Shape | Use |
|---|---|
| Repeated business predicates appear across handlers or repositories | Use this skill |
| Query filtering needs composable `And`, `Or`, and `Not` behavior | Use this skill |
| EF Core queries need reusable expression trees without third-party packages | Use this skill |
| VBD policy ownership needs explicit rule types instead of scattered lambdas | Use this skill |

## When Not to Use

| Prompt or Code Shape | Route |
|---|---|
| One short predicate appears once in one method | Keep the local lambda |
| Rule logic mutates state instead of filtering or asserting | Use domain methods or handlers instead |
| Query needs only projection tuning or include strategy | Use `optimizing-ef-core-queries` |
| Work item asks only for repository and transaction pattern | Use `repository-unitofwork-efcore` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Rule catalog | Yes | Identify reusable predicates and owned policy terms. |
| Entity or aggregate type | Yes | Agent binds each specification to one model type. |
| Query provider | Yes | Agent confirms LINQ provider translation requirements. |
| Composition needs | Yes | Record `And`, `Or`, `Not`, sorting, or pagination interactions. |
| Ownership boundary | No | Map the rule to the engine, manager, or access scope that owns policy change. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent extracts repeated predicates into named specifications. | Read repeated LINQ filters. | Each extracted specification has one business meaning. |
| 2 | Expose `ToExpression` and composition helpers. | Run `rg -n "ToExpression|And\\(|Or\\(|Not\\(" <scope>`. | Specification contracts and combinators exist. |
| 3 | Keep provider-translatable expressions for query use. | Run targeted EF Core query tests. | Expressions translate without client-evaluation defects. |
| 4 | Agent places specifications in the scope that owns the policy change. | Compare placement to VBD boundaries. | Rule ownership matches volatility ownership. |
| 5 | Use repository or query extensions to apply specifications. | Read query entry points. | Call sites stay concise and expressive. |
| 6 | Agent runs targeted build and tests. | Run `dotnet build <project>` and targeted tests. | Build succeeds and changed tests pass. |

## Decision Matrix

| Rule Shape | Use When | Agent Action |
|---|---|---|
| Pure predicate specification | Rule filters one entity set | Implement `Expression<Func<T, bool>>`. |
| Assertion specification | Rule checks one in-memory entity | Add `IsSatisfiedBy`. |
| Composite specification | Rule combines named predicates | Use `And`, `Or`, or `Not`. |
| Query extension | Many callers apply the same repository filter | Expose `Where(specification)` helpers. |

## Implementation Patterns

```csharp
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();
    public bool IsSatisfiedBy(T candidate) => ToExpression().Compile()(candidate);
    public Specification<T> And(Specification<T> other) => new AndSpecification<T>(this, other);
    public Specification<T> Or(Specification<T> other) => new OrSpecification<T>(this, other);
    public Specification<T> Not() => new NotSpecification<T>(this);
}

public sealed class AndSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression() =>
        left.ToExpression().Compose(right.ToExpression(), Expression.AndAlso);
}

public sealed class OpenInvoiceSpecification : Specification<Invoice>
{
    public override Expression<Func<Invoice, bool>> ToExpression() => invoice => !invoice.IsClosed;
}

public static class SpecificationQueryableExtensions
{
    public static IQueryable<T> Where<T>(this IQueryable<T> query, Specification<T> specification) =>
        query.Where(specification.ToExpression());
}
```

| Pattern | Use |
|---|---|
| Base specification | Reusable predicate and optional `IsSatisfiedBy` support |
| `And`, `Or`, `Not` combinators | Named rule composition |
| Queryable extension | Repository and handler call-site reuse |
| Named specifications | Domain vocabulary such as `OpenInvoiceSpecification` |


## Rules

| Rule | Agent Action |
|---|---|
| Business naming | Name specifications from domain vocabulary. |
| Provider translation | Keep expressions translatable by the active LINQ provider. |
| Scope ownership | Agent places specifications with the policy-owning boundary, not the nearest repository only. |
| Projection | Keep projection logic outside specifications. |
| State mutation | Keep specifications side-effect free. |
| Libraries | Agent avoids Ardalis.Specification and similar dependencies. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Named rules and composition | Review extracted predicates and search for `AndSpecification`, `OrSpecification`, and `NotSpecification`. | Reusable policy terms have named types and composable helpers. |
| Provider translation | Run targeted EF Core query tests. | Queries translate with no client-evaluation defects. |
| Library absence and ownership | Search for third-party specification packages and compare placement to the volatility boundary. | The changed scope stays hand-rolled and rules live with the policy owner. |
| Build and tests | Run targeted build and tests. | Build succeeds and changed tests pass. |


## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Specification carries `Include` graphs, paging, and projection all together | Separate predicate reuse from query-shaping concerns unless the repository contract requires both. |
| In-memory helper method inside expression blocks translation | Rewrite the rule as a provider-translatable expression. |
| Repository hides business vocabulary behind anonymous lambdas | Promote stable predicates to named specifications. |
| Specification type lands in an access layer that does not own the policy | Agent moves the rule to the policy-owning boundary. |
| Composite rule names become technical only | Agent renames them in domain vocabulary. |

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find existing specification and predicate reuse | `search_code` | Search for `Specification<`, `ToExpression`, combinators, and reusable query filters before editing. |
| Review specification PR changes | `pull_request_read` | Read changed rule types, repositories, and query handlers before extending composable business rules. |
| Verify translation-safe coverage | `search_code` | Search for EF Core query tests, provider-specific filters, and projection boundaries so composed rules stay translatable. |

## Outputs

- Base specification contracts
- And, Or, and Not combinators
- Queryable extension examples
- Named business-rule examples
- VBD rule-ownership guidance
- Verification steps for translation and tests
