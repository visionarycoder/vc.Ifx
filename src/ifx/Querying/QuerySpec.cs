using System.Linq.Expressions;
using Ifx.Filtering;

namespace Ifx.Querying;

/// <summary>An immutable provider query specification. Keep it local to data access.</summary>
public sealed class QuerySpec<T>
{
    private readonly Func<IQueryable<T>, QueryPlan<T>> compose;
    private readonly IQueryable<T>? source;
    private readonly bool ordered;

    /// <summary>Creates an unrestricted, source-independent query shape.</summary>
    public QuerySpec() : this(QueryPlan<T>.For, null, false) { }

    private QuerySpec(Func<IQueryable<T>, QueryPlan<T>> compose, IQueryable<T>? source, bool ordered)
    {
        this.compose = compose;
        this.source = source;
        this.ordered = ordered;
    }

    /// <summary>Binds a query shape to a provider without executing it.</summary>
    public static QuerySpec<T> For(IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new(QueryPlan<T>.For, source, false);
    }

    /// <summary>Combines a predicate with AND.</summary>
    public QuerySpec<T> Where(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return Add(plan => plan.Where(predicate));
    }

    /// <summary>Combines a typed filter with AND.</summary>
    public QuerySpec<T> Where(FilterSpec<T> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return Where(filter.Predicate);
    }

    /// <summary>Adds a parenthesized predicate group.</summary>
    public QuerySpec<T> Where(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group)
    {
        ArgumentNullException.ThrowIfNull(group);
        return Where(group(new FilterGroupBuilder<T>()).Build());
    }

    /// <summary>Combines a predicate with OR.</summary>
    public QuerySpec<T> Or(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return Add(plan => plan.Or(predicate));
    }

    /// <summary>Combines a predicate group with OR.</summary>
    public QuerySpec<T> Or(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group)
    {
        ArgumentNullException.ThrowIfNull(group);
        return Or(group(new FilterGroupBuilder<T>()).Build().Predicate);
    }

    /// <summary>Combines the negated predicate with AND.</summary>
    public QuerySpec<T> Not(Expression<Func<T, bool>> predicate) => Where(new FilterSpec<T>(predicate).Not());

    /// <summary>Combines a negated group with AND.</summary>
    public QuerySpec<T> Not(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group)
    {
        ArgumentNullException.ThrowIfNull(group);
        return Where(group(new FilterGroupBuilder<T>()).Build().Not());
    }

    /// <summary>Combines a predicate with AND.</summary>
    public QuerySpec<T> And(Expression<Func<T, bool>> predicate) => Where(predicate);

    /// <summary>Combines a predicate group with AND.</summary>
    public QuerySpec<T> And(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) => Where(group);

    /// <summary>Hydrates a portable boundary predicate.</summary>
    public QuerySpec<T> ApplyFilter(FilterNode? filter) => Where(FilterSpec<T>.FromFilterNode(filter));

    /// <summary>Replaces the primary ordering.</summary>
    public QuerySpec<T> OrderBy<TKey>(Expression<Func<T, TKey>> key) => Order(key, false);

