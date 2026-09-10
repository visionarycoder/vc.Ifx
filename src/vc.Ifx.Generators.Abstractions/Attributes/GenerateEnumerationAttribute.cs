using System;

namespace vc.Ifx.Generators.Abstractions.Attributes;

/// <summary>Declares an enum for enumeration-class generation.</summary>
/// <remarks>Values are stored verbatim; generation owns validation and fallback behavior.</remarks>
/// <example><code>[GenerateEnumeration("Status", "Ready")] enum StatusCode { Ready, Busy }</code></example>
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
public sealed class GenerateEnumerationAttribute : Attribute
{
    /// <summary>Gets the requested generated class name.</summary>
    public string ClassName { get; }
    /// <summary>Gets the compatibility alias for <see cref="ClassName"/>.</summary>
    public string Name => ClassName;
    /// <summary>Gets or sets the output namespace; empty delegates selection to the generator.</summary>
    public string Namespace { get; set; }
    /// <summary>Gets or sets the default member name; empty delegates selection to the generator.</summary>
    public string DefaultName { get; set; }
    /// <summary>Gets or sets the base-type namespace; defaults to vc.Ifx.Primitives.</summary>
    public string EnumerationNamespace { get; set; }
    /// <summary>Gets or sets the base-type name; defaults to Enumeration.</summary>
    public string EnumerationTypeName { get; set; }

    /// <summary>Initializes a class name and the documented generation defaults.</summary>
    /// <param name="name">The requested class name, stored without validation.</param>
    public GenerateEnumerationAttribute(string name)
    {
        ClassName = name;
        DefaultName = string.Empty;
        Namespace = string.Empty;
        EnumerationNamespace = "vc.Ifx.Primitives";
        EnumerationTypeName = "Enumeration";
    }

    /// <summary>Initializes a class name and an explicit default member name.</summary>
    /// <param name="name">The requested class name, stored without validation.</param>
    /// <param name="defaultName">The requested default member, stored without validation.</param>
    public GenerateEnumerationAttribute(string name, string defaultName) : this(name)
    {
        DefaultName = defaultName;
    }
}
