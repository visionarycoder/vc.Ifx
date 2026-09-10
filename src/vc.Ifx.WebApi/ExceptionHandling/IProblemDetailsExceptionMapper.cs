using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VisionaryCoder.Framework.WebApi.ExceptionHandling;

public interface IProblemDetailsExceptionMapper
{
    ProblemDetails Map(HttpContext httpContext, Exception exception);
}
