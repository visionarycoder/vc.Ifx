namespace vc.Ifx.Generators.Abstractions.Attributes;

/// <summary>Declares a static method for generated minimal API registration.</summary>
/// <remarks>
/// This attribute only stores metadata. The generator validates declarations;
/// ASP.NET Core owns binding, authorization, and response execution in the consumer.
/// </remarks>
/// <example>
/// <code>
/// [GenerateEndpoint("/health", "GET", Name = "ReadHealth", AllowAnonymous = true)]
/// internal static string ReadHealth() => "healthy";
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class GenerateEndpointAttribute : Attribute
{
    /// <summary>Initializes an explicit route and one HTTP method without validation.</summary>
    /// <param name="route">The ASP.NET route template, stored verbatim.</param>
    /// <param name="httpMethod">One HTTP method token, stored verbatim.</param>
    public GenerateEndpointAttribute(string route, string httpMethod)
    {
        Route = route;
        HttpMethod = httpMethod;
    }

    /// <summary>Gets the declared route template.</summary>
    public string Route { get; }

    /// <summary>Gets the declared HTTP method; the generator validates and normalizes it.</summary>
    public string HttpMethod { get; }

    /// <summary>Gets or sets the optional unique endpoint name; null means unspecified.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the optional OpenAPI summary; null means unspecified.</summary>
    public string? Summary { get; set; }

    /// <summary>Gets or sets the optional OpenAPI description; null means unspecified.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets caller-owned ordered tags; defaults to an empty array.</summary>
    public string[] Tags { get; set; } = [];

    /// <summary>Gets or sets whether to require authorization; defaults to false.</summary>
    public bool RequireAuthorization { get; set; }

    /// <summary>Gets or sets a named authorization policy; a supplied policy implies authorization.</summary>
    public string? AuthorizationPolicy { get; set; }

    /// <summary>Gets or sets explicit anonymous access; defaults to false.</summary>
    /// <remarks>The generator rejects conflicting authorization declarations.</remarks>
    public bool AllowAnonymous { get; set; }

    /// <summary>Gets or sets exclusion from API description; defaults to false.</summary>
    public bool ExcludeFromDescription { get; set; }
}
