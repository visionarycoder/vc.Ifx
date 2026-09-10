using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Filtering;

/// <summary>Translates supported predicates without discarding unsupported conditions.</summary>
public static class ExpressionToFilterNode
{
    // Indexed by the six stable comparison enum values, after Comparison validates them.
    private static readonly FilterOperation[] NegatedComparisons =
    [FilterOperation.NotEquals, FilterOperation.Equals, FilterOperation.LessOrEqual,
        FilterOperation.LessThan, FilterOperation.GreaterOrEqual, FilterOperation.GreaterThan];
    /// <summary>Snapshots a predicate as a portable filter.</summary>
    public static FilterNode Translate<T>(Expression<Func<T, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return TranslateNode(expression.Body, expression.Parameters[0]);
    }

    /// <summary>Translates a Boolean expression or single-parameter predicate.</summary>
    public static FilterNode Translate(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        if (expression is LambdaExpression lambda)
        {
            if (lambda.Parameters.Count != 1) throw Unsupported(expression);
            return TranslateNode(lambda.Body, lambda.Parameters[0]);
        }
        return TranslateNode(expression, null);
    }

    private static FilterNode TranslateNode(Expression expression, ParameterExpression? parameter)
    {
        if (expression.Type != typeof(bool)) throw Unsupported(expression);
        if (expression is BinaryExpression binary)
        {
            if (binary.NodeType is ExpressionType.AndAlso or ExpressionType.OrElse)
            {
                FilterCombination combination = binary.NodeType == ExpressionType.AndAlso
                    ? FilterCombination.And : FilterCombination.Or;
                FilterNode left = TranslateNode(binary.Left, parameter);
                FilterNode right = TranslateNode(binary.Right, parameter);
                return new FilterGroup(combination, Flatten(left, combination).Concat(Flatten(right, combination)).ToArray());
            }
            if (binary.Method is not null && binary.Method.DeclaringType != typeof(string) &&
                binary.Method.DeclaringType != typeof(decimal) && binary.Method.DeclaringType != typeof(DateTime) &&
                binary.Method.DeclaringType != typeof(DateTimeOffset) && binary.Method.DeclaringType != typeof(Guid))
                throw Unsupported(expression);
            string? path = GetPath(binary.Left, parameter);
            bool inverted = path is null;
            path ??= GetPath(binary.Right, parameter);
            if (path is null) throw Unsupported(expression);
            return new FilterCondition(path, Comparison(binary.NodeType, inverted),
                Format(Evaluate(inverted ? binary.Left : binary.Right)));
        }
        if (expression is UnaryExpression { NodeType: ExpressionType.Not } not)
        {
            FilterNode operand = TranslateNode(not.Operand, parameter);
            // Preserve explicit negation for lifted nullable and floating-point comparisons.
            if (operand is FilterCondition condition && not.Operand is BinaryExpression comparison &&
                Nullable.GetUnderlyingType(comparison.Left.Type) is null &&
                Nullable.GetUnderlyingType(comparison.Right.Type) is null &&
                comparison.Left.Type != typeof(float) && comparison.Left.Type != typeof(double) &&
                comparison.Right.Type != typeof(float) && comparison.Right.Type != typeof(double))
                return condition with { Operator = NegatedComparisons[(int)condition.Operator] };
            return new FilterNegation(operand);
        }
        if (expression is MethodCallExpression call) return TranslateCall(call, parameter);
        string? booleanPath = GetPath(expression, parameter);
        return booleanPath is not null
            ? new FilterCondition(booleanPath, FilterOperation.Equals, bool.TrueString)
            : new FilterConstant((bool)Evaluate(expression)!);
    }

    private static IEnumerable<FilterNode> Flatten(FilterNode node, FilterCombination combination) =>
        node is FilterGroup group && group.Combination == combination ? group.Children : [node];

    private static FilterOperation Comparison(ExpressionType type, bool inverted)
    {
        FilterOperation operation = type switch
        {
            ExpressionType.Equal => FilterOperation.Equals,
            ExpressionType.NotEqual => FilterOperation.NotEquals,
            ExpressionType.GreaterThan => FilterOperation.GreaterThan,
            ExpressionType.GreaterThanOrEqual => FilterOperation.GreaterOrEqual,
            ExpressionType.LessThan => FilterOperation.LessThan,
            ExpressionType.LessThanOrEqual => FilterOperation.LessOrEqual,
            _ => throw new NotSupportedException($"Comparison '{type}' is not supported.")
        };
        if (!inverted) return operation;
        return operation switch
        {
            FilterOperation.GreaterThan => FilterOperation.LessThan,
            FilterOperation.GreaterOrEqual => FilterOperation.LessOrEqual,
            FilterOperation.LessThan => FilterOperation.GreaterThan,
            FilterOperation.LessOrEqual => FilterOperation.GreaterOrEqual,
            _ => operation
        };
    }

