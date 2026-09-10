using System.Collections;

namespace Wa.Wsdot.Fin.Idl.Ifx.Generics;

public class GenericReadOnlyCollection<T> : IReadOnlyCollection<T>
{
    protected internal readonly List<T> items = [];

    /// <summary>
    /// Gets the number of materials in the collection.
    /// </summary>
    public int Count => items.Count;

    /// <summary>
    /// Adds an item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item) => items.Add(item);

    public void AddRange(ICollection<T> collection) => items.ToList().ForEach(Add);

    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
}