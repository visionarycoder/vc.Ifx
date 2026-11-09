using System.Linq.Expressions;

namespace Ifx.Querying;

/// <summary>
/// Rebinds an expression built against one <see cref="ParameterExpression"/> to use a different,
/// shared parameter instead — used whenever two independently-authored lambdas (each with their
/// own parameter, e.g. two separate <c>o => ...</c> predicates) must be combined into a single
/// expression tree bound to one common parameter.
/// </summary>
/// <param name="source">The parameter to replace.</param>
/// <param name="target">The parameter that should replace <paramref name="source"/>.</param>
internal sealed class ParameterRebinder(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
{
    /// <inheritdoc />
    protected override Expression VisitParameter(ParameterExpression node) => node == source ? target : node;

    /// <summary>Rebinds <paramref name="body"/> from <paramref name="source"/> to <paramref name="target"/>.</summary>
    public static Expression Rebind(Expression body, ParameterExpression source, ParameterExpression target) =>
        new ParameterRebinder(source, target).Visit(body);
}
