using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Filtering;

/// <summary>
/// Converts LINQ expression trees into the framework's FilterNode representation.
/// </summary>
/// <remarks>
/// This translator supports a subset of expression syntax sufficient for typical
/// filtering scenarios: boolean combinations (<c>&amp;&amp;</c>, <c>||</c>), comparisons (<c>==</c>, <c>!=</c>, <c>&lt;</c>, <c>&gt;</c>, etc.),
/// simple unary negation (!), string operations (Contains/StartsWith/EndsWith) and
/// common Enumerable methods such as Any/All/Contains used in collection predicates.
///
/// The translator intentionally returns <c>null</c> for unsupported nodes; public
/// entry points raise <see cref="NotSupportedException"/> when translation fails.
/// </remarks>
public static class ExpressionToFilterNode
{
    /// <summary>
    /// Translate a strongly-typed predicate expression into a <see cref="FilterNode"/>.
    /// </summary>
    /// <typeparam name="T">The parameter type used in the expression (e.g. entity type).</typeparam>
    /// <param name="expression">The predicate expression to translate.</param>
    /// <returns>A <see cref="FilterNode"/> representing the predicate.</returns>
    /// <exception cref="NotSupportedException">Thrown when the expression contains unsupported constructs.</exception>
    public static FilterNode Translate<T>(Expression<Func<T, bool>> expression) => TranslateNode(expression.Body) ?? throw new NotSupportedException($"Expression '{expression}' is not supported.");

    /// <summary>
    /// Translate a general expression into a <see cref="FilterNode"/>.
    /// </summary>
    /// <param name="expression">The expression to translate.</param>
    /// <returns>A <see cref="FilterNode"/> representing the expression.</returns>
    /// <exception cref="NotSupportedException">Thrown when the expression contains unsupported constructs.</exception>
    public static FilterNode Translate(Expression expression) => TranslateNode(expression) ?? throw new NotSupportedException($"Expression '{expression}' is not supported.");

    /// <summary>
    /// Internal recursive dispatcher that maps expression node types to translator methods.
    /// Returns <c>null</c> when the node is not supported by the translator.
    /// </summary>
    private static FilterNode? TranslateNode(Expression expression) =>
        expression switch
        {
            BinaryExpression binary => TranslateBinary(binary),
            MethodCallExpression call => TranslateMethodCall(call),
            UnaryExpression { NodeType: ExpressionType.Not } unary => TranslateNot(unary),
            _ => null
        };

    /// <summary>
    /// Translates binary expressions. Handles logical groups (AndAlso/OrElse) by
    /// creating <see cref="FilterGroup"/> nodes and comparison operators by delegating
    /// to <see cref="TranslateComparison(BinaryExpression)"/>.
    /// </summary>
    private static FilterNode? TranslateBinary(BinaryExpression binary)
    {
        // Comparison: ==, !=, <, <=, >, >=
        if (binary.NodeType is not (ExpressionType.AndAlso or ExpressionType.OrElse))
        {
            return TranslateComparison(binary);
        }

        // Logical group: && or ||
        FilterCombination combination = binary.NodeType == ExpressionType.AndAlso
            ? FilterCombination.And
            : FilterCombination.Or;
        FilterNode? left = TranslateNode(binary.Left);
        FilterNode? right = TranslateNode(binary.Right);

        var children = new List<FilterNode>();
        if (left is not null) children.AddRange(FlattenIfSameGroup(left, combination));
        if (right is not null) children.AddRange(FlattenIfSameGroup(right, combination));

        return new FilterGroup(combination, children);

    }

    /// <summary>
    /// Helper that flattens nested groups of the same combination type to avoid
    /// deeply nested group trees (for example, <c>(A &amp;&amp; B) &amp;&amp; C</c> becomes <c>A &amp;&amp; B &amp;&amp; C</c>).
    /// </summary>
    private static IEnumerable<FilterNode> FlattenIfSameGroup(FilterNode node, FilterCombination combination)
    {
        if (node is FilterGroup group && group.Combination == combination)
        {
            foreach (FilterNode child in group.Children)
            {
                yield return child;
            }
            yield break;
        }
        yield return node;
    }

    /// <summary>
    /// Handles comparison expressions by normalizing member/constant positions and
    /// mapping the expression to a <see cref="FilterCondition"/>.
    /// </summary>
    private static FilterNode? TranslateComparison(BinaryExpression binary)
    {

        (MemberExpression? memberExpr, Expression? constantExpr, FilterOperation? op) = NormalizeBinary(binary);
        if (memberExpr is null || constantExpr is null || op is null)
        {
            return null;
        }

        string? path = GetMemberPath(memberExpr);
        if (path is null)
        {
            return null;
        }

        string? value = EvaluateToString(constantExpr);
        return new FilterCondition(path, op.Value, value);

    }

