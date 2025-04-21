// ReSharper disable ConvertIfStatementToReturnStatement
#pragma warning disable ClassMethodMissingInterface
#pragma warning disable DerivedClasses
namespace vc.Ifx.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Determines whether the collection is null, empty, or contains only default values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <returns>True if the collection is null, empty, or contains only default values; otherwise, false.</returns>
    public static bool IsNullOrEmpty<T>(this ICollection<T>? collection)
    {
        if (collection is null || collection.Count == 0)
            return true;

        if (collection.All(i => i is null || Equals(i, default(T))))
            return true;

        return false;
    }

    /// <summary>
    /// Determines whether the collection contains any elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <returns>True if the collection contains at least one element; otherwise, false.</returns>
    public static bool HasAny<T>(this ICollection<T>? collection)
    {
        return collection is not null && collection.Count > 0;
    }

    /// <summary>
    /// Determines whether the collection contains duplicate elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <returns>True if the collection contains duplicate elements; otherwise, false.</returns>
    public static bool ContainsDuplicates<T>(this IEnumerable<T>? collection)
    {
        if (collection is null)
            return false;

        var set = new HashSet<T>();
        foreach (var item in collection)
        {
            if (!set.Add(item))
                return true;
        }

        return false;
    }
}
