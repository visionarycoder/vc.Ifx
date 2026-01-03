// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Messaging;

/// <summary>
/// Interface for publishing messages to a message bus.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the message bus.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage;

    /// <summary>
    /// Publishes a batch of messages to the message bus.
    /// </summary>
    /// <typeparam name="TMessage">The type of messages to publish.</typeparam>
    /// <param name="messages">The messages to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishBatchAsync<TMessage>(IEnumerable<TMessage> messages, CancellationToken cancellationToken = default)
        where TMessage : IMessage;

    /// <summary>
    /// Schedules a message to be published at a specified time.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="scheduledTime">The time to publish the message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ScheduleAsync<TMessage>(TMessage message, DateTimeOffset scheduledTime, CancellationToken cancellationToken = default)
        where TMessage : IMessage;
}
