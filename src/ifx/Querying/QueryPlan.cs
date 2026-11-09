using System.Linq.Expressions;
using Ifx.Filtering;
using Ifx.Filtering.Poco;

namespace Ifx.Querying;

/// <summary>
/// A fluent, generic <see cref="IQueryable{T}"/> query composer: filtering, sorting, paging and
/// eager-load hints, without depending on any specific database provider.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="QueryPlan{T}"/> lives in the Access layer, not on the wire. Its job is to compose a
/// query from either developer-authored expressions (<see cref="Where(Expression{Func{T,bool}})"/>,
/// <see cref="OrderBy{TKey}"/>) or from the portable, deserialized request shapes that arrived over
/// the wire (<see cref="ApplyFilter"/>, <see cref="ApplySort"/>) — this is the "hydrate a QueryPlan
/// from a FilterSpec/FilterNode inside the Access component" pattern.
/// </para>
/// <para>
/// This type only ever produces a plain <see cref="IQueryable{T}"/> via <see cref="ToQueryable"/> —
/// standard LINQ operators that any provider (EF Core, an in-memory list, a future provider) can
/// translate. Provider-specific execution (no-tracking, sequential count,
/// typed <c>Include</c>/<c>ThenInclude</c>, compiled queries, etc.) is supplied by a separate
/// extension package such as <c>Ifx.FilterSpec.EntityFrameworkCore</c>, which reads
/// <see cref="Includes"/> and applies them using the target provider's own APIs.
/// </para>
/// </remarks>
/// <typeparam name="T">The entity/item type being queried.</typeparam>
internal sealed class QueryPlan<T>
{
    /// <summary>Stores the base queryable that all composed operations are applied to.</summary>
    private readonly IQueryable<T> source;
    /// <summary>Stores the composed filter specification, when filtering has been requested.</summary>
    private FilterSpec<T>? filter;
    /// <summary>Stores the registered sort keys in the order they should be applied.</summary>
    private readonly List<(LambdaExpression KeySelector, SortDirection Direction)> ordering = [];
    /// <summary>Stores provider-specific eager-load expressions to be applied by execution extensions.</summary>
    private readonly List<LambdaExpression> includes = [];
    /// <summary>Stores the number of rows to skip for offset paging.</summary>
    private int? skip;
    /// <summary>Stores the number of rows to take for offset paging.</summary>
    private int? take;
    /// <summary>Stores the stable key selector used to guarantee deterministic cursor ordering.</summary>
    private LambdaExpression? key;
    /// <summary>Stores the opaque cursor token used to resume keyset pagination.</summary>
    private string? cursorToken;

    /// <summary>Initializes a query specification over the supplied source sequence.</summary>
    /// <param name="source">The base queryable to compose against.</param>
    private QueryPlan(IQueryable<T> source) => this.source = source;

    /// <summary>Starts a new query composition over <paramref name="source"/>.</summary>
    public static QueryPlan<T> For(IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new QueryPlan<T>(source);
    }

    /// <summary>Gets the navigation paths registered via <see cref="Include{TProperty}"/>, for provider extensions to apply.</summary>
    public IReadOnlyList<LambdaExpression> Includes => includes;

    /// <summary>Gets the registered sort clauses, in application order, for provider extensions that need direct access.</summary>
    public IReadOnlyList<(LambdaExpression KeySelector, SortDirection Direction)> Ordering => ordering;

    /// <summary>Gets the number of items to skip, if paging was requested.</summary>
    public int? Skip => skip;

    /// <summary>Gets the maximum number of items to return, if paging was requested.</summary>
    public int? Take => take;

    internal void ClearOrdering() => ordering.Clear();

    // ─── Filtering ───────────────────────────────────────────────────────────

    /// <summary>Adds a predicate, combined with any existing predicate using AND.</summary>
    public QueryPlan<T> Where(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        filter = filter is null ? new FilterSpec<T>(predicate) : filter.Where(predicate);
        return this;
    }

    /// <summary>
    /// Adds an explicitly grouped sub-clause, combined with any existing predicate using AND —
    /// for example <c>.Where(g => g.Where(o => o.IsPriority).Or(o => o.Total &gt; 1000))</c> produces
    /// <c>... AND (IsPriority OR Total &gt; 1000)</c>, with the parenthesization preserved.
    /// </summary>
    public QueryPlan<T> Where(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) => CombineGroup(group, useOr: false);

    /// <summary>Combines the current filter with <paramref name="predicate"/> using AND. Equivalent to <see cref="Where(Expression{Func{T,bool}})"/>.</summary>
    public QueryPlan<T> And(Expression<Func<T, bool>> predicate) => Where(predicate);

