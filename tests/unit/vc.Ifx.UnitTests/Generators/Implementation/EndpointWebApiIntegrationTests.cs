using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using vc.Ifx.Generators;
using VisionaryCoder.Framework.WebApi.DependencyInjection;
using VisionaryCoder.Framework.WebApi.Responses;

namespace VisionaryCoder.Framework.Tests.Generators.Implementation;

[TestClass]
public sealed class EndpointWebApiIntegrationTests
{
    private const string Consumer = """
        using System;
        using System.Collections.Generic;
        using Microsoft.AspNetCore.Http;
        using vc.Ifx.Generators.Abstractions.Attributes;
        using VisionaryCoder.Framework.WebApi.Responses;
        public static class Handlers
        {
            [GenerateEndpoint("/bad", "GET")]
            public static string Bad() => throw new ArgumentException("private diagnostic");
            [GenerateEndpoint("/missing", "GET")]
            public static string Missing() => throw new KeyNotFoundException("private diagnostic");
            [GenerateEndpoint("/unexpected", "GET")]
            public static string Unexpected() => throw new InvalidOperationException("private diagnostic");
            [GenerateEndpoint("/mapped", "GET")]
            public static string Mapped() => throw new FormatException("private diagnostic");
            [GenerateEndpoint("/bad", "HEAD")]
            public static string Head() => throw new ArgumentException("private diagnostic");
            [GenerateEndpoint("/catalog", "GET")]
            public static IResult Catalog()
            {
                var entry = HttpResponseCatalog.UnprocessableContent;
                return Results.Problem(entry.SafeDetail, statusCode: entry.StatusCode, title: entry.DefaultTitle, type: entry.Type);
            }
        }
        """;

    [TestMethod]
    [DataRow("/bad", 400)]
    [DataRow("/missing", 404)]
    [DataRow("/unexpected", 500)]
    [DataRow("/mapped", 422)]
    public async Task GeneratedExceptionsUseWebApiCatalogWithoutLeakingDetails(string route, int status)
    {
        await using var app = CreateHost(Consumer);
        await app.StartAsync();
        using var client = app.GetTestClient();
        using var response = await client.GetAsync(route + "?secret=do-not-disclose");
        ((int)response.StatusCode).Should().Be(status);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("private diagnostic").And.NotContain("do-not-disclose").And.NotContain("exceptionType");
        using var json = JsonDocument.Parse(body);
        AssertCatalog(json.RootElement, status);
        json.RootElement.GetProperty("instance").GetString().Should().Be(route);
        json.RootElement.GetProperty("traceId").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public async Task GeneratedHeadAndExplicitCatalogResultPreserveHttpSemantics()
    {
        await using var app = CreateHost(Consumer);
        await app.StartAsync();
        using var client = app.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Head, "/bad");
        using var head = await client.SendAsync(request);
        head.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await head.Content.ReadAsByteArrayAsync()).Should().BeEmpty();
        using var response = await client.GetAsync("/catalog");
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertCatalog(json.RootElement, 422);
    }

    [TestMethod]
    public async Task WebApiMiddlewareDoesNotConvertGeneratedRequestCancellationToProblemDetails()
    {
        const string source = """
            using System; using System.Threading; using System.Threading.Tasks;
            using Microsoft.AspNetCore.Mvc;
            using vc.Ifx.Generators.Abstractions.Attributes;
            public static class Handlers
            {
                [GenerateEndpoint("/wait", "GET")]
                public static Task<string> Wait([FromServices] Func<CancellationToken, Task<string>> operation, CancellationToken cancellationToken) => operation(cancellationToken);
            }
            """;
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var app = CreateHost(source, services => services.AddSingleton<Func<CancellationToken, Task<string>>>(async token =>
        {
            using var registration = token.Register(() => cancelled.TrySetResult());
            entered.TrySetResult();
            await Task.Delay(Timeout.Infinite, token);
            return "unreachable";
        }));
        await app.StartAsync();
        using var client = app.GetTestClient();
        using var cancellation = new CancellationTokenSource();
        var request = client.GetAsync("/wait", cancellation.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Func<Task> complete = async () => await request;
        await complete.Should().ThrowAsync<OperationCanceledException>();
    }

    private static WebApplication CreateHost(string source, Action<IServiceCollection>? configure = null)
    {
        var assembly = GeneratorHarness.Run(source, new MinimalEndpointGenerator()).Emit();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        configure?.Invoke(builder.Services);
        builder.Services.AddIfxWebApi(options => options.Map<FormatException>(
            HttpResponseCatalog.UnprocessableContent.StatusCode, HttpResponseCatalog.UnprocessableContent.DefaultTitle));
        var app = builder.Build();
        app.UseIfxWebApiExceptionHandling();
        assembly.GetType("vc.Ifx.Generated.IfxEndpointRouteBuilderExtensions")!.GetMethod("MapGeneratedIfxEndpoints")!
            .Invoke(null, new object[] { app }).Should().BeSameAs(app);
        return app;
    }

    private static void AssertCatalog(JsonElement problem, int status)
    {
        var entry = HttpResponseCatalog.Get(status);
        problem.GetProperty("status").GetInt32().Should().Be(entry.StatusCode);
        problem.GetProperty("title").GetString().Should().Be(entry.DefaultTitle);
        problem.GetProperty("detail").GetString().Should().Be(entry.SafeDetail);
        problem.GetProperty("type").GetString().Should().Be(entry.Type);
    }
}
