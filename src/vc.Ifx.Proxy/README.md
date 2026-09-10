# vc.Ifx.Proxy

## Mission

Compose transport-independent invocation policies around one replaceable transport.
Interceptors own one source of variation each: authorization, correlation, caching,
auditing, telemetry, and resilience. HTTP transports and ASP.NET hosting belong in
their adapter packages; this package does not host endpoints.

## Stable Invocation Contracts

`IProxyPipeline`, `IProxyTransport`, `IProxyInterceptor`, `ProxyDelegate<T>`,
`ProxyContext`, and `ProxyResponse<T>` retain their signatures. Context is per-call
mutable state, not a singleton or a concurrently shared request. An interceptor
may replace the context passed to `next`; the transport receives that context.

Interceptors execute in ascending order, preserving registration order for ties.
`IOrderedProxyInterceptor.Order` takes precedence over `ProxyInterceptorOrder`.
An unrelated property named Order does not override an attribute. The built-in
pipeline is scoped so it can consume scoped interceptors without capturing them
in a singleton. Hosts register one transport and register/use the pipeline in a
service scope.

The delegate cancellation token is forwarded through execution. The dedicated
retry interceptor also honors the legacy context token. Cancellation and original
exceptions propagate; retry never transforms business errors into successful
responses. Failed `ProxyResponse<T>` results do not implicitly become exceptions.

## Policy Boundaries

- Retry is opt-in. `RetryInterceptor` only handles `RetryableTransportException`,
  using a validated options snapshot, bounded attempts, exponential backoff with
  jitter, and a 30-second delay cap. Zero retries means one attempt. A transport
  must classify a failure as retryable only when replaying that operation is safe.
  Do not stack it with another retry layer without an explicit attempt budget.
- The standalone `CircuitBreakerInterceptor` uses Polly 8's compatibility API
  to preserve consecutive exception counts, including a threshold of one. It
  excludes cancellation, does not retry, and rejects nonpositive thresholds or
  break durations. New combined resilience policies use `ResiliencePipeline`;
  its default retry/breaker classifiers handle explicitly retryable transport
  exceptions. Do not register both breaker policies for the same boundary.
- Default cache policy requires GET or HEAD, or an explicit operation policy;
  a missing method does not establish that an operation is safe to cache.
  Explicit keys/policies must include all identity, tenant, request, and result
  distinctions relevant to the result. A response cache is not authorization.
  Default keys use structured JSON before hashing, including payload, service,
  identity metadata, and normalized relevant headers. Nonserializable requests
  require a custom key provider. Cache keys are internal, versionable artifacts.
- `AddCaching` retains null providers until explicit providers are chosen.
  Options and logging are registered with the cache services. Authentication and
  authorization policies must be configured explicitly by the host.
- `IProxyAuthorizationPolicy` is the canonical enforcement port consumed by
  `SecurityInterceptor`. Every legacy `AuthorizationExtensions` registration
  installs one scoped bridge over `IAuthorizationPolicy`; all registered legacy
  policies must authorize. Empty, null, or failed legacy results deny. Explicit
  policies replace only the legacy null fallback; canonical host policies remain
  independent and also must authorize. Cancellation propagates through the bridge
  and security interceptor. Register security in the actual pipeline; registering
  policy objects alone does not insert an interceptor into arbitrary pipelines.
- Correlation is copied to both `ProxyContext.CorrelationId` and the legacy Items
  key. Audit records retain null duration until a measurement exists; unknown is
  different from zero elapsed time.
- `MemoryProxyCache.ClearAsync` atomically invalidates the instance's generation;
  old entries expire normally, and unrelated shared-cache entries are untouched.
  Response envelopes are copied on storage and retrieval; payload objects are not
  deep-cloned and must be treated as immutable by the caller.
- Built-in caching consumes duration, operation policies, response predicates,
  and custom key generation. `DefaultPriority` is policy metadata; the legacy
  `IProxyCache.SetAsync` port carries only an absolute expiration, not priority.
  `EnableEvictionLogging`, `MaxCacheSize`, `SlidingExpiration`, `MemorySizeLimit`,
  and `CachePolicy.ShouldRefresh` are retained passive compatibility settings,
  not implemented guarantees of this provider. Configure a host cache or custom
  `IProxyCache` for those requirements. Memory size is not measured in bytes.
  `AddDistributedCaching` is a legacy registration alias, not a distributed
  backend: it preserves null providers until explicitly replaced, and executes
  the configuration callback once. Supply a real distributed `IProxyCache` in
  the host; this package never silently supplies distributed consistency.
