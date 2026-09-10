---
applyTo: '**/*'
---

# Copilot Base Instructions

## Purpose
Provide a concise, modular hub for AI-assisted code and documentation generation, extending the global `.github/copilot-instructions.md` and linking domain capsules under `docs/best-practices/`.

## Precedence & Composition
1. Domain-specific instruction files (e.g., `design-patterns.instructions.md`) override base guidance for that scope.
2. This file (`.copilot/copilot-instructions.md`) supplies aggregation + shared rules.
3. Global file (`.github/copilot-instructions.md`) supplies enterprise baseline.
4. Repository standards (`repo-standards.md`) apply to structure, hygiene, and workflow.
When in doubt, favor the most specific applicable file.

## Core Generation Workflow
1. Identify domain (Security, Integration, Data, Architecture, etc.).
2. Open relevant capsule README (see Domain Index below).
3. Apply C# + naming rules (see C# Core section).
4. Generate minimal, clean code (single responsibility, testable).
5. Add focused unit tests (Moq + FluentAssertions).
6. Add README snippet (intent, usage, extension points).
7. Cross-check for: DI readiness, async correctness, logging (structured), validation, error handling.
8. Ensure no secrets or machine-specific artifacts.
9. Link ADR if architectural in nature.

## C# Domain Topical Extensions
These topics extend baseline guidelines with generation heuristics.
- **Naming & Structure:** PascalCase for public types; camelCase for locals/parameters; no underscore prefixes; one type per file; align namespace to folder.
- **Async & Concurrency:** Prefer async all the way; return `Task`/`ValueTask`; never block with `.Result`/`.Wait()`; use cancellation tokens on external/resource boundaries.
- **Error Handling:** Use custom exception types for domain boundaries; avoid swallowing exceptions; log at boundary once (avoid duplicates); use guard clauses for argument validation.
- **Dependency Injection:** Accept abstractions (interfaces) in constructors; avoid service locator; prefer primary constructors for brevity; avoid static singletons—register hosted services or factory delegates.
- **Validation:** Use FluentValidation for complex business rules; keep validators isolated; aggregate validation failures into structured result objects.
- **Logging & Observability:** Inject `ILogger<T>` / telemetry; include correlation/request IDs; never log secrets or PII; use structured logging (`key=value`).
- **Configuration:** Bind strongly-typed options classes via `IOptions<T>`; validate on startup; segregate environment-specific overrides (`AppSettings.{Environment}.json`).
- **Testing:** Unit tests isolate collaborators via Moq strict mocks where behavior matters; integration tests use in-memory or test containers; naming pattern: `MethodName_WhenCondition_ShouldOutcome`.
- **Performance:** Minimize allocations in hot paths; use `readonly struct` for lightweight value types where appropriate; benchmark critical algorithms with BenchmarkDotNet (optional stage).
- **Resilience:** Apply Polly policies (retry, circuit breaker) at external boundary (HTTP, DB, message bus) not internally; instrument policies with logging + metrics.
- **Patterns Usage:** Select patterns for clarity (Strategy for pluggable algorithms, Mediator for request/response, Decorator for cross-cutting). Reference `design-patterns.instructions.md` for template format.

## Domain Index
| Domain | Capsule | Description |
|--------|---------|-------------|
| Architecture (General) | `docs/best-practices/software-architecture/readme.md` | Structural & logical design principles |
| Cloud Architecture | `docs/best-practices/cloud-architecture/readme.md` | Cloud-specific topologies & deployment patterns |
| Security | `docs/best-practices/security/readme.md` | AuthZ/AuthN, secret management, hardening |
| Integration & APIs | `docs/best-practices/integration/readme.md` | REST/gRPC/GraphQL/messaging guidelines |
| Observability | `docs/best-practices/observability/readme.md` | Logging, tracing, metrics, AIOps |
| DevOps & Delivery | `docs/best-practices/devops/readme.md` | Pipelines, environment strategy, automation |
| Data & Analytics | `docs/best-practices/data-analytics/readme.md` | Data lifecycle, analytics principles |
| Templates & Scaffolding | `docs/best-practices/templates/readme.md` | Boilerplate, reusable module templates |
| Radar | `docs/best-practices/radar.md` | Capability maturity tracking |
| Design Patterns | `.copilot/design-patterns.instructions.md` | Modern C# pattern implementations |
| Core C# | `.copilot/csharp.instructions.md` | Extended C# generation heuristics |

## Output Quality Checklist
Before finalizing AI-generated output:
- Lint & analyzers pass (nullable + style rules).
- Public APIs documented (XML summaries where appropriate).
- Unit tests provided for new logic paths.
- Links added: related ADR / domain capsule / pattern reference.
- No opaque magic numbers—extract constants.
- No dead/unreachable code.
- Async flows propagate cancellation tokens.
- Logging is structured and minimal (no duplication).

## Anti-Pattern Quick Reject List
Reject or refactor if output includes:
- Synchronous wrappers over async operations.
- Static mutable global state.
- Hidden temporal coupling or magic delays (Thread.Sleep).
- Broad catch blocks without discrimination or rethrow strategy.
- Unbounded retries or recursion without safeguards.
- Hard-coded secrets, credentials, or environment paths.

## Contribution Guidance for New Domains
1. Create capsule folder under `docs/best-practices/<domain>/` with `readme.md`.
2. Add entry to Domain Index table here.
3. Include: Scope, Principles, Do/Don't, Example snippet, Cross-cutting concerns.
4. Reference corresponding ADRs if introducing architectural shifts.

## References
- Global Enterprise Guidelines: `.github/copilot-instructions.md`
- Repository Standards: `.copilot/repo-standards.md`
- Design Patterns: `.copilot/design-patterns.instructions.md`

---
*This base file evolves alongside architecture decisions; keep changes atomic and traceable (link ADR in commit message).*
