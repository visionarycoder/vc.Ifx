// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using VisionaryCoder.Framework.Patterns.CQRS.Abstractions;

namespace VisionaryCoder.Framework.Patterns.CQRS;

/// <summary>
/// Default implementation of the mediator pattern for dispatching commands and queries.
/// Uses dependency injection to resolve handlers and pipeline behaviors.
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<Mediator> logger;

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        logger.LogDebug("Sending command {CommandType}", typeof(TCommand).Name);

        ICommandHandler<TCommand>? handler = serviceProvider.GetService<ICommandHandler<TCommand>>();

        if (handler is null)
        {
            string errorMessage = $"No handler registered for command type {typeof(TCommand).Name}";
            logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        // Get pipeline behaviors for this command type
        IEnumerable<IPipelineBehavior<TCommand, Unit>> behaviors = serviceProvider.GetServices<IPipelineBehavior<TCommand, Unit>>();

        // Build the pipeline
        Func<Task<Unit>> handlerFunc = async () =>
        {
            await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        };

        // Wrap handler in behaviors (reverse order so they execute in registration order)
        foreach (IPipelineBehavior<TCommand, Unit> behavior in behaviors.Reverse())
        {
            Func<Task<Unit>> next = handlerFunc;
            handlerFunc = () => behavior.HandleAsync(command, next, cancellationToken);
        }

        // Execute the pipeline
        await handlerFunc().ConfigureAwait(false);

        logger.LogDebug("Command {CommandType} handled successfully", typeof(TCommand).Name);
    }

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        ArgumentNullException.ThrowIfNull(command);

        logger.LogDebug("Sending command {CommandType} expecting response {ResponseType}",
            typeof(TCommand).Name, typeof(TResponse).Name);

        ICommandHandler<TCommand, TResponse>? handler = serviceProvider.GetService<ICommandHandler<TCommand, TResponse>>();

        if (handler is null)
        {
            string errorMessage = $"No handler registered for command type {typeof(TCommand).Name}";
            logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        // Get pipeline behaviors for this command type
        IEnumerable<IPipelineBehavior<TCommand, TResponse>> behaviors = serviceProvider.GetServices<IPipelineBehavior<TCommand, TResponse>>();

        // Build the pipeline
        Func<Task<TResponse>> handlerFunc = () => handler.HandleAsync(command, cancellationToken);

        // Wrap handler in behaviors (reverse order so they execute in registration order)
        foreach (IPipelineBehavior<TCommand, TResponse> behavior in behaviors.Reverse())
        {
            Func<Task<TResponse>> next = handlerFunc;
            handlerFunc = () => behavior.HandleAsync(command, next, cancellationToken);
        }

        // Execute the pipeline
        TResponse response = await handlerFunc().ConfigureAwait(false);

        logger.LogDebug("Command {CommandType} handled successfully with response", typeof(TCommand).Name);

        return response;
    }

    /// <inheritdoc />
    public async Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        ArgumentNullException.ThrowIfNull(query);

        logger.LogDebug("Executing query {QueryType} expecting response {ResponseType}",
            typeof(TQuery).Name, typeof(TResponse).Name);

        IQueryHandler<TQuery, TResponse>? handler = serviceProvider.GetService<IQueryHandler<TQuery, TResponse>>();

        if (handler is null)
        {
            string errorMessage = $"No handler registered for query type {typeof(TQuery).Name}";
            logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        // Get pipeline behaviors for this query type
        IEnumerable<IPipelineBehavior<TQuery, TResponse>> behaviors = serviceProvider.GetServices<IPipelineBehavior<TQuery, TResponse>>();

        // Build the pipeline
        Func<Task<TResponse>> handlerFunc = () => handler.HandleAsync(query, cancellationToken);

        // Wrap handler in behaviors (reverse order so they execute in registration order)
        foreach (IPipelineBehavior<TQuery, TResponse> behavior in behaviors.Reverse())
        {
            Func<Task<TResponse>> next = handlerFunc;
            handlerFunc = () => behavior.HandleAsync(query, next, cancellationToken);
        }

        // Execute the pipeline
        TResponse response = await handlerFunc().ConfigureAwait(false);

        logger.LogDebug("Query {QueryType} executed successfully", typeof(TQuery).Name);

        return response;
    }
}
