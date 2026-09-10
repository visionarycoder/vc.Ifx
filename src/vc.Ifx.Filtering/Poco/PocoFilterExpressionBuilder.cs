using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Filtering.Poco;

internal static class PocoFilterExpressionBuilder
{
    public static Expression? BuildExpression<T>(FilterNode? filter, ParameterExpression parameter)
        => Build(filter, parameter);

    private static Expression? Build(FilterNode? node, ParameterExpression parameter)
    {
        if (node is null) return null;
        return node switch
        {
            FilterGroup g => BuildGroup(g, parameter),
            FilterCondition c => BuildCondition(c, parameter),
            FilterCollectionCondition cc => BuildCollectionCondition(cc, parameter),
            _ => null
        };
    }

    private static Expression? BuildGroup(FilterGroup group, ParameterExpression parameter)
    {
        Expression? combined = null;
        foreach (FilterNode child in group.Children)
        {
            Expression? expr = Build(child, parameter);
            if (expr is null) continue;
            combined = combined is null
                ? expr
                : group.Combination == FilterCombination.And
                    ? Expression.AndAlso(combined, expr)
                    : Expression.OrElse(combined, expr);
        }
        return combined;
    }

    private static Expression? BuildCondition(FilterCondition condition, ParameterExpression parameter)
    {
        MemberExpression? member = BuildMemberAccess(parameter, condition.Path);
        if (member is null) return null;

        // Special handling for IN
        if (condition.Operator == FilterOperation.In)
        {
            // condition.Value holds JSON array of string values
            if (string.IsNullOrEmpty(condition.Value)) return null;
            try
            {
                var items = JsonSerializer.Deserialize<List<string?>>(condition.Value) ?? new();
                if (items.Count == 0) return null;

                // Build OR equals: (member == v1) || (member == v2) ...
                Expression? combined = null;
                foreach (string? s in items)
                {
                    object? parsed = ConvertFromString(s, Nullable.GetUnderlyingType(member.Type) ?? member.Type);
                    if (parsed is null && (Nullable.GetUnderlyingType(member.Type) ?? member.Type).IsValueType && (Nullable.GetUnderlyingType(member.Type) ?? member.Type) != typeof(string))
                        continue;

                    ConstantExpression c = Expression.Constant(parsed, parsed?.GetType() ?? typeof(string));
                    Expression leftExpr = member;
                    if (member.Type != c.Type)
                    {
                        if (Nullable.GetUnderlyingType(member.Type) == c.Type)
                        {
                            // ok
                        }
                        else
                        {
                            leftExpr = Expression.Convert(member, c.Type);
                        }
                    }

                    Expression eq = Expression.Equal(leftExpr, PromoteNull(c, leftExpr.Type));
                    combined = combined is null ? eq : Expression.OrElse(combined, eq);
                }

                return combined;
            }
            catch
            {
                return null;
            }
        }

        Type targetType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;
        object? constantValue = ConvertFromString(condition.Value, targetType);
        if (constantValue is null && targetType.IsValueType && targetType != typeof(string))
            return null;

        ConstantExpression constant = Expression.Constant(constantValue, targetType);
        Expression left = member;
        if (member.Type != constant.Type)
        {
            if (Nullable.GetUnderlyingType(member.Type) == constant.Type)
            {
                // ok
            }
            else
            {
                left = Expression.Convert(member, constant.Type);
            }
        }

        return condition.Operator switch
        {
            FilterOperation.Equals => Expression.Equal(left, PromoteNull(constant, left.Type)),
            FilterOperation.NotEquals => Expression.NotEqual(left, PromoteNull(constant, left.Type)),
            FilterOperation.GreaterThan => Expression.GreaterThan(left, constant),
            FilterOperation.GreaterOrEqual => Expression.GreaterThanOrEqual(left, constant),
            FilterOperation.LessThan => Expression.LessThan(left, constant),
            FilterOperation.LessOrEqual => Expression.LessThanOrEqual(left, constant),
            FilterOperation.Contains => StringMethod(member, nameof(string.Contains), condition.Value),
            FilterOperation.StartsWith => StringMethod(member, nameof(string.StartsWith), condition.Value),
            FilterOperation.EndsWith => StringMethod(member, nameof(string.EndsWith), condition.Value),
            _ => null
        };
    }

