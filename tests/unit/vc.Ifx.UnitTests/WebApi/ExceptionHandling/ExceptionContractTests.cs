using Microsoft.AspNetCore.Http;
using VisionaryCoder.Framework.WebApi.ExceptionHandling;
using VisionaryCoder.Framework.WebApi.Responses;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace VisionaryCoder.Framework.Tests.WebApi.ExceptionHandling;

[TestClass]
public sealed class ExceptionContractTests
{
    private static ProblemDetailsExceptionMapper Mapper(WebApiExceptionHandlingOptions? options = null) =>
        new(OptionsFactory.Create(options ?? new()));

    [TestMethod]
    public void DefaultMappingsAndSpecificOverridesUseSafeCatalogDetails()
    {
        (Exception Exception, int Code)[] cases =
        [
            (new ArgumentException("secret"), 400), (new ArgumentNullException("secret"), 400),
            (new UnauthorizedAccessException("secret"), 403), (new KeyNotFoundException("secret"), 404),
            (new Exception("secret"), 500), (new InvalidOperationException("secret"), 500),
            (new TimeoutException("secret"), 500)
        ];
        foreach (var item in cases)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/resource";
            context.Request.QueryString = new("?password=secret");
            var result = Mapper().Map(context, item.Exception);
            Assert.AreEqual(item.Code, result.Status);
            Assert.AreEqual(HttpResponseCatalog.Get(item.Code).DefaultTitle, result.Title);
            Assert.AreEqual(HttpResponseCatalog.Get(item.Code).Type, result.Type);
            Assert.AreEqual(HttpResponseCatalog.Get(item.Code).SafeDetail, result.Detail);
            Assert.AreEqual("/resource", result.Instance);
            Assert.IsFalse(result.Extensions.ContainsKey("exceptionType"));
        }
        var options = new WebApiExceptionHandlingOptions();
        options.Map<ArgumentNullException>(422, "Missing content");
        Assert.AreEqual(422, Mapper(options).Map(new DefaultHttpContext(), new ArgumentNullException()).Status);
        Assert.AreEqual(400, Mapper(options).Map(new DefaultHttpContext(), new ArgumentException()).Status);
        options.ExceptionMappings.Clear();
        options.DefaultTitle = "Custom failure";
        options.DefaultType = "urn:problem:custom";
        var custom = Mapper(options).Map(new DefaultHttpContext(), new ArgumentException());
        Assert.AreEqual("Custom failure", custom.Title);
        Assert.AreEqual("urn:problem:custom", custom.Type);
    }

    [TestMethod]
    public void MapperRejectsNullInputsAndPreservesCancellation()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ProblemDetailsExceptionMapper(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => Mapper().Map(null!, new Exception()));
        Assert.ThrowsExactly<ArgumentNullException>(() => Mapper().Map(new DefaultHttpContext(), null!));
        using var source = new CancellationTokenSource();
        source.Cancel();
        Assert.ThrowsExactly<OperationCanceledException>(() =>
            Mapper().Map(new DefaultHttpContext {RequestAborted = source.Token}, new Exception()));
        var canceled = new TaskCanceledException("private");
        var options = new WebApiExceptionHandlingOptions();
        options.Map<OperationCanceledException>(503, "Overridden");
        var thrown = Assert.ThrowsExactly<TaskCanceledException>(() => Mapper(options).Map(new DefaultHttpContext(), canceled));
        Assert.AreSame(canceled, thrown);
    }

    [TestMethod]
    public void EveryCatalogStatusIsValidatedForExceptionMapping()
    {
        foreach (var entry in HttpResponseCatalog.All)
        {
            var options = new WebApiExceptionHandlingOptions();
            if (entry.StatusCode < 400 || entry.StatusCode is 418 or 510)
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => options.Map<Exception>(entry.StatusCode, "Error"));
            else
            {
                options.Map<Exception>(entry.StatusCode, entry.DefaultTitle);
                Assert.AreEqual(entry.StatusCode, Mapper(options).Map(new DefaultHttpContext(), new Exception()).Status);
            }
        }
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new WebApiExceptionHandlingOptions().Map<Exception>(999, "Error"));
    }

    [TestMethod]
    public void InvalidOptionsIncludingDirectDictionaryChangesFailBeforeMapping()
    {
        Action<WebApiExceptionHandlingOptions>[] mutations =
        [
            x => x.DefaultTitle = null!, x => x.DefaultTitle = " ", x => x.DefaultType = "relative",
            x => x.DefaultType = null!, x => x.ExceptionMappings[typeof(string)] = new(500, "Error"),
            x => x.ExceptionMappings[typeof(Exception)] = null!,
            x => x.ExceptionMappings[typeof(Exception)] = new(204, "Error"),
            x => x.ExceptionMappings[typeof(Exception)] = new(400, null!),
            x => x.ExceptionMappings[typeof(Exception)] = new(400, " "),
            x => x.ExceptionMappings[typeof(Exception)] = new(400, "Error", "relative"),
            x => x.ExceptionMappings[typeof(Exception)] = new(400, "Error", "")
        ];
        foreach (var mutate in mutations)
        {
            var options = new WebApiExceptionHandlingOptions();
            mutate(options);
            Assert.Throws<ArgumentException>(() => Mapper(options).Map(new DefaultHttpContext(), new Exception()));
        }
        Assert.ThrowsExactly<ArgumentException>(() => new WebApiExceptionHandlingOptions().Map<Exception>(400, " "));
        Assert.ThrowsExactly<ArgumentException>(() => new WebApiExceptionHandlingOptions().Map<Exception>(400, "Error", "relative"));
    }

    [TestMethod]
    public void MappingRecordRetainsValueSemantics()
    {
        var mapping = new ProblemDetailsExceptionMapping(409, "Conflict", "urn:conflict");
        var (status, title, type) = mapping;
        Assert.AreEqual(409, status);
        Assert.AreEqual("Conflict", title);
        Assert.AreEqual("urn:conflict", type);
        Assert.AreEqual(mapping, mapping with { });
        Assert.AreEqual(mapping.GetHashCode(), (mapping with { }).GetHashCode());
        Assert.AreNotEqual(mapping, mapping with {StatusCode = 422});
        Assert.IsTrue(mapping.ToString().Contains("Conflict", StringComparison.Ordinal));
    }
}
