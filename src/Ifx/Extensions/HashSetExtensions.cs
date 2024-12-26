
// ReSharper disable UnusedType.Global
#pragma warning disable DerivedClasses
#pragma warning disable ClassMethodMissingInterface

namespace vc.Ifx.Extensions;

/// <summary>
/// Provides extension methods for <see cref="HashSet{T}"/>.
/// </summary>
public static class HashSetExtensions
{

    /// <summary>
    /// Adds a range of elements to the <see cref="HashSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    /// <param name="target">The target <see cref="HashSet{T}"/>.</param>
    /// <param name="collection">The collection of elements to add.</param>
    public static void AddRange<T>(this HashSet<T> target, ICollection<T> collection)
    {
        foreach (var entry in collection)
        {
            target.Add(entry);
        }
    }
}
