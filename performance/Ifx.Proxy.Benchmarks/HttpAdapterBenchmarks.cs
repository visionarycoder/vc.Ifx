using System.Net;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Ifx.Helpers;
using Ifx.Pipeline;
using Ifx.Pipeline.Abstractions;
using Ifx.Pipeline.Dispatch;

namespace Ifx.Proxy.Benchmarks;

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
