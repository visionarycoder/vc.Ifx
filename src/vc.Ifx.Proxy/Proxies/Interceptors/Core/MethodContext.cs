using System.Reflection;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

/// <summary>
/// Represents the context of a method invocation, including the contract type, method, target instance, and arguments.
/// Provides access to method and type attributes, and allows storing contextual items during interception.
/// </summary>
public sealed class MethodContext
{
    /// <summary>
    /// Gets the contract type (interface) being invoked.
    /// </summary>
    public required Type ContractType { get; init; }

    /// <summary>
    /// Gets the method being invoked.
    /// </summary>
    public required MethodInfo MethodInfo { get; init; }

    /// <summary>
    /// Gets the target instance that the method will be invoked on.
    /// </summary>
    public required object Target { get; init; }

    /// <summary>
    /// Gets the arguments passed to the method invocation.
    /// </summary>
    public required IReadOnlyList<MethodArgument> Arguments { get; init; }

    /// <summary>
    /// Gets a dictionary of items that can be used to store contextual data during interception.
    /// </summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    /// <summary>
    /// Retrieves a custom attribute from the method or contract type.
    /// Searches the method first, then falls back to the contract type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to retrieve.</typeparam>
    /// <returns>The attribute instance if found; otherwise null.</returns>
    public TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute
        => MethodInfo.GetCustomAttribute<TAttribute>(true)
            ?? ContractType.GetCustomAttribute<TAttribute>(true);

    /// <summary>
    /// Determines whether a custom attribute is present on the method or contract type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to check.</typeparam>
    /// <returns>True if the attribute is found; otherwise false.</returns>
    public bool HasAttribute<TAttribute>() where TAttribute : Attribute
        => GetAttribute<TAttribute>() is not null;
}
