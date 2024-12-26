using System.Linq.Expressions;

namespace vc.Ifx.Extensions;

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
        query = query.ApplyOrdering(filter);
        query = query.ApplyPagination(filter);
        return query;
    }

    /// <summary>  
    /// Applies property value filtering based on the provided filter.  
    /// </summary>  
    /// <typeparam name="T">The type of the entities in the query.</typeparam>  
    /// <param name="query">The query to filter.</param>  
    /// <param name="filter">The filter containing the property values to apply.</param>  
    /// <param name="caseInsensitive">Indicates whether the filtering should be case-insensitive.</param>  
    /// <returns>The filtered query.</returns>  
    public static IQueryable<T> ApplyPropertyValueFiltering<T>(this IQueryable<T> query, Filter<T> filter, bool caseInsensitive = true) where T : class
    {
        if (filter.PropertyValues.Count <= 0)
        {
            return query;
        }

        foreach (var (key, value) in filter.PropertyValues)
        {
            var propertyValue = value?.ToString() ?? string.Empty;

            if (typeof(T).GetProperty(key) == null)
            {
                continue; // Skip if property does not exist
            }

            // Use a common expression for filtering
            var predicate = BuildPredicate<T>(key, propertyValue, caseInsensitive);
            query = query.Where(predicate);
        }
        return query;
    }

    /// <summary>
    /// Builds a predicate expression for filtering.
    /// </summary>
    private static Expression<Func<T, bool>> BuildPredicate<T>(string propertyName, string propertyValue, bool caseInsensitive)
    {

        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, propertyName);
        var constant = Expression.Constant(propertyValue);

        Expression body;
        if (caseInsensitive)
        {
            var toStringCall = Expression.Call(property, "ToString", null);
            var toLowerCall = Expression.Call(toStringCall, "ToLower", null);
            var valueToLower = Expression.Constant(propertyValue.ToLower());
            body = Expression.Call(toLowerCall, "Contains", null, valueToLower);
        }
        else
        {
            var toStringCall = Expression.Call(property, "ToString", null);
            body = Expression.Call(toStringCall, "Contains", null, constant);
        }
        return Expression.Lambda<Func<T, bool>>(body, parameter);

    }

    /// <summary>
    /// Applies ordering based on the provided filter.
    /// </summary>
    public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, Filter<T> filter) where T : class
    {
        
        if (!string.IsNullOrEmpty(filter.OrderBy))
        {
            query = ApplyOrder(query, filter.OrderBy, filter.OrderByDescending);
        }

        if (!string.IsNullOrEmpty(filter.ThenBy))
        {
            query = ApplyOrder(query, filter.ThenBy, filter.ThenByDescending, true);
        }
        return query;

    }

    private static IQueryable<T> ApplyOrder<T>(IQueryable<T> query, string propertyName, bool descending, bool thenBy = false) where T : class
    {

        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = thenBy
            ? (descending ? "ThenByDescending" : "ThenBy")
            : (descending ? "OrderByDescending" : "OrderBy");

        var resultExpression = Expression.Call(typeof(Queryable), methodName, [typeof(T), property.Type], query.Expression, Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);

    }

    /// <summary>
    /// Applies pagination based on the provided filter.
    /// </summary>
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, Filter<T> filter) where T : class
    {
        if (filter.Offset.HasValue)
        {
            query = query.Skip(filter.Offset.Value);
        }

        if (filter.Limit.HasValue)
        {
            query = query.Take(filter.Limit.Value);
        }
        return query;
    }

}
