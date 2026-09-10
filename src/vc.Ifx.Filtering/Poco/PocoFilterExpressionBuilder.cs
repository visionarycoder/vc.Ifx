using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Filtering.Poco;

internal static class PocoFilterExpressionBuilder
{
    public static Expression BuildExpression<T>(FilterNode? filter, ParameterExpression parameter) =>
        filter is null ? Expression.Constant(true) : Build(filter, parameter);

    private static Expression Build(FilterNode node, ParameterExpression parameter)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node switch
        {
            FilterConstant constant => Expression.Constant(constant.Value),
            FilterNegation not => Expression.Not(Build(not.Operand, parameter)),
            FilterGroup group => BuildGroup(group, parameter),
            FilterCondition condition => BuildCondition(condition, parameter),
            FilterCollectionCondition collection => BuildCollection(collection, parameter),
            _ => throw new NotSupportedException($"Filter node '{node.GetType().Name}' is not supported.")
        };
    }

    private static Expression BuildGroup(FilterGroup group, ParameterExpression parameter)
    {
        if (!Enum.IsDefined(group.Combination)) throw new ArgumentOutOfRangeException(nameof(group));
        Expression combined = Expression.Constant(group.Combination == FilterCombination.And);
        foreach (FilterNode child in group.Children)
        {
            Expression next = Build(child, parameter);
            combined = group.Combination == FilterCombination.And
                ? Expression.AndAlso(combined, next) : Expression.OrElse(combined, next);
        }
        return combined;
    }

    private static Expression BuildCondition(FilterCondition condition, ParameterExpression parameter)
    {
        Expression member = Member(parameter, condition.Path);
        if (condition.Operator == FilterOperation.In)
        {
            string?[] items = JsonSerializer.Deserialize<string?[]>(condition.Value ??
                throw new ArgumentException("Membership requires a JSON array.", nameof(condition)))
                ?? throw new ArgumentException("Membership requires a JSON array.", nameof(condition));
            NewArrayExpression values = Expression.NewArrayInit(member.Type,
                items.Select(item => Constant(item, member.Type)));
            return Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [member.Type], values, member);
        }
        if (condition.Operator == FilterOperation.Contains && member.Type != typeof(string))
        {
            Type elementType = ElementType(member.Type);
            return Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [elementType],
                member, Constant(condition.Value, elementType));
        }
        if (condition.Operator is FilterOperation.Contains or FilterOperation.StartsWith or FilterOperation.EndsWith)
        {
            if (member.Type != typeof(string)) throw new NotSupportedException("String operations require a string member.");
            if (condition.Value is null) throw new ArgumentException("String operations require a non-null value.", nameof(condition));
            return Expression.Call(member, condition.Operator.ToString(), Type.EmptyTypes,
                Expression.Constant(condition.Value));
        }
        Expression value = Constant(condition.Value, member.Type);
        if (member.Type.IsEnum && condition.Operator is FilterOperation.GreaterThan or FilterOperation.GreaterOrEqual or FilterOperation.LessThan or FilterOperation.LessOrEqual)
        {
            Type underlying = Enum.GetUnderlyingType(member.Type);
            member = Expression.Convert(member, underlying);
            value = Expression.Convert(value, underlying);
        }
        return condition.Operator switch
        {
            FilterOperation.Equals => Expression.Equal(member, value),
            FilterOperation.NotEquals => Expression.NotEqual(member, value),
            FilterOperation.GreaterThan => Expression.GreaterThan(member, value),
            FilterOperation.GreaterOrEqual => Expression.GreaterThanOrEqual(member, value),
            FilterOperation.LessThan => Expression.LessThan(member, value),
            FilterOperation.LessOrEqual => Expression.LessThanOrEqual(member, value),
            _ => throw new NotSupportedException($"Condition operator '{condition.Operator}' is not supported.")
        };
    }

    private static Expression BuildCollection(FilterCollectionCondition condition, ParameterExpression parameter)
    {
        Expression collection = Member(parameter, condition.Path);
        Type elementType = ElementType(collection.Type);
        if (condition.Operator == FilterOperation.HasElements)
        {
            if (condition.Predicate is not null) throw new ArgumentException("HasElements cannot have a predicate.", nameof(condition));
            return Expression.Call(typeof(Enumerable), nameof(Enumerable.Any), [elementType], collection);
        }
        string method = condition.Operator switch
        {
            FilterOperation.Any => nameof(Enumerable.Any),
            FilterOperation.All => nameof(Enumerable.All),
            _ => throw new NotSupportedException($"Collection operator '{condition.Operator}' is not supported.")
        };
        if (condition.Predicate is null) throw new ArgumentException("Any and All require a predicate.", nameof(condition));
        ParameterExpression element = Expression.Parameter(elementType, "element");
        return Expression.Call(typeof(Enumerable), method, [elementType], collection,
            Expression.Lambda(Build(condition.Predicate, element), element));
    }

    private static Expression Member(Expression root, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (path == "$") return root;
        Expression current = root;
        foreach (string segment in path.Split('.'))
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
            PropertyInfo? property = current.Type.GetProperty(segment, flags);
            if (property is not null && property.GetMethod is { IsPublic: true } && property.GetIndexParameters().Length == 0)
            {
                current = Expression.Property(current, property);
                continue;
            }
            FieldInfo? field = current.Type.GetField(segment, flags);
            if (field is null) throw new ArgumentException($"Unknown filter path '{path}'.", nameof(path));
            current = Expression.Field(current, field);
        }
        return current;
    }

    private static Type ElementType(Type type)
    {
        if (type == typeof(string)) throw new NotSupportedException("A string is not a collection filter target.");
        Type? enumerable = type.GetInterfaces().Append(type)
            .FirstOrDefault(candidate => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return enumerable?.GetGenericArguments()[0] ??
            throw new NotSupportedException($"Type '{type}' is not a generic collection.");
    }

    private static Expression Constant(string? text, Type type)
    {
        Type target = Nullable.GetUnderlyingType(type) ?? type;
        if (text is null)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) is null)
                throw new ArgumentException($"Null cannot be compared with '{type}'.", nameof(text));
            return Expression.Constant(null, type);
        }
        object value;
        if (target == typeof(string)) value = text;
        else if (target == typeof(Guid)) value = Guid.Parse(text);
        else if (target == typeof(DateTime)) value = DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        else if (target == typeof(DateTimeOffset)) value = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);
        else if (target == typeof(DateOnly)) value = DateOnly.Parse(text, CultureInfo.InvariantCulture);
        else if (target == typeof(TimeOnly)) value = TimeOnly.Parse(text, CultureInfo.InvariantCulture);
        else if (target == typeof(TimeSpan)) value = TimeSpan.Parse(text, CultureInfo.InvariantCulture);
        else if (target.IsEnum) value = Enum.Parse(target, text, ignoreCase: true);
        else value = Convert.ChangeType(text, target, CultureInfo.InvariantCulture);
        Expression constant = Expression.Constant(value, target);
        return target == type ? constant : Expression.Convert(constant, type);
    }
}
