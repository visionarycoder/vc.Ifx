using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VisionaryCoder.Framework.WebApi.ExceptionHandling;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace VisionaryCoder.Framework.Tests.WebApi.ExceptionHandling;

[TestClass]
public sealed class GlobalExceptionHandlerTests
{
    private static DefaultHttpContext Context()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static GlobalExceptionHandler Handler(IProblemDetailsService writer, int? status = 500)
    {
        var mapper = new Mock<IProblemDetailsExceptionMapper>();
        mapper.Setup(x => x.Map(It.IsAny<HttpContext>(), It.IsAny<Exception>()))
            .Returns(() => new ProblemDetails {Status = status, Title = "Error"});
        return new(mapper.Object, writer, NullLogger<GlobalExceptionHandler>.Instance);
    }

    [TestMethod]
    public async Task NullInputsAndCancellationNeverReachTheWriter()
    {
        var writer = new Writer();
        var mapper = Mock.Of<IProblemDetailsExceptionMapper>();
        Assert.ThrowsExactly<ArgumentNullException>(() => new GlobalExceptionHandler(null!, writer, NullLogger<GlobalExceptionHandler>.Instance));
        Assert.ThrowsExactly<ArgumentNullException>(() => new GlobalExceptionHandler(mapper, null!, NullLogger<GlobalExceptionHandler>.Instance));
        Assert.ThrowsExactly<ArgumentNullException>(() => new GlobalExceptionHandler(mapper, writer, null!));
        var handler = Handler(writer);
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => handler.TryHandleAsync(null!, new Exception(), default).AsTask());
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => handler.TryHandleAsync(Context(), null!, default).AsTask());
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => handler.TryHandleAsync(Context(), new Exception(), source.Token).AsTask());
        var context = Context();
        context.RequestAborted = source.Token;
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => handler.TryHandleAsync(context, new Exception(), default).AsTask());
        var canceled = new TaskCanceledException("secret");
        var thrown = await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => handler.TryHandleAsync(Context(), canceled, default).AsTask());
        Assert.AreSame(canceled, thrown);
        Assert.AreEqual(0, writer.Calls);
    }

    [TestMethod]
    public async Task StartedResponseIsLeftUntouched()
    {
        var context = Context();
        var response = new Mock<IHttpResponseFeature>();
        response.SetupGet(x => x.HasStarted).Returns(true);
        context.Features.Set(response.Object);
        var writer = new Writer();
        Assert.IsFalse(await Handler(writer).TryHandleAsync(context, new Exception(), default));
        Assert.AreEqual(0, writer.Calls);
        response.VerifySet(x => x.StatusCode = It.IsAny<int>(), Times.Never);
    }

    [TestMethod]
    public async Task RequiredHeadersMustExistAndArePreserved()
    {
        foreach (var (status, name, value) in new[]
        {
            (401, "WWW-Authenticate", "Bearer"), (407, "Proxy-Authenticate", "Basic realm=\"proxy\""),
            (405, "Allow", "GET, HEAD"), (426, "Upgrade", "HTTP/2.0")
        })
        {
            var writer = new Writer();
            var context = Context();
            Assert.IsFalse(await Handler(writer, status).TryHandleAsync(context, new Exception(), default));
            Assert.AreEqual(200, context.Response.StatusCode);
            context.Response.Headers[name] = " ";
            Assert.IsFalse(await Handler(writer, status).TryHandleAsync(context, new Exception(), default));
            Assert.AreEqual(0, writer.Calls);
            context.Response.Headers[name] = value;
            Assert.IsTrue(await Handler(writer, status).TryHandleAsync(context, new Exception(), default));
            Assert.AreEqual(status, context.Response.StatusCode);
            Assert.AreEqual(value, context.Response.Headers[name].ToString());
            Assert.AreEqual(1, writer.Calls);
        }
    }

    [TestMethod]
    public async Task HeadHasNoBodyAndCustomMapperMustReturnAnActiveError()
    {
        var writer = new Writer();
        var context = Context();
        context.Request.Method = "HEAD";
        Assert.IsTrue(await Handler(writer, 404).TryHandleAsync(context, new Exception(), default));
        Assert.AreEqual(404, context.Response.StatusCode);
        Assert.AreEqual(0, context.Response.Body.Length);
        Assert.AreEqual(0, writer.Calls);
        foreach (int status in new[] {103, 200, 204, 205, 304, 418, 499, 510, 999})
        {
            context = Context();
            await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() =>
                Handler(writer, status).TryHandleAsync(context, new Exception(), default).AsTask());
            Assert.AreEqual(200, context.Response.StatusCode);
        }
    }

    [TestMethod]
    public async Task WriterReceivesMatchingStatusAndOriginalException()
    {
        var exception = new Exception("secret");
        var context = Context();
        var writer = new Writer
        {
            Write = details =>
            {
                Assert.AreSame(exception, details.Exception);
                Assert.AreSame(context, details.HttpContext);
                Assert.AreEqual(500, details.ProblemDetails.Status);
                return ValueTask.FromResult(true);
            }
        };
        Assert.IsTrue(await Handler(writer, null).TryHandleAsync(context, exception, default));
        Assert.AreEqual(500, context.Response.StatusCode);
    }

    [TestMethod]
    public async Task UnsupportedWriterFallsBackToSafeProblemJson()
    {
        using var provider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var context = Context();
        context.RequestServices = provider;
        context.Request.Path = "/orders";
        context.Request.QueryString = new("?token=secret");
        context.TraceIdentifier = "trace";
        var writer = new Writer {Write = details => ValueTask.FromResult(false)};
        var handler = new GlobalExceptionHandler(new ProblemDetailsExceptionMapper(
            OptionsFactory.Create(new WebApiExceptionHandlingOptions())), writer, NullLogger<GlobalExceptionHandler>.Instance);
        Assert.IsTrue(await handler.TryHandleAsync(context, new Exception("secret"), default));
        Assert.AreEqual("application/problem+json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.AreEqual(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.AreEqual("/orders", json.RootElement.GetProperty("instance").GetString());
        Assert.AreEqual("trace", json.RootElement.GetProperty("traceId").GetString());
        Assert.IsFalse(json.RootElement.GetRawText().Contains("secret", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task CancellationDuringWriteIsLinkedAndOriginalTokenIsRestored()
    {
        foreach (bool cancelCaller in new[] {true, false})
        {
            using var caller = new CancellationTokenSource();
            using var request = new CancellationTokenSource();
            var context = Context();
            context.RequestAborted = request.Token;
            var writer = new Writer
            {
                Write = details =>
                {
                    (cancelCaller ? caller : request).Cancel();
                    details.HttpContext.RequestAborted.ThrowIfCancellationRequested();
                    return ValueTask.FromResult(true);
                }
            };
            await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
                Handler(writer).TryHandleAsync(context, new Exception(), caller.Token).AsTask());
            Assert.AreEqual(request.Token, context.RequestAborted);
        }
    }

    [TestMethod]
    public async Task WriterAndMapperFailuresPropagate()
    {
        var context = Context();
        using var source = new CancellationTokenSource();
        context.RequestAborted = source.Token;
        var failure = new InvalidOperationException("writer failed");
        var writer = new Writer {Write = details => throw failure};
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            Handler(writer).TryHandleAsync(context, new Exception(), default).AsTask()));
        Assert.AreEqual(source.Token, context.RequestAborted);
        var mapper = new Mock<IProblemDetailsExceptionMapper>();
        mapper.Setup(x => x.Map(It.IsAny<HttpContext>(), It.IsAny<Exception>())).Throws(failure);
        var handler = new GlobalExceptionHandler(mapper.Object, writer, NullLogger<GlobalExceptionHandler>.Instance);
        Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            handler.TryHandleAsync(context, new Exception(), default).AsTask()));
    }

    private sealed class Writer : IProblemDetailsService
    {
        public int Calls { get; private set; }
        public Func<ProblemDetailsContext, ValueTask<bool>> Write { get; init; } = details => ValueTask.FromResult(true);
        public ValueTask WriteAsync(ProblemDetailsContext context) => throw new AssertFailedException("Use TryWriteAsync.");
        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Calls++;
            return Write(context);
        }
    }
}
