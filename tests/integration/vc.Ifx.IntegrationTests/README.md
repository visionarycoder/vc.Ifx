# Cross-Package Integration Regressions

This project exercises stable public contracts across package boundaries. All HTTP
traffic uses a real `HttpClient` with a terminal in-memory `HttpMessageHandler`;
the `.invalid` address is never resolved. Each relational test owns a private
SQLite in-memory connection and database. No Azure account, FTP server, HTTP host,
container, credentials, or external service is used.

## Scenarios

- Proxy DI resolves a scoped `DefaultProxyPipeline` with the real core
  `RetryInterceptor` and `HttpProxyTransport`. A bodyless GET retries a transient
  HTTP status or connection failure. Two configured retries mean at most three
  HTTP attempts. POST failures and requests with bodies are never replayed.
- Request metadata remains separate from HTTP payloads. Successful typed JSON and
  final response-header snapshots survive disposal of intermediate responses.
  Failed response content is disposed and not exposed through error messages.
- Borrowed streams start at their current position and remain readable; borrowed
  `HttpContent` and `HttpClient` remain usable after DI scope/provider disposal.
  Either cancellation token stops an active HTTP request without another attempt;
  cancellation during response reading disposes the response stream, not the
  caller's request stream. Signals coordinate cancellation without timing sleeps.
- Filtering specifications round-trip through portable JSON, pass through
  `EfFilterExecutionStrategy`, and compose with Querying ordering, deterministic
  tie-breakers, paging, and projection. Results match explicit expected rows and
  the original in-memory predicate. Command interception verifies deferred query
  construction and one relational read; generated SQL includes server-side
  filtering, ordering, and pagination.
- The legacy Querying schema also rehydrates through Filtering before SQLite
  execution. Explicit null guards, nullable comparison negation, captured
  membership, malformed-child rejection, and canceled enumeration are exercised.
  Cancellation leaves the caller-owned context and connection usable.

The existing unit-project `Observability/PipelineIntegrationTests.cs` already
composes Pipeline and Observability through DI for success, failure, cancellation,
trace/metric counts, and borrowed SDK ownership. That smoke test is deliberately
not duplicated here.

## Verification

Run the complete integration project with the shared mutex runner. Do not supply
a filter or a source/package scope:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -Project tests/integration/vc.Ifx.IntegrationTests/vc.Ifx.IntegrationTests.csproj -WarningsAsErrors
```

Verified 2026-09-09: all 15 tests passed, zero failures/skips and no build warnings,
with warnings treated as errors. Evidence:
`TestResults/tests/vc.Ifx.IntegrationTests/6c57f007ce8b45949b8dadafe7146a9f/tests.trx`.
The adjacent `run-context.json` confirms empty filter, source and package scopes.
The initial run had 11 passes and four test-assumption failures, corrected without
package changes (StreamContent may return a wrapper stream; CLR string predicates
need explicit null guards). Gate 1 records the final-verification handoff.

This integration run is not a coverage measurement. Full unit/integration combined
coverage, solution-wide warning-free builds, release packaging, and final Gate 1
acceptance remain with Orchestrator. SQLite translation and collation evidence is
not a portability guarantee for other database providers. These regressions do
not exercise live socket behavior or real-service authentication.