    /// <summary>
    /// Combines the current filter with an explicitly grouped sub-clause using AND. Equivalent to
    /// <see cref="Where(Func{FilterGroupBuilder{T},FilterGroupBuilder{T}})"/>.
    /// </summary>
    public QueryPlan<T> And(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) => Where(group);

    /// <summary>Combines the current filter with <paramref name="predicate"/> using OR.</summary>
    public QueryPlan<T> Or(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        filter = filter is null ? new FilterSpec<T>(predicate) : filter.Or(new(predicate));
        return this;
    }

    /// <summary>
    /// Combines the current filter with an explicitly grouped sub-clause using OR — for example
    /// <c>.Or(g => g.Where(o => o.IsPriority).And(o => o.Total &gt; 10))</c> produces
    /// <c>... OR (IsPriority AND Total &gt; 10)</c>, with the parenthesization preserved.
    /// </summary>
    public QueryPlan<T> Or(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) => CombineGroup(group, useOr: true);

    /// <summary>Adds a negated predicate, combined with any existing predicate using AND.</summary>
    public QueryPlan<T> Not(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        FilterSpec<T> negated = new FilterSpec<T>(predicate).Not();
        filter = filter is null ? negated : filter.And(negated);
        return this;
    }

    /// <summary>
    /// Adds a negated, explicitly grouped sub-clause, combined with any existing predicate using AND
    /// — for example <c>.Not(g => g.Where(o => o.IsPriority).And(o => o.IsArchived))</c> produces
    /// <c>... AND NOT (IsPriority AND IsArchived)</c>.
    /// </summary>
    public QueryPlan<T> Not(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group)
    {
        ArgumentNullException.ThrowIfNull(group);
        FilterSpec<T> negated = group(new FilterGroupBuilder<T>()).Build().Not();
        filter = filter is null ? negated : filter.And(negated);
        return this;
    }

    /// <summary>Builds a grouped filter and combines it with the current filter using AND or OR.</summary>
    /// <param name="group">The callback that composes the grouped filter.</param>
    /// <param name="useOr"><see langword="true"/> to combine the group with OR; <see langword="false"/> to combine it with AND.</param>
    /// <returns>The current query specification.</returns>
    private QueryPlan<T> CombineGroup(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group, bool useOr)
    {
        ArgumentNullException.ThrowIfNull(group);
        FilterSpec<T> built = group(new FilterGroupBuilder<T>()).Build();
        filter = filter is null ? built : (useOr ? filter.Or(built) : filter.And(built));
        return this;
    }

    /// <summary>
    /// Hydrates (merges with AND) the filter from a portable <see cref="FilterNode"/> — the shape
    /// deserialized from a request DTO. A <c>null</c> node is a no-op.
    /// </summary>
    public QueryPlan<T> ApplyFilter(FilterNode? node)
    {
        if (node is null) return this;
        FilterSpec<T> spec = FilterSpec<T>.FromFilterNode(node);
        filter = filter is null ? spec : filter.And(spec);
        return this;
    }

    /// <summary>Gets the composed filter, or <c>null</c> if none has been applied.</summary>
    public FilterSpec<T>? Filter => filter;

    // ─── Sorting ─────────────────────────────────────────────────────────────

