using Microsoft.AspNetCore.Http;

namespace VisionaryCoder.Framework.WebApi.ExceptionHandling;

public sealed class WebApiExceptionHandlingOptions
{
    public bool IncludeExceptionDetails { get; set; }

    public string DefaultTitle { get; set; } = "An unexpected error occurred.";

    public string DefaultType { get; set; } = "https://httpstatuses.com/500";

    public IDictionary<Type, ProblemDetailsExceptionMapping> ExceptionMappings { get; } =
        new Dictionary<Type, ProblemDetailsExceptionMapping>();

    public WebApiExceptionHandlingOptions Map<TException>(
        int statusCode,
        string title,
        string? type = null)
        where TException : Exception
    {
        ExceptionMappings[typeof(TException)] = new ProblemDetailsExceptionMapping(statusCode, title, type);
        return this;
    }

    internal void AddDefaultMappings()
    {
        Map<ArgumentException>(StatusCodes.Status400BadRequest, "The request is invalid.", "https://httpstatuses.com/400");
        Map<UnauthorizedAccessException>(StatusCodes.Status403Forbidden, "Access is forbidden.", "https://httpstatuses.com/403");
        Map<KeyNotFoundException>(StatusCodes.Status404NotFound, "The requested resource was not found.", "https://httpstatuses.com/404");
        Map<InvalidOperationException>(StatusCodes.Status409Conflict, "The request conflicts with the current resource state.", "https://httpstatuses.com/409");
        Map<TimeoutException>(StatusCodes.Status504GatewayTimeout, "The operation timed out.", "https://httpstatuses.com/504");
    }

    internal ProblemDetailsExceptionMapping? FindMapping(Exception exception)
    {
        for (Type? exceptionType = exception.GetType(); exceptionType is not null; exceptionType = exceptionType.BaseType)
        {
            if (ExceptionMappings.TryGetValue(exceptionType, out ProblemDetailsExceptionMapping? mapping))
            {
                return mapping;
            }
        }

        return null;
    }
}
