using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Dispatch;
using VisionaryCoder.Framework.Pipeline.Routing;

namespace VisionaryCoder.Framework.Tests.Pipeline;

[TestClass]
public sealed class RoutingAndDispatchTests
{
    [TestMethod]
    public void RegistryReplacementIsImmediatelyVisibleAndRoutesAreValidated()
    {
        var registry = new InMemoryServiceRegistry();
        var resolver = new RegistryBasedResolver(registry);
        Assert.IsNull(registry.Lookup(typeof(ReadQuery)));
        Assert.IsTrue(resolver.Resolve(typeof(ReadQuery)).IsLocal);
        registry.Register<ReadQuery>(new("local", new Uri("http://local"), true));
        Assert.IsTrue(resolver.Resolve(typeof(ReadQuery)).IsLocal);
        var remote = new ServiceEntry("remote", new Uri("https://remote/api"));
        registry.Register<ReadQuery>(remote);
        var resolution = resolver.Resolve(typeof(ReadQuery));
        Assert.IsFalse(resolution.IsLocal);
        Assert.AreEqual(remote.ServiceName, resolution.ServiceName);
        Assert.AreEqual(remote.EndpointUri, resolution.Uri);
        Assert.AreSame(remote, registry.Lookup(typeof(ReadQuery)));
        var (isLocal, name, uri) = resolution;
        Assert.IsFalse(isLocal);
        Assert.AreEqual("remote", name);
        Assert.AreEqual(remote.EndpointUri, uri);
        Assert.AreEqual(resolution, resolution with { });
        Assert.AreEqual(resolution.GetHashCode(), (resolution with { }).GetHashCode());
        Assert.AreNotEqual(resolution, resolution with {IsLocal = true});
        Assert.IsTrue(resolution.ToString().Contains("remote", StringComparison.Ordinal));
        Assert.ThrowsExactly<ArgumentNullException>(() => new RegistryBasedResolver(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => resolver.Resolve(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => registry.Lookup(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => registry.Register<ReadQuery>(null!));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceEntry(" ", remote.EndpointUri));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ServiceEntry(null!, remote.EndpointUri));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ServiceEntry("x", null!));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceEntry("x", new Uri("relative", UriKind.Relative)));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceEntry("x", new Uri("ftp://host/")));
    }

    [TestMethod]
    public async Task ConcurrentRegistryWritesKeepValidEntries()
    {
        var registry = new InMemoryServiceRegistry();
        await Task.WhenAll(Enumerable.Range(0, 32).Select(index => Task.Run(() =>
        {
            registry.Register<ReadQuery>(new(index.ToString(), new Uri("https://service/")));
            Assert.IsNotNull(registry.Lookup(typeof(ReadQuery)));
        })));
        Assert.IsNotNull(registry.Lookup(typeof(ReadQuery)));
    }

    [TestMethod]
    public void KubernetesConventionUsesOnlyTrailingSuffixAndRejectsInvalidDnsLabels()
    {
        var registry = new KubernetesDnsRegistry();
        Assert.AreEqual("write", registry.Lookup(typeof(WriteRequest))!.ServiceName);
        Assert.AreEqual("readquery", registry.Lookup(typeof(ReadQuery))!.ServiceName);
        Assert.AreEqual("requestmiddle", registry.Lookup(typeof(RequestMiddleRequest))!.ServiceName);
        Assert.AreEqual(new Uri("http://write.default.svc.cluster.local/api/dispatch"), registry.Lookup(typeof(WriteRequest))!.EndpointUri);
        Assert.IsFalse(registry.Lookup(typeof(WriteRequest))!.IsLocal);
        Assert.ThrowsExactly<ArgumentNullException>(() => registry.Lookup(null!));
        Assert.ThrowsExactly<ArgumentException>(() => registry.Lookup(typeof(Request)));
        Assert.ThrowsExactly<ArgumentException>(() => registry.Lookup(typeof(List<int>)));
    }

    [TestMethod]
    public async Task LocalDispatcherResolvesExactHandlerAndForwardsCancellation()
    {
        var handler = new Handler();
        using var provider = new ServiceCollection().AddSingleton<IRequestHandler<ReadQuery, string?>>(handler).BuildServiceProvider();
        var dispatcher = new LocalDispatcher(provider);
        using var source = new CancellationTokenSource();
        Assert.AreEqual("value", await dispatcher.DispatchAsync<ReadQuery, string?>(new(), source.Token));
        Assert.AreEqual(source.Token, handler.Token);
        Assert.AreEqual("value", await dispatcher.DispatchAsync<ReadQuery, string?>(new()));
        Assert.AreEqual(CancellationToken.None, handler.Token);
        Assert.ThrowsExactly<InvalidOperationException>(() => dispatcher.DispatchAsync<WriteRequest, string?>(new()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new LocalDispatcher(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(null!));
        source.Cancel();
        Assert.ThrowsExactly<OperationCanceledException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(new(), source.Token));
    }

    [TestMethod]
    public void SerializerRejectsMalformedAndNullResponseDocuments()
    {
        var serializer = new SystemTextJsonSerializer();
        Assert.AreEqual("\"hello\"", serializer.Serialize("hello"));
        Assert.AreEqual("null", serializer.Serialize<string?>(null));
        Assert.AreEqual("hello", serializer.Deserialize<string>("\"hello\""));
        Assert.AreEqual(42, serializer.Deserialize<int>("42"));
        Assert.ThrowsExactly<ArgumentNullException>(() => serializer.Deserialize<string>(null!));
        Assert.ThrowsExactly<JsonException>(() => serializer.Deserialize<string>("null"));
        Assert.Throws<JsonException>(() => serializer.Deserialize<string>("{"));
    }

    [TestMethod]
    public async Task HttpDispatchSendsJsonAndPlatformTraceContextAndDisposesMessages()
    {
        using var handler = new HttpFake();
        using var client = new HttpClient(handler);
        var dispatcher = new HttpRemoteDispatcher(client, new SystemTextJsonSerializer());
        using var activity = new Activity("dispatch").SetIdFormat(ActivityIdFormat.W3C).Start();
        activity.TraceStateString = "vendor=state";
        activity.AddBaggage("tenant", "example");
        Assert.AreEqual("answer", await dispatcher.DispatchAsync<ReadQuery, string?>(new("a"), Remote));
        Assert.AreEqual(HttpMethod.Post, handler.Request!.Method);
        Assert.AreEqual(Remote.Uri, handler.Request.RequestUri);
        Assert.AreEqual("application/json", handler.MediaType);
        using var payload = JsonDocument.Parse(handler.Payload!);
        Assert.AreEqual("a", payload.RootElement.GetProperty("Key").GetString());
        Assert.AreEqual(activity.Id, handler.Request.Headers.GetValues("traceparent").Single());
        Assert.AreEqual("vendor=state", handler.Request.Headers.GetValues("tracestate").Single());
        Assert.IsFalse(handler.Request.Headers.Contains("baggage-tenant"));
        Assert.IsTrue(handler.ResponseContent.Disposed);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => handler.Request.Content!.ReadAsStringAsync());
        Assert.IsFalse(handler.Disposed);
    }

    [TestMethod]
    public async Task HttpValidationFailuresCancellationAndBadResponsesPropagate()
    {
        using var handler = new HttpFake();
        using var client = new HttpClient(handler);
        var serializer = new SystemTextJsonSerializer();
        var dispatcher = new HttpRemoteDispatcher(client, serializer);
        Assert.ThrowsExactly<ArgumentNullException>(() => new HttpRemoteDispatcher(null!, serializer));
        Assert.ThrowsExactly<ArgumentNullException>(() => new HttpRemoteDispatcher(client, null!));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(null!, Remote));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(new(), null!));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(new(), new(true)));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(new(), new(false)));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(new(), new(false, Uri: new Uri("file:///tmp"))));
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(new(), Remote, source.Token));
        handler.Status = HttpStatusCode.Forbidden;
        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(new(), Remote));
        Assert.IsTrue(handler.ResponseContent.Disposed);
        handler.Status = HttpStatusCode.OK;
        handler.Body = "null";
        await Assert.ThrowsExactlyAsync<JsonException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(new(), Remote));
        handler.Failure = new HttpRequestException("network");
        Assert.AreSame(handler.Failure, await Assert.ThrowsExactlyAsync<HttpRequestException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(new(), Remote)));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => handler.Request!.Content!.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task CancellationDuringHttpSendReachesTransport()
    {
        using var source = new CancellationTokenSource();
        using var handler = new HttpFake {OnSend = token => {source.Cancel(); token.ThrowIfCancellationRequested();}};
        using var client = new HttpClient(handler);
        var dispatcher = new HttpRemoteDispatcher(client, new SystemTextJsonSerializer());
        await Assert.ThrowsAsync<OperationCanceledException>(() => dispatcher.DispatchAsync<ReadQuery, string?>(new(), Remote, source.Token));
    }

    private static EndpointResolution Remote => new(false, "remote", new Uri("https://service/api"));
    private sealed class Request;
    private sealed class RequestMiddleRequest;
    private sealed class Handler : IRequestHandler<ReadQuery, string?>
    {
        public CancellationToken Token { get; private set; }
        public Task<string?> HandleAsync(ReadQuery request, CancellationToken ct) {Token = ct; return Task.FromResult<string?>("value");}
    }
    private sealed class TrackedContent(string content) : StringContent(content)
    {
        public bool Disposed { get; private set; }
        protected override void Dispose(bool disposing) {Disposed = true; base.Dispose(disposing);}
    }
    private sealed class HttpFake : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? Payload { get; private set; }
        public string? MediaType { get; private set; }
        public bool Disposed { get; private set; }
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public string Body { get; set; } = "\"answer\"";
        public Exception? Failure { get; set; }
        public Action<CancellationToken>? OnSend { get; set; }
        public TrackedContent ResponseContent { get; private set; } = new("");
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Payload = await request.Content!.ReadAsStringAsync(cancellationToken);
            MediaType = request.Content.Headers.ContentType?.MediaType;
            OnSend?.Invoke(cancellationToken);
            if (Failure is not null) throw Failure;
            ResponseContent = new(Body);
            return new HttpResponseMessage(Status) {Content = ResponseContent};
        }
        protected override void Dispose(bool disposing) {Disposed = true; base.Dispose(disposing);}
    }
}
