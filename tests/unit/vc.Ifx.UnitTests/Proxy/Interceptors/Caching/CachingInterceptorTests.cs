using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Caching;

[TestClass]
public class CachingInterceptorTests
{
    private Mock<ILogger<CachingInterceptor>> mockLogger = null!;
    private IMemoryCache cache = null!;
    private CachingOptions options = null!;
    private CachingInterceptor interceptor = null!;

    [TestInitialize]
    public void Setup()
    {
        mockLogger = new Mock<ILogger<CachingInterceptor>>();
        cache = new MemoryCache(new MemoryCacheOptions());
        options = new CachingOptions { DefaultDuration = TimeSpan.FromMinutes(5) };
        interceptor = new CachingInterceptor(mockLogger.Object, cache, options);
    }

    [TestCleanup]
    public void Cleanup()
    {
        cache.Dispose();
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new CachingInterceptor(null!, cache, options));
    }

    [TestMethod]
    public void Constructor_WithNullCache_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new CachingInterceptor(mockLogger.Object, null!, options));
    }

    [TestMethod]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new CachingInterceptor(mockLogger.Object, cache, null!));
    }

    [TestMethod]
    public void Order_ShouldBe50()
    {
        // Assert
        interceptor.Order.Should().Be(50);
    }

    [TestMethod]
    public async Task InvokeAsync_FirstCall_ShouldCacheMiss()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "TestOp" };
        int callCount = 0;

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct)
        {
            callCount++;
            return Task.FromResult(ProxyResponse<string>.Success("data"));
        }

        // Act
        ProxyResponse<string> result = await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        callCount.Should().Be(1);
        context.Metadata["CacheHit"].Should().Be(false);
    }

    [TestMethod]
    public async Task InvokeAsync_SecondCall_ShouldCacheHit()
    {
        // Arrange
        var context1 = new ProxyContext { OperationName = "TestOp" };
        var context2 = new ProxyContext { OperationName = "TestOp" };
        int callCount = 0;

        Task<ProxyResponse<int>> Next(ProxyContext ctx, CancellationToken ct)
        {
            callCount++;
            return Task.FromResult(ProxyResponse<int>.Success(42));
        }

        // Act
        ProxyResponse<int> result1 = await interceptor.InvokeAsync(context1, Next, CancellationToken.None);
        ProxyResponse<int> result2 = await interceptor.InvokeAsync(context2, Next, CancellationToken.None);

        // Assert
        result1.Data.Should().Be(42);
        result2.Data.Should().Be(42);
        callCount.Should().Be(1); // Only called once
        context1.Metadata["CacheHit"].Should().Be(false);
        context2.Metadata["CacheHit"].Should().Be(true);
    }

    [TestMethod]
    public async Task InvokeAsync_WithDisableCache_ShouldBypassCache()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "TestOp" };
        context.Metadata["DisableCache"] = true;
        int callCount = 0;

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct)
        {
            callCount++;
            return Task.FromResult(ProxyResponse<string>.Success("data"));
        }

        // Act
        await interceptor.InvokeAsync(context, Next, CancellationToken.None);
        await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert
        callCount.Should().Be(2); // Called twice, no caching
    }

    [TestMethod]
    public async Task InvokeAsync_WithFailedResponse_ShouldNotCache()
    {
        // Arrange
        var context1 = new ProxyContext { OperationName = "FailOp" };
        var context2 = new ProxyContext { OperationName = "FailOp" };
        int callCount = 0;

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct)
        {
            callCount++;
            return Task.FromResult(ProxyResponse<string>.Failure("Error"));
        }

        // Act
        await interceptor.InvokeAsync(context1, Next, CancellationToken.None);
        await interceptor.InvokeAsync(context2, Next, CancellationToken.None);

        // Assert
        callCount.Should().Be(2); // Called twice because failure is not cached
    }

    [TestMethod]
    public async Task InvokeAsync_WithCustomCacheDuration_ShouldUseCustomDuration()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "CustomDurationOp" };
        context.Metadata["CacheDurationSeconds"] = 60;

        Task<ProxyResponse<bool>> Next(ProxyContext ctx, CancellationToken ct) =>
            Task.FromResult(ProxyResponse<bool>.Success(true));

        // Act
        await interceptor.InvokeAsync(context, Next, CancellationToken.None);

        // Assert - should be cached
        var cachedContext = new ProxyContext { OperationName = "CustomDurationOp" };
        cachedContext.Metadata["CacheDurationSeconds"] = 60;
        ProxyResponse<bool> result = await interceptor.InvokeAsync(cachedContext, Next, CancellationToken.None);

        cachedContext.Metadata["CacheHit"].Should().Be(true);
    }

    [TestMethod]
    public async Task InvokeAsync_WithCustomKeyGenerator_ShouldUseCustomKey()
    {
        // Arrange
        var customOptions = new CachingOptions
        {
            DefaultDuration = TimeSpan.FromMinutes(5),
            KeyGenerator = ctx => $"custom_{ctx.OperationName}"
        };
        var customInterceptor = new CachingInterceptor(mockLogger.Object, cache, customOptions);

        var context1 = new ProxyContext { OperationName = "TestOp" };
        var context2 = new ProxyContext { OperationName = "TestOp" };
        int callCount = 0;

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct)
        {
            callCount++;
            return Task.FromResult(ProxyResponse<string>.Success("data"));
        }

        // Act
        await customInterceptor.InvokeAsync(context1, Next, CancellationToken.None);
        await customInterceptor.InvokeAsync(context2, Next, CancellationToken.None);

        // Assert
        callCount.Should().Be(1); // Second call should hit cache
        context2.Metadata["CacheHit"].Should().Be(true);
    }

    [TestMethod]
    public async Task InvokeAsync_DifferentOperations_ShouldHaveSeparateCacheEntries()
    {
        // Arrange
        var context1 = new ProxyContext { OperationName = "Op1" };
        var context2 = new ProxyContext { OperationName = "Op2" };
        int callCount = 0;

        Task<ProxyResponse<int>> Next(ProxyContext ctx, CancellationToken ct)
        {
            callCount++;
            return Task.FromResult(ProxyResponse<int>.Success(callCount));
        }

        // Act
        ProxyResponse<int> result1 = await interceptor.InvokeAsync(context1, Next, CancellationToken.None);
        ProxyResponse<int> result2 = await interceptor.InvokeAsync(context2, Next, CancellationToken.None);

        // Assert
        result1.Data.Should().Be(1);
        result2.Data.Should().Be(2);
        callCount.Should().Be(2); // Both called because different operations
    }

    [TestMethod]
    public async Task InvokeAsync_WithCancellationToken_ShouldPassThrough()
    {
        // Arrange
        var context = new ProxyContext { OperationName = "TestOp" };
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        Task<ProxyResponse<string>> Next(ProxyContext ctx, CancellationToken ct)
        {
            receivedToken = ct;
            return Task.FromResult(ProxyResponse<string>.Success("data"));
        }

        // Act
        await interceptor.InvokeAsync(context, Next, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }
}
