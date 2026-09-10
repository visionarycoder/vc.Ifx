using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Auditing;
using VisionaryCoder.Framework.Proxy.Interceptors.Logging;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class LoggingAuditEdgeContractTests
{
    [TestMethod]
    public async Task CancellationIsRecordedWithoutReplacingTheOriginalException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new LoggingInterceptor(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new TimingInterceptor(null!));
        foreach (IOrderedProxyInterceptor interceptor in new IOrderedProxyInterceptor[]
        {
            new LoggingInterceptor(NullLogger<LoggingInterceptor>.Instance),
            new TimingInterceptor(NullLogger<TimingInterceptor>.Instance)
        })
        {
            Assert.IsTrue(interceptor.Order > 0);
            using var cancellation = new CancellationTokenSource();
            var context = new ProxyContext();
            var error = new OperationCanceledException(cancellation.Token);
            var actual = await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => interceptor.InvokeAsync<int>(context,
                (call, token) => { cancellation.Cancel(); throw error; }, cancellation.Token));
            Assert.AreSame(error, actual);
            if (interceptor is LoggingInterceptor)
            {
                Assert.AreEqual("Warning", context.Metadata["LogLevel"]);
                Assert.AreEqual("OperationCanceled", context.Metadata["ExceptionType"]);
            }
            else Assert.IsTrue(context.Metadata.ContainsKey("EndTime"));
        }
        var logger = new Mock<ILogger<TimingInterceptor>>();
        var timing = new TimingInterceptor(logger.Object) { CriticalOperationThresholdMs = 0 };
        await timing.InvokeAsync<int>(new(), (context, token) => Task.FromResult(ProxyResponse<int>.Success(1)));
        logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        var failure = ProxyResponse<int>.Failure("failed");
        Assert.AreSame(failure, await timing.InvokeAsync<int>(new(), (context, token) => Task.FromResult(failure)));
    }

    [TestMethod]
    public async Task AuditFallbackCorrelationAndSinkEnumerationFailurePreserveBusinessFailure()
    {
        foreach (object? correlation in new object?[] { null, new NullString() })
        {
            AuditRecord? record = null;
            var sink = new Mock<IAuditSink>();
            sink.Setup(x => x.WriteAsync(It.IsAny<AuditRecord>(), It.IsAny<CancellationToken>()))
                .Callback<AuditRecord, CancellationToken>((value, token) => record = value).Returns(Task.CompletedTask);
            var context = new ProxyContext();
            context.Items["CorrelationId"] = correlation!;
            await new AuditingInterceptor(NullLogger<AuditingInterceptor>.Instance, [sink.Object])
                .InvokeAsync<int>(context, (call, token) => Task.FromResult(ProxyResponse<int>.Success(1)));
            Assert.IsNotNull(record);
            Assert.IsTrue(Guid.TryParse(record.CorrelationId, out var id));
        }
        var sinks = new Mock<IEnumerable<IAuditSink>>();
        sinks.Setup(x => x.GetEnumerator()).Throws(new InvalidOperationException("enumeration"));
        var logger = new Mock<ILogger<AuditingInterceptor>>();
        var error = new InvalidOperationException("business");
        var actual = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new AuditingInterceptor(logger.Object, sinks.Object)
            .InvokeAsync<int>(new(), (context, token) => throw error));
        Assert.AreSame(error, actual);
        logger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    private sealed class NullString { public override string? ToString() => null; }
}
