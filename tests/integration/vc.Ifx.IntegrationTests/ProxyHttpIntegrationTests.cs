using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Interceptors.Retries;
using VisionaryCoder.Framework.Proxy.Transports;

namespace vc.Ifx.IntegrationTests;

[TestClass]
public sealed class ProxyHttpIntegrationTests
{
    [TestMethod]
    public async Task ScopedPipelineRetriesBodylessGetAndKeepsOnlyFinalResponseHeaders()
    {
        var contents = new List<TrackedContent>();
        using var handler = new Handler((request, attempt, token) =>
        {
            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual("https://integration.invalid/api/items", request.RequestUri!.AbsoluteUri);
            Assert.AreEqual("client", request.Headers.GetValues("X-Caller").Single());
            Assert.IsNull(request.Content, "Context.Request is metadata, not the HTTP body.");
            var content = new TrackedContent(attempt == 1 ? "private failure" : "{\"value\":42}");
            contents.Add(content);
            var response = new HttpResponseMessage(attempt == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK) { Content = content };
            response.Headers.Add(attempt == 1 ? "X-First" : "X-Final", "present");
            return Task.FromResult(response);
        });
        using var client = CreateClient(handler);
        using var provider = CreateProvider(client);
        using var scope = provider.CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IProxyPipeline>();
        Assert.AreSame(pipeline, scope.ServiceProvider.GetRequiredService<IProxyPipeline>());
        using var secondScope = provider.CreateScope();
        Assert.AreNotSame(pipeline, secondScope.ServiceProvider.GetRequiredService<IProxyPipeline>());
        var context = Context("GET", new { NotAPayload = true });
        context.Headers["X-Caller"] = "client";

        ProxyResponse<Payload> result = await pipeline.SendAsync<Payload>(context);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(42, result.Data!.Value);
        Assert.AreEqual(2, handler.Attempts, "Only the core owns retries.");
        Assert.IsTrue(contents.All(content => content.Disposals == 1));
        var headers = (IReadOnlyDictionary<string, string[]>)context.Properties[HttpProxyTransport.ResponseHeadersKey]!;
        Assert.IsFalse(headers.ContainsKey("X-First"));
        CollectionAssert.AreEqual(new[] { "present" }, headers["X-Final"]);
    }

    [TestMethod]
    public async Task ReplaySafeGetStopsAtCoreAttemptBudget()
    {
        using var handler = new Handler((request, attempt, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        using var client = CreateClient(handler);
        using var provider = CreateProvider(client);
        using var scope = provider.CreateScope();

        var failure = await Assert.ThrowsExactlyAsync<RetryableTransportException>(() =>
            scope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<string>(Context("GET")));

        Assert.AreEqual(3, handler.Attempts, "Two retries permit exactly three HTTP attempts.");
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, ((HttpRequestException)failure.InnerException!).StatusCode);
    }

    [TestMethod]
    [DataRow("GET", 2)]
    [DataRow("POST", 1)]
    public async Task ConnectionFailureIsRetriedOnlyForReplaySafeMethod(string method, int expectedAttempts)
    {
        var failure = new HttpRequestException(HttpRequestError.ConnectionError, "Connection interrupted");
        using var handler = new Handler((request, attempt, token) => attempt == 1
            ? Task.FromException<HttpResponseMessage>(failure)
            : Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("recovered") }));
        using var client = CreateClient(handler);
        using var provider = CreateProvider(client);
        using var scope = provider.CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IProxyPipeline>();

