using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Retries;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Retries;

[TestClass]
public class NullRetryInterceptorTests
{
    [TestMethod]
    public void Order_ShouldBe200()
    {
        // Arrange
        var interceptor = new NullRetryInterceptor();

        // Act & Assert
        interceptor.Order.Should().Be(200);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldPassThroughToNext()
    {
        // Arrange
        var interceptor = new NullRetryInterceptor();
        var context = new ProxyContext { MethodName = "TestMethod" };
        string expectedData = "test data";
        bool wasCalled = false;

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct)
        {
            wasCalled = true;
            return Task.FromResult(ProxyResponse<string>.Success(expectedData));
        }

        // Act
        ProxyResponse<string> result = await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        wasCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedData);
    }

    [TestMethod]
    public async Task InvokeAsync_WithCancellationToken_ShouldPassThrough()
    {
        // Arrange
        var interceptor = new NullRetryInterceptor();
        var context = new ProxyContext();
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        Task<ProxyResponse<int>> Next(ProxyContext ctx, CancellationToken ct)
        {
            receivedToken = ct;
            return Task.FromResult(ProxyResponse<int>.Success(42));
        }

        // Act
        await interceptor.InvokeAsync(context, Next, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }
}
