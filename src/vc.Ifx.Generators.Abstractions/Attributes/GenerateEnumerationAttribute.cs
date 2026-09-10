using System;

namespace vc.Ifx.Generators.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
public sealed class GenerateEnumerationAttribute : Attribute
{
    public string ClassName { get; }
    public string Name => ClassName;
    public string Namespace { get; set; }
    public string DefaultName { get; set; }
    public string EnumerationNamespace { get; set; }
    public string EnumerationTypeName { get; set; }

    public GenerateEnumerationAttribute(string name)
    {
        ClassName = name;
        DefaultName = string.Empty;
        Namespace = string.Empty;
        EnumerationNamespace = "vc.Ifx.Primitives";
        EnumerationTypeName = "Enumeration";
    }

    public GenerateEnumerationAttribute(string name, string defaultName) : this(name)
    {
        DefaultName = defaultName;
    }
}
