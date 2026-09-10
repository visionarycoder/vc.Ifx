using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Resilience;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Resilience;

[TestClass]
public class NullResilienceInterceptorTests
{
    [TestMethod]
    public void Order_ShouldBe180()
    {
        // Arrange
        var interceptor = new NullResilienceInterceptor();

        // Act & Assert
        interceptor.Order.Should().Be(180);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldPassThroughToNext()
    {
        // Arrange
        var interceptor = new NullResilienceInterceptor();
        var context = new ProxyContext { MethodName = "ResilientMethod" };
        var expectedData = new { Value = 42 };
        bool wasCalled = false;

        Task<ProxyResponse<object>> Next(ProxyContext ctx, CancellationToken ct)
        {
            wasCalled = true;
            return Task.FromResult(ProxyResponse<object>.Success(expectedData));
        }

        // Act
        ProxyResponse<object> result = await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        wasCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(expectedData);
    }

    [TestMethod]
    public async Task InvokeAsync_WithCancellationToken_ShouldPassThrough()
    {
        // Arrange
        var interceptor = new NullResilienceInterceptor();
        var context = new ProxyContext();
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        Task<ProxyResponse<bool>> Next(ProxyContext ctx, CancellationToken ct)
        {
            receivedToken = ct;
            return Task.FromResult(ProxyResponse<bool>.Success(true));
        }

        // Act
        await interceptor.InvokeAsync(context, Next, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }
}
