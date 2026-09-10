---
applyTo: "**/*.cs"
---

# Copilot Instructions: Core C# Generation

## Purpose
Provide domain-aware heuristics for generating clean, modern, testable C# 12 / .NET 8+ code aligned with repository and enterprise standards.

## Guiding Principles
- Prefer readability and maintainability over premature micro-optimizations.
- Use latest C# features (records, primary constructors, pattern matching, required members).
- Favor composition, DI, and small focused types.
- Keep public surface minimal; internal details encapsulated.

## Naming & Structure
- PascalCase for public types/members; camelCase for parameters & locals; no underscore prefixes.
- One type per file (exception: small related record types).
- Namespace mirrors folder structure.
- Avoid generic or ambiguous names (e.g., `Helper`, `Manager`).

## Async & Concurrency
- Use async end-to-end for I/O and external calls.
- Return `Task` / `ValueTask` (avoid `async void` except event handlers).
- Accept `CancellationToken` on boundary APIs; propagate inward.
- Avoid blocking (`.Result`, `.Wait()`); use `await`.

## Error Handling
- Throw domain-specific exceptions at boundaries; never expose raw low-level exceptions externally.
- Use guard clauses for argument validation.
- Avoid broad `catch (Exception)` unless rethrowing or translating.
- Do not swallow exceptions silently—log with context once at boundary.

## Dependency Injection
- Inject abstractions (interfaces) rather than concrete implementations.
- Use primary constructors for brevity when suitable.
- Avoid static mutable state; use DI scopes or options.
- Provide factory delegates for complex creation logic.

## Configuration & Options
- Bind strongly-typed options (`IOptions<T>` / `IOptionsMonitor<T>`).
- Validate options on startup (throw early if invalid).
- Keep environment overrides in `AppSettings.{Environment}.json`.

## Validation
- Use FluentValidation for non-trivial business rules.
- Separate validation from core logic; fail fast on invalid inputs.
- Aggregate multiple failures into structured result objects where appropriate.

## Logging & Observability
- Use `ILogger<T>` with structured key-value pairs.
- Include correlation/request IDs when available.
- Avoid logging secrets or PII.
- Keep logs at appropriate levels; no duplicated stack traces.

## Performance
- Measure before optimizing (BenchmarkDotNet for critical paths).
- Minimize allocations in hot paths (consider `readonly struct` when justified).
- Prefer streaming (`IAsyncEnumerable<T>`) for large data sets over materializing lists.
- Use caching (memory/Redis) at higher-level orchestration, not deep internals.

## Validation & Result Patterns
- Prefer result objects (`ServiceResult<T>`) encapsulating success + diagnostics.
- Use nullability annotations; enable nullable reference types.
- Avoid returning `null` for collections—return empty enumerables.

## Testing Heuristics
- Unit tests: deterministic, isolated (Moq for collaborator mocks; FluentAssertions for clarity).
- Integration tests: exercise real HTTP/database boundaries with test containers or in-memory providers.
- Naming: `MethodName_WhenCondition_ShouldOutcome`.
- Avoid brittle tests tied to implementation details (mock behavior— not internal state).

## Resilience
- Apply Polly policies (retry, timeout, circuit breaker, bulkhead) at external integration boundaries only.
- Instrument all resilience policies with logging + metrics.
- Guard against transient fault amplification (limit max retries + exponential backoff).

## Data Access
- Use EF Core with async APIs; avoid synchronous calls.
- Apply optimistic concurrency (row version or concurrency tokens) where conflict risk exists.
- Keep queries composable and projection-focused; avoid over-fetching.

## Serialization
- Use `System.Text.Json` defaults; configure only when necessary.
- Avoid dynamic typing; prefer strongly-typed DTOs / records.
- Validate inbound payloads prior to use (FluentValidation or manual checks).

## Patterns Reference
- Strategy, Decorator, Mediator, Repository, CQRS: choose for clarity, not ceremony.
- See `.copilot/design-patterns.instructions.md` for exemplar format and pattern scaffolds.

## Output Checklist (Before Finalizing)
1. File + namespace alignment verified.
2. Async usage correct (no blocking calls).
3. Dependencies injected, not newed inline (except trivial value objects).
4. Validation + error paths covered by tests.
5. Logging statements structured; no secrets.
6. Public API documented where non-trivial.
7. No magic numbers (extract constants).
8. Domain exceptions purposeful and minimal.

## Anti-Patterns (Reject Immediately)
- Static service locators or ambient context.
- Deep inheritance chains for behavior variation (prefer Strategy/Decorator).
- Leaking infrastructure concerns (HTTP, DB contexts) directly to higher layers.
- Mixing synchronous and asynchronous flows arbitrarily.
- Returning partially initialized objects.

## Example Skeleton
```csharp
public interface IWidgetProcessor
{
	Task<WidgetResult> ProcessAsync(WidgetRequest request, CancellationToken cancellationToken);
}

public sealed record WidgetRequest(string Name, int Quantity);
public sealed record WidgetResult(bool Success, string? Message);

public sealed class WidgetProcessor : IWidgetProcessor
{
	private readonly IWidgetRepository repository;
	private readonly ILogger<WidgetProcessor> logger;

	public WidgetProcessor(IWidgetRepository repository, ILogger<WidgetProcessor> logger)
	{
		this.repository = repository;
		this.logger = logger;
	}

	public async Task<WidgetResult> ProcessAsync(WidgetRequest request, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.Name) || request.Quantity <= 0)
			return new WidgetResult(false, "Invalid widget request");

		try
		{
			var exists = await repository.ExistsAsync(request.Name, cancellationToken);
			if (!exists)
			{
				await repository.CreateAsync(request.Name, request.Quantity, cancellationToken);
				logger.LogInformation("Created widget {Name} x{Quantity}", request.Name, request.Quantity);
			}

			return new WidgetResult(true, null);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed processing widget {Name}", request.Name);
			return new WidgetResult(false, "Processing failure");
		}
	}
}
```

---
*Evolve incrementally; link ADRs for substantive paradigm shifts.*