    private static FilterNode TranslateCall(MethodCallExpression call, ParameterExpression? parameter)
    {
        if (call.Method.DeclaringType == typeof(string) && call.Object is not null &&
            call.Arguments.Count == 1 && call.Arguments[0].Type == typeof(string))
        {
            FilterOperation operation = call.Method.Name switch
            {
                nameof(string.Contains) => FilterOperation.Contains,
                nameof(string.StartsWith) => FilterOperation.StartsWith,
                nameof(string.EndsWith) => FilterOperation.EndsWith,
                _ => throw Unsupported(call)
            };
            return new FilterCondition(GetPath(call.Object, parameter) ?? throw Unsupported(call),
                operation, Format(Evaluate(call.Arguments[0])));
        }
        bool isLinq = call.Method.DeclaringType == typeof(Enumerable) || call.Method.DeclaringType == typeof(Queryable);
        if (isLinq && call.Method.Name is nameof(Enumerable.Any) or nameof(Enumerable.All))
        {
            string path = GetPath(call.Arguments[0], parameter) ?? throw Unsupported(call);
            if (call.Arguments.Count == 1)
                return new FilterCollectionCondition(path, FilterOperation.HasElements, null);
            Expression predicate = call.Arguments[^1];
            if (predicate is UnaryExpression { NodeType: ExpressionType.Quote } quote) predicate = quote.Operand;
            if (predicate is not LambdaExpression lambda) throw Unsupported(call);
            return new FilterCollectionCondition(path,
                call.Method.Name == nameof(Enumerable.Any) ? FilterOperation.Any : FilterOperation.All,
                TranslateNode(lambda.Body, lambda.Parameters[0]));
        }
        if (call.Method.Name == nameof(Enumerable.Contains))
        {
            Expression source;
            Expression value;
            if ((isLinq || call.Method.DeclaringType == typeof(MemoryExtensions)) && call.Arguments.Count == 2)
            {
                source = UnwrapArraySpan(call.Arguments[0]);
                value = call.Arguments[1];
            }
            else if (call.Object is not null && call.Arguments.Count == 1 && call.Object.Type.IsGenericType &&
                call.Object.Type.GetGenericTypeDefinition() == typeof(List<>))
            {
                source = call.Object;
                value = call.Arguments[0];
            }
            else throw Unsupported(call);
            string? path = GetPath(source, parameter);
            if (path is not null) return new FilterCondition(path, FilterOperation.Contains, Format(Evaluate(value)));
            path = GetPath(value, parameter) ?? throw Unsupported(call);
            if (Evaluate(source) is not IEnumerable items) throw Unsupported(call);
            return new FilterCondition(path, FilterOperation.In,
                JsonSerializer.Serialize(items.Cast<object?>().Select(Format).ToArray()));
        }
        throw Unsupported(call);
    }

    private static Expression UnwrapArraySpan(Expression expression)
    {
        if (expression is MethodCallExpression { Method.Name: "op_Implicit", Arguments.Count: 1 } conversion &&
            conversion.Type.IsGenericType &&
            (conversion.Type.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>) ||
             conversion.Type.GetGenericTypeDefinition() == typeof(Span<>)) && conversion.Arguments[0].Type.IsArray)
        {
            Expression array = conversion.Arguments[0];
            return array is UnaryExpression { NodeType: ExpressionType.Convert, Method: null } cast &&
                cast.Type.IsAssignableFrom(cast.Operand.Type) ? cast.Operand : array;
        }
        return expression;
    }

    private static string? GetPath(Expression expression, ParameterExpression? parameter)
    {
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert, Method: null } conversion &&
            conversion.Operand.Type.IsEnum && conversion.Type == Enum.GetUnderlyingType(conversion.Operand.Type))
            expression = conversion.Operand;
        var parts = new Stack<string>();
        Expression? current = expression;
        while (current is MemberExpression member)
        {
            parts.Push(member.Member.Name);
            current = member.Expression;
        }
        if (current is not ParameterExpression root || (parameter is not null && root != parameter)) return null;
        return parts.Count == 0 ? "$" : string.Join('.', parts);
    }

    private static object? Evaluate(Expression expression) => expression switch
    {
        ConstantExpression constant => constant.Value,
        MemberExpression { Member: FieldInfo field } member => field.GetValue(
            member.Expression is null ? null : Evaluate(member.Expression)),
        MemberExpression { Member: PropertyInfo property } member => property.GetValue(
            member.Expression is null ? null : Evaluate(member.Expression)),
        NewArrayExpression { NodeType: ExpressionType.NewArrayInit } array => array.Expressions.Select(Evaluate).ToArray(),
        UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked, Method: null } conversion =>
            ConvertValue(Evaluate(conversion.Operand), conversion.Type),
        _ => throw Unsupported(expression)
    };

    private static object? ConvertValue(object? value, Type type)
    {
        if (value is null || type.IsInstanceOfType(value)) return value;
        Type target = Nullable.GetUnderlyingType(type) ?? type;
        return target.IsEnum ? Enum.ToObject(target, value) : Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
    }

    private static string? Format(object? value) => value switch
    {
        null => null,
        DateTime date => date.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset date => date.ToString("O", CultureInfo.InvariantCulture),
        TimeOnly time => time.ToString("O", CultureInfo.InvariantCulture),
        IFormattable formatted => formatted.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    private static NotSupportedException Unsupported(Expression expression) =>
        new($"Expression '{expression}' is not supported by portable filters.");
}
