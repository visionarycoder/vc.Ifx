using System.Collections;
using System.Reflection;

/* Unmerged change from project 'Ifx (net8.0)'
Before:
// ReSharper disable MemberCanBePrivate.Global
After:
using vc;
using vc.Ifx;
using vc.Ifx;
using vc.Ifx.Extensions;
// ReSharper disable MemberCanBePrivate.Global
*/

// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable ClassMethodMissingInterface
#pragma warning disable DerivedClasses

namespace vc.Ifx.Extensions;

/// <summary>
/// Provides extension methods for <see cref="PropertyInfo"/>.
/// </summary>
public static class PropertyInfoExtensions
{
    /// <summary>
    /// Determines whether the property is a non-string enumerable.
    /// </summary>
    /// <param name="pi">The property information.</param>
    /// <returns><c>true</c> if the property is a non-string enumerable; otherwise, <c>false</c>.</returns>
    public static bool IsNonStringEnumerable(this PropertyInfo pi)
    {
        return pi.PropertyType.IsNonStringEnumerable();
    }

    /// <summary>
    /// Determines whether the instance is a non-string enumerable.
    /// </summary>
    /// <param name="instance">The instance to check.</param>
    /// <returns><c>true</c> if the instance is a non-string enumerable; otherwise, <c>false</c>.</returns>
    public static bool IsNonStringEnumerable(this object instance)
    {
        return instance.GetType().IsNonStringEnumerable();
    }

    /// <summary>
    /// Determines whether the type is a non-string enumerable.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is a non-string enumerable; otherwise, <c>false</c>.</returns>
    public static bool IsNonStringEnumerable(this Type type)
    {
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }
}