    private static Expression? BuildCollectionCondition(FilterCollectionCondition condition, ParameterExpression parameter)
    {
        MemberExpression? collection = BuildMemberAccess(parameter, condition.Path);
        if (collection is null) return null;
        Type? elementType = GetElementType(collection.Type);
        if (elementType is null) return null;

        string? anyAllMethodName = condition.Operator switch
        {
            FilterOperation.HasElements => nameof(Enumerable.Any),
            FilterOperation.Any => nameof(Enumerable.Any),
            FilterOperation.All => nameof(Enumerable.All),
            _ => null
        };
        if (anyAllMethodName is null) return null;

        if (condition.Operator == FilterOperation.HasElements)
        {
            return Expression.Call(
                typeof(Enumerable), anyAllMethodName, new[] { elementType }, collection);
        }

        if (condition.Predicate is null) return null;
        ParameterExpression elemParam = Expression.Parameter(elementType, "e");
        Expression? inner = Build(condition.Predicate, elemParam);
        if (inner is null) return null;
        LambdaExpression lambda = Expression.Lambda(inner, elemParam);
        return Expression.Call(
            typeof(Enumerable), anyAllMethodName, new[] { elementType }, collection, lambda);
    }

    private static Expression? StringMethod(Expression member, string method, string? arg)
    {
        if (member.Type != typeof(string)) return null;
        MethodInfo mi = typeof(string).GetMethod(method, new[] { typeof(string) })!;
        return Expression.Call(member, mi, Expression.Constant(arg ?? string.Empty));
    }

    private static MemberExpression? BuildMemberAccess(Expression root, string path)
    {
        Expression current = root;
        foreach (string segment in path.Split('.'))
        {
            PropertyInfo? prop = current.Type.GetProperty(segment, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                current = Expression.Property(current, prop);
                continue;
            }
            FieldInfo? field = current.Type.GetField(segment, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (field is not null)
            {
                current = Expression.Field(current, field);
                continue;
            }
            return null;
        }
        return current as MemberExpression ?? (current.NodeType == ExpressionType.MemberAccess ? (MemberExpression)current : null);
    }

    private static Type? GetElementType(Type type)
    {
        if (type.IsArray) return type.GetElementType();
        Type? ienum = type.GetInterfaces().Append(type)
            .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return ienum?.GetGenericArguments()[0];
    }

    private static object? ConvertFromString(string? text, Type targetType)
    {
        if (text is null) return targetType == typeof(string) ? string.Empty : null;
        if (targetType == typeof(string)) return text;
        if (targetType == typeof(Guid)) return Guid.TryParse(text, out Guid g) ? g : null;
        if (targetType == typeof(bool)) return bool.TryParse(text, out bool b) ? b : null;
        if (targetType == typeof(int)) return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i) ? i : null;
        if (targetType == typeof(long)) return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l) ? l : null;
        if (targetType == typeof(short)) return short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out short s) ? s : null;
        if (targetType == typeof(decimal)) return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal d) ? d : null;
        if (targetType == typeof(double)) return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double dbl) ? dbl : null;
        if (targetType == typeof(float)) return float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float f) ? f : null;
        if (targetType == typeof(DateTime)) return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime dt) ? dt : null;
        if (targetType.IsEnum) return Enum.TryParse(targetType, text, ignoreCase: true, out object? e) ? e : null;
        return text;
    }

    private static Expression PromoteNull(Expression constant, Type targetType)
        => constant.Type == targetType ? constant : Expression.Convert(constant, targetType);
}
