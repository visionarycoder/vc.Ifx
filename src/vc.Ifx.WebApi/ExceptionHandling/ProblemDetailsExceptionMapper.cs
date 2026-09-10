using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace VisionaryCoder.Framework.WebApi.ExceptionHandling;

public sealed class ProblemDetailsExceptionMapper(IOptions<WebApiExceptionHandlingOptions> options) : IProblemDetailsExceptionMapper
{
    public ProblemDetails Map(HttpContext httpContext, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        WebApiExceptionHandlingOptions handlingOptions = options.Value;
        ProblemDetailsExceptionMapping mapping = handlingOptions.FindMapping(exception)
            ?? new ProblemDetailsExceptionMapping(
                StatusCodes.Status500InternalServerError,
                handlingOptions.DefaultTitle,
                handlingOptions.DefaultType);

        var problemDetails = new ProblemDetails
        {
            Status = mapping.StatusCode,
            Title = mapping.Title,
            Type = mapping.Type,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (handlingOptions.IncludeExceptionDetails)
        {
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
        }

        return problemDetails;
    }
}
