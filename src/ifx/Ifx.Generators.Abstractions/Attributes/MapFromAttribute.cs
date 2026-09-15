using System;

namespace Ifx.Generators.Abstractions.Attributes;

/// <summary>Redirects a generated mapper member to read from a differently named counterpart member.</summary>
/// <remarks>The source member name is stored verbatim; generation owns lookup and validation.</remarks>
/// <example><code>public class RemoteOrder { [MapFrom("Id")] public int OrderId { get; set; } }</code></example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MapFromAttribute : Attribute
{
    /// <summary>Initializes the counterpart member name to read from.</summary>
    /// <param name="memberName">The counterpart property name, stored verbatim.</param>
    public MapFromAttribute(string memberName)
    {
        MemberName = memberName;
    }

    /// <summary>Gets the declared counterpart member name.</summary>
    public string MemberName { get; }
}
