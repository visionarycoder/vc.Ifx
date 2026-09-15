using System.Linq.Expressions;
using Ifx.Filtering.Abstractions;
using Ifx.Filtering.Poco;

namespace Ifx.Filtering;

/// <summary>Creates provider-consumable expressions from portable filter nodes.</summary>
public static class FilterExpression
{
    /// <summary>Builds a predicate; a null filter matches every item. Invalid filters throw.</summary>
    public static Expression<Func<T, bool>> Create<T>(FilterNode? filter)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "item");
        return Expression.Lambda<Func<T, bool>>(PocoFilterExpressionBuilder.BuildExpression<T>(filter, parameter), parameter);
    }
}
