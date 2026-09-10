using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors;

[TestClass]
public class OrderedProxyInterceptorTests
{
    [TestMethod]
    public void Constructor_ShouldStoreOrderAndInnerInterceptor()
    {
        // Arrange
        var mockInner = new Mock<IProxyInterceptor>();
        int order = 100;

        // Act
        var interceptor = new OrderedProxyInterceptor<IProxyInterceptor>(mockInner.Object, order);

        // Assert
        interceptor.Order.Should().Be(order);
    }

    [TestMethod]
    [DataRow(-100)]
    [DataRow(0)]
    [DataRow(100)]
    [DataRow(1000)]
    public void Order_ShouldReturnConstructorValue(int order)
    {
        // Arrange
        var mockInner = new Mock<IProxyInterceptor>();

        // Act
        var interceptor = new OrderedProxyInterceptor<IProxyInterceptor>(mockInner.Object, order);

        // Assert
        interceptor.Order.Should().Be(order);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldDelegateToInnerInterceptor()
    {
        // Arrange
        var mockInner = new Mock<IProxyInterceptor>();
        var context = new ProxyContext { MethodName = "TestMethod" };
        var expectedResponse = ProxyResponse<string>.Success("test data");
        ProxyDelegate<string> nextDelegate = (ctx, ct) =>
            Task.FromResult(ProxyResponse<string>.Success("next"));

        mockInner.Setup(i => i.InvokeAsync(
            It.IsAny<ProxyContext>(),
            It.IsAny<ProxyDelegate<string>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var interceptor = new OrderedProxyInterceptor<IProxyInterceptor>(mockInner.Object, 50);

        // Act
        ProxyResponse<string> result = await interceptor.InvokeAsync(context, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResponse);
        mockInner.Verify(i => i.InvokeAsync(
            It.Is<ProxyContext>(c => c.MethodName == "TestMethod"),
            It.IsAny<ProxyDelegate<string>>(),
            It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldPassThroughCancellationToken()
    {
        // Arrange
        var mockInner = new Mock<IProxyInterceptor>();
        var context = new ProxyContext();
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;
        ProxyDelegate<int> nextDelegate = (ctx, ct) =>
        {
            receivedToken = ct;
            return Task.FromResult(ProxyResponse<int>.Success(1));
        };

        mockInner.Setup(i => i.InvokeAsync(
            It.IsAny<ProxyContext>(),
            It.IsAny<ProxyDelegate<int>>(),
            It.IsAny<CancellationToken>()))
            .Callback<ProxyContext, ProxyDelegate<int>, CancellationToken>((ctx, next, ct) => receivedToken = ct)
            .ReturnsAsync(ProxyResponse<int>.Success(42));

        var interceptor = new OrderedProxyInterceptor<IProxyInterceptor>(mockInner.Object, 0);

        // Act
        await interceptor.InvokeAsync(context, nextDelegate, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    [TestMethod]
    public void OrderedProxyInterceptor_ShouldImplementIOrderedProxyInterceptor()
    {
        // Arrange
        var mockInner = new Mock<IProxyInterceptor>();
        var interceptor = new OrderedProxyInterceptor<IProxyInterceptor>(mockInner.Object, 10);

        // Assert
        interceptor.Should().BeAssignableTo<IOrderedProxyInterceptor>();
        interceptor.Should().BeAssignableTo<IProxyInterceptor>();
    }
}
