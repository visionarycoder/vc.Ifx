// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Patterns.CQRS.Abstractions;

/// <summary>
/// Mediator for dispatching commands and queries to their respective handlers.
/// Provides a central point of entry for all CQRS operations.
/// </summary>
/// <remarks>
/// The mediator pattern:
/// - Decouples senders from receivers
/// - Enables pipeline behaviors (validation, logging, transactions)
/// - Simplifies testing and maintenance
/// - Supports cross-cutting concerns
/// </remarks>
public interface IMediator
{
    /// <summary>
    /// Sends a command to its handler.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to send.</typeparam>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;

    /// <summary>
    /// Sends a command to its handler and returns a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to send.</typeparam>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the command response.</returns>
    Task<TResponse> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResponse>;

    /// <summary>
    /// Sends a query to its handler and returns a response.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to send.</typeparam>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="query">The query to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the query response.</returns>
    Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResponse>;
}
