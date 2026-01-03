// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Abstractions.CQRS;

/// <summary>
/// Marker interface for queries that retrieve data without modifying state.
/// Queries represent requests for information.
/// </summary>
/// <typeparam name="TResponse">The type of data returned by the query.</typeparam>
/// <remarks>
/// Use IQuery&lt;TResponse&gt; for operations that:
/// - Only read data (never modify)
/// - Are idempotent (can be called multiple times safely)
/// - Return data to the caller
/// Example: GetOrderByIdQuery, SearchProductsQuery, GetUserProfileQuery
/// 
/// Following CQRS principles:
/// - Queries should NEVER modify state
/// - Queries can use optimized read models
/// - Queries can bypass business logic validation
/// </remarks>
public interface IQuery<out TResponse>
{
}
