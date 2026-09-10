using System.Linq.Expressions;
using VisionaryCoder.Framework.Filtering;
using Core = VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Querying.Serialization;

/// <summary>Adapts legacy serialized filters to validated provider expressions.</summary>
public static class QueryFilterRehydrator
{
    /// <summary>Rehydrates a filter without executing a query or dropping invalid children.</summary>
    public static QueryFilter<T> ToQueryFilter<T>(this FilterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        FilterNode snapshot = QueryFilterSerializer.Deserialize(QueryFilterSerializer.Serialize(node))!;
        return Build<T>(snapshot);
    }

    private static QueryFilter<T> Build<T>(FilterNode node)
    {
        if (node is PropertyFilter property) return BuildProperty<T>(property);
        // The structural reader has validated and snapshotted every node in this tree.
        var composite = (CompositeFilter)node;
        return composite.Operator == "Not" ? Build<T>(composite.Children[0]).Not()
            : composite.Children.Select(Build<T>).Join(composite.Operator == "And");
    }

    private static QueryFilter<T> BuildProperty<T>(PropertyFilter property)
    {
        if (property.IgnoreCase)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "item");
            Expression member = parameter;
            foreach (string segment in property.Property.Split('.')) member = Expression.PropertyOrField(member, segment);
            if (member.Type != typeof(string)) throw new NotSupportedException("ignoreCase requires a string member.");
            if (property.Operator is not ("Contains" or "StartsWith" or "EndsWith" or "Equals" or "NotEquals"))
                throw new NotSupportedException("ignoreCase is supported for string matching and equality only.");
            if (property.Value is null && property.Operator is not ("Equals" or "NotEquals"))
                throw new ArgumentException("String matching requires a non-null value.", nameof(property));
            Expression value = Expression.Constant(property.Value, typeof(string));
            Expression body = property.Operator is "Equals" or "NotEquals"
                ? Expression.Call(typeof(string), nameof(string.Equals), Type.EmptyTypes, member, value, Expression.Constant(StringComparison.OrdinalIgnoreCase))
                : Expression.AndAlso(Expression.NotEqual(member, Expression.Constant(null, typeof(string))),
                    Expression.Call(member, property.Operator, Type.EmptyTypes, value, Expression.Constant(StringComparison.OrdinalIgnoreCase)));
            if (property.Operator == "NotEquals") body = Expression.Not(body);
            return new QueryFilter<T>(Expression.Lambda<Func<T, bool>>(body, parameter));
        }
        Core.FilterOperation operation = QueryFilterOperations.All[property.Operator];
        Core.FilterNode condition = new Core.FilterCondition(property.Property, operation, property.Value);
        if (property.Operator == "NotIn") condition = new Core.FilterNegation(condition);
        return new QueryFilter<T>(FilterExpression.Create<T>(condition));
    }
}
