using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Caching;

[TestClass]
public class NullCachingInterceptorTests
{
    private NullCachingInterceptor interceptor = null!;

    [TestInitialize]
    public void Setup()
    {
        interceptor = new NullCachingInterceptor();
    }

    [TestMethod]
    public void Order_ShouldReturn150()
    {
        // Act
        int order = interceptor.Order;

        // Assert
        order.Should().Be(150);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldPassThroughWithoutCaching()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };
        var expectedResponse = ProxyResponse<string>.Success("Test Result");
        bool wasCalled = false;

        ProxyDelegate<string> next = (ctx, ct) =>
        {
            wasCalled = true;
            ctx.Should().Be(context);
            return Task.FromResult(expectedResponse);
        };

        // Act
        ProxyResponse<string> result = await interceptor.InvokeAsync(context, next);

        // Assert
        result.Should().Be(expectedResponse);
        wasCalled.Should().BeTrue();
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldCallNextDelegate()
    {
        // Arrange
        var context = new ProxyContext();
        int callCount = 0;

        ProxyDelegate<int> next = (ctx, ct) =>
        {
            callCount++;
            return Task.FromResult(ProxyResponse<int>.Success(42));
        };

        // Act
        ProxyResponse<int> result = await interceptor.InvokeAsync(context, next);

        // Assert
        result.Data.Should().Be(42);
        callCount.Should().Be(1);
    }

    [TestMethod]
    public async Task InvokeAsync_CalledMultipleTimes_ShouldNotCache()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetData",
            Method = "GET",
            Url = "https://api.example.com/data"
        };
        int callCount = 0;

        ProxyDelegate<int> next = (ctx, ct) =>
        {
            callCount++;
            return Task.FromResult(ProxyResponse<int>.Success(callCount));
        };

        // Act - call multiple times with same context
        ProxyResponse<int> result1 = await interceptor.InvokeAsync(context, next);
        ProxyResponse<int> result2 = await interceptor.InvokeAsync(context, next);
        ProxyResponse<int> result3 = await interceptor.InvokeAsync(context, next);

        // Assert - each call should execute next delegate (no caching)
        result1.Data.Should().Be(1);
        result2.Data.Should().Be(2);
        result3.Data.Should().Be(3);
        callCount.Should().Be(3);
    }

    [TestMethod]
    public async Task InvokeAsync_WithCancellationToken_ShouldPassThrough()
    {
        // Arrange
        var context = new ProxyContext();
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        ProxyDelegate<string> next = (ctx, ct) =>
        {
            receivedToken = ct;
            return Task.FromResult(ProxyResponse<string>.Success("Result"));
        };

        // Act
        await interceptor.InvokeAsync(context, next, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    [TestMethod]
    public async Task InvokeAsync_WithNullData_ShouldPassThrough()
    {
        // Arrange
        var context = new ProxyContext();
        var expectedResponse = ProxyResponse<string?>.Success(null);

        ProxyDelegate<string?> next = (ctx, ct) => Task.FromResult(expectedResponse);

        // Act
        ProxyResponse<string?> result = await interceptor.InvokeAsync(context, next);

        // Assert
        result.Should().Be(expectedResponse);
        result.Data.Should().BeNull();
    }

    [TestMethod]
    [DataRow("GET", "https://api.example.com/users")]
    [DataRow("POST", "https://api.example.com/users")]
    [DataRow("PUT", "https://api.example.com/users/1")]
    [DataRow("DELETE", "https://api.example.com/users/1")]
    public async Task InvokeAsync_WithDifferentHttpMethods_ShouldPassThrough(string method, string url)
    {
        // Arrange
        var context = new ProxyContext
        {
            Method = method,
            Url = url
        };
        bool wasCalled = false;

        ProxyDelegate<object> next = (ctx, ct) =>
        {
            wasCalled = true;
            return Task.FromResult(ProxyResponse<object>.Success(new object()));
        };

        // Act
        await interceptor.InvokeAsync(context, next);

        // Assert
        wasCalled.Should().BeTrue();
    }

    [TestMethod]
    public async Task InvokeAsync_WithComplexResponseType_ShouldPassThrough()
    {
        // Arrange
        var context = new ProxyContext();
        var expectedData = new ComplexType
        {
            Id = 123,
            Name = "Test",
            Items = ["A", "B", "C"]
        };
        var expectedResponse = ProxyResponse<ComplexType>.Success(expectedData);

        ProxyDelegate<ComplexType> next = (ctx, ct) => Task.FromResult(expectedResponse);

        // Act
        ProxyResponse<ComplexType> result = await interceptor.InvokeAsync(context, next);

        // Assert
        result.Data.Should().BeEquivalentTo(expectedData);
    }

    [TestMethod]
    public async Task InvokeAsync_WithException_ShouldPropagateException()
    {
        // Arrange
        var context = new ProxyContext();
        var expectedException = new InvalidOperationException("Test exception");

        ProxyDelegate<string> next = (ctx, ct) => throw expectedException;

        // Act
        Func<Task> act = async () => await interceptor.InvokeAsync(context, next);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [TestMethod]
    public async Task InvokeAsync_WithCanceledToken_ShouldThrowTaskCanceledException()
    {
        // Arrange
        var context = new ProxyContext();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        ProxyDelegate<string> next = (ctx, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(ProxyResponse<string>.Success("Result"));
        };

        // Act
        Func<Task> act = async () => await interceptor.InvokeAsync(context, next, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task InvokeAsync_CalledConcurrently_ShouldNotCache()
    {
        // Arrange
        var context = new ProxyContext();
        int counter = 0;

        ProxyDelegate<int> next = (ctx, ct) =>
        {
            Interlocked.Increment(ref counter);
            return Task.FromResult(ProxyResponse<int>.Success(counter));
        };

        // Act - call concurrently
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => interceptor.InvokeAsync(context, next))
            .ToList();

        ProxyResponse<int>[] results = await Task.WhenAll(tasks);

        // Assert - all calls should execute (no caching)
        counter.Should().Be(10);
        results.Select(r => r.Data).Should().OnlyHaveUniqueItems();
    }

    private class ComplexType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Items { get; set; } = [];
    }
}
