// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Abstractions.Events;

namespace VisionaryCoder.Framework.Core.Events;

/// <summary>
/// Dispatcher for domain events.
/// Finds and executes all handlers for a given event type.
/// </summary>
public sealed class DomainEventDispatcher
{
    private readonly IServiceProvider serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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

        IEnumerable<IDomainEventHandler<TEvent>> handlers = serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

        foreach (IDomainEventHandler<TEvent> handler in handlers)
        {
            try
            {
                await handler.HandleAsync(domainEvent, cancellationToken);
            }
            catch
            {
                // Continue processing other handlers even if one fails
                // Consumers should add their own error handling/logging if needed
                throw;
            }
        }
    }

    /// <summary>
    /// Dispatches multiple domain events sequentially.
    /// </summary>
    /// <param name="domainEvents">The domain events to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DispatchManyAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            // Use reflection to call the generic DispatchAsync method
            var dispatchMethod = typeof(DomainEventDispatcher)
                .GetMethod(nameof(DispatchAsync))
                ?.MakeGenericMethod(domainEvent.GetType());

            if (dispatchMethod != null)
            {
                var task = dispatchMethod.Invoke(this, [domainEvent, cancellationToken]) as Task;
                if (task != null)
                {
                    await task;
                }
            }
        }
    }
}
