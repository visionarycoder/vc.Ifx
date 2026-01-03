// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Abstractions.CQRS;

namespace VisionaryCoder.Framework.Core.CQRS;

/// <summary>
/// Default implementation of the mediator pattern for dispatching commands and queries.
/// Uses dependency injection to resolve handlers and pipeline behaviors.
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        ICommandHandler<TCommand>? handler = serviceProvider.GetService<ICommandHandler<TCommand>>();
        
        if (handler is null)
        {
            throw new InvalidOperationException($"No handler registered for command type {typeof(TCommand).Name}");
        }

        // Get pipeline behaviors for this command type
        IEnumerable<IPipelineBehavior<TCommand, Unit>> behaviors = 
            serviceProvider.GetServices<IPipelineBehavior<TCommand, Unit>>();

        // Build the pipeline
        Func<Task<Unit>> handlerFunc = async () =>
        {
            await handler.HandleAsync(command, cancellationToken);
            return Unit.Value;
        };

        // Wrap handler in behaviors (reverse order so they execute in registration order)
        foreach (IPipelineBehavior<TCommand, Unit> behavior in behaviors.Reverse())
        {
            Func<Task<Unit>> next = handlerFunc;
            handlerFunc = () => behavior.HandleAsync(command, next, cancellationToken);
        }

        // Execute the pipeline
        await handlerFunc();
    }

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default) 
        where TCommand : ICommand<TResponse>
    {
        ArgumentNullException.ThrowIfNull(command);

        ICommandHandler<TCommand, TResponse>? handler = serviceProvider.GetService<ICommandHandler<TCommand, TResponse>>();
        
        if (handler is null)
        {
            throw new InvalidOperationException($"No handler registered for command type {typeof(TCommand).Name}");
        }

        // Get pipeline behaviors for this command type
        IEnumerable<IPipelineBehavior<TCommand, TResponse>> behaviors = 
            serviceProvider.GetServices<IPipelineBehavior<TCommand, TResponse>>();

        // Build the pipeline
        Func<Task<TResponse>> handlerFunc = () => handler.HandleAsync(command, cancellationToken);

        // Wrap handler in behaviors (reverse order so they execute in registration order)
        foreach (IPipelineBehavior<TCommand, TResponse> behavior in behaviors.Reverse())
        {
            Func<Task<TResponse>> next = handlerFunc;
            handlerFunc = () => behavior.HandleAsync(command, next, cancellationToken);
        }

        // Execute the pipeline
        return await handlerFunc();
    }

    /// <inheritdoc />
    public async Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default) 
        where TQuery : IQuery<TResponse>
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryHandler<TQuery, TResponse>? handler = serviceProvider.GetService<IQueryHandler<TQuery, TResponse>>();
        
        if (handler is null)
        {
            throw new InvalidOperationException($"No handler registered for query type {typeof(TQuery).Name}");
        }

        // Get pipeline behaviors for this query type
        IEnumerable<IPipelineBehavior<TQuery, TResponse>> behaviors = 
            serviceProvider.GetServices<IPipelineBehavior<TQuery, TResponse>>();

        // Build the pipeline
        Func<Task<TResponse>> handlerFunc = () => handler.HandleAsync(query, cancellationToken);

        // Wrap handler in behaviors (reverse order so they execute in registration order)
        foreach (IPipelineBehavior<TQuery, TResponse> behavior in behaviors.Reverse())
        {
            Func<Task<TResponse>> next = handlerFunc;
            handlerFunc = () => behavior.HandleAsync(query, next, cancellationToken);
        }

        // Execute the pipeline
        return await handlerFunc();
    }
}
