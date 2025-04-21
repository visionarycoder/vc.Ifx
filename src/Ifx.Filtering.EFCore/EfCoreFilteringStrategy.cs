using Ifx.Filtering.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Ifx.Filtering.EFCore;

public class EfCoreFilteringStrategy<T> : IFilteringStrategy<T> where T : class
{
    public IQueryable<T> ApplyFiltering(IQueryable<T> query, Filter<T> filter)
    {
        foreach (var criterion in filter.Criteria.Values)
        {
            if (typeof(T).GetProperty(criterion.PropertyName) == null)
                continue;

            if (query.Provider is EntityQueryProvider) query = ApplyEfCoreFiltering(query, criterion);
        }

        return query;
    }

    private static IQueryable<T> ApplyEfCoreFiltering(IQueryable<T> query, Filter.Criterion criterion)
    {
        var propertyName = criterion.PropertyName;
        var value = criterion.PropertyValue?.ToString() ?? string.Empty;

        return criterion.ComparisonOperator switch
        {
            Filter.ComparisonOperatorOption.Equals => query.Where(e => EF.Property<object>(e, propertyName).Equals(criterion.PropertyValue)),
            Filter.ComparisonOperatorOption.NotEquals => query.Where(e => !EF.Property<object>(e, propertyName).Equals(criterion.PropertyValue)),
            Filter.ComparisonOperatorOption.GreaterThan => query.Where(e => EF.Property<IComparable>(e, propertyName).CompareTo(criterion.PropertyValue) > 0),
            Filter.ComparisonOperatorOption.LessThan => query.Where(e => EF.Property<IComparable>(e, propertyName).CompareTo(criterion.PropertyValue) < 0),
            Filter.ComparisonOperatorOption.Contains => query.Where(e => EF.Functions.Like(EF.Property<string>(e, propertyName), $"%{value}%")),
            _ => throw new NotSupportedException($"ComparisonOperator {criterion.ComparisonOperator} is not supported for EFCore filtering.")
        };
    }
}