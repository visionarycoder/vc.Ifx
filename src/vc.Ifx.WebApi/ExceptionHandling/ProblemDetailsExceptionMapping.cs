namespace VisionaryCoder.Framework.WebApi.ExceptionHandling;

/// <summary>Describes an explicit application mapping to a standard error status.</summary>
/// <param name="StatusCode">Registered active 4xx/5xx status.</param>
/// <param name="Title">Client-safe title selected by the application.</param>
/// <param name="Type">Absolute type URI, or null to use the catalog RFC reference.</param>
public sealed record ProblemDetailsExceptionMapping(int StatusCode, string Title, string? Type = null);
