using System.Buffers.Binary;
using System.Net;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using VisionaryCoder.Framework.Pipeline;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Dispatch;
using VisionaryCoder.Framework.Pipeline.Dispatch.Abstractions;

namespace VisionaryCoder.Framework.Tests.PipelineGrpc;

[TestClass]
public sealed class GrpcTests
{
    private static EndpointResolution Endpoint => new(false, "service", new Uri("https://service.example"));

    [TestMethod]
    public async Task PipelineTokenReachesInFlightRpcAndCancellationReleasesCall()
    {
        using var source = new CancellationTokenSource();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var generated = new ClientFake { Response = async token =>
        {
            entered.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return new InvokeResponse { Payload = "unreachable" };
        } };
        var serializer = new SerializerFake();
        IRemoteDispatcher dispatcher = new GrpcRemoteDispatcher(serializer, uri => generated);
        var invoker = new PipelineInvoker([], new RemoteResolver(), new UnusedLocal(), dispatcher);
        var operation = invoker.InvokeAsync<Request, string>(new("x"), source.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        source.Cancel();
        var canceled = await Assert.ThrowsAsync<OperationCanceledException>(() => operation);
        Assert.AreEqual(source.Token, canceled.CancellationToken);
        Assert.AreEqual(source.Token, generated.Token);
        Assert.AreEqual(1, generated.Calls);
        Assert.AreEqual(1, generated.Disposals);
        Assert.AreEqual(0, serializer.Deserializations);
        Assert.IsNotNull(new GrpcRemoteDispatcher(serializer));
    }

    [TestMethod]
    public async Task GenericClientMapsWireFieldsForwardsTokensAndDisposesCalls()
    {
        var generated = new ClientFake();
        var client = new GenericGrpcClient(generated);
        using var source = new CancellationTokenSource();
        Assert.AreEqual("result", await client.InvokeAsync("payload", "Request", source.Token));
        Assert.AreEqual("payload", generated.Request!.Payload);
        Assert.AreEqual("Request", generated.Request.RequestType);
        Assert.AreEqual(source.Token, generated.Token);
        Assert.AreEqual(1, generated.Disposals);
        Assert.AreEqual("result", await client.InvokeAsync("", "Request"));
        Assert.AreEqual(CancellationToken.None, generated.Token);
        Assert.AreEqual("", generated.Request!.Payload);
        Assert.AreEqual(2, generated.Disposals);
    }

    [TestMethod]
    public async Task GenericClientValidatesBeforeCalling()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new GenericGrpcClient((GrpcChannel)null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new GenericGrpcClient((GenericInvoker.GenericInvokerClient)null!));
        var generated = new ClientFake();
        var client = new GenericGrpcClient(generated);
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => client.InvokeAsync(null!, "Request"));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => client.InvokeAsync("x", null!));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => client.InvokeAsync("x", " "));
        using var source = new CancellationTokenSource();
        source.Cancel();
        var error = await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => client.InvokeAsync("x", "Request", source.Token));
        Assert.AreEqual(source.Token, error.CancellationToken);
        Assert.AreEqual(0, generated.Calls);
    }

    [TestMethod]
    public async Task RpcStatusFailuresRemainUnchangedAndNeverRetry()
    {
        foreach (var status in new[] { StatusCode.Cancelled, StatusCode.DeadlineExceeded, StatusCode.Unavailable, StatusCode.PermissionDenied, StatusCode.Internal })
        {
            var failure = new RpcException(new Status(status, "failure"));
            var generated = new ClientFake { Response = token => Task.FromException<InvokeResponse>(failure) };
            Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<RpcException>(() => new GenericGrpcClient(generated).InvokeAsync("x", "Request")));
            Assert.AreEqual(1, generated.Calls);
            Assert.AreEqual(1, generated.Disposals);
        }
        var nonRpc = new InvalidOperationException("failure");
        var other = new ClientFake { Response = token => Task.FromException<InvokeResponse>(nonRpc) };
        Assert.AreSame(nonRpc, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new GenericGrpcClient(other).InvokeAsync("x", "Request")));
        Assert.AreEqual(1, other.Disposals);
    }

    [TestMethod]
    public async Task CallerCancellationIsNormalizedButUnrelatedStatusIsPreserved()
    {
        foreach (var status in new[] { StatusCode.Cancelled, StatusCode.Unavailable })
        {
            using var source = new CancellationTokenSource();
            var rpc = new RpcException(new Status(status, "failure"));
            var generated = new ClientFake { Response = token => { source.Cancel(); return Task.FromException<InvokeResponse>(rpc); } };
            var client = new GenericGrpcClient(generated);
            if (status == StatusCode.Cancelled)
            {
                var canceled = await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => client.InvokeAsync("x", "Request", source.Token));
                Assert.AreEqual(source.Token, canceled.CancellationToken);
                Assert.AreSame(rpc, canceled.InnerException);
            }
            else Assert.AreSame(rpc, await Assert.ThrowsExactlyAsync<RpcException>(() => client.InvokeAsync("x", "Request", source.Token)));
            Assert.AreEqual(1, generated.Disposals);
        }
    }

    [TestMethod]
    public async Task SuccessAfterCancellationIsNotReturned()
    {
        using var source = new CancellationTokenSource();
        var generated = new ClientFake { Response = token => { source.Cancel(); return Task.FromResult(new InvokeResponse { Payload = "late" }); } };
        var canceled = await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => new GenericGrpcClient(generated).InvokeAsync("x", "Request", source.Token));
        Assert.AreEqual(source.Token, canceled.CancellationToken);
        Assert.AreEqual(1, generated.Disposals);
    }

    [TestMethod]
    public async Task DispatcherBorrowsFactoryClientsAndPreservesSerializerContract()
    {
        var generated = new ClientFake();
        var serializer = new SerializerFake();
        Uri? observed = null;
        var dispatcher = new GrpcRemoteDispatcher(serializer, uri => { observed = uri; return generated; });
        using var source = new CancellationTokenSource();
        var request = new Request("value");
        Assert.AreEqual("decoded", await dispatcher.DispatchAsync<Request, string>(request, Endpoint, source.Token));
        Assert.AreSame(request, serializer.Request);
        Assert.AreEqual("encoded", generated.Request!.Payload);
        Assert.AreEqual(nameof(Request), generated.Request.RequestType);
        Assert.AreEqual("result", serializer.Payload);
        Assert.AreEqual(source.Token, generated.Token);
        Assert.AreEqual(Endpoint.Uri, observed);
        Assert.AreEqual("decoded", await dispatcher.DispatchAsync<Request, string>(request, Endpoint));
        Assert.AreEqual(2, generated.Calls);
        Assert.AreEqual(2, generated.Disposals);
        Assert.AreEqual(CancellationToken.None, generated.Token);
    }

    [TestMethod]
    public async Task DispatcherArgumentsAreValidatedBeforeFactoryOrSerialization()
    {
        var serializer = new SerializerFake();
        var generated = new ClientFake();
        Assert.ThrowsExactly<ArgumentNullException>(() => new GrpcRemoteDispatcher(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new GrpcRemoteDispatcher(null!, uri => generated));
        Assert.ThrowsExactly<ArgumentNullException>(() => new GrpcRemoteDispatcher(serializer, (GrpcChannelOptions)null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new GrpcRemoteDispatcher(serializer, (Func<Uri, GenericInvoker.GenericInvokerClient>)null!));
        var dispatcher = new GrpcRemoteDispatcher(serializer, uri => generated);
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => dispatcher.DispatchAsync<Request, string>(null!, Endpoint));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), null!));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), new(true)));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), new(false)));
        foreach (var uri in new[] { new Uri("relative", UriKind.Relative), new Uri("ftp://service") })
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), new(false, Uri: uri)));
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), Endpoint, source.Token));
        Assert.AreEqual(0, serializer.Serializations);
        Assert.AreEqual(0, generated.Calls);
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => new GrpcRemoteDispatcher(serializer, uri => null!).DispatchAsync<Request, string>(new("x"), Endpoint));
    }

    [TestMethod]
    public async Task DispatcherPropagatesSerializerFactoryAndRpcFailuresWithoutRetry()
    {
        var failure = new InvalidOperationException("failure");
        var generated = new ClientFake();
        var serializer = new SerializerFake { OnSerialize = () => throw failure };
        var dispatcher = new GrpcRemoteDispatcher(serializer, uri => generated);
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), Endpoint)));
        Assert.AreEqual(0, generated.Calls);
        serializer.OnSerialize = null;
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new GrpcRemoteDispatcher(serializer, uri => throw failure).DispatchAsync<Request, string>(new("x"), Endpoint)));
        serializer.OnDeserialize = () => throw failure;
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), Endpoint)));
        Assert.AreEqual(1, generated.Disposals);
        var rpc = new RpcException(new Status(StatusCode.Unavailable, "failure"));
        generated.Response = token => Task.FromException<InvokeResponse>(rpc);
        Assert.AreSame(rpc, await Assert.ThrowsExactlyAsync<RpcException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), Endpoint)));
        Assert.AreEqual(2, generated.Calls);
        Assert.AreEqual(2, generated.Disposals);
    }

    [TestMethod]
    public async Task CancellationDuringSerializationOrRpcPreventsFurtherWork()
    {
        using var source = new CancellationTokenSource();
        var generated = new ClientFake();
        var serializer = new SerializerFake { OnSerialize = source.Cancel };
        var dispatcher = new GrpcRemoteDispatcher(serializer, uri => generated);
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), Endpoint, source.Token));
        Assert.AreEqual(0, generated.Calls);
        using var duringCall = new CancellationTokenSource();
        serializer.OnSerialize = null;
        generated.Response = token => { duringCall.Cancel(); return Task.FromException<InvokeResponse>(new RpcException(new Status(StatusCode.Cancelled, "canceled"))); };
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), Endpoint, duringCall.Token));
        Assert.AreEqual(0, serializer.Deserializations);
        Assert.AreEqual(1, generated.Disposals);
    }

    [TestMethod]
    public async Task RealGeneratedWireMappingRunsInMemoryAndOwnedChannelsDisposeHandlers()
    {
        foreach (var scheme in new[] { "http", "https" })
        {
            using var handler = new WireHandler();
            var dispatcher = new GrpcRemoteDispatcher(new SystemTextJsonSerializer(), new GrpcChannelOptions { HttpHandler = handler, DisposeHttpClient = true });
            Assert.AreEqual("answer", await dispatcher.DispatchAsync<Request, string>(new("wire"), new(false, Uri: new Uri(scheme + "://service.example"))));
            Assert.AreEqual("/GenericInvoker/Invoke", handler.Path);
            Assert.AreEqual(nameof(Request), handler.Request!.RequestType);
            Assert.AreEqual("{\"Value\":\"wire\"}", handler.Request.Payload);
            Assert.AreEqual(1, handler.Calls);
            Assert.IsTrue(handler.Disposed);
        }
    }

    [TestMethod]
    public async Task ChannelConstructorBorrowsChannelAndOwnedFailureReleasesResources()
    {
        using var handler = new WireHandler();
        using var channel = GrpcChannel.ForAddress("https://service.example", new GrpcChannelOptions { HttpHandler = handler, DisposeHttpClient = true });
        var client = new GenericGrpcClient(channel);
        Assert.AreEqual("\"answer\"", await client.InvokeAsync("", "Request"));
        Assert.IsFalse(handler.Disposed);
        Assert.AreEqual("\"answer\"", await client.InvokeAsync("", "Request"));
        using var failingHandler = new WireHandler { RpcStatus = "14" };
        var dispatcher = new GrpcRemoteDispatcher(new SerializerFake(), new GrpcChannelOptions { HttpHandler = failingHandler, DisposeHttpClient = true });
        var error = await Assert.ThrowsExactlyAsync<RpcException>(() => dispatcher.DispatchAsync<Request, string>(new("x"), Endpoint));
        Assert.AreEqual(StatusCode.Unavailable, error.StatusCode);
        Assert.AreEqual(1, failingHandler.Calls);
        Assert.IsTrue(failingHandler.Disposed);
    }

    private sealed record Request(string Value) : IRequest<string>;

    private sealed class RemoteResolver : IEndpointResolver
    {
        public EndpointResolution Resolve(Type requestType) => Endpoint;
    }

    private sealed class UnusedLocal : ILocalDispatcher
    {
        public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse> =>
            throw new AssertFailedException("Remote requests must not dispatch locally.");
    }

    private sealed class SerializerFake : ISerializer
    {
        public object? Request { get; private set; }
        public string? Payload { get; private set; }
        public int Serializations { get; private set; }
        public int Deserializations { get; private set; }
        public Action? OnSerialize { get; set; }
        public Action? OnDeserialize { get; set; }
        public string Serialize<T>(T value) { Serializations++; Request = value; OnSerialize?.Invoke(); return "encoded"; }
        public T Deserialize<T>(string json) { Deserializations++; Payload = json; OnDeserialize?.Invoke(); return (T)(object)"decoded"; }
    }

    private sealed class ClientFake : GenericInvoker.GenericInvokerClient
    {
        public InvokeRequest? Request { get; private set; }
        public CancellationToken Token { get; private set; }
        public int Calls { get; private set; }
        public int Disposals { get; private set; }
        public Func<CancellationToken, Task<InvokeResponse>> Response { get; set; } = token => Task.FromResult(new InvokeResponse { Payload = "result" });
        public override AsyncUnaryCall<InvokeResponse> InvokeAsync(InvokeRequest request, Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        {
            Request = request; Token = cancellationToken; Calls++;
            return new AsyncUnaryCall<InvokeResponse>(Response(cancellationToken), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => Disposals++);
        }
    }

    private sealed class WireHandler : HttpMessageHandler
    {
        public InvokeRequest? Request { get; private set; }
        public string? Path { get; private set; }
        public bool Disposed { get; private set; }
        public int Calls { get; private set; }
        public string RpcStatus { get; set; } = "0";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Path = request.RequestUri!.AbsolutePath;
            byte[] frame = await request.Content!.ReadAsByteArrayAsync(cancellationToken);
            Assert.AreEqual(0, frame[0]);
            Assert.AreEqual(frame.Length - 5, BinaryPrimitives.ReadInt32BigEndian(frame.AsSpan(1, 4)));
            Request = InvokeRequest.Parser.ParseFrom(frame, 5, frame.Length - 5);
            byte[] message = new InvokeResponse { Payload = "\"answer\"" }.ToByteArray();
            var responseFrame = new byte[message.Length + 5];
            BinaryPrimitives.WriteInt32BigEndian(responseFrame.AsSpan(1, 4), message.Length);
            message.CopyTo(responseFrame, 5);
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Version = new Version(2, 0), Content = new ByteArrayContent(responseFrame) };
            response.Content.Headers.ContentType = new("application/grpc");
            response.TrailingHeaders.Add("grpc-status", RpcStatus);
            return response;
        }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
}
