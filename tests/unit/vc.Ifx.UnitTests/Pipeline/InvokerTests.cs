using Polly;
using VisionaryCoder.Framework.Pipeline;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Interceptors;

namespace VisionaryCoder.Framework.Tests.Pipeline;

[TestClass]
public sealed class InvokerTests
{
    [TestMethod]
    public async Task OrderIsOuterFirstAndRegistrationIsSnapshotted()
    {
        var events = new List<string>();
        var resolver = new ResolverFake();
        var local = new LocalFake { Call = (request, token) => { events.Add("dispatch"); return Task.FromResult<object?>("answer"); } };
        var interceptors = new List<IInterceptor> { new Ordered("a", events), new Ordered("b", events) };
        var invoker = new PipelineInvoker(interceptors, resolver, local, new LegacyRemote());
        interceptors.Clear();
        Assert.AreEqual("answer", await invoker.InvokeAsync<ReadQuery, string?>(new()));
        CollectionAssert.AreEqual(new[] { "a:before", "b:before", "dispatch", "b:after", "a:after" }, events);
        Assert.AreEqual(1, resolver.Calls);
    }

    [TestMethod]
    public async Task RemoteAndShortCircuitRoutingAreDeterministic()
    {
        var resolver = new ResolverFake { Endpoint = new(false, "remote", new Uri("https://remote/")) };
        var remote = new LegacyRemote();
        var invoker = new PipelineInvoker([], resolver, new LocalFake(), remote);
        Assert.IsNull(await invoker.InvokeAsync<ReadQuery, string?>(new(), default));
        Assert.AreSame(resolver.Endpoint, remote.Endpoint);
        var cache = new CachingInterceptor(new CacheFake { Hit = true, Value = "cached" }, request => "key", TimeSpan.FromMinutes(1));
        Assert.AreEqual("cached", await new PipelineInvoker([cache], resolver, new LocalFake(), remote).InvokeAsync<ReadQuery, string?>(new()));
        Assert.AreEqual(1, resolver.Calls);
    }

    [TestMethod]
    public async Task CorrelationIsScopedAndCancellationAndFailurePropagate()
    {
        Correlation.CurrentId = null;
        string? observed = null;
        using var source = new CancellationTokenSource();
        var failure = new InvalidOperationException("failure");
        var local = new LocalFake { Call = (request, token) => { observed = Correlation.CurrentId; Assert.AreEqual(source.Token, token); throw failure; } };
        var invoker = new PipelineInvoker([], new ResolverFake(), local, new LegacyRemote());
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => invoker.InvokeAsync<ReadQuery, string?>(new(), source.Token)));
        Assert.IsTrue(Guid.TryParseExact(observed, "N", out var parsed));
        Assert.IsNull(Correlation.CurrentId);
        Correlation.CurrentId = "parent";
        local.Call = (request, token) => { Assert.AreEqual("parent", Correlation.CurrentId); return Task.FromResult<object?>("ok"); };
        await invoker.InvokeAsync<ReadQuery, string?>(new());
        Assert.AreEqual("parent", Correlation.CurrentId);
        Correlation.CurrentId = null;
        var canceller = new LegacyInterceptor { Before = source.Cancel };
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => new PipelineInvoker([canceller], new ResolverFake(), local, new LegacyRemote()).InvokeAsync<ReadQuery, string?>(new(), source.Token));
        Assert.IsNull(Correlation.CurrentId);
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => invoker.InvokeAsync<ReadQuery, string?>(new(), source.Token));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => invoker.InvokeAsync<ReadQuery, string?>(null!));
    }

    [TestMethod]
    public async Task ConcurrentInvocationsHaveIndependentCorrelations()
    {
        Correlation.CurrentId = null;
        var arrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int count = 0;
        var local = new LocalFake { Call = async (request, token) =>
        {
            var id = Correlation.CurrentId;
            if (Interlocked.Increment(ref count) == 2) arrived.SetResult();
            await arrived.Task;
            Assert.AreEqual(id, Correlation.CurrentId);
            return id;
        } };
        var invoker = new PipelineInvoker([], new ResolverFake(), local, new LegacyRemote());
        var first = invoker.InvokeAsync<ReadQuery, string?>(new());
        var second = invoker.InvokeAsync<ReadQuery, string?>(new());
        Assert.AreNotEqual(await first, await second);
        Assert.IsNull(Correlation.CurrentId);
    }

    [TestMethod]
    public void ConstructorRejectsInvalidCollaboratorsAndDuplicateResilience()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new PipelineInvoker(null!, new ResolverFake(), new LocalFake(), new LegacyRemote()));
        Assert.ThrowsExactly<ArgumentException>(() => new PipelineInvoker([null!], new ResolverFake(), new LocalFake(), new LegacyRemote()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new PipelineInvoker([], null!, new LocalFake(), new LegacyRemote()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new PipelineInvoker([], new ResolverFake(), null!, new LegacyRemote()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new PipelineInvoker([], new ResolverFake(), new LocalFake(), null!));
        var resilience = new ResilienceInterceptor(ResiliencePipeline.Empty);
        Assert.ThrowsExactly<ArgumentException>(() => new PipelineInvoker([resilience, resilience], new ResolverFake(), new LocalFake(), new LegacyRemote()));
    }

    private sealed class Ordered(string name, List<string> events) : IInterceptor
    {
        public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, Func<TRequest, Task<TResponse>> next) where TRequest : IRequest<TResponse>
        {
            events.Add(name + ":before");
            var result = await next(request);
            events.Add(name + ":after");
            return result;
        }
    }
}
