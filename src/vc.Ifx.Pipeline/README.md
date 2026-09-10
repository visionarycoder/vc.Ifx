# vc.Ifx.Pipeline

Request dispatch and policy interception for .NET 10 / C# 14. This package owns
its request, handler, routing, cache, authorization, tracing, and metrics contracts.
No application domain-object library or dependency on WebApi/proxy is introduced.

## Stable Adapter Contract

All existing no-token signatures remain, including IInvoker.InvokeAsync,
IInterceptor.InvokeAsync, ILocalDispatcher/IRemoteDispatcher.DispatchAsync,
ICache.TryGetAsync/SetAsync, and IAuthorizationService.AuthorizeAsync.
IRequestHandler.HandleAsync(request, CancellationToken ct), ISerializer,
ITracer, ISpan, and IMetrics are unchanged, including existing namespaces.
New overloads append a required CancellationToken. IInterceptor's new next
delegate is Func<TRequest, CancellationToken, Task<TResponse>>.

Default interface implementations check cancellation before delegating to old
implementations, keeping existing gRPC/Observability/cache/auth adapters usable.
The interceptor bridge checks cancellation before invoking its continuation.
Bridges cannot interrupt legacy work. Consumers should implement the new
overloads and forward tokens to transport/cache/auth operations; do not replace
the old methods. Built-ins implement both overloads and forward tokens throughout.
There is no cancellation-token ambient/global state.

## Invocation and Policy Order

PipelineInvoker snapshots registrations and runs the first registered interceptor
outermost, with reverse completion order. It rejects null collaborators/elements
and multiple built-in ResilienceInterceptor registrations. Exceptions propagate
without wrapping. Routing occurs at the terminal, so rejected or cached requests
do not dispatch. Suggested order: logging, tracing, metrics, authorization,
caching, resilience, dispatcher. Authorization must precede caching to prevent
cached responses bypassing authorization.

Each invocation uses the existing Correlation.CurrentId or creates one, sharing
it through the call and restoring the previous value afterward, including failures.
Nested/concurrent invocations preserve AsyncLocal isolation.

LocalDispatcher resolves the exact IRequestHandler<TRequest,TResponse> from the
supplied IServiceProvider and forwards cancellation; missing handlers fail
clearly. The provider owns handler lifetimes. RegistryBasedResolver defaults
unregistered requests to local and reads the registry for each resolution, so
updated registrations are visible. InMemoryServiceRegistry supports concurrent
lookup/replacement. ServiceEntry requires a nonblank name and absolute HTTP(S)
URI. KubernetesDnsRegistry removes only a trailing Request suffix, lowercases the
name, and validates it as a DNS label; generic/invalid names fail explicitly.

HttpRemoteDispatcher validates remote HTTP(S) endpoints, sends JSON POST with the
caller token, validates status, reads/deserializes content, and disposes request
and response messages. It borrows HttpClient and ISerializer. Trace context is
injected through the platform DistributedContextPropagator, replacing nonstandard
baggage-* headers. JSON deserialization rejects null documents for response
contracts; serializer errors and unsuccessful status responses propagate.

## Interceptors

Caching retains the existing Query suffix convention for compatibility. Commands
bypass cache/key selection. Cache keys are application-owned and must include
all identity/tenant/authorization and result-type dimensions; keys are validated
as nonblank. TTL must be positive. A hit may contain null. Misses cache successful
results, including null, but never failed or canceled work. Cache failures
propagate. There is no single-flight or automatic cache invalidation.

Authorization exceptions stop the chain. Logging scopes are disposed and preserve
the same correlation used by the invoker; canceled operations are distinguished
from errors. Tracing records success/error/canceled, ends each created span once,
and disposes it once. Trace error tags include exception messages and are for a
trusted diagnostic sink, not clients.

Metrics increments requests_total for every completed invocation, plus
requests_failed_total or requests_canceled_total as appropriate, and records one
request_duration_ms sample. Logging and metrics accept an optional TimeProvider
for deterministic elapsed time. Diagnostic sink failures propagate; sinks should
be nonthrowing to avoid masking operation failures.

## Resilience and Migration

Use the new ResilienceInterceptor(ResiliencePipeline) constructor for Polly 8.
The existing AsyncPolicy constructor and DefaultPolicy() remain compatibility
entry points. Exactly one supplied policy runs; there is no nested default policy.
DefaultPolicy now returns a no-op policy because a generic request does not prove
replay safety. DefaultPipeline() likewise does not retry. Calling
DefaultPipeline(operationsAreReplaySafe: true) explicitly enables three retries,
200ms exponential jittered delay, only for HttpRequestException with no status or
408/429/502/503/504. Other statuses, arbitrary exceptions, and cancellation are
not transient by default. Optional TimeProvider allows deterministic delay tests.
Custom pipelines own their retry/timeout/circuit-breaker budgets. Avoid a second
retry policy in an HTTP/gRPC handler or another interceptor, and never opt in
without idempotency and replayable requests. Retry-After-aware protocols require
an application policy. The centrally pinned Polly packages are 8.6.4. The legacy AsyncPolicy API remains
available for compatibility; retaining that API does not imply a Polly 7 package dependency.

Behavior changes: live registry resolution replaces indefinitely stale cached
routes; invalid arguments/routes fail early; cancellation flows through built-ins;
metrics now counts failed/canceled calls in total; unsafe default retries are
disabled; HTTP baggage uses platform propagation. Existing positional
EndpointResolution and ServiceEntry constructor shapes are retained.

The gRPC adapter implements the cancellation-aware IRemoteDispatcher overload
and forwards its token to the gRPC call. Observability adapters retain their
interfaces and verify idempotent span End/Dispose behavior and the updated
metric meanings. Hosts should register supplied collaborators explicitly; this
package does not invent provider DI registrations.

## Verification

Use the infrastructure wrapper:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Pipeline -TestSourceScope Pipeline -TestPackage vc.Ifx.Pipeline -Filter FullyQualifiedName~Tests.Pipeline -WarningsAsErrors
```

Verified 2026-09-09: 33/33 deterministic scoped tests; 314/314 lines and 92/92
branches (100%), with warnings treated as errors. Evidence:
`TestResults/coverage/vc.Ifx.Pipeline/292ab125e54744308434cc6310e17e3a/summary.json`
and the adjacent `vc.Ifx.UnitTests/tests.trx`. No package coverage exclusions were
added. Serialized, warnings-as-errors builds of vc.Ifx.Pipeline.Grpc and
vc.Ifx.Observability passed unchanged with zero warnings/errors. These builds
prove source compatibility, not full consumer behavior or binary compatibility.

These initial scoped results are superseded by
[final local acceptance](../../docs/planning/local-verification-20260910.md), which
passed the full solution, suite, strict package coverage, reporting, and packaging.
Hosted execution, publication, and hands-on IDE acceptance remain external checks.
Primary references: [Polly retry](https://www.pollydocs.org/strategies/retry.html)
and [platform context injection](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.distributedcontextpropagator.inject?view=net-10.0).
