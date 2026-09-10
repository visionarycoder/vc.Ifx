using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Runtime.ExceptionServices;
using VisionaryCoder.Framework.WebApi.Responses;

namespace VisionaryCoder.Framework.WebApi.ExceptionHandling;

/// <summary>Writes safe error responses using required headers present at the direct-handler boundary.</summary>
/// <remarks>
/// Direct calls propagate cancellation and retain supplied headers. Built-in hosting middleware
/// handles aborted cancellation/I/O failures before this handler and clears other responses first;
/// a trusted mapper may supply required headers after that reset. Pre-throw headers are not retained.
/// </remarks>
/// <param name="exceptionMapper">Maps exceptions to safe problem details.</param>
/// <param name="problemDetailsService">Chooses the host's problem-details writer.</param>
/// <param name="logger">Records responses that have already started.</param>
public sealed class GlobalExceptionHandler(
    IProblemDetailsExceptionMapper exceptionMapper,
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly IProblemDetailsExceptionMapper exceptionMapper = exceptionMapper ?? throw new ArgumentNullException(nameof(exceptionMapper));
    private readonly IProblemDetailsService problemDetailsService = problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
    private readonly ILogger<GlobalExceptionHandler> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);
        cancellationToken.ThrowIfCancellationRequested();
        httpContext.RequestAborted.ThrowIfCancellationRequested();
        if (exception is OperationCanceledException)
            ExceptionDispatchInfo.Throw(exception);

        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning(exception, "The response has already started. The global exception handler cannot write problem details.");
            return false;
        }

        ProblemDetails problemDetails = exceptionMapper.Map(httpContext, exception);
        int status = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        HttpResponseCatalog.GetError(status);
        string? requiredHeader = status switch
        {
            401 => "WWW-Authenticate",
            407 => "Proxy-Authenticate",
            405 => "Allow",
            426 => "Upgrade",
            _ => null
        };
        if (requiredHeader is not null && string.IsNullOrWhiteSpace(httpContext.Response.Headers[requiredHeader]))
        {
            return false;
        }

        problemDetails.Status = status;
        httpContext.Response.StatusCode = status;
        if (!HttpResponseCatalog.CanWriteBody(status, httpContext.Request.Method))
        {
            return true;
        }

        CancellationToken requestAborted = httpContext.RequestAborted;
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(requestAborted, cancellationToken);
        httpContext.RequestAborted = linkedCancellation.Token;
        try
        {
            if (!await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            }))
            {
                await httpContext.Response.WriteAsJsonAsync(problemDetails,
                    options: null, contentType: "application/problem+json", cancellationToken: linkedCancellation.Token);
            }
        }
        finally
        {
            httpContext.RequestAborted = requestAborted;
        }

        return true;
    }
}
