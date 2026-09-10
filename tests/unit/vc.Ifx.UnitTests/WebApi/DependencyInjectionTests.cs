using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using VisionaryCoder.Framework.WebApi.DependencyInjection;
using VisionaryCoder.Framework.WebApi.ExceptionHandling;
using VisionaryCoder.Framework.WebApi.Resilience;

namespace VisionaryCoder.Framework.Tests.WebApi;

[TestClass]
public sealed class DependencyInjectionTests
{
    [TestMethod]
    public async Task DefaultRegistrationsResolveAndProduceProblemJson()
    {
        var services = new ServiceCollection().AddLogging();
        Assert.AreSame(services, services.AddIfxWebApi());
        using var provider = services.BuildServiceProvider();
        Assert.IsInstanceOfType<ProblemDetailsExceptionMapper>(provider.GetRequiredService<IProblemDetailsExceptionMapper>());
        Assert.IsInstanceOfType<WebApiTransientFailureClassifier>(provider.GetRequiredService<IWebApiTransientFailureClassifier>());
        Assert.IsInstanceOfType<WebApiResiliencePipelineFactory>(provider.GetRequiredService<IWebApiResiliencePipelineFactory>());
        Assert.AreSame(TimeProvider.System, provider.GetRequiredService<TimeProvider>());
        Assert.IsNotNull(provider.GetRequiredService<IWebApiResiliencePipelineFactory>().CreatePipeline());
        var context = new DefaultHttpContext {RequestServices = provider};
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();
        var handler = provider.GetServices<IExceptionHandler>().Single();
        Assert.IsTrue(await handler.TryHandleAsync(context, new KeyNotFoundException("secret"), default));
        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.AreEqual(404, json.RootElement.GetProperty("status").GetInt32());
        Assert.AreEqual("Not Found", json.RootElement.GetProperty("title").GetString());
        var app = new ApplicationBuilder(provider);
        Assert.AreSame(app, app.UseIfxWebApiExceptionHandling());
    }

    [TestMethod]
    public void ApplicationReplacementsAndConfigurationArePreserved()
    {
        var mapper = Mock.Of<IProblemDetailsExceptionMapper>();
        var classifier = Mock.Of<IWebApiTransientFailureClassifier>();
        var factory = Mock.Of<IWebApiResiliencePipelineFactory>();
        var clock = Mock.Of<TimeProvider>();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(mapper).AddSingleton(classifier).AddSingleton(factory).AddSingleton(clock);
        services.AddIfxWebApi(
            options => {options.IncludeExceptionDetails = true; options.Map<ArgumentException>(422, "Invalid content");},
            options => {options.EnableTimeout = false; options.UseJitter = false;});
        using var provider = services.BuildServiceProvider();
        Assert.AreSame(mapper, provider.GetRequiredService<IProblemDetailsExceptionMapper>());
        Assert.AreSame(classifier, provider.GetRequiredService<IWebApiTransientFailureClassifier>());
        Assert.AreSame(factory, provider.GetRequiredService<IWebApiResiliencePipelineFactory>());
        Assert.AreSame(clock, provider.GetRequiredService<TimeProvider>());
        var exceptionOptions = provider.GetRequiredService<IOptions<WebApiExceptionHandlingOptions>>().Value;
        Assert.IsTrue(exceptionOptions.IncludeExceptionDetails);
        Assert.AreEqual(422, exceptionOptions.ExceptionMappings[typeof(ArgumentException)].StatusCode);
        Assert.IsFalse(provider.GetRequiredService<IOptions<WebApiResilienceOptions>>().Value.EnableTimeout);
    }

    [TestMethod]
    public async Task InvalidOptionsFailAtHostStartup()
    {
        foreach (bool invalidRetry in new[] {true, false})
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddIfxWebApi(
                options => {if (!invalidRetry) options.DefaultTitle = "";},
                options => {if (invalidRetry) options.EnableRetry = true;});
            using var host = builder.Build();
            await Assert.ThrowsAsync<ArgumentException>(() => host.StartAsync());
        }
    }

    [TestMethod]
    public void RegistrationAndMiddlewareRejectNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => WebApiServiceCollectionExtensions.AddIfxWebApi(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => WebApiServiceCollectionExtensions.UseIfxWebApiExceptionHandling(null!));
    }
}