    /// <summary>
    /// Normalizes a binary expression so that the member expression is on the left
    /// and the constant-like expression is on the right. If operands are reversed the
    /// comparison operator will be inverted accordingly.
    /// </summary>
    private static (MemberExpression? member, Expression? constant, FilterOperation?) NormalizeBinary(BinaryExpression binary)
    {

        MemberExpression? leftMember = GetMember(binary.Left);
        MemberExpression? rightMember = GetMember(binary.Right);

        bool leftIsConstLike = IsConstantLike(binary.Left);
        bool rightIsConstLike = IsConstantLike(binary.Right);

        // member op constant
        if (leftMember is not null && rightIsConstLike)
        {
            FilterOperation? op = MapComparisonOperator(binary.NodeType, invert: false);
            return (leftMember, binary.Right, op);
        }

        // constant op member -> invert operator
        if (rightMember is not null && leftIsConstLike)
        {
            FilterOperation? op = MapComparisonOperator(binary.NodeType, invert: true);
            return (rightMember, binary.Left, op);
        }

        return (null, null, null);
    }

    /// <summary>
    /// Maps ExpressionType comparison nodes to framework <see cref="FilterOperation"/>,
    /// optionally inverting the operator when the constant appears on the left.
    /// </summary>
    private static FilterOperation? MapComparisonOperator(ExpressionType nodeType, bool invert)
    {

        return (nodeType, invert) switch
        {
            (ExpressionType.Equal, _) => FilterOperation.Equals,
            (ExpressionType.NotEqual, _) => FilterOperation.NotEquals,

            (ExpressionType.GreaterThan, false) => FilterOperation.GreaterThan,
            (ExpressionType.GreaterThan, true) => FilterOperation.LessThan,

            (ExpressionType.GreaterThanOrEqual, false) => FilterOperation.GreaterOrEqual,
            (ExpressionType.GreaterThanOrEqual, true) => FilterOperation.LessOrEqual,

            (ExpressionType.LessThan, false) => FilterOperation.LessThan,
            (ExpressionType.LessThan, true) => FilterOperation.GreaterThan,

            (ExpressionType.LessThanOrEqual, false) => FilterOperation.LessOrEqual,
            (ExpressionType.LessThanOrEqual, true) => FilterOperation.GreaterOrEqual,

            _ => null
        };

    }

    /// <summary>
    /// Translates supported method calls into FilterNode forms.
    /// Supported patterns include string methods (Contains/StartsWith/EndsWith),
    /// Enumerable static methods (Any/All/Contains) and instance collection Contains.
    /// </summary>
    private static FilterNode? TranslateMethodCall(MethodCallExpression call)
    {

        // string.Contains / StartsWith / EndsWith
        if (call.Object is not null && call.Object.Type == typeof(string) && call.Arguments.Count == 1)
        {
            MemberExpression? targetMember = GetMember(call.Object);
            if (targetMember is null)
            {
                return null;
            }

            string? path = GetMemberPath(targetMember);
            if (path is null)
            {
                return null;
            }

            Expression arg = call.Arguments[0];
            string? value = EvaluateToString(arg);

            FilterOperation? op = call.Method.Name switch
            {
                nameof(string.Contains) => FilterOperation.Contains,
                nameof(string.StartsWith) => FilterOperation.StartsWith,
                nameof(string.EndsWith) => FilterOperation.EndsWith,
                _ => null
            };

            return op is null
                ? null
                : new FilterCondition(path, op.Value, value);
        }

        // Collection methods: Any(), All(), Contains()
        if (call.Method.DeclaringType == typeof(Enumerable))
        {
            return TranslateEnumerableMethod(call);
        }

        // Collection instance methods: Contains() on List/ICollection
        if (call.Object is null || !IsCollectionType(call.Object.Type) || call.Method.Name != nameof(List<object>.Contains) || call.Arguments.Count != 1)
        {
            return null;
        }

        {
            MemberExpression? collectionMember = GetMember(call.Object);
            if (collectionMember is null)
            {
                return null;
            }

            string? path = GetMemberPath(collectionMember);
            if (path is null)
            {
                return null;
            }

            string? value = EvaluateToString(call.Arguments[0]);
            return new FilterCondition(path, FilterOperation.Contains, value);
        }

        // Custom methods can be added here by checking call.Method.DeclaringType and Method.Name
        // Example for custom method support:
        // if (call.Method.DeclaringType == typeof(MyCustomClass) && call.Method.Name == "MyMethod")
        // {
        //     // Extract parameters and create appropriate FilterNode
        //     return new FilterCondition(...);
        // }

    }

