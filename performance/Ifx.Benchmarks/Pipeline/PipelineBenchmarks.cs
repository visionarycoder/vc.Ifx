using System.Net;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Ifx.Helpers;
using Ifx.Pipeline;
using Ifx.Pipeline.Dispatch;

namespace Ifx.Benchmarks.Pipeline;

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
