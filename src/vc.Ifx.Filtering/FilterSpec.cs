using System.Linq.Expressions;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Filtering;

/// <summary>An immutable predicate independent of database query shape.</summary>
public sealed class FilterSpec<T>(Expression<Func<T, bool>> predicate)
{
    /// <summary>Gets the uncompiled predicate.</summary>
    public Expression<Func<T, bool>> Predicate { get; } = predicate ?? throw new ArgumentNullException(nameof(predicate));

    /// <summary>Gets a specification matching every item.</summary>
    public static FilterSpec<T> All { get; } = new(item => true);

    /// <summary>Combines this predicate with another using AND.</summary>
    public FilterSpec<T> Where(Expression<Func<T, bool>> predicate) => And(new FilterSpec<T>(predicate));

    /// <summary>Combines two predicates using short-circuit AND.</summary>
    public FilterSpec<T> And(FilterSpec<T> other) => Combine(other, Expression.AndAlso);

    /// <summary>Combines two predicates using short-circuit OR.</summary>
    public FilterSpec<T> Or(FilterSpec<T> other) => Combine(other, Expression.OrElse);

    /// <summary>Negates this predicate.</summary>
    public FilterSpec<T> Not() => new(Expression.Lambda<Func<T, bool>>(Expression.Not(Predicate.Body), Predicate.Parameters));

    /// <summary>Snapshots the supported expression as a portable filter.</summary>
    public FilterNode ToFilterNode() => ExpressionToFilterNode.Translate(Predicate);

    /// <summary>Applies the predicate to a query provider without enumerating results.</summary>
    public IQueryable<T> Apply(IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Where(Predicate);
    }

    /// <summary>Applies the predicate to an in-memory sequence.</summary>
    public IEnumerable<T> Apply(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Where(Predicate.Compile());
    }

    private FilterSpec<T> Combine(FilterSpec<T> other, Func<Expression, Expression, BinaryExpression> combine)
    {
        ArgumentNullException.ThrowIfNull(other);
        Expression right = new ParameterSubstitution(other.Predicate.Parameters[0], Predicate.Parameters[0]).Visit(other.Predicate.Body);
        return new(Expression.Lambda<Func<T, bool>>(combine(Predicate.Body, right), Predicate.Parameters));
    }

    private sealed class ParameterSubstitution(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == source ? target : node;
    }
}
