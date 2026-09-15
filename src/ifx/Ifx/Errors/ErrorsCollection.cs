using System.Collections;

namespace Ifx.Errors;

/// <summary>
/// Represents an immutable collection of <see cref="Error"/> instances.
/// </summary>
public sealed class ErrorCollection : IReadOnlyCollection<Error>
{
    private readonly IReadOnlyList<Error> items;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorCollection"/> class.
    /// </summary>
    /// <param name="errors">The errors to include in the collection.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
    public ErrorCollection(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        items = errors.ToArray();
    }

    /// <summary>
    /// Gets the number of errors in the collection.
    /// </summary>
    public int Count => items.Count;

    /// <summary>
    /// Gets a value indicating whether the collection contains any errors.
    /// </summary>
    public bool HasErrors => items.Count > 0;

    /// <summary>
    /// Gets a value indicating whether every error in the collection is recoverable.
    /// An empty collection is considered recoverable.
    /// </summary>
    public IsRecoverableError IsRecoverableError => items.All(i => i.IsRecoverableError == IsRecoverableError.Yes)
            ? IsRecoverableError.Yes
            : IsRecoverableError.No;

    /// <inheritdoc />
    public IEnumerator<Error> GetEnumerator() => items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
}
