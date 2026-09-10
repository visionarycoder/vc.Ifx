using VisionaryCoder.Framework.WebApi.Responses;

namespace VisionaryCoder.Framework.WebApi.ExceptionHandling;

/// <summary>Application exception mappings and explicit diagnostic disclosure settings.</summary>
public sealed class WebApiExceptionHandlingOptions
{
    /// <summary>Gets or sets whether exception messages and type names are exposed; false by default.</summary>
    public bool IncludeExceptionDetails { get; set; }

    /// <summary>Gets or sets the safe title for an unrecognized failure.</summary>
    public string DefaultTitle { get; set; } = HttpResponseCatalog.InternalServerError.DefaultTitle;

    /// <summary>Gets or sets the absolute problem type URI for an unrecognized failure.</summary>
    public string DefaultType { get; set; } = HttpResponseCatalog.InternalServerError.Type;

    /// <summary>Gets startup configuration mappings, validated again before use. Do not mutate during requests.</summary>
    public IDictionary<Type, ProblemDetailsExceptionMapping> ExceptionMappings { get; } =
        new Dictionary<Type, ProblemDetailsExceptionMapping>
        {
            [typeof(ArgumentException)] = new(400, HttpResponseCatalog.Get(400).DefaultTitle),
            [typeof(UnauthorizedAccessException)] = new(403, HttpResponseCatalog.Forbidden.DefaultTitle),
            [typeof(KeyNotFoundException)] = new(404, HttpResponseCatalog.NotFound.DefaultTitle)
        };

    /// <summary>Validates and registers an application mapping; the nearest mapped base class wins.</summary>
    public WebApiExceptionHandlingOptions Map<TException>(
        int statusCode,
        string title,
        string? type = null)
        where TException : Exception
    {
        ValidateMapping(new ProblemDetailsExceptionMapping(statusCode, title, type));
        ExceptionMappings[typeof(TException)] = new ProblemDetailsExceptionMapping(statusCode, title, type);
        return this;
    }

    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(DefaultTitle);
        if (!Uri.TryCreate(DefaultType, UriKind.Absolute, out _))
        {
            throw new ArgumentException("DefaultType must be an absolute URI.", nameof(DefaultType));
        }

        foreach (var pair in ExceptionMappings)
        {
            if (!typeof(Exception).IsAssignableFrom(pair.Key))
            {
                throw new ArgumentException("Mapping keys must derive from Exception.", nameof(ExceptionMappings));
            }

            ValidateMapping(pair.Value);
        }
    }

    private static void ValidateMapping(ProblemDetailsExceptionMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        HttpResponseCatalog.GetError(mapping.StatusCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(mapping.Title);
        if (mapping.Type is not null && !Uri.TryCreate(mapping.Type, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Mapping type must be an absolute URI.", nameof(mapping));
        }
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
