using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Runtime.ExceptionServices;
using VisionaryCoder.Framework.WebApi.Responses;

namespace VisionaryCoder.Framework.WebApi.ExceptionHandling;

/// <summary>Maps known exceptions to safe catalog details; cancellation propagates from direct calls.</summary>
/// <remarks>Built-in hosting middleware can short-circuit aborted requests before invoking this mapper.</remarks>
/// <param name="options">Validated exception mappings and disclosure settings.</param>
public sealed class ProblemDetailsExceptionMapper(IOptions<WebApiExceptionHandlingOptions> options) : IProblemDetailsExceptionMapper
{
    private readonly IOptions<WebApiExceptionHandlingOptions> options = options ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public ProblemDetails Map(HttpContext httpContext, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);
        httpContext.RequestAborted.ThrowIfCancellationRequested();
        if (exception is OperationCanceledException)
            ExceptionDispatchInfo.Throw(exception);

        WebApiExceptionHandlingOptions handlingOptions = options.Value;
        handlingOptions.Validate();
        ProblemDetailsExceptionMapping mapping = handlingOptions.FindMapping(exception)
            ?? new ProblemDetailsExceptionMapping(
                StatusCodes.Status500InternalServerError,
                handlingOptions.DefaultTitle,
                handlingOptions.DefaultType);

        var problemDetails = new ProblemDetails
        {
            Status = mapping.StatusCode,
            Title = mapping.Title,
            Type = mapping.Type ?? HttpResponseCatalog.GetError(mapping.StatusCode).Type,
            Detail = HttpResponseCatalog.GetError(mapping.StatusCode).SafeDetail,
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
