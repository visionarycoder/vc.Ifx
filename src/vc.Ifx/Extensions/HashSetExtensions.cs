namespace VisionaryCoder.Framework.Extensions;

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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> or <paramref name="collection"/> is null.</exception>
    public static void AddRange<T>(this HashSet<T> target, ICollection<T> collection)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(collection);
        target.UnionWith(collection);
    }
    /// <summary>
    /// Removes a range of elements from the <see cref="HashSet{T}"/>.
    /// </summary>
    /// <param name="target">The target <see cref="HashSet{T}"/>.</param>
    /// <param name="collection">The collection of elements to remove.</param>
    public static void RemoveRange<T>(this HashSet<T> target, ICollection<T> collection)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(collection);
        target.ExceptWith(collection);
    }

    /// <summary>
    /// Determines whether the <see cref="HashSet{T}"/> contains all elements in the specified collection.
    /// </summary>
    /// <param name="target">The target <see cref="HashSet{T}"/>.</param>
    /// <param name="collection">The collection of elements to check.</param>
    /// <returns>True if the <see cref="HashSet{T}"/> contains all elements in the collection; otherwise, false.</returns>
    public static bool ContainsAll<T>(this HashSet<T> target, ICollection<T> collection)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(collection);
        return collection.All(target.Contains);
    }

    /// <summary>
    /// Determines whether the <see cref="HashSet{T}"/> contains any element in the specified collection.
    /// </summary>
    /// <param name="target">The target <see cref="HashSet{T}"/>.</param>
    /// <param name="collection">The collection of elements to check.</param>
    /// <returns>True if the <see cref="HashSet{T}"/> contains any element in the collection; otherwise, false.</returns>
    public static bool ContainsAny<T>(this HashSet<T> target, ICollection<T> collection)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(collection);
        return collection.Any(target.Contains);
    }
}