    /// <summary>Adds an ascending sort key. The first call establishes the primary sort; subsequent calls add tie-breakers.</summary>
    public QueryPlan<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector) => AddOrdering(keySelector, SortDirection.Ascending);

    /// <summary>Adds a descending sort key. The first call establishes the primary sort; subsequent calls add tie-breakers.</summary>
    public QueryPlan<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector) => AddOrdering(keySelector, SortDirection.Descending);

    /// <summary>Adds an ascending tie-breaker sort key. Alias for <see cref="OrderBy{TKey}"/> for readability at call sites.</summary>
    public QueryPlan<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector) => AddOrdering(keySelector, SortDirection.Ascending);

    /// <summary>Adds a descending tie-breaker sort key. Alias for <see cref="OrderByDescending{TKey}"/> for readability at call sites.</summary>
    public QueryPlan<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector) => AddOrdering(keySelector, SortDirection.Descending);

    /// <summary>
    /// Hydrates (appends) sort clauses from the portable <see cref="SortClause"/> shape deserialized
    /// from a request DTO.
    /// </summary>
    public QueryPlan<T> ApplySort(IEnumerable<SortClause>? clauses)
    {
        if (clauses is null) return this;
        foreach (SortClause clause in clauses)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "item");
            Expression member = PocoFilterExpressionBuilder.Member(parameter, clause.Path);
            LambdaExpression keySelector = Expression.Lambda(member, parameter);
            ordering.Add((keySelector, clause.Direction));
        }
        return this;
    }

    private QueryPlan<T> AddOrdering<TKey>(Expression<Func<T, TKey>> keySelector, SortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ordering.Add((keySelector, direction));
        return this;
    }

    // ─── Cursor (keyset) pagination ──────────────────────────────────────────

    /// <summary>
    /// Registers the entity's stable identity key (typically its primary key). This key is the
    /// default sort order when no <see cref="OrderBy{TKey}"/> has been specified, and is always
    /// appended as the final tie-breaker after any explicit ordering — guaranteeing a total,
    /// deterministic order, which cursor pagination requires to be well-defined. User-supplied
    /// ordering always takes precedence as the primary sort; this key only breaks ties.
    /// </summary>
    /// <remarks>
    /// Prefer non-nullable keys/sort columns for cursor pagination. Seeking past a row whose sort
    /// value is <c>null</c> is not supported: database engines disagree on whether <c>NULL</c> sorts
    /// first or last, so there is no portable seek predicate for it. Filter out nulls, or supply a
    /// non-nullable substitute column, if a sort column can be null.
    /// </remarks>
    public QueryPlan<T> UseKey<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        key = keySelector;
        return this;
    }

    /// <summary>
    /// Supplies the opaque cursor token returned as <see cref="CursorPage{T}.NextCursor"/>
    /// from a previous page, to seek directly to the next page without an <c>OFFSET</c> scan. Pass
    /// <c>null</c> (or omit the call) to request the first page.
    /// </summary>
    /// <remarks>
    /// Requires <see cref="UseKey{TKey}"/> to have been called, and cannot be combined with
    /// <see cref="SkipItems"/>/<see cref="Page"/> (offset paging) — the two strategies are mutually
    /// exclusive. The cursor is not encrypted; it only encodes the current sort keys' values for the
    /// last row of the previous page, so it must not be relied on as a security boundary.
    /// </remarks>
    public QueryPlan<T> AfterCursor(string? cursor)
    {
        cursorToken = cursor;
        return this;
    }

    /// <summary>
    /// Builds the opaque cursor token that resumes immediately after <paramref name="lastItem"/>,
    /// using the currently-registered sort order (see <see cref="UseKey{TKey}"/>). Execution
    /// extensions call this once per page to compute <see cref="CursorPage{T}.NextCursor"/>.
    /// </summary>
    public string BuildCursor(T lastItem)
    {
        ArgumentNullException.ThrowIfNull(lastItem);
        if (key is null) throw new InvalidOperationException($"Cursor pagination requires {nameof(UseKey)}(...) to establish a stable sort/tie-breaker key.");

        IReadOnlyList<(LambdaExpression KeySelector, SortDirection Direction)> effective = EffectiveOrdering();
        string?[] values = new string?[effective.Count];
        for (int i = 0; i < effective.Count; i++)
        {
            object? value = effective[i].KeySelector.Compile().DynamicInvoke(lastItem);
            values[i] = PortableValueConverter.Format(value);
        }
        return Paging.CursorCodec.Encode(values);
    }

    /// <summary>
    /// Executes cursor (keyset) pagination against any <see cref="IQueryable{T}"/> provider: seeks
    /// directly past the supplied cursor (see <see cref="AfterCursor"/>) instead of using
    /// <c>OFFSET</c>, so query cost stays flat regardless of how deep the page is — the throughput
    /// benefit over offset paging. Requests one extra row to detect whether a further page exists.
    /// </summary>
    public CursorPage<T> ToCursorPage(int pageSize)
    {
        if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be 1 or greater.");

        List<T> rows = BuildCursorQueryable(pageSize + 1).ToList();
        bool hasMore = rows.Count > pageSize;
        if (hasMore) rows.RemoveAt(rows.Count - 1);
        string? nextCursor = hasMore ? BuildCursor(rows[^1]) : null;
        return new CursorPage<T>(rows, nextCursor);
    }

    /// <summary>
    /// Builds filter + seek predicate (if a cursor was supplied) + ordering, and applies
    /// <paramref name="take"/> in place of any <see cref="TakeItems"/> value. Internal so execution
    /// extensions (e.g. <c>Ifx.FilterSpec.EntityFrameworkCore</c>) can layer provider-specific
    /// concerns (<c>AsNoTracking</c>, <c>Include</c>, async execution) on top without duplicating
    /// the seek-predicate logic.
    /// </summary>
    internal IQueryable<T> BuildCursorQueryable(int take)
    {
        if (key is null) throw new InvalidOperationException($"Cursor pagination requires {nameof(UseKey)}(...) to establish a stable sort/tie-breaker key.");
        if (skip is not null) throw new InvalidOperationException($"Cursor pagination cannot be combined with {nameof(SkipItems)}/{nameof(Page)} (offset paging); the two strategies are mutually exclusive.");

        IReadOnlyList<(LambdaExpression KeySelector, SortDirection Direction)> effective = EffectiveOrdering();
        IQueryable<T> query = filter is null ? source : filter.Apply(source);
        if (cursorToken is not null)
            query = query.Where(BuildSeekPredicate(effective, DecodeCursorValues(effective, cursorToken)));

        return ApplyOrdering(query, effective).Take(take);
    }

    /// <summary>Decodes cursor payload values and converts them to the runtime types of the effective sort keys.</summary>
    /// <param name="effective">The effective ordering used to interpret the cursor payload.</param>
    /// <param name="token">The opaque cursor token to decode.</param>
    /// <returns>The decoded sort-key values in ordering position.</returns>
    private static object?[] DecodeCursorValues(IReadOnlyList<(LambdaExpression KeySelector, SortDirection Direction)> effective, string token)
    {
        IReadOnlyList<string?> raw = Paging.CursorCodec.Decode(token);
        if (raw.Count != effective.Count)
            throw new FormatException(
                $"The supplied cursor does not match the current sort (expected {effective.Count} value(s), found {raw.Count}). " +
                "This usually means the sort order changed between requests.");

        object?[] values = new object?[raw.Count];
        for (int i = 0; i < raw.Count; i++)
        {
            Type targetType = Nullable.GetUnderlyingType(effective[i].KeySelector.ReturnType) ?? effective[i].KeySelector.ReturnType;
            values[i] = PortableValueConverter.Parse(raw[i], targetType);
        }
        return values;
    }

    /// <summary>Builds the lexicographic seek predicate needed to continue after a cursor row.</summary>
    /// <param name="effective">The effective ordering that defines the cursor sequence.</param>
    /// <param name="values">The decoded sort-key values from the cursor.</param>
    /// <returns>An expression that matches rows appearing strictly after the cursor row.</returns>
    private static Expression<Func<T, bool>> BuildSeekPredicate(
        IReadOnlyList<(LambdaExpression KeySelector, SortDirection Direction)> effective, object?[] values)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "item");
        Expression[] members = new Expression[effective.Count];
        Expression[] constants = new Expression[effective.Count];
        for (int i = 0; i < effective.Count; i++)
        {
            Expression member = ParameterRebinder.Rebind(effective[i].KeySelector.Body, effective[i].KeySelector.Parameters[0], parameter);
            members[i] = member;
            // Constant is promoted to the member's own (possibly nullable) type, not the reverse,
            // preserving CLR lifted-nullable comparison semantics.
            constants[i] = Expression.Constant(values[i], member.Type);
        }

        Expression? disjunction = null;
        for (int i = 0; i < effective.Count; i++)
        {
            Expression clause = effective[i].Direction == SortDirection.Ascending
                ? Expression.GreaterThan(members[i], constants[i])
                : Expression.LessThan(members[i], constants[i]);
            for (int j = 0; j < i; j++)
                clause = Expression.AndAlso(Expression.Equal(members[j], constants[j]), clause);
            disjunction = disjunction is null ? clause : Expression.OrElse(disjunction, clause);
        }
        return Expression.Lambda<Func<T, bool>>(disjunction!, parameter);
    }

    /// <summary>
    /// Gets the ordering actually applied at execution time: the user-supplied <see cref="Ordering"/>
    /// with the registered <see cref="UseKey{TKey}"/> key appended as a final tie-breaker (or, if no
    /// ordering was supplied at all, the key alone). Returns <see cref="Ordering"/> unchanged if no
    /// key was registered, preserving prior behavior for callers that never opt into keys/cursors.
    /// </summary>
    private IReadOnlyList<(LambdaExpression KeySelector, SortDirection Direction)> EffectiveOrdering()
    {
        if (key is null) return ordering;
        if (ordering.Count == 0) return [(key, SortDirection.Ascending)];
        return [.. ordering, (key, SortDirection.Ascending)];
    }

    // ─── Paging ──────────────────────────────────────────────────────────────

    /// <summary>Skips the given number of items.</summary>
    public QueryPlan<T> SkipItems(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), count, "Skip count must be non-negative.");
        skip = count;
        return this;
    }

    /// <summary>Limits the result to the given number of items.</summary>
    public QueryPlan<T> TakeItems(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), count, "Take count must be non-negative.");
        take = count;
        return this;
    }

    /// <summary>Applies 1-based page-number pagination.</summary>
    public QueryPlan<T> Page(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "Page number must be 1 or greater.");
        if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be 1 or greater.");
        skip = (pageNumber - 1) * pageSize;
        take = pageSize;
        return this;
    }

    // ─── Eager loading ───────────────────────────────────────────────────────

    /// <summary>
    /// Registers a navigation property to eagerly load. Recorded as a typed expression so the
    /// executing provider (e.g. EF Core) can translate it with full compile-time safety; this
    /// package itself never interprets the path.
    /// </summary>
    public QueryPlan<T> Include<TProperty>(Expression<Func<T, TProperty>> navigationPath)
    {
        ArgumentNullException.ThrowIfNull(navigationPath);
        includes.Add(navigationPath);
        return this;
    }

    // ─── Materialization ─────────────────────────────────────────────────────

    /// <summary>
    /// Builds the composed <see cref="IQueryable{T}"/>: filter, then sort, then paging. Does not
    /// apply <see cref="Includes"/> — those are provider-specific and applied by an execution
    /// extension (see remarks on <see cref="QueryPlan{T}"/>).
    /// </summary>
    public IQueryable<T> ToQueryable()
    {
        IQueryable<T> query = filter is null ? source : filter.Apply(source);
        query = ApplyOrdering(query, EffectiveOrdering());
        if (skip is > 0) query = query.Skip(skip.Value);
        if (take is >= 0) query = query.Take(take.Value);
        return query;
    }

    /// <summary>
    /// Builds the composed <see cref="IQueryable{T}"/> with the filter and sort applied but without
    /// paging — used by execution extensions to compute an accurate total count.
    /// </summary>
    public IQueryable<T> ToUnpagedQueryable() => ApplyOrdering(filter is null ? source : filter.Apply(source), EffectiveOrdering());

    /// <summary>Applies the registered ordering sequence to the supplied query.</summary>
    /// <param name="query">The query to order.</param>
    /// <param name="ordering">The ordered list of key selectors and directions to apply.</param>
    /// <returns>The ordered query.</returns>
    private static IQueryable<T> ApplyOrdering(IQueryable<T> query, IReadOnlyList<(LambdaExpression KeySelector, SortDirection Direction)> ordering)
    {
        if (ordering.Count == 0) return query;

        IOrderedQueryable<T>? ordered = null;
        for (int i = 0; i < ordering.Count; i++)
        {
            (LambdaExpression keySelector, SortDirection direction) = ordering[i];
            ordered = i == 0
                ? ApplyOrderBy(query, keySelector, direction)
                : ApplyThenBy(ordered!, keySelector, direction);
        }
        return ordered!;
    }

    /// <summary>Applies the initial <c>OrderBy</c> or <c>OrderByDescending</c> call for a sort key.</summary>
    /// <param name="query">The query to order.</param>
    /// <param name="keySelector">The sort-key selector.</param>
    /// <param name="direction">The requested sort direction.</param>
    /// <returns>The ordered query.</returns>
    private static IOrderedQueryable<T> ApplyOrderBy(IQueryable<T> query, LambdaExpression keySelector, SortDirection direction) =>
        Invoke(query, direction == SortDirection.Ascending ? "OrderBy" : "OrderByDescending", keySelector);

    /// <summary>Applies a subsequent <c>ThenBy</c> or <c>ThenByDescending</c> call for a tie-breaker key.</summary>
    /// <param name="query">The already-ordered query.</param>
    /// <param name="keySelector">The tie-breaker key selector.</param>
    /// <param name="direction">The requested sort direction.</param>
    /// <returns>The ordered query with the additional key applied.</returns>
    private static IOrderedQueryable<T> ApplyThenBy(IOrderedQueryable<T> query, LambdaExpression keySelector, SortDirection direction) =>
        Invoke(query, direction == SortDirection.Ascending ? "ThenBy" : "ThenByDescending", keySelector);

    /// <summary>Invokes the matching <see cref="Queryable"/> ordering method for a runtime key type.</summary>
    /// <param name="query">The query instance to pass to the LINQ operator.</param>
    /// <param name="methodName">The <see cref="Queryable"/> ordering method name to invoke.</param>
    /// <param name="keySelector">The key selector expression supplying the sort key.</param>
    /// <returns>The ordered query returned by the invoked LINQ operator.</returns>
    private static IOrderedQueryable<T> Invoke(IQueryable<T> query, string methodName, LambdaExpression keySelector)
    {
        Type keyType = keySelector.ReturnType;
        System.Reflection.MethodInfo method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), keyType);
        return (IOrderedQueryable<T>)method.Invoke(null, [query, keySelector])!;
    }
}
