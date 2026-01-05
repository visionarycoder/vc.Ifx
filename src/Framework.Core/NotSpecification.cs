using System.Linq.Expressions;

namespace VisionaryCoder.Framework;

/// <summary>
/// Negates a specification.
/// </summary>
internal sealed class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> specification;

    public NotSpecification(Specification<T> specification)
    {
        this.specification = specification;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> expr = specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.Not(Expression.Invoke(expr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}