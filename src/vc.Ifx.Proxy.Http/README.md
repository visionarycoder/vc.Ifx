# vc.Ifx.Proxy.Http

HTTP transport for the stable `vc.Ifx.Proxy` invocation contracts, targeting .NET 10
and C# 14. Namespace: `VisionaryCoder.Framework.Proxy.Transports`. HTTP serialization,
headers, message ownership and transport error classification stay in this package;
the core pipeline owns invocation policy and optional retries.

## Construction

```csharp
var transport = new HttpProxyTransport(httpClient, new HttpProxyTransportOptions
{
    RequestTimeout = TimeSpan.FromSeconds(30),
    MaxRequestBodyBytes = 4 * 1024 * 1024,
    MaxResponseBodyBytes = 4 * 1024 * 1024
});
var context = new ProxyContext
{
    Method = "POST",
    Url = "orders",
    Body = new { ProductId = productId, Quantity = quantity }
};
ProxyResponse<Order> response = await transport.SendCoreAsync<Order>(context, token);
```

The existing `HttpProxyTransport(HttpClient)` constructor remains and uses defaults.
The formerly internal transport is now public so consumers can register it directly
as `IProxyTransport`. An additive constructor accepts `HttpProxyTransportOptions`.
Neither constructor sends requests, changes the client's settings or takes ownership
of the client/handler. A host can register the client and transport with normal DI;
there is no second HTTP-client abstraction or aggregator registration in this package.

Configure base address, credentials, proxy, redirects, decompression, cookies, certificate
validation and default headers on the host-owned client/handler. They remain host policy.
Only HTTP(S) URLs without userinfo/fragments are accepted. Relative URLs require a client
base address. This validation is not an SSRF defense or host allowlist; do not accept
untrusted destinations without an application authorization/network policy.

## Request Mapping

- `Method` is forwarded as an HTTP method; null defaults to GET. Empty/invalid method
  tokens fail before sending. The context is not rewritten to insert that default.
- `Url` resolves against `HttpClient.BaseAddress` when relative. Absolute URLs are
  preserved and are not silently retargeted to the base address.
- `Body` is the HTTP payload. `Request` remains core invocation/telemetry metadata and
  is not used as an implicit payload fallback. Set `Body` explicitly.
- A null body sends no content. A string is raw UTF-8 content with `application/json`
  as its default media type, suitable for already-serialized JSON; it is not JSON-quoted.
  Override `Content-Type` for plain text or another raw format.
- Byte arrays and streams are raw bytes with `application/octet-stream`. Arrays are
  copied; streams are read from their current position without seeking or disposal.
- Caller-supplied `HttpContent` is copied, including its content headers except
  `Content-Length`. It and its underlying stream remain caller-owned. It may internally
  buffer when its own stream is obtained; the transport does not control that allocation.
- Other objects serialize as their runtime type using System.Text.Json web defaults.
  Custom `JsonOptions` are copied and frozen at construction. User converters/resolvers
  must themselves be suitable for the host's concurrency model.
- Request and content headers are routed through .NET's header collections. Content
  headers require a body, and explicit context content headers override generated ones.
  Invalid header names, duplicate names differing only by case, line breaks in values,
  and manual `Content-Length`/`Transfer-Encoding` framing are rejected. Ordinary custom
  headers follow .NET behavior; a header such as `Age` is not assumed to be rejected
  merely because it is usually seen on responses. Copied content values also reject
  line breaks. Host-supplied default headers remain the host's responsibility.

Outbound content is buffered up to `MaxRequestBodyBytes` (default 4 MiB); incoming success
content is bounded by `MaxResponseBodyBytes` (default 4 MiB). Both limits must be positive.
This is a typed, buffered transport, not an unbounded streaming API. JSON/string/array
encoding may allocate before the request-size check; these limits are wire-body limits,
not a strict allocation budget. Do not concurrently mutate a context or its payload.

## Response Mapping And Ownership

Every received status is preserved in `ProxyResponse<T>.StatusCode` for returned envelopes.
For a successful response, `T = string` returns raw HTTP-decoded text, `T = byte[]` returns
raw bytes, and other types deserialize JSON. The HTTP charset/BOM is handled through
`HttpContent` decoding before JSON deserialization. Empty success, HEAD, 204 and 205 return
successful envelopes with default data. JSON null stays null. Invalid JSON/encoding or
unsupported serialization propagates its original exception and is never made retryable.

The old prototype attempted JSON deserialization even for string/byte-array responses,
ignored request bodies, swallowed all exceptions, and omitted non-success status codes.
Those behaviors are intentionally corrected. Callers requiring a JSON string value rather
than raw text should request a JSON model/element and interpret that value explicitly.

