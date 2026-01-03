// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Events;

/// <summary>
/// Dispatcher for domain events.
/// Finds and executes all handlers for a given event type.
/// </summary>
public sealed class DomainEventDispatcher
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<DomainEventDispatcher> logger;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<DomainEventDispatcher> logger)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Dispatches a domain event to all registered handlers.
    /// </summary>
    /// <typeparam name="TEvent">The type of domain event.</typeparam>
    /// <param name="domainEvent">The domain event to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        logger.LogDebug(
            "Dispatching domain event {EventType} with ID {EventId}",
            typeof(TEvent).Name,
            domainEvent.EventId);

        var handlers = serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

        var handlerList = handlers.ToList();
        if (handlerList.Count == 0)
        {
            logger.LogWarning(
                "No handlers registered for domain event {EventType}",
                typeof(TEvent).Name);
            return;
        }

        foreach (var handler in handlerList)
        {
            try
            {
                await handler.HandleAsync(domainEvent, cancellationToken);

                logger.LogDebug(
                    "Handler {HandlerType} processed event {EventType} with ID {EventId}",
                    handler.GetType().Name,
                    typeof(TEvent).Name,
                    domainEvent.EventId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error in handler {HandlerType} processing event {EventType} with ID {EventId}",
                    handler.GetType().Name,
                    typeof(TEvent).Name,
                    domainEvent.EventId);

                // Continue processing other handlers
            }
        }

        logger.LogInformation(
            "Dispatched domain event {EventType} with ID {EventId} to {HandlerCount} handler(s)",
            typeof(TEvent).Name,
            domainEvent.EventId,
            handlerList.Count);
    }

    /// <summary>
    /// Dispatches multiple domain events sequentially.
    /// </summary>
    /// <param name="domainEvents">The domain events to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DispatchManyAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            // Use reflection to call the generic DispatchAsync method
            var dispatchMethod = typeof(DomainEventDispatcher)
                .GetMethod(nameof(DispatchAsync))
                ?.MakeGenericMethod(domainEvent.GetType());

            if (dispatchMethod != null)
            {
                var task = dispatchMethod.Invoke(this, new object[] { domainEvent, cancellationToken }) as Task;
                if (task != null)
                {
                    await task;
                }
            }
        }
    }
}
