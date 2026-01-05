// Copyright (c) VisionaryCoder. Licensed under the MIT License.

using VisionaryCoder.Framework.Querying.Serialization;

namespace VisionaryCoder.Framework.Querying.Abstractions;

/// <summary>
/// Defines a strategy for applying filter nodes to data sources.
/// </summary>
public interface IFilterExecutionStrategy
{
    /// <summary>
    /// Applies filter criteria to an IQueryable data source (e.g., Entity Framework queries).
    /// </summary>
    /// <typeparam name="T">The element type of the query.</typeparam>
    /// <param name="source">The queryable data source.</param>
    /// <param name="filter">The filter node to apply, or null for no filtering.</param>
    /// <returns>A filtered queryable source.</returns>
    IQueryable<T> Apply<T>(IQueryable<T> source, FilterNode? filter);

    /// <summary>
    /// Applies filter criteria to an in-memory IEnumerable data source.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="source">The enumerable data source.</param>
    /// <param name="filter">The filter node to apply, or null for no filtering.</param>
    /// <returns>A filtered enumerable source.</returns>
    IEnumerable<T> Apply<T>(IEnumerable<T> source, FilterNode? filter);
}
