using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Correlation;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class CorrelationFlowContractTests
{
    [TestMethod]
    public async Task NestedAndConcurrentFlowsCopyDataAndRestoreAmbientState()
    {
        var ambient = new DefaultCorrelationContext { CorrelationId = "parent", Data = new() { ["parent"] = "retained" } };
        Dictionary<string, string> original = ambient.Data;
        var interceptor = new CorrelationInterceptor(NullLogger<CorrelationInterceptor>.Instance, ambient, new GuidCorrelationIdGenerator());
        async Task Run(string child)
        {
            var context = new ProxyContext();
            await interceptor.InvokeAsync<int>(context, async (current, token) =>
            {
                Assert.AreEqual("parent", current.CorrelationId);
                Assert.AreNotSame(original, ambient.Data);
                ambient.Data[child] = child;
                ambient.CorrelationId = child;
                await interceptor.InvokeAsync<int>(new(), async (nested, nestedToken) =>
                {
                    await Task.Yield();
                    Assert.AreEqual(child, ambient.CorrelationId);
                    Assert.AreEqual(child, nested.CorrelationId);
                    ambient.Data["nested"] = "local";
                    return ProxyResponse<int>.Success(1);
                });
                Assert.AreEqual(child, ambient.CorrelationId);
                Assert.IsFalse(ambient.Data.ContainsKey("nested"));
                return ProxyResponse<int>.Success(1);
            });
            Assert.AreEqual("parent", ambient.CorrelationId);
            Assert.AreSame(original, ambient.Data);
        }
        await Task.WhenAll(Run("first"), Run("second"));
        Assert.AreEqual(1, original.Count);
        Assert.AreEqual("parent", ambient.CorrelationId);
        ambient.CorrelationId = null;
        await interceptor.InvokeAsync<int>(new(), (context, token) =>
        {
            Assert.IsTrue(Guid.TryParse(ambient.CorrelationId, out var parsed));
            return Task.FromResult(ProxyResponse<int>.Success(1));
        });
        Assert.IsNull(ambient.CorrelationId);
    }

    [TestMethod]
    public async Task FailureCancellationAndDefaultContextSettersHaveExplicitSemantics()
    {
        var ambient = new DefaultCorrelationContext();
        ambient.Data = null!;
        Assert.AreEqual(0, ambient.Data.Count);
        ambient.SetCorrelationId("restore");
        var data = ambient.Data;
        var interceptor = new CorrelationInterceptor(NullLogger<CorrelationInterceptor>.Instance, ambient, new GuidCorrelationIdGenerator());
        var failure = new InvalidOperationException("original");
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => interceptor.InvokeAsync<int>(new(), (context, token) =>
        {
            ambient.SetCorrelationId("temporary"); ambient.Data["temporary"] = "value";
            throw failure;
        })));
        Assert.AreEqual("restore", ambient.CorrelationId);
        Assert.AreSame(data, ambient.Data);
        Assert.AreEqual(0, data.Count);
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<int>(null!, (context, token) => Task.FromResult(ProxyResponse<int>.Success(1))));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<int>(new(), null!));
        await Assert.ThrowsAsync<OperationCanceledException>(() => interceptor.InvokeAsync<int>(new(), (context, token) => throw new AssertFailedException(), new(true)));
        var generator = new Mock<ICorrelationIdGenerator>(); generator.Setup(value => value.GenerateId()).Throws(failure);
        ambient.CorrelationId = null;
        var failedGeneration = new CorrelationInterceptor(NullLogger<CorrelationInterceptor>.Instance, ambient, generator.Object);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => failedGeneration.InvokeAsync<int>(new(), (context, token) => throw new AssertFailedException()));
        Assert.IsNull(ambient.CorrelationId);
        Assert.AreSame(data, ambient.Data);
    }
}
