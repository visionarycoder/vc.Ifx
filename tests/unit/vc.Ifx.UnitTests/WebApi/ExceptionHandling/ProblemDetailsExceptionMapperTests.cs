using Microsoft.AspNetCore.Http;
using VisionaryCoder.Framework.WebApi.ExceptionHandling;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace VisionaryCoder.Framework.Tests.WebApi.ExceptionHandling;

[TestClass]
public sealed class ProblemDetailsExceptionMapperTests
{
    [TestMethod]
    public void Map_WithUnknownException_ShouldCreateDefaultProblemDetails()
    {
        var options = new WebApiExceptionHandlingOptions();
        var mapper = new ProblemDetailsExceptionMapper(OptionsFactory.Create(options));
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "trace-123"
        };
        httpContext.Request.Path = "/orders/42";

        var problemDetails = mapper.Map(httpContext, new Exception("hidden"));

        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
        problemDetails.Title.Should().Be("An unexpected error occurred.");
        problemDetails.Type.Should().Be("https://httpstatuses.com/500");
        problemDetails.Instance.Should().Be("/orders/42");
        problemDetails.Detail.Should().BeNull();
        problemDetails.Extensions["traceId"].Should().Be("trace-123");
        problemDetails.Extensions.Should().NotContainKey("exceptionType");
    }

    [TestMethod]
    public void Map_WithConfiguredExceptionDetails_ShouldIncludeSafeDebugDetails()
    {
        var options = new WebApiExceptionHandlingOptions
        {
            IncludeExceptionDetails = true
        };
        options.Map<ArgumentException>(StatusCodes.Status400BadRequest, "The request is invalid.", "https://httpstatuses.com/400");
        var mapper = new ProblemDetailsExceptionMapper(OptionsFactory.Create(options));
        var httpContext = new DefaultHttpContext();

        var problemDetails = mapper.Map(httpContext, new ArgumentException("bad input"));

        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("The request is invalid.");
        problemDetails.Detail.Should().Be("bad input");
        problemDetails.Extensions["exceptionType"].Should().Be(typeof(ArgumentException).FullName);
    }

    [TestMethod]
    public void Map_WithCustomBaseExceptionMapping_ShouldApplyMappingToDerivedException()
    {
        var options = new WebApiExceptionHandlingOptions();
        options.Map<InvalidOperationException>(StatusCodes.Status422UnprocessableEntity, "Cannot process request.", "https://httpstatuses.com/422");
        var mapper = new ProblemDetailsExceptionMapper(OptionsFactory.Create(options));
        var httpContext = new DefaultHttpContext();

        var problemDetails = mapper.Map(httpContext, new CustomInvalidOperationException());

        problemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails.Title.Should().Be("Cannot process request.");
        problemDetails.Type.Should().Be("https://httpstatuses.com/422");
    }

    private sealed class CustomInvalidOperationException : InvalidOperationException;
}
