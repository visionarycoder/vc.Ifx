using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VisionaryCoder.Framework.WebApi.ExceptionHandling;

/// <summary>Replaceable mapping boundary between exceptions and client-facing error facts.</summary>
public interface IProblemDetailsExceptionMapper
{
    /// <summary>Creates error details, preserving cancellation and avoiding private diagnostic disclosure.</summary>
    /// <remarks>
    /// A trusted host mapper may set required response headers here from explicit configuration
    /// before returning the matching error status. Built-in exception middleware has already
    /// cleared pre-throw headers; this is not an arbitrary header-restoration mechanism.
    /// Aborted cancellation/I/O failures may be handled by middleware without invoking this method.
    /// </remarks>
    ProblemDetails Map(HttpContext httpContext, Exception exception);
}
