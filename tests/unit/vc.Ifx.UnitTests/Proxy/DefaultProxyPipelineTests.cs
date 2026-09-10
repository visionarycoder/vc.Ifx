// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Attributes;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public class DefaultProxyPipelineTests
{
    [TestMethod]
    public void Constructor_WithValidInterceptorsAndTransport_ShouldCreateInstance()
    {
        // Arrange
        var mockInterceptors = new List<IProxyInterceptor>
        {
            Mock.Of<IProxyInterceptor>(),
            Mock.Of<IProxyInterceptor>()
        };
        IProxyTransport mockTransport = Mock.Of<IProxyTransport>();

        // Act
        var pipeline = new DefaultProxyPipeline(mockInterceptors, mockTransport);

        // Assert
        pipeline.Should().NotBeNull();
    }

    [TestMethod]
    public void Constructor_WithNullTransport_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockInterceptors = new List<IProxyInterceptor>();

        // Act
        Action act = () => new DefaultProxyPipeline(mockInterceptors, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("transport");
    }

    [TestMethod]
    public void Constructor_WithNullInterceptors_ShouldThrowArgumentNullException()
    {
        // Arrange
        IProxyTransport mockTransport = Mock.Of<IProxyTransport>();

        // Act
        Action act = () => new DefaultProxyPipeline(null!, mockTransport);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Constructor_WithEmptyInterceptors_ShouldCreateInstance()
    {
        // Arrange
        var mockInterceptors = new List<IProxyInterceptor>();
        IProxyTransport mockTransport = Mock.Of<IProxyTransport>();

        // Act
        var pipeline = new DefaultProxyPipeline(mockInterceptors, mockTransport);

        // Assert
        pipeline.Should().NotBeNull();
    }

    [TestMethod]
    public async Task SendAsync_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        IProxyTransport mockTransport = Mock.Of<IProxyTransport>();
        var pipeline = new DefaultProxyPipeline([], mockTransport);

        // Act
        Func<Task> act = async () => await pipeline.SendAsync<string>(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("context");
    }

    [TestMethod]
    public async Task SendAsync_WithNoInterceptors_ShouldCallTransportDirectly()
    {
        // Arrange
        var mockTransport = new Mock<IProxyTransport>();
        var context = new ProxyContext { Request = "TestRequest" };
        var expectedResponse = ProxyResponse<string>.Success("TestResponse");

        mockTransport.Setup(t => t.SendCoreAsync<string>(context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var pipeline = new DefaultProxyPipeline([], mockTransport.Object);

        // Act
        ProxyResponse<string> proxyResponse = await pipeline.SendAsync<string>(context, CancellationToken.None);

        // Assert
        proxyResponse.Should().Be(expectedResponse);
        mockTransport.Verify(t => t.SendCoreAsync<string>(context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task SendAsync_WithSingleInterceptor_ShouldCallInterceptorAndTransport()
    {
        // Arrange
        var mockInterceptor = new Mock<IProxyInterceptor>();
        var mockTransport = new Mock<IProxyTransport>();
        var context = new ProxyContext { Request = "TestRequest" };
        var expectedResponse = ProxyResponse<string>.Success("TestResponse");

        mockTransport.Setup(t => t.SendCoreAsync<string>(context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        mockInterceptor.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) => await next(ctx, ct));

        var pipeline = new DefaultProxyPipeline([mockInterceptor.Object], mockTransport.Object);

        // Act
        ProxyResponse<string> proxyResponse = await pipeline.SendAsync<string>(context, CancellationToken.None);

        // Assert
        proxyResponse.Should().Be(expectedResponse);
        mockInterceptor.Verify(i => i.InvokeAsync<string>(
            It.IsAny<ProxyContext>(),
            It.IsAny<ProxyDelegate<string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        mockTransport.Verify(t => t.SendCoreAsync<string>(context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task SendAsync_WithMultipleInterceptors_ShouldCallInOrder()
    {
        // Arrange
        var callOrder = new List<string>();
        var context = new ProxyContext { Request = "TestRequest" };
        var expectedResponse = ProxyResponse<string>.Success("TestResponse");

        var interceptor1 = new Mock<IProxyInterceptor>();
        interceptor1.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) =>
                {
                    callOrder.Add("Interceptor1-Before");
                    ProxyResponse<string> proxyResponse = await next(ctx, ct);
                    callOrder.Add("Interceptor1-After");
                    return proxyResponse;
                });

        var interceptor2 = new Mock<IProxyInterceptor>();
        interceptor2.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) =>
                {
                    callOrder.Add("Interceptor2-Before");
                    ProxyResponse<string> proxyResponse = await next(ctx, ct);
                    callOrder.Add("Interceptor2-After");
                    return proxyResponse;
                });

        var mockTransport = new Mock<IProxyTransport>();
        mockTransport.Setup(t => t.SendCoreAsync<string>(context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callOrder.Add("Transport");
                return expectedResponse;
            });

        var pipeline = new DefaultProxyPipeline([interceptor1.Object, interceptor2.Object], mockTransport.Object);

        // Act
        ProxyResponse<string> proxyResponse = await pipeline.SendAsync<string>(context, CancellationToken.None);

        // Assert
        proxyResponse.Should().Be(expectedResponse);
        callOrder.Should().ContainInOrder(
            "Interceptor1-Before",
            "Interceptor2-Before",
            "Transport",
            "Interceptor2-After",
            "Interceptor1-After");
    }

    [TestMethod]
    public async Task SendAsync_WithOrderedInterceptors_ShouldRespectOrderProperty()
    {
        // Arrange
        var callOrder = new List<int>();
        var context = new ProxyContext { Request = "TestRequest" };
        var expectedResponse = ProxyResponse<string>.Success("TestResponse");

        var interceptorOrder100 = new Mock<IOrderedProxyInterceptor>();
        interceptorOrder100.Setup(i => i.Order).Returns(100);
        interceptorOrder100.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) =>
                {
                    callOrder.Add(100);
                    return await next(ctx, ct);
                });

        var interceptorOrder50 = new Mock<IOrderedProxyInterceptor>();
        interceptorOrder50.Setup(i => i.Order).Returns(50);
        interceptorOrder50.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) =>
                {
                    callOrder.Add(50);
                    return await next(ctx, ct);
                });

        var mockTransport = new Mock<IProxyTransport>();
        mockTransport.Setup(t => t.SendCoreAsync<string>(context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Add in reverse order to test ordering
        var pipeline = new DefaultProxyPipeline(
            [interceptorOrder100.Object, interceptorOrder50.Object],
            mockTransport.Object);

        // Act
        ProxyResponse<string> proxyResponse = await pipeline.SendAsync<string>(context, CancellationToken.None);

        // Assert
        proxyResponse.Should().Be(expectedResponse);
        callOrder.Should().ContainInOrder(50, 100);
    }

    [TestMethod]
    public async Task SendAsync_WithAttributeBasedOrder_ShouldRespectOrderAttribute()
    {
        // Arrange
        var callOrder = new List<string>();
        var context = new ProxyContext { Request = "TestRequest" };
        var expectedResponse = ProxyResponse<string>.Success("TestResponse");

        var lowOrderInterceptor = new TestAttributeInterceptor(10, "Low", callOrder);
        var highOrderInterceptor = new TestAttributeInterceptor(200, "High", callOrder);

        var mockTransport = new Mock<IProxyTransport>();
        mockTransport.Setup(t => t.SendCoreAsync<string>(context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Add in reverse order to test ordering
        var pipeline = new DefaultProxyPipeline(
            [highOrderInterceptor, lowOrderInterceptor],
            mockTransport.Object);

        // Act
        ProxyResponse<string> proxyResponse = await pipeline.SendAsync<string>(context, CancellationToken.None);

        // Assert
        proxyResponse.Should().Be(expectedResponse);
        callOrder.Should().ContainInOrder("Low", "High");
    }

    [TestMethod]
    public async Task SendAsync_WithSameOrderInterceptors_ShouldMaintainRegistrationOrder()
    {
        // Arrange
        var callOrder = new List<string>();
        var context = new ProxyContext { Request = "TestRequest" };
        var expectedResponse = ProxyResponse<string>.Success("TestResponse");

        var interceptor1 = new Mock<IOrderedProxyInterceptor>();
        interceptor1.Setup(i => i.Order).Returns(100);
        interceptor1.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) =>
                {
                    callOrder.Add("First");
                    return await next(ctx, ct);
                });

        var interceptor2 = new Mock<IOrderedProxyInterceptor>();
        interceptor2.Setup(i => i.Order).Returns(100);
        interceptor2.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) =>
                {
                    callOrder.Add("Second");
                    return await next(ctx, ct);
                });

        var mockTransport = new Mock<IProxyTransport>();
        mockTransport.Setup(t => t.SendCoreAsync<string>(context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var pipeline = new DefaultProxyPipeline(
            [interceptor1.Object, interceptor2.Object],
            mockTransport.Object);

        // Act
        ProxyResponse<string> proxyResponse = await pipeline.SendAsync<string>(context, CancellationToken.None);

        // Assert
        proxyResponse.Should().Be(expectedResponse);
        callOrder.Should().ContainInOrder("First", "Second");
    }

    [TestMethod]
    public async Task SendAsync_WithCancellationToken_ShouldPropagateCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var context = new ProxyContext { Request = "TestRequest" };
        CancellationToken capturedToken = default;

        var mockInterceptor = new Mock<IProxyInterceptor>();
        mockInterceptor.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) =>
                {
                    capturedToken = ct;
                    return await next(ctx, ct);
                });

        var mockTransport = new Mock<IProxyTransport>();
        mockTransport.Setup(t => t.SendCoreAsync<string>(context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProxyResponse<string>.Success("TestResponse"));

        var pipeline = new DefaultProxyPipeline([mockInterceptor.Object], mockTransport.Object);

        // Act
        await pipeline.SendAsync<string>(context, cts.Token);

        // Assert
        capturedToken.Should().Be(cts.Token);
    }

    [TestMethod]
    public async Task SendAsync_WithInterceptorModifyingContext_ShouldPassModifiedContext()
    {
        // Arrange
        var context = new ProxyContext { Request = "OriginalRequest" };
        var expectedResponse = ProxyResponse<string>.Success("TestResponse");
        ProxyContext? transportContext = null;

        var mockInterceptor = new Mock<IProxyInterceptor>();
        mockInterceptor.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) =>
                {
                    ctx.Request = "ModifiedRequest";
                    return await next(ctx, ct);
                });

        var mockTransport = new Mock<IProxyTransport>();
        mockTransport.Setup(t => t.SendCoreAsync<string>(It.IsAny<ProxyContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProxyContext ctx, CancellationToken _) =>
            {
                transportContext = ctx;
                return expectedResponse;
            });

        var pipeline = new DefaultProxyPipeline([mockInterceptor.Object], mockTransport.Object);

        // Act
        ProxyResponse<string> proxyResponse = await pipeline.SendAsync<string>(context, CancellationToken.None);

        // Assert
        proxyResponse.Should().Be(expectedResponse);
        transportContext.Should().NotBeNull();
        transportContext!.Request.Should().Be("ModifiedRequest");
    }

    [TestMethod]
    public async Task SendAsync_WithInterceptorThrowingException_ShouldPropagateException()
    {
        // Arrange
        var context = new ProxyContext { Request = "TestRequest" };
        var expectedException = new InvalidOperationException("Test exception");

        var mockInterceptor = new Mock<IProxyInterceptor>();
        mockInterceptor.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        IProxyTransport mockTransport = Mock.Of<IProxyTransport>();
        var pipeline = new DefaultProxyPipeline([mockInterceptor.Object], mockTransport);

        // Act
        Func<Task> act = async () => await pipeline.SendAsync<string>(context, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [TestMethod]
    public async Task SendAsync_WithNegativeOrderInterceptors_ShouldExecuteBeforePositiveOrder()
    {
        // Arrange
        var callOrder = new List<int>();
        var context = new ProxyContext { Request = "TestRequest" };
        var expectedResponse = ProxyResponse<string>.Success("TestResponse");

        var negativeOrderInterceptor = new Mock<IOrderedProxyInterceptor>();
        negativeOrderInterceptor.Setup(i => i.Order).Returns(-100);
        negativeOrderInterceptor.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) =>
                {
                    callOrder.Add(-100);
                    return await next(ctx, ct);
                });

        var positiveOrderInterceptor = new Mock<IOrderedProxyInterceptor>();
        positiveOrderInterceptor.Setup(i => i.Order).Returns(100);
        positiveOrderInterceptor.Setup(i => i.InvokeAsync<string>(
                It.IsAny<ProxyContext>(),
                It.IsAny<ProxyDelegate<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProxyContext, ProxyDelegate<string>, CancellationToken>(
                async (ctx, next, ct) =>
                {
                    callOrder.Add(100);
                    return await next(ctx, ct);
                });

        var mockTransport = new Mock<IProxyTransport>();
        mockTransport.Setup(t => t.SendCoreAsync<string>(context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Add in reverse order to test ordering
        var pipeline = new DefaultProxyPipeline(
            [positiveOrderInterceptor.Object, negativeOrderInterceptor.Object],
            mockTransport.Object);

        // Act
        ProxyResponse<string> proxyResponse = await pipeline.SendAsync<string>(context, CancellationToken.None);

        // Assert
        proxyResponse.Should().Be(expectedResponse);
        callOrder.Should().ContainInOrder(-100, 100);
    }

    // Test helper class with attribute-based ordering
    [ProxyInterceptorOrderAttribute(10)]
    private class TestAttributeInterceptor(int order, string name, List<string> callOrder) : IProxyInterceptor
    {
        public int Order { get; } = order;

        public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
        {
            callOrder.Add(name);
            return next(context, cancellationToken);
        }
    }
}
