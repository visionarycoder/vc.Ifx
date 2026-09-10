using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Interceptors.Abstractions;

namespace VisionaryCoder.Framework.Tests.Pipeline;

[TestClass]
public sealed class CompatibilityTests
{
    [TestMethod]
    public async Task LegacyDispatchAndInvokerImplementationsRemainCallableWithTokens()
    {
        IInvoker invoker = new LegacyInvoker();
        ILocalDispatcher local = new LegacyLocal();
        IRemoteDispatcher remote = new LegacyRemote();
        var request = new ReadQuery();
        var endpoint = new EndpointResolution(false, "remote", new Uri("https://service/"));
        Assert.IsNull(await invoker.InvokeAsync<ReadQuery, string?>(request, default));
        Assert.IsNull(await local.DispatchAsync<ReadQuery, string?>(request, default));
        Assert.IsNull(await remote.DispatchAsync<ReadQuery, string?>(request, endpoint, default));
        using var source = new CancellationTokenSource();
        source.Cancel();
        Assert.ThrowsExactly<OperationCanceledException>(() => invoker.InvokeAsync<ReadQuery, string?>(request, source.Token));
        Assert.ThrowsExactly<OperationCanceledException>(() => local.DispatchAsync<ReadQuery, string?>(request, source.Token));
        Assert.ThrowsExactly<OperationCanceledException>(() => remote.DispatchAsync<ReadQuery, string?>(request, endpoint, source.Token));
    }

    [TestMethod]
    public async Task LegacyInterceptorBridgeForwardsTokenAndChecksBeforeContinuation()
    {
        var implementation = new LegacyInterceptor();
        IInterceptor interceptor = implementation;
        using var source = new CancellationTokenSource();
        Assert.AreEqual("ok", await interceptor.InvokeAsync<ReadQuery, string?>(new(), (request, token) =>
        {
            Assert.AreEqual(source.Token, token);
            return Task.FromResult<string?>("ok");
        }, source.Token));
        implementation.Before = source.Cancel;
        Assert.ThrowsExactly<OperationCanceledException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(),
            (request, token) => throw new AssertFailedException(), source.Token));
        Assert.ThrowsExactly<OperationCanceledException>(() => interceptor.InvokeAsync<ReadQuery, string?>(new(),
            (request, token) => throw new AssertFailedException(), source.Token));
    }

    [TestMethod]
    public async Task LegacyCacheAndAuthorizationBridgesPreserveOperations()
    {
        ICache cache = new CacheFake();
        IAuthorizationService auth = new AuthFake();
        await cache.SetAsync("key", "value", TimeSpan.FromSeconds(1), default);
        Assert.AreEqual("value", (await cache.TryGetAsync<string>("key", default)).value);
        await auth.AuthorizeAsync(new ReadQuery(), default);
        using var source = new CancellationTokenSource();
        source.Cancel();
        Assert.ThrowsExactly<OperationCanceledException>(() => cache.TryGetAsync<string>("key", source.Token));
        Assert.ThrowsExactly<OperationCanceledException>(() => cache.SetAsync("key", "value", TimeSpan.FromSeconds(1), source.Token));
        Assert.ThrowsExactly<OperationCanceledException>(() => auth.AuthorizeAsync(new ReadQuery(), source.Token));
    }
}