    /// <summary>Replaces the primary ordering with descending order.</summary>
    public QuerySpec<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> key) => Order(key, true);

    private QuerySpec<T> Order<TKey>(Expression<Func<T, TKey>> key, bool descending)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Add(plan => { plan.ClearOrdering(); if (descending) plan.OrderByDescending(key); else plan.OrderBy(key); }, true);
    }

    /// <summary>Adds an ascending tie-breaker.</summary>
    public QuerySpec<T> ThenBy<TKey>(Expression<Func<T, TKey>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (!ordered) throw new InvalidOperationException("ThenBy requires OrderBy.");
        return Add(plan => plan.ThenBy(key));
    }

    /// <summary>Adds a descending tie-breaker.</summary>
    public QuerySpec<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (!ordered) throw new InvalidOperationException("ThenByDescending requires OrderBy.");
        return Add(plan => plan.ThenByDescending(key));
    }

    /// <summary>Sets an ordered offset/size window.</summary>
    public QuerySpec<T> Page(int offset, int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        if (!ordered) throw new InvalidOperationException("Pagination requires explicit ordering.");
        return Add(plan => plan.SkipItems(offset).TakeItems(size));
    }

    /// <summary>Sets a nonnegative offset.</summary>
    public QuerySpec<T> SkipItems(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return Add(plan => plan.SkipItems(count));
    }

    /// <summary>Sets a nonnegative maximum result count.</summary>
    public QuerySpec<T> TakeItems(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return Add(plan => plan.TakeItems(count));
    }

    /// <summary>Sets the stable identity key used in cursor paging.</summary>
    public QuerySpec<T> UseKey<TKey>(Expression<Func<T, TKey>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Add(plan => plan.UseKey(key));
    }

    /// <summary>Resumes cursor paging after an opaque token.</summary>
    public QuerySpec<T> AfterCursor(string? cursor) => Add(plan => plan.AfterCursor(cursor));

    /// <summary>Registers a provider eager-load path.</summary>
    public QuerySpec<T> Include<TProperty>(Expression<Func<T, TProperty>> path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return Add(plan => plan.Include(path));
    }

    /// <summary>Adds validated portable sort clauses.</summary>
    public QuerySpec<T> ApplySort(IEnumerable<SortClause>? clauses)
    {
        SortClause[] snapshot = clauses?.ToArray() ?? [];
        return Add(plan => plan.ApplySort(snapshot), ordered || snapshot.Length > 0);
    }

    /// <summary>Gets provider eager-load paths for the bound query.</summary>
    public IReadOnlyList<LambdaExpression> Includes => BoundPlan().Includes;

    /// <summary>Creates the next cursor token for the supplied last item.</summary>
    public string BuildCursor(T lastItem) => BoundPlan().BuildCursor(lastItem);

    /// <summary>Executes a bound in-process cursor page.</summary>
    public CursorPage<T> ToCursorPage(int pageSize) => BoundPlan().ToCursorPage(pageSize);

    /// <summary>Composes the bound query without executing it.</summary>
    public IQueryable<T> ToQueryable() => BoundPlan().ToQueryable();

    /// <summary>Composes the bound query without offset/limit paging.</summary>
    public IQueryable<T> ToUnpagedQueryable() => BoundPlan().ToUnpagedQueryable();

    internal IQueryable<T> BuildCursorQueryable(int take) => BoundPlan().BuildCursorQueryable(take);

    /// <summary>Applies the shape to a provider without enumerating it.</summary>
    public IQueryable<T> Apply(IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return compose(source).ToQueryable();
    }

    /// <summary>Adds a typed provider projection.</summary>
    public QuerySpec<T, TResult> Select<TResult>(Expression<Func<T, TResult>> selector) => new(this, selector);

    private QueryPlan<T> BoundPlan() => compose(source ?? throw new InvalidOperationException("Bind a source using QuerySpec.For before executing a bound query."));

    private QuerySpec<T> Add(Action<QueryPlan<T>> configure, bool? ordered = null) =>
        new(input => { QueryPlan<T> plan = compose(input); configure(plan); return plan; }, source, ordered ?? this.ordered);
}

/// <summary>A local provider query with a typed projection.</summary>
public sealed class QuerySpec<T, TResult>
{
    private readonly QuerySpec<T> source;
    private readonly Expression<Func<T, TResult>> selector;

    internal QuerySpec(QuerySpec<T> source, Expression<Func<T, TResult>> selector)
    {
        this.source = source;
        this.selector = selector ?? throw new ArgumentNullException(nameof(selector));
    }

    /// <summary>Applies the query shape before projecting.</summary>
    public IQueryable<TResult> Apply(IQueryable<T> query) => source.Apply(query).Select(selector);
}
