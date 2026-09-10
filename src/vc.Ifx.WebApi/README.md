# vc.Ifx.WebApi

ASP.NET Core hosting infrastructure for .NET 10: immutable HTTP status metadata,
safe Problem Details mapping, and replaceable Polly resilience policies.
Dependencies are Microsoft.AspNetCore.App and Polly.Core; this package has no
dependency on proxy infrastructure or application domain objects.

## Hosting

```csharp
using VisionaryCoder.Framework.WebApi.DependencyInjection;

builder.Services.AddIfxWebApi();
var app = builder.Build();
app.UseIfxWebApiExceptionHandling();
```

Register once. Defaults map ArgumentException to 400, UnauthorizedAccessException
to 403, and KeyNotFoundException to 404. Derived exceptions use the nearest mapped
base type. Other failures return catalog 500 details. InvalidOperationException
does not automatically mean 409; TimeoutException does not automatically mean
504. Explicitly map application exceptions when their semantics establish those
statuses:

```csharp
builder.Services.AddIfxWebApi(options =>
    options.Map<OrderConflictException>(409, "Conflict"));
```

The example assumes an application-defined OrderConflictException.
Unknown/invalid mappings fail validation, including direct edits to the options
dictionary. Only registered active error statuses can be mapped. Custom type
URIs must be absolute. Null types use the catalog RFC reference.

Responses contain a safe detail, status, title, type, request path (without query
parameters), and traceId. Exception messages and type names are hidden unless
IncludeExceptionDetails is explicitly enabled for trusted development use.
The default mapper/direct handler propagates cancellation; an uncanceled
OperationCanceledException is rethrown unchanged. Hosting uses built-in ASP.NET
UseExceptionHandler, which short-circuits OperationCanceledException/IOException
when RequestAborted is canceled, before handlers or mappers run. An unstarted
response is assigned 499 without a Problem Details body. This is server-side
response state, not a promise of delivery to a disconnected client; 499 remains
outside the package catalog. An uncanceled OperationCanceledException propagates
through the default hosting pipeline.

Before invoking handlers, ASP.NET clears the unstarted response and its headers,
and resets status to 500. Pre-throw authentication/protocol headers are NOT
preserved. The handler declines mapped 401 without WWW-Authenticate, 407 without
Proxy-Authenticate, 405 without Allow, or 426 without Upgrade. If those headers
were only assigned before throwing, default middleware fallback returns 500.

Prefer normal authentication challenges, routing and protocol responses. For
intentional exception mapping, register a trusted IProblemDetailsExceptionMapper
before AddIfxWebApi. During Map it can set the required headers AFTER reset from
trusted host configuration, then return matching valid ProblemDetails. The
application owns the actual schemes/protocols (including Connection: Upgrade
where required for 426). No scheme is invented and no stale headers or framing
are restored. The direct handler retains these post-reset headers.

Started responses are left alone. HEAD writes no body. The registered Problem
Details writer is preferred, with application/problem+json fallback. Writer
failures propagate from the direct handler; ASP.NET owns surrounding exception
middleware behavior. Caller/request cancellation is linked during writing.

## Catalog

```csharp
using VisionaryCoder.Framework.WebApi.Responses;

var missing = HttpResponseCatalog.NotFound;
var status = HttpResponseCatalog.Get(422); // Unprocessable Content
bool known = HttpResponseCatalog.TryGet(503, out var unavailable);
bool hasBody = HttpResponseCatalog.CanWriteBody(204, "GET"); // false
```

All is an immutable ordered list of immutable entries. Each entry exposes
StatusCode, ReasonPhrase, DefaultTitle, SafeDetail, Type, Retryability,
IsDetailSafeForClients, and AllowsBody. The catalog covers RFC-registered statuses
including 103 and 425. Reserved 306/418 and historic 305/510 are lookup metadata;
their presence does not authorize emission. Vendor 419/499/509 and temporary
draft status 104 are excluded. Numeric aliases resolve to one entry.

No 1xx, 204, 205 or 304 response permits a body. HEAD and successful CONNECT
add further method restrictions. Catalog metadata is not a response writer.
422 uses the current name Unprocessable Content (formerly Unprocessable Entity).

The source-of-truth table is
[docs/webapi/http-response-catalog.md](../../docs/webapi/http-response-catalog.md).
Standards: [IANA](https://www.iana.org/assignments/http-status-codes/),
[RFC 9110](https://www.rfc-editor.org/rfc/rfc9110.html),
[RFC 9457](https://www.rfc-editor.org/rfc/rfc9457.html).

## Resilience

IWebApiResiliencePipelineFactory.CreatePipeline returns a Polly pipeline.
The default enables a 30-second cooperative timeout per attempt and disables
retry. Pass the execution token into the operation's asynchronous work.

```csharp
builder.Services.AddIfxWebApi(configureResilience: options =>
{
    options.EnableRetry = true;
    options.OperationsAreReplaySafe = true;
});
```

Only opt in when every operation using this factory has established replay
safety. The enabled retry defaults to three retries with 200ms exponential
backoff and jitter. Retry wraps the per-attempt timeout; there is no total
operation timeout. Respect an external deadline through the execution token.
Retry counts must be 1..100, delays 0..one day, and timeout 10ms through one day.
Invalid values fail even when their strategy is disabled.

The default IWebApiTransientFailureClassifier accepts HttpRequestException
without a status or with 408/429/502/503/504, TimeoutException, and Polly
TimeoutRejectedException. It excludes arbitrary 5xx, authentication failures,
and cancellation. PotentiallyTransient catalog metadata is never a statement
that an operation is safe to retry. HTTP result objects are not retried.
The generic factory cannot inspect Retry-After or recreate request bodies;
replace it with an HTTP-aware policy when those are required.

Register an application IProblemDetailsExceptionMapper,
IWebApiTransientFailureClassifier, IWebApiResiliencePipelineFactory, or
TimeProvider before AddIfxWebApi to replace defaults. For direct construction,
the factory accepts an optional classifier and TimeProvider. Test clocks can
advance retries/timeouts without sleeping.

## Verification

Tests live under tests/unit/vc.Ifx.UnitTests/WebApi and use deterministic fakes.
Hosting regressions also build and invoke the real built-in exception-handler
pipeline using a Production WebApplication service provider, without a listener.
On 2026-09-10, strict Release verification passed 109/109 tests, 280/280 lines and
116/116 branches (100%), with zero skips/build/test warnings. Evidence:
`TestResults/coverage/vc.Ifx.WebApi/aa310c6abd184df4b7fed25be48871a1/summary.json`.
All eleven new hosting regressions additionally passed after full-source Release
test compilation, artifact `TestResults/tests/vc.Ifx.UnitTests/723f918ac5ba4b0bb68da8600e2da0b7/tests.trx`.
The catalog retains prior Debug evidence and exact verification commands.
Package coverage and solution-wide gates are recorded in the planning section.
The package is not declared complete until measured line/branch coverage and
the orchestrator's global verification both pass.
