using System;

namespace Ifx.Generators.Abstractions.Attributes;

/// <summary>Declares a class as the source of a generated, reflection-free property mapper.</summary>
/// <remarks>Values are stored verbatim; generation owns validation and fallback behavior.</remarks>
/// <example><code>[GenerateMapper(typeof(RemoteOrder))] public class LocalOrder { public int Id { get; set; } }</code></example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class GenerateMapperAttribute : Attribute
{
    /// <summary>Initializes the declared mapping target type.</summary>
    /// <param name="targetType">The mapping target type, stored verbatim.</param>
    public GenerateMapperAttribute(Type targetType)
    {
        TargetType = targetType;
    }

    /// <summary>Gets the declared mapping target type.</summary>
    public Type TargetType { get; }

    /// <summary>Gets or sets whether a reverse target-to-source mapper is also generated.</summary>
    public bool Bidirectional { get; set; }
}
