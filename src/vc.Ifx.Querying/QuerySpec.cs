using System.Linq.Expressions;
using VisionaryCoder.Framework.Filtering;

namespace VisionaryCoder.Framework.Querying;

/// <summary>An immutable database query shape, evaluated by the caller's provider.</summary>
public sealed class QuerySpec<T>
{
    private readonly FilterSpec<T> filter;
    private readonly Func<IQueryable<T>, IOrderedQueryable<T>>? ordering;
    private readonly int? offset;
    private readonly int? limit;

    /// <summary>Creates an unrestricted query specification.</summary>
    public QuerySpec() : this(FilterSpec<T>.All, null, null, null) { }

    private QuerySpec(FilterSpec<T> filter, Func<IQueryable<T>, IOrderedQueryable<T>>? ordering, int? offset, int? limit)
    {
        this.filter = filter;
        this.ordering = ordering;
        this.offset = offset;
        this.limit = limit;
    }

    /// <summary>Adds a predicate without executing the query.</summary>
    public QuerySpec<T> Where(Expression<Func<T, bool>> predicate) => Where(new FilterSpec<T>(predicate));

    /// <summary>Adds an existing filter specification.</summary>
    public QuerySpec<T> Where(FilterSpec<T> predicate) => new(filter.And(predicate), ordering, offset, limit);

    /// <summary>Replaces the primary ordering.</summary>
    public QuerySpec<T> OrderBy<TKey>(Expression<Func<T, TKey>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return new(filter, source => source.OrderBy(key), offset, limit);
    }

    /// <summary>Replaces the primary ordering with descending order.</summary>
    public QuerySpec<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return new(filter, source => source.OrderByDescending(key), offset, limit);
    }

    /// <summary>Adds an ascending tie-breaker after an explicit primary ordering.</summary>
    public QuerySpec<T> ThenBy<TKey>(Expression<Func<T, TKey>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var prior = ordering ?? throw new InvalidOperationException("ThenBy requires OrderBy.");
        return new(filter, source => prior(source).ThenBy(key), offset, limit);
    }

    /// <summary>Adds a descending tie-breaker after an explicit primary ordering.</summary>
    public QuerySpec<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var prior = ordering ?? throw new InvalidOperationException("ThenByDescending requires OrderBy.");
        return new(filter, source => prior(source).ThenByDescending(key), offset, limit);
    }

    /// <summary>Sets one ordered offset/limit window. Applications supply unique tie-breakers.</summary>
    public QuerySpec<T> Page(int offset, int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        if (ordering is null) throw new InvalidOperationException("Pagination requires explicit ordering.");
        return new(filter, ordering, offset, size);
    }

    /// <summary>Adds a projection that remains in the query provider.</summary>
    public QuerySpec<T, TResult> Select<TResult>(Expression<Func<T, TResult>> selector) => new(this, selector);

    /// <summary>Applies the shape without enumerating the source.</summary>
    public IQueryable<T> Apply(IQueryable<T> source)
    {
        IQueryable<T> query = filter.Apply(source);
        if (ordering is not null) query = ordering(query);
        if (offset.HasValue) query = query.Skip(offset.Value).Take(limit!.Value);
        return query;
    }
}

/// <summary>A database query specification with a typed provider projection.</summary>
public sealed class QuerySpec<T, TResult>
{
    private readonly QuerySpec<T> source;
    private readonly Expression<Func<T, TResult>> selector;

    internal QuerySpec(QuerySpec<T> source, Expression<Func<T, TResult>> selector)
    {
        this.source = source;
        this.selector = selector ?? throw new ArgumentNullException(nameof(selector));
    }

    /// <summary>Applies filtering, ordering and pagination before projecting.</summary>
    public IQueryable<TResult> Apply(IQueryable<T> query) => source.Apply(query).Select(selector);
}
