using Microsoft.AspNetCore.Http;
using VisionaryCoder.Framework.WebApi.ExceptionHandling;

namespace VisionaryCoder.Framework.Tests.WebApi.ExceptionHandling;

[TestClass]
public sealed class WebApiExceptionHandlingOptionsTests
{
    [TestMethod]
    public void Map_ShouldRegisterExceptionMappingAndReturnOptions()
    {
        var options = new WebApiExceptionHandlingOptions();

        WebApiExceptionHandlingOptions returned = options.Map<TimeoutException>(
            StatusCodes.Status503ServiceUnavailable,
            "Service unavailable.",
            "https://httpstatuses.com/503");

        returned.Should().BeSameAs(options);
        options.ExceptionMappings[typeof(TimeoutException)].Should().Be(
            new ProblemDetailsExceptionMapping(
                StatusCodes.Status503ServiceUnavailable,
                "Service unavailable.",
                "https://httpstatuses.com/503"));
    }
}