Non-success responses normally return `IsSuccess = false` with the real status and a
generic status message. Error bodies are neither read nor exposed, and custom reason
phrases/URLs/credentials are not interpolated into that message. Replay-safe transient
failures are the exception described below. Exceptions retain their underlying causes;
applications must still avoid logging sensitive details from exception chains.

Because the stable core response has no headers property, a detached
`IReadOnlyDictionary<string, string[]>` is written to
`context.Properties[HttpProxyTransport.ResponseHeadersKey]`. It includes response,
content and available trailing headers with case-insensitive lookup and separate values.
The dictionary is read-only; its array values belong to the context and are not deeply
immutable. Error responses include headers available before disposal, not unread trailers.
The reserved key is cleared at each invocation so failures cannot leave an old snapshot.
Header values can contain credentials/cookies; do not log the snapshot indiscriminately.

The transport disposes every HTTP request, generated request content, response, and response
content on success, failure and cancellation. It does not dispose the HttpClient, caller
body stream or caller HttpContent. An input stream can be advanced even when a later
header/send operation fails. There is no rollback or stream-position restoration.

## Cancellation And Deadlines

Both `SendCoreAsync`'s token and the legacy `ProxyContext.CancellationToken` are linked
for body preparation, sending and response reading. Caller cancellation propagates as
`OperationCanceledException` carrying the original caller token. If both cancel, the
method argument token takes precedence. No cancellation becomes a failed success envelope.

`RequestTimeout = null` uses the client's timeout as a whole-attempt deadline, including
response body reading. An explicit positive value overrides that transport deadline;
`Timeout.InfiniteTimeSpan` disables it. The client's own configured send/header timeout
still applies and is not mutated. Unrelated handler cancellation remains cancellation,
not an invented timeout. Serialization itself is synchronous and is not preempted mid-call.
Infinite client/transport timeouts require callers to supply an appropriate cancellation policy.

The transport uses `ResponseHeadersRead` so it can enforce its own size limits and dispose
error responses without reading their bodies. This explicitly accounts for .NET's
[header-only HttpClient timeout scope](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpcompletionoption?view=net-10.0).

## Replay And Retry Ownership

There is exactly one `HttpClient.SendAsync` call per invocation and no transport retry
loop or Polly policy. `RetryableTransportException` is emitted only when classification
is enabled **and** the method is GET or HEAD **and** the context body is null. This assumes
the service honors those methods' read-only semantics; no client can prove server behavior.
POST, PATCH, PUT, DELETE, OPTIONS, custom methods and any call with a body are not treated
as replay-safe. An idempotency header alone is not sufficient evidence to broaden that rule.

Within that narrow eligibility, the transport classifies:

- HTTP 408, 429, 500, 502, 503 and 504 responses.
- `HttpRequestException` with ConnectionError, NameResolutionError or ResponseEnded.
- `HttpIOException` with ResponseEnded during response-body reading.
- A transport deadline or an identified HttpClient timeout, preserving a TimeoutException cause.

Other status codes remain failure envelopes; other network/TLS/authentication/configuration
errors retain their original exceptions. Non-replay-safe timeout is a TimeoutException.
The transport does not retry body-bearing operations, even if their source was buffered.
See the [.NET HTTP I/O exception contract](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpioexception?view=net-10.0).

Core retry is optional and handles the typed signal. Do not also install an HTTP retry
handler or multiple core retry interceptors without an explicit total attempt budget.
Set `ClassifyRetryableFailures = false` when classification/retry belongs elsewhere;
then transient statuses remain response envelopes and network exceptions remain original.
`Retry-After` is preserved in the header snapshot but is not consumed by the stable core
exception contract. There is no claim of Retry-After-aware scheduling or exactly-once calls.

## Verification

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -TestSourceScope ProxyHttp -TestPackage vc.Ifx.Proxy.Http -CoveragePackage vc.Ifx.Proxy.Http -Filter FullyQualifiedName~ProxyHttp
```

Tests use real HttpClient instances with fake HttpMessageHandlers; no network listeners,
live services, sleeps as correctness assertions, or mocked HttpClient methods are required.
They cover methods/URLs, headers, bodies, JSON/text/binary responses, ownership/disposal,
bounded bodies, caller/context cancellation, header and body deadlines, exception identity,
replay classification, disabled classification, DI construction and serializer snapshots.
Exact measured evidence and the explicit final-verification handoff are in the package's
section of `docs/planning/framework-upgrade-parallel-plan.md`. Full core integration,
zero-warning solution/Release/full-suite/global coverage and repack remain Orchestrator gates.