    /// <summary>
    /// Translates a simple logical negation. Only supports negation of a comparison
    /// expression (e.g. <c>!x.IsActive</c> or <c>! (x.Value &gt; 4)</c>).
    /// </summary>
    private static FilterNode? TranslateNot(UnaryExpression unary)
    {
        // Only handle simple negation of a comparison or method call for now
        // e.g. !c.IsActive or !c.Name.Contains("x")
        if (unary.Operand is not BinaryExpression binary)
        {
            return null;
        }

        // Flip operator if possible
        (MemberExpression? memberExpr, Expression? constantExpr, FilterOperation? op) = NormalizeBinary(binary);
        if (memberExpr is null || constantExpr is null || op is null)
        {
            return null;
        }

        FilterOperation negated = NegateOperator(op.Value);
        string? path = GetMemberPath(memberExpr);
        string? value = EvaluateToString(constantExpr);
        return new FilterCondition(path!, negated, value);
        
    }

    /// <summary>
    /// Negates a filter operator when a logical NOT is applied to a condition.
    /// </summary>
    private static FilterOperation NegateOperator(FilterOperation op) =>
        op switch
        {
            FilterOperation.Equals => FilterOperation.NotEquals,
            FilterOperation.NotEquals => FilterOperation.Equals,
            FilterOperation.GreaterThan => FilterOperation.LessOrEqual,
            FilterOperation.GreaterOrEqual => FilterOperation.LessThan,
            FilterOperation.LessThan => FilterOperation.GreaterOrEqual,
            FilterOperation.LessOrEqual => FilterOperation.GreaterThan,
            _ => throw new NotSupportedException($"Cannot negate operator '{op}'.")
        };

    /// <summary>
    /// Attempts to obtain a <see cref="MemberExpression"/> from an expression.
    /// Handles trivial conversions (e.g. boxing/unboxing) by unwrapping unary convert nodes.
    /// </summary>
    private static MemberExpression? GetMember(Expression expression) =>
        expression switch
        {
            MemberExpression m => m,
            UnaryExpression { NodeType: ExpressionType.Convert, Operand: MemberExpression inner } => inner,
            _ => null
        };

    /// <summary>
    /// Determines whether an expression is "constant-like" (literal, captured closure, or nested constant conversion).
    /// </summary>
    private static bool IsConstantLike(Expression expression) => expression.NodeType is ExpressionType.Constant || expression is MemberExpression { Expression: ConstantExpression } || (expression is UnaryExpression u && IsConstantLike(u.Operand));

    /// <summary>
    /// Builds a dotted member path for nested member expressions (e.g. <c>x.Address.City</c> -> "Address.City").
    /// Returns null if path cannot be determined.
    /// </summary>
    private static string? GetMemberPath(MemberExpression member)
    {
        var parts = new Stack<string>();
        Expression? current = member;

        while (current is MemberExpression m)
        {
            parts.Push(m.Member.Name);
            current = m.Expression;
        }

        // Stop at the root parameter (e.g. x)
        return string.Join('.', parts);
    }

