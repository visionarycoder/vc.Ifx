using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VisionaryCoder.Framework.WebApi.DependencyInjection;
using VisionaryCoder.Framework.WebApi.ExceptionHandling;
using VisionaryCoder.Framework.WebApi.Responses;

namespace VisionaryCoder.Framework.Tests.WebApi;

[TestClass]
public sealed class HostingPipelineContractTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task AbortedCancellationAndIoFailuresFinishWith499BeforeMapping(bool ioFailure)
    {
        var mapper = new ObservingMapper((context, error) => throw new AssertFailedException("Aborted requests must not reach mapping."));
        await using var host = CreateHost(mapper);
        using var scope = host.Services.CreateScope();
        using var body = new MemoryStream();
        using var aborted = new CancellationTokenSource();
        var context = CreateContext(scope.ServiceProvider, body);
        context.RequestAborted = aborted.Token;
        Exception error = ioFailure ? new IOException("disconnected") : new OperationCanceledException(aborted.Token);
        var pipeline = CreatePipeline(host, call => { aborted.Cancel(); throw error; });

        await pipeline(context);

        Assert.AreEqual(499, context.Response.StatusCode);
        Assert.AreEqual(0, mapper.Calls);
        Assert.AreEqual(0L, body.Length);
        Assert.IsNull(context.Response.ContentType);
        Assert.AreEqual(aborted.Token, context.RequestAborted);
        Assert.IsFalse(HttpResponseCatalog.TryGet(499, out var definition));
    }

    [TestMethod]
    public async Task UncanceledOperationCanceledExceptionPropagatesWithoutMappingOrBody()
    {
        var mapper = new ObservingMapper((context, error) => throw new AssertFailedException("The direct handler propagates cancellation before mapping."));
        await using var host = CreateHost(mapper);
        using var scope = host.Services.CreateScope();
        using var body = new MemoryStream();
        var context = CreateContext(scope.ServiceProvider, body);
        var error = new OperationCanceledException("not request-aborted");
        var pipeline = CreatePipeline(host, call => throw error);

        var actual = await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => pipeline(context));

        Assert.AreSame(error, actual);
        Assert.IsFalse(context.RequestAborted.IsCancellationRequested);
        Assert.AreEqual(0, mapper.Calls);
        Assert.AreEqual(0L, body.Length);
    }

    [TestMethod]
    [DataRow(401, "WWW-Authenticate", "Bearer realm=\"configured-api\"")]
    [DataRow(405, "Allow", "GET, HEAD")]
    [DataRow(407, "Proxy-Authenticate", "Basic realm=\"configured-proxy\"")]
    [DataRow(426, "Upgrade", "configured-protocol/2")]
    public async Task PreExceptionRequiredHeadersAreClearedAndDefaultFallbackIs500(int status, string header, string value)
    {
        var defaultMapper = CreateDefaultMapper(status);
        var mapper = new ObservingMapper((context, error) =>
        {
            AssertReset(context, header);
            return defaultMapper.Map(context, error);
        });
        await using var host = CreateHost(mapper);
        using var scope = host.Services.CreateScope();
        using var body = new MemoryStream();
        var context = CreateContext(scope.ServiceProvider, body);
        var pipeline = CreatePipeline(host, call => ThrowAfterSettingHeaders(call, status, header, value));

        await pipeline(context);

        Assert.AreEqual(1, mapper.Calls);
        Assert.AreEqual(500, context.Response.StatusCode);
        Assert.IsFalse(context.Response.Headers.ContainsKey(header));
        Assert.IsFalse(context.Response.Headers.ContainsKey("X-Before-Exception"));
        using var json = await ReadProblem(context);
        Assert.AreEqual(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.IsFalse(json.RootElement.ToString().Contains("private failure", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow(401, "WWW-Authenticate", "Bearer realm=\"configured-api\"")]
    [DataRow(405, "Allow", "GET, HEAD")]
    [DataRow(407, "Proxy-Authenticate", "Basic realm=\"configured-proxy\"")]
    [DataRow(426, "Upgrade", "configured-protocol/2")]
    public async Task TrustedMapperSuppliesRequiredHeadersAfterResetAndProducesMatchingProblem(int status, string header, string configuredValue)
    {
        var defaultMapper = CreateDefaultMapper(status);
        var mapper = new ObservingMapper((context, error) =>
        {
            AssertReset(context, header);
            context.Response.Headers[header] = configuredValue;
            if (status == 426) context.Response.Headers.Connection = "Upgrade";
            return defaultMapper.Map(context, error);
        });
        await using var host = CreateHost(mapper);
        using var scope = host.Services.CreateScope();
        using var body = new MemoryStream();
        var context = CreateContext(scope.ServiceProvider, body);
        var pipeline = CreatePipeline(host, call => ThrowAfterSettingHeaders(call, status, header, "stale-value"));

        await pipeline(context);

        Assert.AreEqual(1, mapper.Calls);
        Assert.AreEqual(status, context.Response.StatusCode);
        Assert.AreEqual(configuredValue, context.Response.Headers[header].ToString());
        if (status == 426) Assert.AreEqual("Upgrade", context.Response.Headers.Connection.ToString());
        Assert.IsFalse(context.Response.Headers.ContainsKey("X-Before-Exception"));
        using var json = await ReadProblem(context);
        var definition = HttpResponseCatalog.Get(status);
        Assert.AreEqual(status, json.RootElement.GetProperty("status").GetInt32());
        Assert.AreEqual(definition.DefaultTitle, json.RootElement.GetProperty("title").GetString());
        Assert.AreEqual(definition.Type, json.RootElement.GetProperty("type").GetString());
        Assert.AreEqual(definition.SafeDetail, json.RootElement.GetProperty("detail").GetString());
        Assert.AreEqual("/resource", json.RootElement.GetProperty("instance").GetString());
        Assert.IsFalse(json.RootElement.ToString().Contains("private failure", StringComparison.Ordinal));
    }

    private static WebApplication CreateHost(IProblemDetailsExceptionMapper mapper)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = Environments.Production, Args = [] });
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(mapper);
        builder.Services.AddIfxWebApi();
        var host = builder.Build();
        Assert.AreEqual(Environments.Production, host.Environment.EnvironmentName);
        return host;
    }

    private static RequestDelegate CreatePipeline(WebApplication host, RequestDelegate terminal)
    {
        var app = new ApplicationBuilder(host.Services);
        app.UseIfxWebApiExceptionHandling();
        app.Run(terminal);
        return app.Build();
    }

    private static DefaultHttpContext CreateContext(IServiceProvider services, Stream body)
    {
        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Method = "GET";
        context.Request.Path = "/resource";
        context.Request.Headers.Accept = "application/problem+json";
        context.Response.Body = body;
        return context;
    }

    private static ProblemDetailsExceptionMapper CreateDefaultMapper(int status)
    {
        var options = new WebApiExceptionHandlingOptions();
        options.Map<InvalidOperationException>(status, HttpResponseCatalog.Get(status).DefaultTitle);
        return new ProblemDetailsExceptionMapper(Microsoft.Extensions.Options.Options.Create(options));
    }

    private static Task ThrowAfterSettingHeaders(HttpContext context, int status, string header, string value)
    {
        context.Response.StatusCode = status;
        context.Response.Headers[header] = value;
        context.Response.Headers["X-Before-Exception"] = "stale";
        context.Response.ContentLength = 123;
        throw new InvalidOperationException("private failure");
    }

    private static void AssertReset(HttpContext context, string header)
    {
        Assert.AreEqual(500, context.Response.StatusCode);
        Assert.IsFalse(context.Response.Headers.ContainsKey(header));
        Assert.IsFalse(context.Response.Headers.ContainsKey("X-Before-Exception"));
        Assert.IsNull(context.Response.ContentLength);
    }

    private static async Task<JsonDocument> ReadProblem(HttpContext context)
    {
        Assert.IsNotNull(context.Response.ContentType);
        StringAssert.StartsWith(context.Response.ContentType, "application/problem+json");
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }

    private sealed class ObservingMapper(Func<HttpContext, Exception, ProblemDetails> map) : IProblemDetailsExceptionMapper
    {
        public int Calls { get; private set; }
        public ProblemDetails Map(HttpContext context, Exception error) { Calls++; return map(context, error); }
    }
}