        if (method == "GET") Assert.AreEqual("recovered", (await pipeline.SendAsync<string>(Context(method))).Data);
        else Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<HttpRequestException>(() => pipeline.SendAsync<string>(Context(method))));

        Assert.AreEqual(expectedAttempts, handler.Attempts);
    }

    [TestMethod]
    [DataRow("POST", false)]
    [DataRow("POST", true)]
    [DataRow("GET", false)]
    public async Task BodyIsNeverReplayedAndRemainsCallerOwnedAfterScopeDisposal(string method, bool useHttpContent)
    {
        using var body = new MemoryStream(Encoding.UTF8.GetBytes("skip:payload"));
        body.Position = 5;
        using var borrowedContent = new StreamContent(body);
        var ownedContents = new List<TrackedContent>();
        using var handler = new Handler(async (request, attempt, token) =>
        {
            if (attempt == 1)
            {
                Assert.AreEqual(method, request.Method.Method);
                Assert.AreEqual("payload", await request.Content!.ReadAsStringAsync(token));
                Assert.AreNotSame(borrowedContent, request.Content);
                var content = new TrackedContent("private server diagnostic");
                ownedContents.Add(content);
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = content };
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("client still usable") };
        });
        using var client = CreateClient(handler);
        TimeSpan originalTimeout = client.Timeout;
        using (var provider = CreateProvider(client))
        using (var scope = provider.CreateScope())
        {
            var context = Context(method);
            context.Body = useHttpContent ? borrowedContent : body;
            var result = await scope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<string>(context);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(503, result.StatusCode);
            Assert.IsFalse(result.ErrorMessage!.Contains("private", StringComparison.Ordinal));
            Assert.AreEqual(1, handler.Attempts);
        }

        Assert.IsTrue(body.CanRead);
        Assert.AreEqual(body.Length, body.Position);
        Assert.IsTrue((await borrowedContent.ReadAsStreamAsync()).CanRead);
        Assert.AreEqual(1, ownedContents.Single().Disposals);
        Assert.AreEqual(originalTimeout, client.Timeout);
        Assert.AreEqual("client still usable", await client.GetStringAsync("probe"));
        Assert.AreEqual(2, handler.Attempts);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task EitherCancellationTokenStopsPendingHttpWithoutRetry(bool cancelContextToken)
    {
        using var callerCancellation = new CancellationTokenSource();
        using var contextCancellation = new CancellationTokenSource();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new Handler(async (request, attempt, token) =>
        {
            if (attempt > 1) return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("alive") };
            Assert.IsTrue(token.CanBeCanceled);
            entered.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new AssertFailedException("The pending request must be canceled.");
        });
        using var client = CreateClient(handler);
        using var provider = CreateProvider(client);
        using var scope = provider.CreateScope();
        var context = Context("GET");
        context.CancellationToken = contextCancellation.Token;
        Task<ProxyResponse<string>> pending = scope.ServiceProvider.GetRequiredService<IProxyPipeline>()
            .SendAsync<string>(context, callerCancellation.Token);
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            (cancelContextToken ? contextCancellation : callerCancellation).Cancel();
            var failure = await Assert.ThrowsAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(failure.CancellationToken.IsCancellationRequested);
            Assert.AreEqual(1, handler.Attempts);
            Assert.AreEqual("alive", await client.GetStringAsync("probe"));
        }
        finally
        {
            callerCancellation.Cancel();
            try { await pending; } catch (OperationCanceledException) { }
        }
    }

    [TestMethod]
    public async Task CancellationDuringResponseReadDisposesResponseButNotRequestBody()
    {
        using var cancellation = new CancellationTokenSource();
        using var body = new MemoryStream(Encoding.UTF8.GetBytes("payload"));
        var responseStream = new PendingReadStream();
        using var handler = new Handler((request, attempt, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        { Content = new StreamContent(responseStream) }));
        using var client = CreateClient(handler);
        using var provider = CreateProvider(client);
        using var scope = provider.CreateScope();
        var context = Context("POST");
        context.Body = body;
        Task<ProxyResponse<string>> pending = scope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<string>(context, cancellation.Token);
        try
        {
            await responseStream.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.AreEqual(1, handler.Attempts);
            Assert.IsTrue(body.CanRead);
            Assert.IsTrue(responseStream.Disposed);
        }
        finally
        {
            cancellation.Cancel();
            try { await pending; } catch (OperationCanceledException) { }
        }
    }

    private static ProxyContext Context(string method, object? metadata = null) => new() { Method = method, Url = "items", Request = metadata };

    private static HttpClient CreateClient(HttpMessageHandler handler) => new(handler, disposeHandler: false)
    { BaseAddress = new Uri("https://integration.invalid/api/"), Timeout = Timeout.InfiniteTimeSpan };

    private static ServiceProvider CreateProvider(HttpClient client) => new ServiceCollection()
        .AddLogging()
        .Configure<ProxyOptions>(options => { options.MaxRetryAttempts = 2; options.RetryDelay = TimeSpan.Zero; })
        .AddSingleton(client)
        .AddProxyPipeline()
        .AddProxyTransport<HttpProxyTransport>()
        .AddProxyInterceptor<RetryInterceptor>(ServiceLifetime.Scoped)
        .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

    public sealed record Payload(int Value);

    private sealed class Handler(Func<HttpRequestMessage, int, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        public int Attempts { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request, ++Attempts, cancellationToken);
    }

    private sealed class TrackedContent(string value) : ByteArrayContent(Encoding.UTF8.GetBytes(value))
    {
        public int Disposals { get; private set; }
        protected override void Dispose(bool disposing) { if (disposing) Disposals++; base.Dispose(disposing); }
    }

    private sealed class PendingReadStream : MemoryStream
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool Disposed { get; private set; }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Entered.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new AssertFailedException("Response read must be canceled.");
        }
        protected override void Dispose(bool disposing) { Disposed |= disposing; base.Dispose(disposing); }
    }
}