    /// <summary>
    /// Translates LINQ Enumerable method calls (Any, All, Contains) to FilterNode structures.
    /// Supports: Any(), Any(predicate), All(predicate), Enumerable.Contains(source, value).
    /// </summary>
    /// <param name="call">The method call expression representing a LINQ Enumerable method.</param>
    /// <returns>A FilterNode representing the collection operation, or null if translation is not supported.</returns>
    private static FilterNode? TranslateEnumerableMethod(MethodCallExpression call)
    {
        // First argument should be the collection (source)
        if (call.Arguments.Count == 0)
        {
            return null;
        }

        Expression collectionExpr = call.Arguments[0];
        MemberExpression? collectionMember = GetMember(collectionExpr);
        if (collectionMember is not null)
        {
            string? path = GetMemberPath(collectionMember);
            if (path is null)
            {
                return null;
            }

            switch (call.Method.Name)
            {
                case nameof(Enumerable.Any):
                    switch (call.Arguments.Count)
                    {

                        // Any() without predicate - just check if collection has elements
                        case 1:
                            return new FilterCollectionCondition(path, FilterOperation.HasElements, null);

                        // Any(predicate) - check if any element matches the predicate
                        case 2 when call.Arguments[1] is UnaryExpression { Operand: LambdaExpression anyLambdaPredicate }:
                            {
                                FilterNode? predicateFilter = TranslateNode(anyLambdaPredicate.Body);
                                return predicateFilter is null
                                    ? null
                                    : new FilterCollectionCondition(path, FilterOperation.Any, predicateFilter);
                            }
                    }

                    break;

                case nameof(Enumerable.All):

                    // All(predicate) - check if all elements match the predicate
                    if (call.Arguments.Count == 2 && call.Arguments[1] is UnaryExpression { Operand: LambdaExpression allLambdaPredicate })
                    {
                        FilterNode? predicateFilter = TranslateNode(allLambdaPredicate.Body);
                        return predicateFilter is null
                            ? null
                            : new FilterCollectionCondition(path, FilterOperation.All, predicateFilter);
                    }
                    break;

                case nameof(Enumerable.Contains):
                    // Contains(value) - check if collection contains a specific value
                    if (call.Arguments.Count == 2)
                    {
                        string? value = EvaluateToString(call.Arguments[1]);
                        return new FilterCondition(path, FilterOperation.Contains, value);
                    }
                    break;
            }

            return null;
        }

        // collectionExpr is not a member (likely a constant or complex expression)
        // Support patterns like: new[] { "A", "B" }.Contains(x.Prop) -> translates to IN
        if (call.Method.Name == nameof(Enumerable.Contains) && call.Arguments.Count == 2)
        {
            // If the first arg is constant-like (collection) and the second arg is a member, create an IN
            if (IsConstantLike(collectionExpr))
            {
                // evaluate collection
                object? raw = EvaluateExpression(collectionExpr);
                if (raw is System.Collections.IEnumerable items)
                {
                    // collect string forms
                    var list = new List<string?>();
                    foreach (object? it in items)
                    {
                        list.Add(it?.ToString());
                    }

                    // second argument should be member expression representing the property
                    MemberExpression? memberExpr = GetMember(call.Arguments[1]);
                    if (memberExpr is null) return null;
                    string? path = GetMemberPath(memberExpr);
                    if (path is null) return null;

                    string json = JsonSerializer.Serialize(list);
                    return new FilterCondition(path, FilterOperation.In, json);
                }
            }

            // Also support instance-method form: (new[] {"A"}).Contains(x.Prop)
            if (call.Object is not null && IsConstantLike(call.Object))
            {
                object? raw = EvaluateExpression(call.Object);
                if (raw is System.Collections.IEnumerable items)
                {
                    var list = new List<string?>();
                    foreach (object? it in items)
                    {
                        list.Add(it?.ToString());
                    }

                    // argument[0] is the element (member)
                    MemberExpression? memberExpr = GetMember(call.Arguments[0]);
                    if (memberExpr is null) return null;
                    string? path = GetMemberPath(memberExpr);
                    if (path is null) return null;

                    string json = JsonSerializer.Serialize(list);
                    return new FilterCondition(path, FilterOperation.In, json);
                }
            }
        }

        return null;
    }

    private static object? EvaluateExpression(Expression expression)
    {
        Expression expr = expression;
        while (expr is UnaryExpression u && expr.NodeType == ExpressionType.Convert)
        {
            expr = u.Operand;
        }

        if (expr is ConstantExpression constant)
        {
            return constant.Value;
        }

        try
        {
            LambdaExpression lambda = Expression.Lambda(expr);
            return lambda.Compile().DynamicInvoke();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if a type is a collection type (array or implements IEnumerable&lt;T&gt;, ICollection&lt;T&gt;, or IList&lt;T&gt;).
    /// Strings are explicitly excluded.
    /// </summary>
    private static bool IsCollectionType(Type type)
    {
        if (type == typeof(string))
        {
            return false;
        }
        return type.IsArray || type.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IEnumerable<>) || i.GetGenericTypeDefinition() == typeof(ICollection<>) || i.GetGenericTypeDefinition() == typeof(IList<>)));
    }

    /// <summary>
    /// Evaluates an expression to its string representation. Handles constants, captured closures
    /// and simple expressions by compiling and invoking the expression where necessary.
    /// </summary>
    private static string? EvaluateToString(Expression expression)
    {
        // Normalize to underlying expression
        Expression expr = expression;

        // Strip conversions
        while (expr is UnaryExpression u && expr.NodeType == ExpressionType.Convert)
        {
            expr = u.Operand;
        }

        if (expr is ConstantExpression constant)
        {
            return constant.Value?.ToString();
        }

        // Captured local / closure / more complex constant
        LambdaExpression lambda = Expression.Lambda(expr);
        object? value = lambda.Compile().DynamicInvoke();
        return value?.ToString();
    }
}
