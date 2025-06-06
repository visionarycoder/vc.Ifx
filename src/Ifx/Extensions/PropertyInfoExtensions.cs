using System.Collections;
using System.Reflection;

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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pi"/> is null.</exception>
    public static bool IsNonStringEnumerable(this PropertyInfo pi)
    {
        ArgumentNullException.ThrowIfNull(pi);
        return pi.PropertyType.IsNonStringEnumerable();
    }

    /// <summary>
    /// Determines whether the instance is a non-string enumerable.
    /// </summary>
    /// <param name="instance">The instance to check.</param>
    /// <returns><c>true</c> if the instance is a non-string enumerable; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> is null.</exception>
    public static bool IsNonStringEnumerable(this object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        return instance.GetType().IsNonStringEnumerable();
    }

    /// <summary>
    /// Determines whether the type is a non-string enumerable.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is a non-string enumerable; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    public static bool IsNonStringEnumerable(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }

    /// <summary>
    /// Gets the value of the property for a given object.
    /// </summary>
    /// <param name="pi">The property information.</param>
    /// <param name="obj">The object to retrieve the value from.</param>
    /// <returns>The value of the property.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pi"/> or <paramref name="obj"/> is null.</exception>
    public static object? GetPropertyValue(this PropertyInfo pi, object obj)
    {
        ArgumentNullException.ThrowIfNull(pi);
        ArgumentNullException.ThrowIfNull(obj);
        return pi.GetValue(obj);
    }

    /// <summary>
    /// Determines whether the property is readable.
    /// </summary>
    /// <param name="pi">The property information.</param>
    /// <returns><c>true</c> if the property is readable; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pi"/> is null.</exception>
    public static bool IsReadable(this PropertyInfo pi)
    {
        ArgumentNullException.ThrowIfNull(pi);
        return pi.CanRead;
    }

    /// <summary>
    /// Determines whether the property is writable.
    /// </summary>
    /// <param name="pi">The property information.</param>
    /// <returns><c>true</c> if the property is writable; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pi"/> is null.</exception>
    public static bool IsWritable(this PropertyInfo pi)
    {
        ArgumentNullException.ThrowIfNull(pi);
        return pi.CanWrite;
    }

    /// <summary>
    /// Gets a custom attribute of the specified type from the property.
    /// </summary>
    /// <typeparam name="T">The type of the custom attribute.</typeparam>
    /// <param name="pi">The property information.</param>
    /// <returns>The custom attribute of the specified type, or <c>null</c> if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pi"/> is null.</exception>
    public static T? GetCustomAttribute<T>(this PropertyInfo pi) where T : Attribute
    {
        ArgumentNullException.ThrowIfNull(pi);
        return pi.GetCustomAttribute<T>();
    }

}
