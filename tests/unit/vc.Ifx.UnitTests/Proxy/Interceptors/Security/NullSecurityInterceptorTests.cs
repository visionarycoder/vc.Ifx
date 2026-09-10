using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Security;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Security;

[TestClass]
public class NullSecurityInterceptorTests
{
    [TestMethod]
    public void Order_ShouldBeNegative200()
    {
        // Arrange
        var interceptor = new NullSecurityInterceptor();

        // Act & Assert
        interceptor.Order.Should().Be(-200);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldPassThroughToNext()
    {
        // Arrange
        var interceptor = new NullSecurityInterceptor();
        var context = new ProxyContext { MethodName = "SecureMethod" };
        int expectedData = 123;
        bool wasCalled = false;

        Task<ProxyResponse<int>> Next(ProxyContext ctx, CancellationToken ct)
        {
            wasCalled = true;
            return Task.FromResult(ProxyResponse<int>.Success(expectedData));
        }

        // Act
        ProxyResponse<int> result = await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        wasCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedData);
    }

    [TestMethod]
    public async Task InvokeAsync_WithCancellationToken_ShouldPassThrough()
    {
        // Arrange
        var interceptor = new NullSecurityInterceptor();
        var context = new ProxyContext();
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct)
        {
            receivedToken = ct;
            return Task.FromResult(ProxyResponse<string>.Success("secure"));
        }

        // Act
        await interceptor.InvokeAsync(context, Next, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }
}
