using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using vc.Ifx.Generators;

namespace VisionaryCoder.Framework.Tests.Generators.Implementation;

[TestClass]
public sealed class EndpointHostingTests
{
    [TestMethod]
    public async Task GeneratedConsumerBindsRequestsServicesCancellationAndResponseMetadata()
    {
        var result = GeneratorHarness.Run("""
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc;
            using vc.Ifx.Generators.Abstractions.Attributes;
            public static class Handlers
            {
                [GenerateEndpoint("/items/{id:int}", "GET", Name="ReadItem", Summary="Read an item", Description="Item detail", Tags=new[]{"Items"}, AllowAnonymous=true)]
                public static string Read(int id, [FromQuery] string label, [FromHeader(Name="X-Prefix")] string prefix, [FromServices] Func<string> suffix, CancellationToken cancellationToken)
                    => $"{id}:{label}:{prefix}:{suffix()}:{cancellationToken.CanBeCanceled}";

                [GenerateEndpoint("/items", "POST", Name="CreateItem")]
                public static Task<IResult> Create([FromBody] Payload payload) => Task.FromResult(Results.Ok(payload));

                [GenerateEndpoint("/private", "GET", Name="Private", AuthorizationPolicy="Read", ExcludeFromDescription=true)]
                public static string Private() => "secret";

                [GenerateEndpoint("/authenticated", "GET", RequireAuthorization=true)]
                public static string Authenticated() => "authenticated";

                [GenerateEndpoint("/failure", "GET")]
                public static string Failure() => throw new InvalidOperationException("private exception detail");
            }
            public sealed record Payload(string Value);
            """, new MinimalEndpointGenerator());
        var assembly = result.Emit();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<Func<string>>(() => "service");
        builder.Services.AddAuthentication("Fixture").AddScheme<AuthenticationSchemeOptions, FixtureAuthenticationHandler>("Fixture", options => { });
        builder.Services.AddAuthorization(options => options.AddPolicy("Read", policy => policy.RequireClaim("permission", "read")));
        await using var app = builder.Build();
        app.UseExceptionHandler(error => error.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("safe error");
        }));
        app.UseAuthentication();
        app.UseAuthorization();
        var map = assembly.GetType("vc.Ifx.Generated.IfxEndpointRouteBuilderExtensions")!.GetMethod("MapGeneratedIfxEndpoints")!;
        map.Invoke(null, new object[] { app }).Should().BeSameAs(app);
        Action nullMap = () => map.Invoke(null, new object?[] { null });
        nullMap.Should().Throw<System.Reflection.TargetInvocationException>().WithInnerException<ArgumentNullException>();
        await app.StartAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Prefix", "header");
        (await client.GetStringAsync("/items/7?label=query")).Should().Be("7:query:header:service:True");
        (await client.GetAsync("/items/not-an-int?label=query")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.GetAsync("/items/7")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var created = await client.PostAsJsonAsync("/items", new { Value = "body" });
        (await created.Content.ReadFromJsonAsync<Dictionary<string, string>>())!["value"].Should().Be("body");
        (await client.GetAsync("/private")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.GetAsync("/authenticated")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        client.DefaultRequestHeaders.Add("X-FixtureUser", "forbidden");
        (await client.GetAsync("/private")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        client.DefaultRequestHeaders.Remove("X-FixtureUser");
        client.DefaultRequestHeaders.Add("X-FixtureUser", "reader");
        (await client.GetStringAsync("/private")).Should().Be("secret");
        (await client.GetStringAsync("/authenticated")).Should().Be("authenticated");
        var failure = await client.GetAsync("/failure");
        failure.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        (await failure.Content.ReadAsStringAsync()).Should().Be("safe error");

        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).ToArray();
        var read = endpoints.Single(endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == "ReadItem");
        read.Metadata.GetMetadata<IEndpointSummaryMetadata>()!.Summary.Should().Be("Read an item");
        read.Metadata.GetMetadata<IEndpointDescriptionMetadata>()!.Description.Should().Be("Item detail");
        read.Metadata.GetMetadata<ITagsMetadata>()!.Tags.Should().Equal("Items");
        read.Metadata.GetMetadata<IAllowAnonymous>().Should().NotBeNull();
        var secured = endpoints.Single(endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == "Private");
        secured.Metadata.GetMetadata<IAuthorizeData>()!.Policy.Should().Be("Read");
        secured.Metadata.GetMetadata<IExcludeFromDescriptionMetadata>()!.ExcludeFromDescription.Should().BeTrue();
    }

    [TestMethod]
    public async Task AuthorizedGroupRetainsTypedResultsAndExplicitAnonymousOverride()
    {
        var result = GeneratorHarness.Run("""
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Http.HttpResults;
            using vc.Ifx.Generators.Abstractions.Attributes;
            public static class Handlers
            {
                [GenerateEndpoint("/public", "GET", AllowAnonymous=true)]
                public static Ok<string> Public() => TypedResults.Ok("public");
                [GenerateEndpoint("/private", "GET")]
                public static string Private() => "private";
            }
            """, new MinimalEndpointGenerator());
        var assembly = result.Emit();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthentication("Fixture").AddScheme<AuthenticationSchemeOptions, FixtureAuthenticationHandler>("Fixture", options => { });
        builder.Services.AddAuthorization();
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        var group = app.MapGroup("/group").RequireAuthorization();
        assembly.GetType("vc.Ifx.Generated.IfxEndpointRouteBuilderExtensions")!.GetMethod("MapGeneratedIfxEndpoints")!.Invoke(null, new object[] { group }).Should().BeSameAs(group);
        await app.StartAsync();
        using var client = app.GetTestClient();
        (await client.GetFromJsonAsync<string>("/group/public")).Should().Be("public");
        (await client.GetAsync("/group/private")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        client.DefaultRequestHeaders.Add("X-FixtureUser", "reader");
        (await client.GetStringAsync("/group/private")).Should().Be("private");
        var endpoint = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .Single(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null);
        endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>().Should().Contain(metadata => metadata.StatusCode == 200 && metadata.Type == typeof(string));
    }

    [TestMethod]
    public async Task RequestCancellationReachesOriginalHandlerThroughServiceBinding()
    {
        var result = GeneratorHarness.Run("""
            using System; using System.Threading; using System.Threading.Tasks;
            using Microsoft.AspNetCore.Mvc;
            using vc.Ifx.Generators.Abstractions.Attributes;
            public static class Handlers
            {
                [GenerateEndpoint("/wait", "GET")]
                public static Task<string> Wait([FromServices] Func<CancellationToken, Task<string>> operation, CancellationToken cancellationToken) => operation(cancellationToken);
            }
            """, new MinimalEndpointGenerator());
        var assembly = result.Emit();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<Func<CancellationToken, Task<string>>>(async token =>
        {
            using var registration = token.Register(() => cancelled.TrySetResult());
            entered.TrySetResult();
            await Task.Delay(Timeout.Infinite, token);
            return "unreachable";
        });
        await using var app = builder.Build();
        assembly.GetType("vc.Ifx.Generated.IfxEndpointRouteBuilderExtensions")!.GetMethod("MapGeneratedIfxEndpoints")!.Invoke(null, new object[] { app });
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

    private sealed class FixtureAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-FixtureUser", out var name)) { return Task.FromResult(AuthenticateResult.NoResult()); }
            var identity = new ClaimsIdentity(new[] { new Claim("permission", name == "reader" ? "read" : "write") }, "Fixture");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Fixture")));
        }
    }
}
