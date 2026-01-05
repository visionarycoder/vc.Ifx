// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace VisionaryCoder.Framework.Filtering.EntityFramework;

/// <summary>
/// Builds EF Core expression trees from FilterNode objects for database queries.
/// Converts CompositeFilter and PropertyFilter into LINQ expressions.
/// </summary>
internal static class EfFilterExpressionBuilder
{
    /// <summary>
    /// Builds an expression tree from a FilterNode for use with EF Core queries.
    /// </summary>
    /// <typeparam name="T">The entity type being filtered</typeparam>
    /// <param name="filter">The filter node to convert</param>
    /// <param name="parameter">The parameter expression for the entity</param>
    /// <param name="dbContext">The EF Core DbContext for database-specific operations</param>
    /// <returns>An expression representing the filter, or null if the filter cannot be built</returns>
    public static Expression? BuildExpression<T>(FilterNode? filter, ParameterExpression parameter, DbContext dbContext)
    {
        if (filter is null) return null;

        return filter switch
        {
            CompositeFilter composite => BuildCompositeExpression<T>(composite, parameter, dbContext),
            PropertyFilter property => BuildPropertyExpression<T>(property, parameter, dbContext),
            _ => null
        };
    }

    private static Expression? BuildCompositeExpression<T>(CompositeFilter composite, ParameterExpression parameter, DbContext dbContext)
    {
        if (composite.Filters is null || composite.Filters.Count == 0)
            return null;

        var expressions = composite.Filters
            .Select(f => BuildExpression<T>(f, parameter, dbContext))
            .Where(e => e is not null)
            .Cast<Expression>()
            .ToList();

        if (expressions.Count == 0) return null;

        return composite.Operator.ToUpperInvariant() switch
        {
            "AND" => expressions.Aggregate(Expression.AndAlso),
            "OR" => expressions.Aggregate(Expression.OrElse),
            "NOT" => expressions.Count == 1 ? Expression.Not(expressions[0]) : null,
            _ => null
        };
    }

    private static Expression? BuildPropertyExpression<T>(PropertyFilter property, ParameterExpression parameter, DbContext dbContext)
    {
        if (string.IsNullOrWhiteSpace(property.Property) || string.IsNullOrWhiteSpace(property.Operator))
            return null;

        // Build property access expression
        Expression propertyAccess = parameter;
        foreach (var propName in property.Property.Split('.'))
        {
            var propInfo = propertyAccess.Type.GetProperty(propName);
            if (propInfo is null) return null;
            propertyAccess = Expression.Property(propertyAccess, propInfo);
        }

        // Convert value to property type
        var valueExpression = property.Value is not null
            ? Expression.Constant(Convert.ChangeType(property.Value, propertyAccess.Type))
            : Expression.Constant(null, propertyAccess.Type);

        // Build comparison expression based on operator
        return property.Operator.ToUpperInvariant() switch
        {
            "EQ" or "=" or "==" => Expression.Equal(propertyAccess, valueExpression),
            "NE" or "!=" or "<>" => Expression.NotEqual(propertyAccess, valueExpression),
            "GT" or ">" => Expression.GreaterThan(propertyAccess, valueExpression),
            "GTE" or ">=" => Expression.GreaterThanOrEqual(propertyAccess, valueExpression),
            "LT" or "<" => Expression.LessThan(propertyAccess, valueExpression),
            "LTE" or "<=" => Expression.LessThanOrEqual(propertyAccess, valueExpression),
            "CONTAINS" => BuildStringContainsExpression(propertyAccess, valueExpression),
            "STARTSWITH" => BuildStringStartsWithExpression(propertyAccess, valueExpression),
            "ENDSWITH" => BuildStringEndsWithExpression(propertyAccess, valueExpression),
            "IN" => BuildInExpression(propertyAccess, property.Value),
            "NOTIN" => Expression.Not(BuildInExpression(propertyAccess, property.Value)),
            "ISNULL" => Expression.Equal(propertyAccess, Expression.Constant(null, propertyAccess.Type)),
            "ISNOTNULL" => Expression.NotEqual(propertyAccess, Expression.Constant(null, propertyAccess.Type)),
            _ => null
        };
    }

    private static Expression BuildStringContainsExpression(Expression property, Expression value)
    {
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
        return Expression.Call(property, containsMethod!, value);
    }

    private static Expression BuildStringStartsWithExpression(Expression property, Expression value)
    {
        var startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
        return Expression.Call(property, startsWithMethod!, value);
    }

    private static Expression BuildStringEndsWithExpression(Expression property, Expression value)
    {
        var endsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
        return Expression.Call(property, endsWithMethod!, value);
    }

    private static Expression BuildInExpression(Expression property, object? value)
    {
        if (value is null) return Expression.Constant(false);

        // Assuming value is a collection
        var valueType = property.Type;
        var collectionType = typeof(IEnumerable<>).MakeGenericType(valueType);
        var containsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(valueType);

        var collection = Expression.Constant(value, collectionType);
        return Expression.Call(containsMethod, collection, property);
    }
}
