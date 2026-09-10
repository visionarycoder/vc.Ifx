---
title: HTTP Response Catalog
doc_type: reference
status: active
last_updated: 2026-09-10
---
# HTTP Response Catalog

Contract owner: vc.Ifx.WebApi. Date: 2026-09-09. Dependencies remain Microsoft.AspNetCore.App and Polly.Core 8.6.4; no vc.Ifx project dependencies.

## Sources and Scope

Verified against the [IANA registry](https://www.iana.org/assignments/http-status-codes/), [ASP.NET Core 10 StatusCodes](https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Http/Http.Abstractions/src/StatusCodes.cs), [HTTP semantics](https://www.rfc-editor.org/rfc/rfc9110.html), and [Problem Details](https://www.rfc-editor.org/rfc/rfc9457.html).
Include permanent RFC registrations, including 103 and 425 even where ASP.NET constants lag. Include reserved 306/418 and historic 305/510 as lookup metadata, not permission to emit them. Exclude unassigned/vendor 419/499/509 and temporary draft status 104.

## Immutable Contract

Namespace: VisionaryCoder.Framework.WebApi.Responses.
HttpResponseCatalog.All is an immutable, status-ordered read-only list. Get(int) throws ArgumentOutOfRangeException for unknown codes; TryGet(int, out HttpResponseDefinition?) returns false/null. Entries have get-only StatusCode, ReasonPhrase, DefaultTitle, SafeDetail, Type, Retryability, IsDetailSafeForClients, and AllowsBody properties. DefaultTitle equals ReasonPhrase; every SafeDetail below is safe for clients. Type is the absolute RFC link below. Common typed properties: Unauthorized, Forbidden, NotFound, Conflict, UnprocessableContent, TooManyRequests, InternalServerError, BadGateway, ServiceUnavailable, GatewayTimeout.

Retryability is NeverByDefault for every entry except 408/429/502/503/504, which are PotentiallyTransient. Neither classification establishes replay safety. Never automatically retry arbitrary 5xx, authentication failures, cancellation, or 425 (requires early-data context). Applications must establish operation idempotency, replayable content, retry budget, and any Retry-After timing.

AllowsBody is false for 1xx, 204, 205 and 304. CanWriteBody(status, method) additionally rejects HEAD and successful CONNECT. These are metadata rules, not a general-purpose response writer.

## Catalog

Each title is the reason phrase. N = NeverByDefault; T = PotentiallyTransient. The full type URI is https://www.rfc-editor.org/rfc/rfc followed by the reference below and .html before #.

| Code | Reason / default title | Safe detail | RFC reference | Retry |
| --- | --- | --- | --- | --- |
| 100 | Continue | The request may continue. | 9110#section-15.2.1 | N |
| 101 | Switching Protocols | The connection is switching protocols. | 9110#section-15.2.2 | N |
| 102 | Processing | The request is being processed. | 2518#section-10.1 | N |
| 103 | Early Hints | Preliminary response headers are available. | 8297#section-2 | N |
| 200 | OK | The request succeeded. | 9110#section-15.3.1 | N |
| 201 | Created | The resource was created. | 9110#section-15.3.2 | N |
| 202 | Accepted | The request was accepted for processing. | 9110#section-15.3.3 | N |
| 203 | Non-Authoritative Information | The response contains transformed information. | 9110#section-15.3.4 | N |
| 204 | No Content | The request succeeded without response content. | 9110#section-15.3.5 | N |
| 205 | Reset Content | The request succeeded; reset the document view. | 9110#section-15.3.6 | N |
| 206 | Partial Content | The response contains the requested range. | 9110#section-15.3.7 | N |
| 207 | Multi-Status | The response reports multiple operation results. | 4918#section-11.1 | N |
| 208 | Already Reported | The resource was already reported. | 5842#section-7.1 | N |
| 226 | IM Used | The response contains instance manipulations. | 3229#section-10.4.1 | N |
| 300 | Multiple Choices | Multiple representations are available. | 9110#section-15.4.1 | N |
| 301 | Moved Permanently | The resource has a permanent new location. | 9110#section-15.4.2 | N |
| 302 | Found | The resource is temporarily at another location. | 9110#section-15.4.3 | N |
| 303 | See Other | Retrieve the result from another resource. | 9110#section-15.4.4 | N |
| 304 | Not Modified | The stored representation is still valid. | 9110#section-15.4.5 | N |
| 305 | Use Proxy | This deprecated status is retained for lookup only. | 9110#section-15.4.6 | N |
| 306 | (Unused) | This status is reserved and must not be emitted. | 9110#section-15.4.7 | N |
| 307 | Temporary Redirect | Repeat the request at the temporary location. | 9110#section-15.4.8 | N |
| 308 | Permanent Redirect | Repeat the request at the permanent location. | 9110#section-15.4.9 | N |
| 400 | Bad Request | The request is invalid. | 9110#section-15.5.1 | N |
| 401 | Unauthorized | Authentication is required. | 9110#section-15.5.2 | N |
| 402 | Payment Required | Payment is required. | 9110#section-15.5.3 | N |
| 403 | Forbidden | Access to this resource is forbidden. | 9110#section-15.5.4 | N |
| 404 | Not Found | The requested resource was not found. | 9110#section-15.5.5 | N |
| 405 | Method Not Allowed | The request method is not supported for this resource. | 9110#section-15.5.6 | N |
| 406 | Not Acceptable | No acceptable representation is available. | 9110#section-15.5.7 | N |
| 407 | Proxy Authentication Required | Authentication with the proxy is required. | 9110#section-15.5.8 | N |
| 408 | Request Timeout | The request was not received in time. | 9110#section-15.5.9 | T |
| 409 | Conflict | The request conflicts with the current resource state. | 9110#section-15.5.10 | N |
| 410 | Gone | The requested resource is no longer available. | 9110#section-15.5.11 | N |
| 411 | Length Required | The request requires a content length. | 9110#section-15.5.12 | N |
| 412 | Precondition Failed | A request precondition was not satisfied. | 9110#section-15.5.13 | N |
| 413 | Content Too Large | The request content exceeds the accepted size. | 9110#section-15.5.14 | N |
| 414 | URI Too Long | The request URI exceeds the accepted length. | 9110#section-15.5.15 | N |
| 415 | Unsupported Media Type | The request content format is not supported. | 9110#section-15.5.16 | N |
| 416 | Range Not Satisfiable | The requested range cannot be supplied. | 9110#section-15.5.17 | N |
| 417 | Expectation Failed | A request expectation cannot be satisfied. | 9110#section-15.5.18 | N |
| 418 | (Unused) | This status is reserved and must not be emitted. | 9110#section-15.5.19 | N |
| 421 | Misdirected Request | The request was directed to an unsuitable server. | 9110#section-15.5.20 | N |
| 422 | Unprocessable Content | The request content could not be processed. | 9110#section-15.5.21 | N |
| 423 | Locked | The resource is locked. | 4918#section-11.3 | N |
| 424 | Failed Dependency | A required operation failed. | 4918#section-11.4 | N |
| 425 | Too Early | The server declined to risk replaying the request. | 8470#section-5.2 | N |
| 426 | Upgrade Required | The request requires a different protocol. | 9110#section-15.5.22 | N |
| 428 | Precondition Required | The request requires a precondition. | 6585#section-3 | N |
| 429 | Too Many Requests | The request rate exceeds the current limit. | 6585#section-4 | T |
| 431 | Request Header Fields Too Large | The request headers exceed the accepted size. | 6585#section-5 | N |
| 451 | Unavailable For Legal Reasons | The resource is unavailable for legal reasons. | 7725#section-3 | N |
| 500 | Internal Server Error | An unexpected error prevented processing the request. | 9110#section-15.6.1 | N |
| 501 | Not Implemented | The requested functionality is not implemented. | 9110#section-15.6.2 | N |
| 502 | Bad Gateway | An upstream service returned an invalid response. | 9110#section-15.6.3 | T |
| 503 | Service Unavailable | The service is currently unavailable. | 9110#section-15.6.4 | T |
| 504 | Gateway Timeout | An upstream service did not respond in time. | 9110#section-15.6.5 | T |
| 505 | HTTP Version Not Supported | The HTTP version is not supported. | 9110#section-15.6.6 | N |
| 506 | Variant Also Negotiates | The server could not select a representation. | 2295#section-8.1 | N |
| 507 | Insufficient Storage | The server cannot store the required representation. | 4918#section-11.5 | N |
| 508 | Loop Detected | The operation encountered a dependency loop. | 5842#section-7.2 | N |
| 510 | Not Extended | This historic extension status is retained for lookup only. | 2774#section-7 | N |
| 511 | Network Authentication Required | Network access requires authentication. | 6585#section-6 | N |

## Exception Mapping and Hosting

Retain IProblemDetailsExceptionMapper.Map(HttpContext, Exception) and IWebApiResiliencePipelineFactory.CreatePipeline().
Default exception mappings: ArgumentException -> 400, UnauthorizedAccessException -> 403, KeyNotFoundException -> 404. Derived types use the closest mapped base type. Unrecognized exceptions, InvalidOperationException and TimeoutException -> 500; a generic exception does not prove conflict or gateway semantics. Applications explicitly map their own conflict/validation/upstream failures.
At the default mapper/direct-handler boundary, OperationCanceledException is rethrown unchanged when RequestAborted is not canceled, even if configured in the mapping dictionary; canceled caller/request tokens are propagated by their cancellation guards. This is not an end-to-end hosting guarantee. UseIfxWebApiExceptionHandling delegates to built-in ASP.NET UseExceptionHandler: an OperationCanceledException or IOException with canceled RequestAborted short-circuits before registered handlers/mappers. For an unstarted response, ASP.NET assigns 499 and returns without a Problem Details body. This describes server-side response state, not delivery to a disconnected client. 499 remains excluded from this package's catalog. An OperationCanceledException without canceled RequestAborted reaches the direct handler and propagates unchanged through this default hosting pipeline.
Options are validated on startup and before use, including dictionary edits: exception-derived keys, registered 4xx/5xx statuses except reserved 418/historic 510, nonempty titles, and absolute type URIs. Defaults use catalog 500 metadata. Null mapping types resolve to the status RFC URI.
ProblemDetails uses catalog safe detail, request path (no query), traceId and mapped status/title/type. IncludeExceptionDetails is false by default; true explicitly exposes exception.Message and exceptionType and must be limited to trusted development environments.
GlobalExceptionHandler returns false on started responses or missing required headers (401 WWW-Authenticate, 407 Proxy-Authenticate, 405 Allow, 426 Upgrade). Built-in exception middleware clears the unstarted response, including headers, and sets status 500 BEFORE invoking the handler/mapper. Headers assigned before throwing are therefore not preserved. With the default registration, a mapped 401/405/407/426 lacking its required post-reset header is declined and the middleware's Problem Details fallback retains 500. Direct-handler tests with prepopulated headers do not establish preservation through hosting.
Prefer normal authentication challenge/forbid and routing/protocol responses instead of throwing to produce these statuses. When application semantics require exception mapping, a trusted application IProblemDetailsExceptionMapper registered before AddIfxWebApi may set the required header on HttpContext.Response during Map, AFTER middleware reset, then return corresponding valid ProblemDetails. Header values must come from trusted host configuration for the actual authentication/protocol contract; a 426 also needs the applicable Connection: Upgrade semantics. The package invents no authentication schemes and restores no arbitrary stale headers, content lengths or framing. The direct handler leaves these newly supplied headers intact. Custom mapper statuses are validated before writing. HEAD is handled without content. Informational/success/redirect mappings are rejected.
The direct handler links caller cancellation and RequestAborted while invoking IProblemDetailsService.TryWriteAsync, restores RequestAborted afterward, and falls back to application/problem+json when no registered writer supports the response. Errors from mappers/writers propagate from the direct handler; the surrounding ASP.NET exception middleware owns its own error handling. This package adds no custom exception middleware or header-restoration API.

## Replaceable Resilience

IWebApiTransientFailureClassifier classifies failures, independently of the catalog lookup API. The default accepts HttpRequestException only when its status is absent or 408/429/502/503/504, plus TimeoutException and Polly TimeoutRejectedException; all other failures are excluded.
CreatePipeline is opt-in for replay: EnableRetry defaults false. Enabling retry also requires OperationsAreReplaySafe=true; unsafe configuration fails validation. Defaults: three retries, 200ms exponential delay with jitter, 30s cooperative timeout per attempt. Timeout is inside retry. Applications must not opt into generic retries where Retry-After is required: replace the factory with an HTTP-aware policy. No HTTP response/result retry is implied; only thrown classified failures are considered.
Options validate retry count (1..100), nonnegative delay (up to one day), and timeout (10ms through one day, matching Polly validation) even for disabled strategies. TimeProvider is replaceable through DI or factory construction for deterministic tests. Cancellation tokens are never classified as transient; canceled contexts skip retries.
TryAdd registrations preserve application mapper/classifier/factory/time-provider replacements. Register once via AddIfxWebApi and use UseIfxWebApiExceptionHandling in the ASP.NET pipeline.

## Verification Gates

Tests belong exclusively to tests/unit/vc.Ifx.UnitTests/WebApi. Required evidence is measured package-only 100% line and branch coverage, targeted tests, warning-free package build, plus orchestrator-owned full solution build/suite and package metadata checks. Shared framework/language and test project changes remain infrastructure-owned. Infrastructure has applied stable C# 14.0 globally; the package inherits net10.0. The shared tests currently reference both Polly 7 and Polly.Core 8, so timeout tests assert the exception's runtime type and assembly to avoid ambiguous compile-time names.

Verified 2026-09-09 using the infrastructure serialization wrapper:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.WebApi -Filter FullyQualifiedName~WebApi
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project src/vc.Ifx.WebApi/vc.Ifx.WebApi.csproj -NoRestore
```

98 tests passed; measured coverage is 327/327 lines and 116/116 branches (both 100%), with 100% methods. No custom coverage exclusions were introduced. OpenCover evidence: `TestResults/coverage/vc.Ifx.WebApi/6cd18b8e77bc466983bc22bc05992885/vc.Ifx.UnitTests/coverage.opencover.xml`. Package build: zero warnings/errors. The project remains In-flight for the orchestrator's whole-solution build, full test suite/global coverage, and final packaged artifact verification.

### Hosting Review Verification: 2026-09-10

F2/F3 are addressed by the hosting boundary corrections above and eleven real
Production WebApplication/ApplicationBuilder pipeline regressions in
`tests/unit/vc.Ifx.UnitTests/WebApi/HostingPipelineContractTests.cs`. These tests
build and invoke UseIfxWebApiExceptionHandling, not just the direct handler.
They cover aborted OperationCanceledException/IOException (499, no mapper/body),
uncanceled OperationCanceledException identity/propagation, all four pre-throw
required-header losses with 500 fallback, and trusted post-reset headers with
matching 401/405/407/426 Problem Details. No listener or live disconnect is used.

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.WebApi -TestSourceScope WebApi -TestPackage vc.Ifx.WebApi -Filter FullyQualifiedName~Tests.WebApi. -Configuration Release -WarningsAsErrors
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -Filter FullyQualifiedName~HostingPipelineContractTests -Configuration Release -WarningsAsErrors
```

Strict Release package proof: 109/109 tests, zero skips or build/test warnings;
280/280 lines, 116/116 branches and 100% methods. Release optimization accounts
for the line-count difference from the preserved Debug checkpoint above.
Evidence: `TestResults/coverage/vc.Ifx.WebApi/aa310c6abd184df4b7fed25be48871a1/summary.json`
and adjacent `vc.Ifx.UnitTests/tests.trx`. The second command compiled the full
unscoped test assembly and passed all eleven hosting tests, zero skips/warnings:
`TestResults/tests/vc.Ifx.UnitTests/723f918ac5ba4b0bb68da8600e2da0b7/tests.trx`.
The initial scoped eleven-test Release run also passed:
`TestResults/tests/vc.Ifx.UnitTests/77ea4339d28847c69c8529a2ee2c47a6/tests.trx`.
These are package/targeted results, not a new full-suite run or proof for every
ASP.NET servicing runtime. Built-in middleware, executable package behavior,
public signatures and dependencies are unchanged; no coverage exclusions added.
Final global verification and review acceptance remain with Orchestrator.
