using System.Linq.Expressions;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Filtering.Poco;

public sealed class PocoFilterExecutionStrategy : IFilterExecutionStrategy
{
    public IQueryable<T> Apply<T>(IQueryable<T> source, FilterNode? filter)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (filter is null) return source;
        return source.Where(FilterExpression.Create<T>(filter));
    }

    public IEnumerable<T> Apply<T>(IEnumerable<T> source, FilterNode? filter)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (filter is null) return source;
        return source.Where(FilterExpression.Create<T>(filter).Compile());
    }
}
