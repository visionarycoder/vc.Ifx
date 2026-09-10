using System.Collections.ObjectModel;

namespace VisionaryCoder.Framework.Extensions;
public static class EnumerableExtensions
{
    /// <summary>
    /// Determines whether the enumerable contains duplicate elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="comparer">Equality comparer.</param>
    /// <returns>True if the enumerable collection contains duplicate elements; otherwise, false.</returns>
    public static bool ContainsDuplicates<T>(this IEnumerable<T>? collection, IEqualityComparer<T>? comparer = null)
    {
        if (collection is null)
        {
            return false;
        }
        var instance = collection.ToList();
        if (instance.Count is 0 or 1)
        {
            return false;
        }
        HashSet<T> set = comparer == null ? [] : new HashSet<T>(comparer);
        return instance.Any(item => !set.Add(item));
    }
    /// Determines whether the sequence is null or empty.
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to check.</param>
    /// <returns>True if the sequence is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// Executes an action on each element of the sequence.
    /// </summary>
    /// <param name="source">The sequence of elements.</param>
    /// <param name="action">The action to execute on each element.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);
        foreach (T item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes an action on each element of the sequence with its index.
    /// </summary>
    /// <param name="source">The sequence of elements.</param>
    /// <param name="action">The action to execute on each element and its index.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);
        int index = 0;
        foreach (T item in source)
        {
            action(item, index++);
        }
    }

    /// <summary>
    /// Returns distinct elements from a sequence by using a specified selector function to determine uniqueness.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used to determine uniqueness.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>A sequence of elements with distinct keys.</returns>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        var seenKeys = new HashSet<TKey>();
        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    /// <summary>
    /// Batches the source sequence into sized buckets.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="size">The maximum size of each batch.</param>
    /// <returns>A sequence of batches, each containing at most the specified number of elements.</returns>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
    {
        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size), "Batch size must be greater than 0.");
        using IEnumerator<T> enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return GetBatch(enumerator, size);
        }

        static IEnumerable<T> GetBatch(IEnumerator<T> enumerator, int size)
        {
            yield return enumerator.Current;
            for (int i = 1; i < size && enumerator.MoveNext(); i++)
            {
                yield return enumerator.Current;
            }
        }
    }

    /// <summary>
    /// Shuffles the elements of a sequence using the specified random number generator.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="random">The random number generator to use.</param>
    /// <returns>A sequence whose elements are randomly ordered.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return source.OrderBy(_ => random.Next());
    }

    /// <summary>
    /// Safely tries to get the first element of a sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="value">When this method returns, contains the first element if found, or the default value if not.</param>
    /// <returns>True if an element was found; otherwise, false.</returns>
    public static bool TryFirst<T>(this IEnumerable<T>? source, out T? value)
    {
        if (source != null)
        {
            foreach (T item in source)
            {
                value = item;
                return true;
            }
        }
        value = default;
        return false;
    }
    /// <summary>
    /// Safely tries to get the last element of a sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="value">When this method returns, contains the last element if found, or the default value if not.</param>
    /// <returns>True if an element was found; otherwise, false.</returns>
    public static bool TryLast<T>(this IEnumerable<T>? source, out T? value)
    {
        if (source is ICollection<T> { Count: > 0 } collection)
        {
            if (collection is List<T> list)
            {
                value = list[^1];
                return true;
            }
            if (collection is T[] array)
            {
                value = array[^1];
                return true;
            }
        }
        if (source != null)
        {
            using IEnumerator<T> enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                T last = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    last = enumerator.Current;
                }
                value = last;
                return true;
            }
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Concatenates the elements of a sequence using the specified separator.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="separator">The string to use as a separator.</param>
    /// <returns>A string that consists of the elements in the sequence delimited by the separator string.</returns>
    public static string ToDelimitedString<T>(this IEnumerable<T> source, string separator = ", ")
    {
        return string.Join(separator, source);
    }

    /// <summary>
    /// Returns a read-only collection containing the elements of the specified sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>A read-only collection containing the elements of the specified sequence.</returns>
    public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
    {
        return new ReadOnlyCollection<T>(source.ToList());
    }

    /// <summary>
    /// Creates a sequence of elements paired with their indices.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>A sequence of tuples containing each element and its index.</returns>
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }

    /// <summary>
    /// Converts a sequence of key-value pairs into a dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="source">The sequence of key-value pairs.</param>
    /// <returns>A dictionary containing the key-value pairs.</returns>
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull
    {
        return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Gets the index of the first element in the sequence that satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>The index of the first element that satisfies the condition, or -1 if no such element is found.</returns>
    public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        int index = 0;
        foreach (T item in source)
        {
            if (predicate(item))
            {
                return index;
            }
            index++;
        }
        return -1;
    }
}
