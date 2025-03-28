using System.Linq.Expressions;
using Ifx.Filtering.Contracts;

namespace Ifx.Filtering.Linq;

public class LinqFilteringStrategy<T> : IFilteringStrategy<T> where T : class
{
    public IQueryable<T> ApplyFiltering(IQueryable<T> query, Filter<T> filter)
    {
        foreach (var criterion in filter.Criteria.Values)
        {
            if (typeof(T).GetProperty(criterion.PropertyName) == null)
                continue;

            var predicate = BuildPredicate(criterion);
            query = query.Where(predicate);
        }

        return query;
    }

    private Expression<Func<T, bool>> BuildPredicate(Filter.Criterion criterion)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, criterion.PropertyName);
        var constant = Expression.Constant(criterion.PropertyValue);

        Expression body;
        switch (criterion.ComparisonOperator)
        {
            case Filter.ComparisonOperatorOption.Equals:
                body = Expression.Equal(property, constant);
                break;
            case Filter.ComparisonOperatorOption.NotEquals:
                body = Expression.NotEqual(property, constant);
                break;
            case Filter.ComparisonOperatorOption.GreaterThan:
                body = Expression.GreaterThan(property, constant);
                break;
            case Filter.ComparisonOperatorOption.LessThan:
                body = Expression.LessThan(property, constant);
                break;
            case Filter.ComparisonOperatorOption.Contains when criterion.IgnoreCase == Filter.IgnoreCaseOption.Yes:
                body = BuildCaseInsensitiveContains(property, constant);
                break;
            case Filter.ComparisonOperatorOption.Contains:
                body = Expression.Call(property, "Contains", null, constant);
                break;
            default:
                throw new NotSupportedException($"ComparisonOperator {criterion.ComparisonOperator} is not supported.");
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression BuildCaseInsensitiveContains(Expression property, Expression constant)
    {
        var toStringCall = Expression.Call(property, "ToString", null);
        var toLowerCall = Expression.Call(toStringCall, "ToLower", null);
        var valueToLower = Expression.Call(constant, "ToString", null);
        var constantToLower = Expression.Call(valueToLower, "ToLower", null);
        return Expression.Call(toLowerCall, "Contains", null, constantToLower);
    }
}