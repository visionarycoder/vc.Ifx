using System.Net;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Pipeline;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Dispatch;

namespace vc.Ifx.Proxy.Benchmarks;

[MemoryDiagnoser]
public class PipelineBenchmarks
{
    private ServiceProvider services = null!;
    private LocalDispatcher dispatcher = null!;
    private PipelineInvoker invoker = null!;
    private readonly WorkRequest request = new();

    [Params(0, 1, 8)] public int InterceptorCount { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        services = new ServiceCollection().AddSingleton<IRequestHandler<WorkRequest, int>, Handler>().BuildServiceProvider();
        dispatcher = new LocalDispatcher(services);
        invoker = new PipelineInvoker(Enumerable.Range(0, InterceptorCount).Select(index => new PassThrough()), new Resolver(), dispatcher, new UnusedRemote());
        if (await DirectDispatch() != 42 || await ThroughInterceptors() != 42)
            throw new InvalidOperationException("Pipeline fixture returned an unexpected result.");
    }

    [Benchmark(Baseline = true)] public Task<int> DirectDispatch() => dispatcher.DispatchAsync<WorkRequest, int>(request, CancellationToken.None);
    [Benchmark] public Task<int> ThroughInterceptors() => invoker.InvokeAsync<WorkRequest, int>(request, CancellationToken.None);
    [GlobalCleanup] public void Cleanup() => services.Dispose();

    public sealed record WorkRequest : IRequest<int>;
    public sealed class Handler : IRequestHandler<WorkRequest, int>
    {
        public Task<int> HandleAsync(WorkRequest request, CancellationToken ct) => Task.FromResult(42);
    }
    private sealed class Resolver : IEndpointResolver
    {
        public EndpointResolution Resolve(Type requestType) => new(true);
    }
    private sealed class PassThrough : IInterceptor
    {
        public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next)
            where TRequest : IRequest<TResponse> => next(request);
        public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse> => next(request, cancellationToken);
    }
    private sealed class UnusedRemote : IRemoteDispatcher
    {
        public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, EndpointResolution endpoint)
            where TRequest : IRequest<TResponse> => throw new InvalidOperationException("This benchmark must remain local.");
    }
}

[MemoryDiagnoser]
public class HttpAdapterBenchmarks
{
    private HttpClient client = null!;
    private HttpRemoteDispatcher dispatcher = null!;
    private PayloadRequest request = null!;
    private readonly EndpointResolution endpoint = new(false, "fixture", new Uri("https://benchmark.invalid/dispatch"));

    [Params(64, 65536)] public int PayloadCharacters { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        client = new HttpClient(new InMemoryHandler());
        dispatcher = new HttpRemoteDispatcher(client, new SystemTextJsonSerializer());
        request = new(new string('x', PayloadCharacters));
        if ((await SerializeDispatchDeserialize()).Value != 42) throw new InvalidOperationException("Unexpected HTTP adapter result.");
    }

    [Benchmark] public Task<PayloadResponse> SerializeDispatchDeserialize() =>
        dispatcher.DispatchAsync<PayloadRequest, PayloadResponse>(request, endpoint, CancellationToken.None);
    [GlobalCleanup] public void Cleanup() => client.Dispose();

    public sealed record PayloadRequest(string Content) : IRequest<PayloadResponse>;
    public sealed record PayloadResponse(int Value);
    private sealed class InMemoryHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await request.Content!.CopyToAsync(Stream.Null, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"Value\":42}") };
        }
    }
}
