// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.CQRS;

/// <summary>
/// Pipeline behavior for processing commands and queries.
/// Enables cross-cutting concerns like validation, logging, caching, transactions, etc.
/// </summary>
/// <typeparam name="TRequest">The type of request (command or query).</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
/// <remarks>
/// Pipeline behaviors execute in the order they are registered:
/// 1. Logging Behavior
/// 2. Validation Behavior  
/// 3. Transaction Behavior
/// 4. Handler Execution
/// 
/// Each behavior can:
/// - Inspect/modify the request
/// - Short-circuit the pipeline
/// - Wrap handler execution (try/catch)
/// - Inspect/modify the response
/// </remarks>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    /// <summary>
    /// Handles the request by executing cross-cutting logic and calling the next behavior or handler.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">Delegate to the next behavior or handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the response.</returns>
    Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken);
}
