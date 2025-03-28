using System.Linq.Expressions;
using System.Reflection;

namespace Ifx.Filtering.Extensions;

/// <summary>
///     Provides extension methods for applying filters, ordering, and pagination to IQueryable objects.
/// </summary>
public static class QueryableExtensions
{

    // Get MethodInfo references for ToString, ToLower, and Contains
    private static readonly MethodInfo ToStringMethod = typeof(object).GetMethod("ToString", Type.EmptyTypes)!;
    private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;

    /// <summary>
    ///     Applies the specified filter to the query.
    /// </summary>
    /// <typeparam name="T">The type of the entity being queried.</typeparam>
    /// <param name="query">The query to apply the filter to.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, Filter<T> filter) where T : class
    {
        foreach (var criterion in filter.Criteria.Values)
        {
            var property = typeof(T).GetProperty(criterion.PropertyName);
            if (property == null) continue;

            var predicate = BuildPredicate<T>(criterion);
            query = query.Where(predicate);
        }

        query = query.ApplyOrdering(filter);
        query = query.ApplyPagination(filter);
        return query;
    }

    /// <summary>
    ///     Applies ordering to the query based on the specified filter.
    /// </summary>
    /// <typeparam name="T">The type of the entity being queried.</typeparam>
    /// <param name="query">The query to apply ordering to.</param>
    /// <param name="filter">The filter containing ordering information.</param>
    /// <returns>The ordered query.</returns>
    private static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, Filter<T> filter) where T : class
    {
        if (string.IsNullOrEmpty(filter.OrderBy)) return query;
        query = ApplyOrderingImp(query, filter.OrderBy, filter.OrderByDirection);

        if (!string.IsNullOrEmpty(filter.ThenBy)) query = ApplyOrderingImp(query, filter.ThenBy, filter.ThenByDirection, true);
        return query;
    }

    /// <summary>
    ///     Applies ordering to the query based on the specified property name and direction.
    /// </summary>
    /// <typeparam name="T">The type of the entity being queried.</typeparam>
    /// <param name="query">The query to apply ordering to.</param>
    /// <param name="propertyName">The name of the property to order by.</param>
    /// <param name="direction">The direction of the ordering.</param>
    /// <param name="thenBy">Indicates whether this is a secondary ordering.</param>
    /// <returns>The ordered query.</returns>
    private static IQueryable<T> ApplyOrderingImp<T>(IQueryable<T> query, string propertyName, Filter.SortDirectionOption direction, bool thenBy = false) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = thenBy
            ? direction == Filter.SortDirectionOption.Descending ? "ThenByDescending" : "ThenBy"
            : direction == Filter.SortDirectionOption.Descending
                ? "OrderByDescending"
                : "OrderBy";
        var resultExpression = Expression.Call(typeof(Queryable), methodName, [typeof(T), property.Type], query.Expression, Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    /// <summary>
    ///     Applies pagination to the query based on the specified filter.
    /// </summary>
    /// <typeparam name="T">The type of the entity being queried.</typeparam>
    /// <param name="query">The query to apply pagination to.</param>
    /// <param name="filter">The filter containing pagination information.</param>
    /// <returns>The paginated query.</returns>
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, Filter<T> filter) where T : class
    {
        if (filter.Skip is > 0) 
            query = query.Skip(filter.Skip.Value);

        if (filter.Take is not > 0) 
            return query;

        // If take is valid, but skip is not, set skip to 0
        if (filter.Skip is null or < 0) 
            query = query.Skip(0);
        query = query.Take(filter.Take.Value);
        return query;
    }

    /// <summary>
    ///     Builds a predicate expression based on the specified criterion.
    /// </summary>
    /// <typeparam name="T">The type of the entity being queried.</typeparam>
    /// <param name="criterion">The criterion to build the predicate from.</param>
    /// <returns>The predicate expression.</returns>
    private static Expression<Func<T, bool>> BuildPredicate<T>(Filter.Criterion criterion) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, criterion.PropertyName);
        var constant = Expression.Constant(criterion.PropertyValue);

        var body = criterion.ComparisonOperator switch
        {
            Filter.ComparisonOperatorOption.Equals => Expression.Equal(property, constant),
            Filter.ComparisonOperatorOption.NotEquals => Expression.NotEqual(property, constant),
            Filter.ComparisonOperatorOption.GreaterThan => Expression.GreaterThan(property, constant),
            Filter.ComparisonOperatorOption.LessThan => Expression.LessThan(property, constant),
            Filter.ComparisonOperatorOption.Contains when criterion.IgnoreCase == Filter.IgnoreCaseOption.Yes => BuildCaseInsensitiveContains(property, constant),
            Filter.ComparisonOperatorOption.Contains => Expression.Call(property, "Contains", null, constant),
            _ => throw new NotSupportedException($"ComparisonOperator {criterion.ComparisonOperator} is not supported.")
        };

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    /// <summary>
    ///     Builds a case-insensitive "Contains" expression.
    /// </summary>
    /// <param name="property">The property expression.</param>
    /// <param name="constant">The constant expression.</param>
    /// <returns>The case-insensitive "Contains" expression.</returns>
    private static Expression BuildCaseInsensitiveContains(Expression property, Expression constant)
    {
        // Convert property to string and to lowercase
        var propertyToString = Expression.Call(property, ToStringMethod);
        var propertyToLower = Expression.Call(propertyToString, ToLowerMethod);

        // Convert constant to string and to lowercase
        var constantToString = Expression.Call(constant, ToStringMethod);
        var constantToLower = Expression.Call(constantToString, ToLowerMethod);

        // Build the "Contains" call
        return Expression.Call(propertyToLower, ContainsMethod, constantToLower);
    }
}