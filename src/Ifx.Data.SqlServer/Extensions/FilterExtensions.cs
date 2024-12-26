using System.Linq.Expressions;

namespace vc.Ifx.Data.Extensions;

public static class FilterExtensions
{

    /// <summary>  
    /// Applies the specified filter to the query, including property value filtering, order filtering, and pagination filtering.  
    /// </summary>  
    /// <typeparam name="T">The type of the entities in the query.</typeparam>  
    /// <param name="query">The query to apply the filter to.</param>  
    /// <param name="filter">The filter containing the criteria to apply.</param>  
    /// <param name="caseInsensitive">Indicates whether the filtering should be case-insensitive.</param>  
    /// <returns>The filtered query.</returns>  
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, Filter<T> filter, bool caseInsensitive) where T : class
    {
        query = query.ApplyPropertyValueFiltering(filter, caseInsensitive);
        query = query.ApplyOrderFiltering(filter);
        query = query.ApplyPaginationFiltering(filter);
        return query;
    }

    /// <summary>  
    /// Applies pagination filtering to the query based on the specified filter.  
    /// </summary>  
    /// <typeparam name="T">The type of the entities in the query.</typeparam>  
    /// <param name="query">The query to apply the pagination to.</param>  
    /// <param name="filter">The filter containing pagination criteria.</param>  
    /// <returns>The paginated query.</returns>  
    public static IQueryable<T> ApplyPaginationFiltering<T>(this IQueryable<T> query, Filter<T> filter) where T : class
    {
        if (filter is { Offset: not null, Limit: not null })
        {
            query = query
                .Skip((filter.Offset.Value - 1) * filter.Limit.Value)
                .Take(filter.Limit.Value);
        }
        return query;
    }

    /// <summary>  
    /// Applies order filtering to the query based on the specified filter.  
    /// </summary>  
    /// <typeparam name="T">The type of the entities in the query.</typeparam>  
    /// <param name="query">The query to apply the order to.</param>  
    /// <param name="filter">The filter containing order criteria.</param>  
    /// <returns>The ordered query.</returns>  
    public static IQueryable<T> ApplyOrderFiltering<T>(this IQueryable<T> query, Filter<T> filter) where T : class
    {
        if (string.IsNullOrEmpty(filter.OrderBy))
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "p");
        var orderByExpression = Expression.Lambda(Expression.Property(parameter, filter.OrderBy), parameter);

        var methodName = filter.OrderByDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(typeof(Queryable), methodName, [typeof(T), orderByExpression.Body.Type], query.Expression, Expression.Quote(orderByExpression));
        query = query.Provider.CreateQuery<T>(resultExpression);

        if (string.IsNullOrEmpty(filter.ThenBy))
        {
            return query;
        }

        var thenByExpression = Expression.Lambda(Expression.Property(parameter, filter.ThenBy), parameter);
        methodName = filter.ThenByDescending ? "ThenByDescending" : "ThenBy";
        resultExpression = Expression.Call(typeof(Queryable), methodName, [typeof(T), thenByExpression.Body.Type], query.Expression, Expression.Quote(thenByExpression));
        query = query.Provider.CreateQuery<T>(resultExpression);
        return query;
    }

    /// <summary>  
    /// Applies property value filtering to the query based on the specified filter.  
    /// </summary>  
    /// <typeparam name="T">The type of the entities in the query.</typeparam>  
    /// <param name="query">The query to apply the filter to.</param>  
    /// <param name="filter">The filter containing property values to filter by.</param>  
    /// <param name="caseInsensitive">Indicates whether the filtering should be case-insensitive.</param>  
    /// <returns>The filtered query.</returns>  
    public static IQueryable<T> ApplyPropertyValueFiltering<T>(this IQueryable<T> query, Filter<T> filter, bool caseInsensitive = true) where T : class
    {

        if (filter.PropertyValues.Count <= 0)
        {
            return query;
        }

        if (typeof(Entity).IsAssignableFrom(typeof(T)))
        {
            foreach (var kvp in filter.PropertyValues)
            {
                query = caseInsensitive
                    ? query.Where(e => EF.Functions.Like(EF.Property<string>(e, kvp.Key), (kvp.Value != null ? kvp.Value.ToString() : "")))
                    : query.Where(e => EF.Property<object>(e, kvp.Key) == kvp.Value);
            }
            return query;
        }

        foreach (var (propertyName, value) in filter.PropertyValues)
        {
            var propertyValue = value?.ToString() ?? string.Empty;
            query = caseInsensitive
                ? query.Where(e => e.GetType().GetProperty(propertyName)!.GetValue(e)!.ToString()!.Contains(propertyValue, StringComparison.OrdinalIgnoreCase) == true)
                : query.Where(e => e.GetType().GetProperty(propertyName)!.GetValue(e)!.ToString()! == propertyValue);
        }

        return query;
    }
    
    /// <summary>  
    /// Converts a filter of one type to a filter of another type.  
    /// </summary>  
    /// <typeparam name="TSource">The source type of the filter.</typeparam>  
    /// <typeparam name="TTarget">The target type of the filter.</typeparam>  
    /// <param name="source">The source filter to convert.</param>  
    /// <returns>The converted filter.</returns>  
    public static Filter<TTarget> Convert<TSource, TTarget>(this Filter<TSource> source) where TSource : class where TTarget : class
    {
        var target = new Filter<TTarget>
        {
            PropertyValues = source.PropertyValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Offset = source.Offset,
            Limit = source.Limit,
            OrderBy = source.OrderBy,
            OrderByDescending = source.OrderByDescending,
            ThenBy = source.ThenBy,
            ThenByDescending = source.ThenByDescending
        };
        return target;
    }

}
