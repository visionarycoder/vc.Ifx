using System.Diagnostics.Contracts;

namespace vc.Ifx.Extension;

public static class HashSetExtension
{

    /// <summary>
    /// Extend the HashSet to enable adding multiple objects to the collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="collection"></param>
    public static void AddRange<T>(this HashSet<T> target, ICollection<T>? collection)
    {

        Contract.Assert(target != null, "Input target is null.");

        if (collection == null)
        {
            return;
        }

        foreach (var entry in collection)
        {
            target.Add(entry);
        }

    }

}