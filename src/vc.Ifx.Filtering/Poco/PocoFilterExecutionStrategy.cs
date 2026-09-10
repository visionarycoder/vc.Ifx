using System.Linq.Expressions;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Filtering.Poco;

public sealed class PocoFilterExecutionStrategy : IFilterExecutionStrategy
{
    public IQueryable<T> Apply<T>(IQueryable<T> source, FilterNode? filter)
    {
        if (filter is null) return source;

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        Expression? body = PocoFilterExpressionBuilder.BuildExpression<T>(filter, parameter);
        if (body is null) return source;

        var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
        return source.Where(lambda);
    }

    public IEnumerable<T> Apply<T>(IEnumerable<T> source, FilterNode? filter)
    {
        if (filter is null) return source;
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        Expression? body = PocoFilterExpressionBuilder.BuildExpression<T>(filter, parameter);
        if (body is null) return source;

        Func<T, bool> lambda = Expression.Lambda<Func<T, bool>>(body, parameter).Compile();
        return source.Where(lambda);
    }
}
