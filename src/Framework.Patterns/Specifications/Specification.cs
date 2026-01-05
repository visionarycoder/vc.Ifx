// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Linq.Expressions;

namespace VisionaryCoder.Framework.Patterns.Specifications;

/// <summary>
/// Base class for specifications.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    /// <inheritdoc />
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <inheritdoc />
    public bool IsSatisfiedBy(T entity)
    {
        Func<T, bool> predicate = ToExpression().Compile();
        return predicate(entity);
    }

    /// <summary>
    /// Combines this specification with another using AND logic.
    /// </summary>
    public Specification<T> And(Specification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    /// <summary>
    /// Combines this specification with another using OR logic.
    /// </summary>
    public Specification<T> Or(Specification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }

    /// <summary>
    /// Negates this specification.
    /// </summary>
    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }

    /// <summary>
    /// Implicit conversion from specification to expression.
    /// </summary>
    public static implicit operator Expression<Func<T, bool>>(Specification<T> specification)
    {
        return specification.ToExpression();
    }
}