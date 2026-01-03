using System.Linq.Expressions;
using System.Reflection;

namespace VisionaryCoder.Framework.Querying.Serialization;

public static class QueryFilterRehydrator
{
    public static QueryFilter<T> ToQueryFilter<T>(this FilterNode node)
    {
        return node switch
        {
            PropertyFilter pf => BuildPropertyFilter<T>(pf),
            CompositeFilter cf => BuildCompositeFilter<T>(cf),
            _ => throw new NotSupportedException($"Unknown filter node: {node?.GetType().Name}")
        };
    }

    private static QueryFilter<T> BuildPropertyFilter<T>(PropertyFilter pf)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        MemberExpression prop = Expression.PropertyOrField(param, pf.Property);
        ConstantExpression constant = Expression.Constant(pf.Value ?? string.Empty);

        Expression body = pf.Operator switch
        {
            "Contains" => CallStringMethod(prop, "Contains", constant, pf.IgnoreCase),
            "StartsWith" => CallStringMethod(prop, "StartsWith", constant, pf.IgnoreCase),
            "EndsWith" => CallStringMethod(prop, "EndsWith", constant, pf.IgnoreCase),
            _ => throw new NotSupportedException($"Unsupported operator {pf.Operator}")
        };

        return new QueryFilter<T>(Expression.Lambda<Func<T, bool>>(body, param));
    }

    private static QueryFilter<T> BuildCompositeFilter<T>(CompositeFilter cf)
    {
        if (cf is { Operator: "Not", Children.Count: 1 })
            return cf.Children[0].ToQueryFilter<T>().Not();

        return cf.Operator switch
        {
            "And" => cf.Children.Select(c => c.ToQueryFilter<T>()).Join(useAnd: true),
            "Or" => cf.Children.Select(c => c.ToQueryFilter<T>()).Join(useAnd: false),
            _ => throw new NotSupportedException($"Unsupported composite operator {cf.Operator}")
        };
    }

    private static Expression CallStringMethod(Expression prop, string method, ConstantExpression constant, bool ignoreCase)
    {
        if (!ignoreCase)
        {
            return Expression.Call(prop, typeof(string).GetMethod(method, [typeof(string)])!, constant);
        }

        MethodInfo toLower = typeof(string).GetMethod(nameof(string.ToLowerInvariant), Type.EmptyTypes)!;
        MethodCallExpression loweredProp = Expression.Call(prop, toLower);
        ConstantExpression loweredConst = Expression.Constant(((string)constant.Value!).ToLowerInvariant());
        return Expression.Call(loweredProp, typeof(string).GetMethod(method, [typeof(string)])!, loweredConst);
    }
}
