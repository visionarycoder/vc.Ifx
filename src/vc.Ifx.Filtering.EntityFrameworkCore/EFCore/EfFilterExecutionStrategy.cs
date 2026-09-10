using Microsoft.EntityFrameworkCore;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Filtering.EFCore;

/// <summary>Applies portable predicates without leaving the source query provider.</summary>
public sealed class EfFilterExecutionStrategy : IFilterExecutionStrategy
{
    /// <summary>Validates the caller-owned context retained by the compatible constructor.</summary>
    public EfFilterExecutionStrategy(DbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
    }

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
