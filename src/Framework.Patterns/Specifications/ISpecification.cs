using System.Linq.Expressions;

namespace VisionaryCoder.Framework.Patterns.Specifications;

/// <summary>
/// Interface for specification pattern.
/// Encapsulates query logic that can be reused and tested independently.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Converts the specification to an expression.
    /// </summary>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Determines if the entity satisfies the specification.
    /// </summary>
    bool IsSatisfiedBy(T entity);
}