- Rate limiting uses the .NET segmented sliding-window implementation with ten
  segments, no queue, and atomic permit acquisition. The window must be at least
  10 milliseconds. Limits are per instance, operation, and user/client identity,
  not distributed limits. Dispose manually constructed interceptors; DI disposes
  container-owned instances. This replaces the racy prototype's exact timestamp
  queue with explicit segmented-window semantics.
- `DefaultTokenProvider` reads standard OAuth snake-case response fields, keeps
  caller-owned HttpClient settings unchanged, and never logs response bodies.
  Both validation entry points require a signed JWT; `SigningKey` is a UTF-8
  symmetric HMAC key. Missing keys fail closed. This provider does not implement
  JWKS discovery or asymmetric signing-key loading; use a host-specific
  `ITokenProvider` for those trust models. `ExtractClaims` is informational only.
- Outbound OAuth and secret-token interceptors now fail closed by default for
  missing credentials, errors, and timeouts. Their named failure switches permit
  deliberate unauthenticated fallback only when explicitly false. Invalid or
  expired tokens are never attached after rejection. Secret-token inspection
  checks encoding/lifetime only and trusts the configured secret source; it is
  not inbound authentication or signature validation. Default secret-token
  refresh returns unavailable; a subclass must implement a real issuer flow.
- Local configuration uses .NET configuration reload notifications, not a second
  file watcher. Explicit refresh reloads source files before invalidating cached
  objects, and disposal releases the owned root. Invalid section binding now
  propagates rather than silently returning a new default object; missing
  sections still return a default object. Async refresh can cancel while waiting
  for another refresh; the underlying JSON root reload is synchronous.
- Azure configuration explicit refresh now reloads the configuration root before
  clearing cached values, rather than merely dropping the cache. Its additive
  root constructor borrows an existing root unless ownership is transferred;
  notifications invalidate cached values and are disposed with the provider.
  The factory constructor builds an owned root for host startup customization
  or offline testing. Legacy builder registration validates Azure-specific
  settings immediately, including connection-string mode without an endpoint.
  `IsAvailable` checks local snapshot readability, not Azure connectivity.
  SDK sentinel refresh is demand-driven, not an automatically scheduled poll.
  `ApplyTo` configures SDK selectors/credentials without network loading. Invalid
  section bindings propagate; malformed scalar conversion retains its explicit
  fallback. Stop active reads/refreshes before disposing either provider.
- Ambient correlation data is copied per invocation and restored in a finally
  block, along with the previous correlation ID. Context dictionaries remain
  caller-owned mutable state; manually shared concurrent writes require locking.
  Header assignment replaces case-insensitive logical duplicates without changing
  the public dictionary comparer. Cache-Control directives are matched by name,
  not by substrings inside unrelated directive names.

The source under `Proxies/` is a pre-existing, excluded prototype, not a second
supported compiled API. Active implementations live in `Interceptors/`.
Azure.Identity and Azure App Configuration are deliberately retained in Proxy
for the current 1.x public identity. Every Proxy consumer, including applications
that use neither Azure provider, receives that package dependency footprint plus
the existing Querying and Secrets.Abstractions dependencies. This is not a minimal
invocation-only package. A future extraction requires a versioned compatibility
migration; this release adds no provider package, moves no public types, and
introduces no shared business-domain model library.

## Verification Status

Initial scoped evidence: on 2026-09-10, the serialized
strict package gate passed 463 tests, 2294/2294 lines and 692/692 branches (100%),
with warnings treated as errors. Coverage measures the entire compiled Proxy
module; test sources were scoped to `Proxy`, not the full solution suite.

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Proxy -TestSourceScope Proxy -TestPackage vc.Ifx.Proxy -Filter FullyQualifiedName~Tests.Proxy. -WarningsAsErrors
```

Evidence: `TestResults/coverage/vc.Ifx.Proxy/9dd7e51162384aa586c68073ef9dd05d/summary.json`.
The corrected legacy authentication registration contract also passed all eight
targeted tests in Release with warnings as errors, artifact
`TestResults/tests/vc.Ifx.UnitTests/41dc673d374949c0bfe24b837eb3a566/tests.trx`.
No handwritten source was excluded to achieve coverage.
[Final local acceptance](../../docs/planning/local-verification-20260910.md)
subsequently passed the full suite, warning-free solution build, strict coverage,
reporting, and package validation. The package is locally complete; hosted
execution, publication, and hands-on IDE acceptance remain external checks.
Retry implementation follows the [Polly retry contract](https://www.pollydocs.org/strategies/retry.html).
