using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Transports;

namespace VisionaryCoder.Framework.Tests.ProxyHttp;

[TestClass]
public sealed class HttpProxyTransportTests
{
    private static ProxyContext Context(string? method = null, object? body = null) => new() { Url = "resource", Method = method, Body = body };
    private static HttpResponseMessage Response(string body = "{\"value\":7}", HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new TrackingContent(Encoding.UTF8.GetBytes(body)) };

    [TestMethod]
    public void ConstructorOptionsAndDiAreValidatedWithoutSending()
    {
        using var client = new HttpClient(new Handler((request, token) => throw new AssertFailedException("Unexpected I/O")));
        Assert.Throws<ArgumentNullException>(() => new HttpProxyTransport(null!));
        Assert.Throws<ArgumentNullException>(() => new HttpProxyTransport(client, null!));
        Assert.Throws<ArgumentNullException>(() => new HttpProxyTransport(client, new() { JsonOptions = null! }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HttpProxyTransport(client, new() { MaxRequestBodyBytes = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HttpProxyTransport(client, new() { MaxResponseBodyBytes = -1 }));
        foreach (TimeSpan timeout in new[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(-2), TimeSpan.FromDays(51) })
            Assert.Throws<ArgumentOutOfRangeException>(() => new HttpProxyTransport(client, new() { RequestTimeout = timeout }));
        new HttpProxyTransportOptions { RequestTimeout = Timeout.InfiniteTimeSpan }.Validate();
        var services = new ServiceCollection();
        services.AddSingleton(client);
        services.AddTransient<IProxyTransport, HttpProxyTransport>();
        using var scope = services.BuildServiceProvider();
        Assert.IsInstanceOfType<HttpProxyTransport>(scope.GetRequiredService<IProxyTransport>());
    }

    [TestMethod]
    public async Task DefaultGetMapsResponseAndOwnsMessagesButNotClient()
    {
        HttpRequestMessage? sent = null;
        TrackingContent? content = null;
        using var fixture = new Fixture((request, token) =>
        {
            sent = request;
            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual("https://unit-test.invalid/api/resource", request.RequestUri!.AbsoluteUri);
            Assert.IsNull(request.Content);
            var response = Response();
            content = (TrackingContent)response.Content;
            return Task.FromResult(response);
        });
        var result = await fixture.Transport.SendCoreAsync<Payload>(Context());
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(7, result.Data!.Value);
        Assert.IsTrue(content!.Disposed);
        using var newContent = new StringContent("test");
        Assert.Throws<ObjectDisposedException>(() => sent!.Content = newContent);
        Assert.AreEqual(Timeout.InfiniteTimeSpan, fixture.Client.Timeout);
        using HttpResponseMessage alive = await fixture.Client.GetAsync("resource");
        Assert.AreEqual(2, fixture.Calls);
    }

    [TestMethod]
    public async Task MethodsAndAbsoluteUrisAreForwardedWithoutMutation()
    {
        using var fixture = new Fixture((request, token) => Task.FromResult(Response("", HttpStatusCode.NoContent)));
        foreach (string method in new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS", "HEAD", "PROPFIND" })
        {
            var context = new ProxyContext { Method = method, Url = "https://unit-test.invalid/other?x=1" };
            var result = await fixture.Transport.SendCoreAsync<object>(context);
            Assert.AreEqual(method, fixture.LastRequest!.Method.Method);
            Assert.AreEqual("https://unit-test.invalid/other?x=1", fixture.LastRequest.RequestUri!.AbsoluteUri);
            Assert.AreEqual(method, context.Method);
            Assert.IsTrue(result.IsSuccess);
        }
    }

    [TestMethod]
    public async Task GuardsRejectInvalidContextsUrlsAndMethodsBeforeIo()
    {
        using var fixture = new Fixture();
        await Assert.ThrowsAsync<ArgumentNullException>(() => fixture.Transport.SendCoreAsync<object>(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => fixture.Transport.SendCoreAsync<object>(new() { Headers = null! }));
        await Assert.ThrowsAsync<ArgumentNullException>(() => fixture.Transport.SendCoreAsync<object>(new() { Properties = null! }));
        foreach (string? url in new[] { null, "", " ", "http://[", "ftp://server/file", "https://user:secret@host/path", "https://host/path#part" })
            await Assert.ThrowsAsync<ArgumentException>(() => fixture.Transport.SendCoreAsync<object>(new() { Url = url }));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Transport.SendCoreAsync<object>(Context("")));
        await Assert.ThrowsAsync<FormatException>(() => fixture.Transport.SendCoreAsync<object>(Context("BAD METHOD")));
        fixture.Client.BaseAddress = null;
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Transport.SendCoreAsync<object>(Context()));
        Assert.AreEqual(0, fixture.Calls);
    }

    [TestMethod]
    public async Task JsonAndRawBodiesAreSerializedAndRequestMetadataIsNotPayload()
    {
        var observed = new List<(byte[] Bytes, string? Type)>();
        using var fixture = new Fixture(async (request, token) =>
        {
            if (request.Content is not null) observed.Add((await request.Content.ReadAsByteArrayAsync(token), request.Content.Headers.ContentType?.MediaType));
            return Response();
        });
        await fixture.Transport.SendCoreAsync<Payload>(Context("POST", new Payload(9)));
        await fixture.Transport.SendCoreAsync<Payload>(Context("POST", "{\"raw\":true}"));
        byte[] raw = [1, 2, 3];
        await fixture.Transport.SendCoreAsync<Payload>(Context("POST", raw));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("{\"value\":9}"), observed[0].Bytes);
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("{\"raw\":true}"), observed[1].Bytes);
        CollectionAssert.AreEqual(raw, observed[2].Bytes);
        Assert.AreEqual("application/json", observed[0].Type);
        Assert.AreEqual("application/octet-stream", observed[2].Type);
        var context = Context();
        context.Request = new Payload(100);
        await fixture.Transport.SendCoreAsync<Payload>(context);
        Assert.AreEqual(3, observed.Count);
    }

    [TestMethod]
    public async Task StreamAndHttpContentAreCopiedFromCurrentPositionAndLeftOpen()
    {
        var received = new List<byte[]>();
        using var fixture = new Fixture(async (request, token) =>
        {
            received.Add(await request.Content!.ReadAsByteArrayAsync(token));
            return Response();
        });
        using var stream = new NonseekableStream([1, 2, 3]);
        Assert.AreEqual(1, stream.ReadByte());
        await fixture.Transport.SendCoreAsync<Payload>(Context("POST", stream));
        Assert.IsTrue(stream.CanRead);
        using var source = new TrackingContent([4, 5]);
        source.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        source.Headers.ContentLength = 2;
        source.Headers.TryAddWithoutValidation("Content-Disposition", "attachment");
        await fixture.Transport.SendCoreAsync<Payload>(Context("POST", source));
        Assert.IsFalse(source.Disposed);
        CollectionAssert.AreEqual(new byte[] { 2, 3 }, received[0]);
        CollectionAssert.AreEqual(new byte[] { 4, 5 }, received[1]);
        Assert.AreEqual("image/png", fixture.LastRequest!.Content!.Headers.ContentType!.MediaType);
        Assert.AreEqual("attachment", fixture.LastRequest.Content.Headers.ContentDisposition!.DispositionType);
        Assert.AreEqual(2L, fixture.LastRequest.Content.Headers.ContentLength);
    }

    [TestMethod]
    public async Task HeadersRouteToRequestAndContentAndResponsesAreDetached()
    {
        using var fixture = new Fixture((request, token) =>
        {
            Assert.AreEqual("Bearer credential", request.Headers.Authorization!.ToString());
            Assert.AreEqual("text/plain", request.Content!.Headers.ContentType!.MediaType);
            Assert.AreEqual("en-US", request.Content.Headers.ContentLanguage.Single());
            var response = Response();
            response.Headers.TryAddWithoutValidation("X-Multi", new[] { "one", "two" });
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response.TrailingHeaders.TryAddWithoutValidation("X-Trailer", "complete");
            return Task.FromResult(response);
        });
        var context = Context("POST", "raw text");
        context.Headers = new() { ["Authorization"] = "Bearer credential", ["Content-Type"] = "text/plain", ["Content-Language"] = "en-US" };
        await fixture.Transport.SendCoreAsync<Payload>(context);
        var headers = (IReadOnlyDictionary<string, string[]>)context.Properties[HttpProxyTransport.ResponseHeadersKey]!;
        CollectionAssert.AreEqual(new[] { "one", "two" }, headers["x-multi"]);
        Assert.AreEqual("application/json", headers["Content-Type"][0]);
        Assert.AreEqual("complete", headers["X-Trailer"][0]);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, string[]>)headers).Add("new", ["x"]));
    }

    [TestMethod]
    public async Task HeaderValidationRejectsInjectionFramingDuplicatesAndWrongHeaderKinds()
    {
        using var fixture = new Fixture();
        Dictionary<string, string>[] invalid = [
            new() { [""] = "value" }, new() { ["X-Test"] = null! },
            new() { ["X-Test"] = "value\rnext" }, new() { ["X-Test"] = "value\nnext" },
            new() { ["Content-Length"] = "10" }, new() { ["Transfer-Encoding"] = "chunked" },
            new() { ["X-Test"] = "one", ["x-test"] = "two" }
        ];
        foreach (var headers in invalid)
        {
            var context = Context("POST", "body");
            context.Headers = headers;
            await Assert.ThrowsAsync<ArgumentException>(() => fixture.Transport.SendCoreAsync<object>(context));
        }
        var missing = Context();
        missing.Headers["Content-Type"] = "application/json";
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Transport.SendCoreAsync<object>(missing));
        var badName = Context();
        badName.Headers["bad name"] = "value";
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Transport.SendCoreAsync<object>(badName));
        var invalidContentType = Context("POST", "body");
        invalidContentType.Headers["Content-Type"] = "not/a/media/type";
        await Assert.ThrowsAsync<FormatException>(() => fixture.Transport.SendCoreAsync<object>(invalidContentType));
        Assert.AreEqual(0, fixture.Calls);
    }

    [TestMethod]
    public async Task CopiedContentHeadersRejectInjectionWithoutDisposingCallerContent()
    {
        using var fixture = new Fixture();
        foreach (string value in new[] { "part\rnext", "part\nnext" })
        {
            using var content = new TrackingContent([1]);
            content.Headers.TryAddWithoutValidation("Content-Disposition", value);
            await Assert.ThrowsAsync<ArgumentException>(() => fixture.Transport.SendCoreAsync<object>(Context("POST", content)));
            Assert.IsFalse(content.Disposed);
        }
        Assert.AreEqual(0, fixture.Calls);
    }

    [TestMethod]
    public async Task PrematureResponseEndsAreRetryableOnlyWhenReplayIsSafe()
    {
        HttpRequestError kind = HttpRequestError.ResponseEnded;
        HttpIOException? failure = null;
        using var fixture = new Fixture((request, token) =>
        {
            failure = new HttpIOException(kind, "response body failed");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StreamContent(new CallbackStream((memory, cancellation) => Task.FromException<int>(failure))) });
        });
        var retryable = await Assert.ThrowsAsync<RetryableTransportException>(() => fixture.Transport.SendCoreAsync<string>(Context()));
        Assert.AreSame(failure, retryable.InnerException);
        await Assert.ThrowsAsync<HttpIOException>(() => fixture.Transport.SendCoreAsync<string>(Context("POST")));
        kind = HttpRequestError.InvalidResponse;
        await Assert.ThrowsAsync<HttpIOException>(() => fixture.Transport.SendCoreAsync<string>(Context()));
        Assert.AreEqual(3, fixture.Calls);
    }

    [TestMethod]
    public async Task TypedJsonTextBinaryAndEmptyResponsesHaveExplicitSemantics()
    {
        using var fixture = new Fixture((request, token) => Task.FromResult(Response("plain text")));
        Assert.AreEqual("plain text", (await fixture.Transport.SendCoreAsync<string>(Context())).Data);
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("plain text"), (await fixture.Transport.SendCoreAsync<byte[]>(Context())).Data);
        fixture.Callback = (request, token) => Task.FromResult(Response("null"));
        Assert.IsNull((await fixture.Transport.SendCoreAsync<Payload>(Context())).Data);
        foreach (HttpStatusCode status in new[] { HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.ResetContent })
        {
            fixture.Callback = (request, token) => Task.FromResult(Response("", status));
            Assert.IsNull((await fixture.Transport.SendCoreAsync<Payload>(Context())).Data);
        }
        fixture.Callback = (request, token) => Task.FromResult(Response("not json"));
        Assert.IsNull((await fixture.Transport.SendCoreAsync<Payload>(Context("HEAD"))).Data);
    }

    [TestMethod]
    public async Task ResponseCharsetAndBomUseHttpContentDecoding()
    {
        using var fixture = new Fixture((request, token) =>
        {
            byte[] text = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("{\"value\":5}")).ToArray();
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new TrackingContent(text) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-16" };
            return Task.FromResult(response);
        });
        Assert.AreEqual(5, (await fixture.Transport.SendCoreAsync<Payload>(Context())).Data!.Value);
    }

    [TestMethod]
    public async Task SerializerOptionsAreSnapshottedAndMalformedJsonIsNotRetryable()
    {
        var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        using var fixture = new Fixture((request, token) => Task.FromResult(Response("{\"VALUE\":8}")), new() { JsonOptions = json });
        json.PropertyNameCaseInsensitive = false;
        Assert.AreEqual(8, (await fixture.Transport.SendCoreAsync<Payload>(Context())).Data!.Value);
        TrackingContent? content = null;
        fixture.Callback = (request, token) => { var response = Response("not JSON"); content = (TrackingContent)response.Content; return Task.FromResult(response); };
        await Assert.ThrowsAsync<JsonException>(() => fixture.Transport.SendCoreAsync<Payload>(Context()));
        Assert.IsTrue(content!.Disposed);
        Assert.AreEqual(2, fixture.Calls);
        var cyclic = new Cycle();
        cyclic.Next = cyclic;
        await Assert.ThrowsAsync<JsonException>(() => fixture.Transport.SendCoreAsync<Payload>(Context("POST", cyclic)));
        Assert.AreEqual(2, fixture.Calls);
    }

    [TestMethod]
    public async Task NonSuccessPreservesStatusWithoutExposingOrReadingErrorBody()
    {
        TrackingContent? content = null;
        using var fixture = new Fixture((request, token) =>
        {
            var response = Response("secret response body", HttpStatusCode.BadRequest);
            content = (TrackingContent)response.Content;
            return Task.FromResult(response);
        });
        var result = await fixture.Transport.SendCoreAsync<Payload>(Context());
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(400, result.StatusCode);
        Assert.IsFalse(result.ErrorMessage!.Contains("secret", StringComparison.Ordinal));
        Assert.IsTrue(content!.Disposed);
    }

    [TestMethod]
    public async Task OnlyReplaySafeMethodsWithoutBodiesSignalTransientStatuses()
    {
        using var fixture = new Fixture();
        foreach (int status in new[] { 408, 429, 500, 502, 503, 504 })
        {
            fixture.Callback = (request, token) => Task.FromResult(Response("", (HttpStatusCode)status));
            var error = await Assert.ThrowsAsync<RetryableTransportException>(() => fixture.Transport.SendCoreAsync<Payload>(Context()));
            Assert.AreEqual((HttpStatusCode)status, ((HttpRequestException)error.InnerException!).StatusCode);
        }
        await Assert.ThrowsAsync<RetryableTransportException>(() => fixture.Transport.SendCoreAsync<Payload>(Context("HEAD")));
        foreach (string method in new[] { "POST", "PUT", "DELETE", "PATCH", "OPTIONS" })
            Assert.IsFalse((await fixture.Transport.SendCoreAsync<Payload>(Context(method))).IsSuccess);
        Assert.IsFalse((await fixture.Transport.SendCoreAsync<Payload>(Context("GET", "body"))).IsSuccess);
        fixture.Callback = (request, token) => Task.FromResult(Response("", HttpStatusCode.NotImplemented));
        Assert.IsFalse((await fixture.Transport.SendCoreAsync<Payload>(Context())).IsSuccess);
    }

    [TestMethod]
    public async Task ClassificationCanBeDisabledWhenAnotherLayerOwnsRetries()
    {
        using var fixture = new Fixture((request, token) => Task.FromResult(Response("", HttpStatusCode.ServiceUnavailable)), new() { ClassifyRetryableFailures = false });
        Assert.AreEqual(503, (await fixture.Transport.SendCoreAsync<object>(Context())).StatusCode);
        var failure = new HttpRequestException(HttpRequestError.ConnectionError, "connection failed");
        fixture.Callback = (request, token) => Task.FromException<HttpResponseMessage>(failure);
        Assert.AreSame(failure, await Assert.ThrowsAsync<HttpRequestException>(() => fixture.Transport.SendCoreAsync<object>(Context())));
        Assert.AreEqual(2, fixture.Calls);
    }

    [TestMethod]
    public async Task NetworkClassificationIsNarrowAndPreservesOriginalCause()
    {
        using var fixture = new Fixture();
        foreach (HttpRequestError kind in new[] { HttpRequestError.ConnectionError, HttpRequestError.NameResolutionError, HttpRequestError.ResponseEnded })
        {
            var failure = new HttpRequestException(kind, "network");
            fixture.Callback = (request, token) => Task.FromException<HttpResponseMessage>(failure);
            Assert.AreSame(failure, (await Assert.ThrowsAsync<RetryableTransportException>(() => fixture.Transport.SendCoreAsync<object>(Context()))).InnerException);
            Assert.AreSame(failure, await Assert.ThrowsAsync<HttpRequestException>(() => fixture.Transport.SendCoreAsync<object>(Context("POST"))));
        }
        foreach (HttpRequestError kind in new[] { HttpRequestError.Unknown, HttpRequestError.SecureConnectionError, HttpRequestError.UserAuthenticationError, HttpRequestError.ConfigurationLimitExceeded })
        {
            var failure = new HttpRequestException(kind, "permanent");
            fixture.Callback = (request, token) => Task.FromException<HttpResponseMessage>(failure);
            Assert.AreSame(failure, await Assert.ThrowsAsync<HttpRequestException>(() => fixture.Transport.SendCoreAsync<object>(Context())));
        }
    }

    [TestMethod]
    public async Task BodyLimitsDisposeResponsesAndKeepCallerBodiesOpen()
    {
        TrackingContent? responseContent = null;
        using var fixture = new Fixture((request, token) =>
        {
            var response = Response("large response"); responseContent = (TrackingContent)response.Content; return Task.FromResult(response);
        }, new() { MaxRequestBodyBytes = 2, MaxResponseBodyBytes = 3 });
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Transport.SendCoreAsync<string>(Context()));
        Assert.IsTrue(responseContent!.Disposed);
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Transport.SendCoreAsync<object>(Context("POST", "long")));
        using var source = new NonseekableStream([1, 2, 3]);
        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Transport.SendCoreAsync<object>(Context("POST", source)));
        Assert.IsTrue(source.CanRead);
        Assert.AreEqual(1, fixture.Calls);
    }

    [TestMethod]
    public async Task PreCanceledTokensPreventSendingAndClearStaleResponseHeaders()
    {
        using var fixture = new Fixture();
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        var context = Context();
        context.Properties[HttpProxyTransport.ResponseHeadersKey] = "stale";
        var error = await Assert.ThrowsAsync<OperationCanceledException>(() => fixture.Transport.SendCoreAsync<object>(context, canceled.Token));
        Assert.AreEqual(canceled.Token, error.CancellationToken);
        Assert.IsFalse(context.Properties.ContainsKey(HttpProxyTransport.ResponseHeadersKey));
        context.CancellationToken = canceled.Token;
        error = await Assert.ThrowsAsync<OperationCanceledException>(() => fixture.Transport.SendCoreAsync<object>(context));
        Assert.AreEqual(canceled.Token, error.CancellationToken);
        Assert.AreEqual(0, fixture.Calls);
    }

    [TestMethod]
    public async Task EitherCallerTokenCancelsPendingIoWithOriginalToken()
    {
        using var argument = new CancellationTokenSource();
        using var legacy = new CancellationTokenSource();
        using var fixture = new Fixture(async (request, token) => { argument.Cancel(); await Task.Delay(Timeout.Infinite, token); return Response(); });
        var error = await Assert.ThrowsAsync<OperationCanceledException>(() => fixture.Transport.SendCoreAsync<object>(Context(), argument.Token));
        Assert.AreEqual(argument.Token, error.CancellationToken);
        fixture.Callback = async (request, token) => { legacy.Cancel(); await Task.Delay(Timeout.Infinite, token); return Response(); };
        var context = Context();
        context.CancellationToken = legacy.Token;
        error = await Assert.ThrowsAsync<OperationCanceledException>(() => fixture.Transport.SendCoreAsync<object>(context));
        Assert.AreEqual(legacy.Token, error.CancellationToken);
    }

    [TestMethod]
    public async Task TimeoutIsRetryableOnlyForReplaySafeCalls()
    {
        using var fixture = new Fixture(async (request, token) => { await Task.Delay(Timeout.Infinite, token); return Response(); }, new() { RequestTimeout = TimeSpan.FromMilliseconds(10) });
        var error = await Assert.ThrowsAsync<RetryableTransportException>(() => fixture.Transport.SendCoreAsync<object>(Context()));
        Assert.IsInstanceOfType<TimeoutException>(error.InnerException);
        await Assert.ThrowsAsync<TimeoutException>(() => fixture.Transport.SendCoreAsync<object>(Context("POST")));
        Assert.AreEqual(Timeout.InfiniteTimeSpan, fixture.Client.Timeout);
    }

    [TestMethod]
    public async Task HttpClientTimeoutAndUnrelatedCancellationAreDistinguished()
    {
        using var fixture = new Fixture((request, token) => Task.FromException<HttpResponseMessage>(new TaskCanceledException("client timeout", new TimeoutException())), new() { RequestTimeout = Timeout.InfiniteTimeSpan });
        await Assert.ThrowsAsync<RetryableTransportException>(() => fixture.Transport.SendCoreAsync<object>(Context()));
        var unrelated = new OperationCanceledException("handler cancellation");
        fixture.Callback = (request, token) => Task.FromException<HttpResponseMessage>(unrelated);
        Assert.AreSame(unrelated, await Assert.ThrowsAsync<OperationCanceledException>(() => fixture.Transport.SendCoreAsync<object>(Context())));
    }

    [TestMethod]
    public async Task ResponseBodyCancellationDisposesContentAndPreservesCallerToken()
    {
        using var source = new CancellationTokenSource();
        var stream = new CallbackStream(async (memory, token) => { source.Cancel(); await Task.Delay(Timeout.Infinite, token); return 0; });
        using var fixture = new Fixture((request, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) }));
        var error = await Assert.ThrowsAsync<OperationCanceledException>(() => fixture.Transport.SendCoreAsync<string>(Context(), source.Token));
        Assert.AreEqual(source.Token, error.CancellationToken);
        Assert.IsTrue(stream.Disposed);
    }

    [TestMethod]
    public async Task DeadlineAlsoCoversResponseBodyAfterHeaders()
    {
        var stream = new CallbackStream(async (memory, token) => { await Task.Delay(Timeout.Infinite, token); return 0; });
        using var fixture = new Fixture((request, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) }), new() { RequestTimeout = TimeSpan.FromMilliseconds(10) });
        await Assert.ThrowsAsync<TimeoutException>(() => fixture.Transport.SendCoreAsync<string>(Context("POST")));
        Assert.IsTrue(stream.Disposed);
    }

    public sealed record Payload(int Value);
    private sealed class Cycle { public Cycle? Next { get; set; } }
    private sealed class TrackingContent(byte[] bytes) : ByteArrayContent(bytes)
    {
        public bool Disposed { get; private set; }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
    private sealed class NonseekableStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    }
    private sealed class CallbackStream(Func<Memory<byte>, CancellationToken, Task<int>> read) : MemoryStream
    {
        public bool Disposed { get; private set; }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => new(read(buffer, cancellationToken));
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request, cancellationToken);
    }
    private sealed class Fixture : IDisposable
    {
        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Callback { get; set; }
        public HttpClient Client { get; }
        public HttpProxyTransport Transport { get; }
        public int Calls { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }
        public Fixture(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? callback = null, HttpProxyTransportOptions? options = null)
        {
            Callback = callback ?? ((request, token) => Task.FromResult(Response()));
            Client = new HttpClient(new Handler((request, token) => { Calls++; LastRequest = request; return Callback(request, token); }))
            { BaseAddress = new Uri("https://unit-test.invalid/api/"), Timeout = Timeout.InfiniteTimeSpan };
            Transport = options is null ? new HttpProxyTransport(Client) : new HttpProxyTransport(Client, options);
        }
        public void Dispose() => Client.Dispose();
    }
}
