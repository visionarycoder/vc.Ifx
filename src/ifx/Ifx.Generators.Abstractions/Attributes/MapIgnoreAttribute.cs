using System;

namespace Ifx.Generators.Abstractions.Attributes;

/// <summary>Excludes a member from generated-mapper match requirements, leaving it at its default value when unmapped.</summary>
/// <example><code>public class RemoteOrder { [MapIgnore] public string? Diagnostics { get; set; } }</code></example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MapIgnoreAttribute : Attribute
{
}
