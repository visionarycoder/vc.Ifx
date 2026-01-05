using System.Linq.Expressions;

namespace VisionaryCoder.Framework.Patterns.Specifications;

/// <summary>
/// Combines two specifications using AND logic.
/// </summary>
internal sealed class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> left;
    private readonly Specification<T> right;

    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        this.left = left;
        this.right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> leftExpr = left.ToExpression();
        Expression<Func<T, bool>> rightExpr = right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(leftExpr, parameter),
            Expression.Invoke(rightExpr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}