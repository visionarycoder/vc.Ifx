using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Telemetry;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Telemetry;

[TestClass]
public class NullTelemetryInterceptorTests
{
    [TestMethod]
    public void Order_ShouldBeNegative50()
    {
        // Arrange
        var interceptor = new NullTelemetryInterceptor();

        // Act & Assert
        interceptor.Order.Should().Be(-50);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldPassThroughToNext()
    {
        // Arrange
        var interceptor = new NullTelemetryInterceptor();
        var context = new ProxyContext { MethodName = "TrackedMethod" };
        var expectedData = new List<int> { 1, 2, 3 };
        bool wasCalled = false;

        Task<ProxyResponse<List<int>>> Next(ProxyContext ctx, CancellationToken ct)
        {
            wasCalled = true;
            return Task.FromResult(ProxyResponse<List<int>>.Success(expectedData));
        }

        // Act
        ProxyResponse<List<int>> result = await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        wasCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(expectedData);
    }

    [TestMethod]
    public async Task InvokeAsync_WithCancellationToken_ShouldPassThrough()
    {
        // Arrange
        var interceptor = new NullTelemetryInterceptor();
        var context = new ProxyContext();
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        Task<ProxyResponse<decimal>> Next(ProxyContext ctx, CancellationToken ct)
        {
            receivedToken = ct;
            return Task.FromResult(ProxyResponse<decimal>.Success(3.14m));
        }

        // Act
        await interceptor.InvokeAsync(context, Next, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }
}
