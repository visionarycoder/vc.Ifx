namespace VisionaryCoder.Framework.WebApi.ExceptionHandling;

public sealed record ProblemDetailsExceptionMapping(int StatusCode, string Title, string? Type = null);
