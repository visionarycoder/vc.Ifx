namespace VisionaryCoder.Framework.Extensions;

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
        return !collection.Any(item => item is not null && !Equals(item, default(T)));
    }
    
    /// Determines whether the collection contains any elements.
    /// <returns>True if the collection contains at least one element; otherwise, false.</returns>
    public static bool HasAny<T>(this ICollection<T>? collection)
    {
        return collection is not null && collection.Count > 0;
    }

    /// <summary>
    /// Adds a range of items to the collection.
    /// </summary>
    /// <param name="collection">The collection to add items to.</param>
    /// <param name="items">The items to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if collection is null.</exception>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);
        foreach (T item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Safely tries to get an element at the specified index.
    /// </summary>
    /// <param name="collection">The collection to get the element from.</param>
    /// <param name="index">The index of the element to get.</param>
    /// <param name="value">The value of the element if found, default otherwise.</param>
    /// <returns>True if the element was found; otherwise, false.</returns>
    public static bool TryGetElement<T>(this ICollection<T> collection, int index, out T? value)
    {
        if (index >= 0 && index < collection.Count)
        {
            value = collection.ElementAt(index);
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Removes all elements that match the condition defined by the specified predicate.
    /// </summary>
    /// <param name="collection">The collection to remove elements from.</param>
    /// <param name="predicate">The condition to match.</param>
    /// <returns>The number of elements removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection or predicate is null.</exception>
    public static int RemoveWhere<T>(this ICollection<T> collection, Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(predicate);
        var itemsToRemove = collection.Where(item => predicate(item)).ToList();
        foreach (T item in itemsToRemove)
        {
            collection.Remove(item);
        }
        return itemsToRemove.Count;
    }

    /// <summary>
    /// Adds an item to the collection if it satisfies the specified condition.
    /// </summary>
    /// <param name="collection">The collection to add the item to.</param>
    /// <param name="item">The item to add.</param>
    /// <param name="condition">The condition to check.</param>
    /// <returns>True if the item was added; otherwise, false.</returns>
    public static bool AddIf<T>(this ICollection<T> collection, T item, Func<T, bool> condition)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(condition);
        if (!condition(item))
        {
            return false;
        }
        collection.Add(item);
        return true;
    }
}
