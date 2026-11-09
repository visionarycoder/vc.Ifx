using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Ifx.Querying;

namespace Ifx.Querying.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core execution extensions for <see cref="QuerySpec{T}"/>: no-tracking reads,
/// typed <c>Include</c> application, and paging using sequential page and count queries.
/// </summary>
public static class QuerySpecEfCoreExtensions
{
    /// <summary>
    /// Caches the generic Entity Framework Core Include method used to apply typed include expressions.
    /// </summary>
    private static readonly MethodInfo IncludeMethod = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(method => method.Name == nameof(EntityFrameworkQueryableExtensions.Include)
                           && method.GetGenericArguments().Length == 2
                           && method.GetParameters()[1].ParameterType.IsGenericType
                           && method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>));

    /// <summary>
    /// Applies filtering, ordering, paging and registered <c>Include</c> paths, then executes the
    /// query with <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}"/> semantics.
    /// </summary>
    public static async Task<List<T>> ToListAsync<T>(this QuerySpec<T> spec, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        IQueryable<T> query = ApplyIncludes(spec.ToQueryable(), spec).AsNoTracking();
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the filtered query and then its matching total count, returning a
    /// <see cref="PagedResult{T}"/>. The count query ignores paging (Skip/Take) but honors the filter.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this QuerySpec<T> spec, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);

        IQueryable<T> pagedQuery = ApplyIncludes(spec.ToQueryable(), spec).AsNoTracking();
        IQueryable<T> unpagedQuery = spec.ToUnpagedQueryable();

        List<T> items = await pagedQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        int count = await unpagedQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<T>(items, count);
    }

    /// <summary>Counts the filtered results without paging.</summary>
    public static Task<int> CountAsync<T>(this QuerySpec<T> spec, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        return spec.ToUnpagedQueryable().CountAsync(cancellationToken);
    }

    /// <summary>Checks whether the filtered query has results without paging.</summary>
    public static Task<bool> AnyAsync<T>(this QuerySpec<T> spec, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        return spec.ToUnpagedQueryable().AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Executes cursor (keyset) pagination: seeks directly past the cursor supplied via
    /// <see cref="QuerySpec{T}.AfterCursor"/> using an indexed <c>WHERE</c> predicate instead of
    /// <c>OFFSET</c>, so throughput stays flat at any page depth. Requires
    /// <see cref="QuerySpec{T}.UseKey{TKey}"/> to have been called. Applies registered
    /// <c>Include</c> paths and <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}"/>.
    /// </summary>
    public static async Task<CursorPage<T>> ToCursorPageAsync<T>(this QuerySpec<T> spec, int pageSize, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be 1 or greater.");

        IQueryable<T> query = ApplyIncludes(spec.BuildCursorQueryable(pageSize + 1), spec).AsNoTracking();
        List<T> rows = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        bool hasMore = rows.Count > pageSize;
        if (hasMore) rows.RemoveAt(rows.Count - 1);
        string? nextCursor = hasMore ? spec.BuildCursor(rows[^1]) : null;
        return new CursorPage<T>(rows, nextCursor);
    }

    /// <summary>
    /// Applies the registered include expressions to an Entity Framework Core query.
    /// </summary>
    /// <typeparam name="T">The entity type being queried.</typeparam>
    /// <param name="query">The base query to extend.</param>
    /// <param name="spec">The query specification that provides include expressions.</param>
    /// <returns>The query with each registered include applied.</returns>
    private static IQueryable<T> ApplyIncludes<T>(IQueryable<T> query, QuerySpec<T> spec) where T : class
    {
        foreach (LambdaExpression include in spec.Includes)
        {
            MethodInfo generic = IncludeMethod.MakeGenericMethod(typeof(T), include.ReturnType);
            query = (IQueryable<T>)generic.Invoke(null, [query, include])!;
        }
        return query;
    }
}
