using System.Linq.Expressions;

namespace Ifx.Filtering;

/// <summary>
/// A scoped builder used inside a grouped sub-clause — for example
/// <c>QuerySpec&lt;Order&gt;.Or(g => g.Where(o => o.IsPriority).And(o => o.Total &gt; 10))</c> — to
/// compose a nested, explicitly parenthesized <see cref="FilterSpec{T}"/> as a single unit before it
/// is combined into the enclosing filter.
/// </summary>
/// <typeparam name="T">The item type the predicate applies to.</typeparam>
public sealed class FilterGroupBuilder<T>
{
    /// <summary>Holds the accumulated filter for the group while clauses are being added.</summary>
    private FilterSpec<T>? spec;

    /// <summary>Initializes an empty grouped filter builder.</summary>
    internal FilterGroupBuilder()
    {
    }

    /// <summary>Starts (or continues, combined with AND) the group with <paramref name="predicate"/>.</summary>
    public FilterGroupBuilder<T> Where(Expression<Func<T, bool>> predicate) => And(predicate);

    /// <summary>Combines the group with <paramref name="predicate"/> using AND.</summary>
    public FilterGroupBuilder<T> And(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        spec = spec is null ? new FilterSpec<T>(predicate) : spec.Where(predicate);
        return this;
    }

    /// <summary>Combines the group with <paramref name="predicate"/> using OR.</summary>
    public FilterGroupBuilder<T> Or(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        spec = spec is null ? new FilterSpec<T>(predicate) : spec.Or(new(predicate));
        return this;
    }

    /// <summary>Negates <paramref name="predicate"/>, combined with the group using AND.</summary>
    public FilterGroupBuilder<T> Not(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        FilterSpec<T> negated = new FilterSpec<T>(predicate).Not();
        spec = spec is null ? negated : spec.And(negated);
        return this;
    }

    /// <summary>Materializes the composed group. An empty group (no clauses added) matches everything.</summary>
    /// <returns>The composed filter specification for the group.</returns>
    internal FilterSpec<T> Build() => spec ?? FilterSpec<T>.All;
}
