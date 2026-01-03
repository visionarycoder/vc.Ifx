// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Abstractions.Messaging;

/// <summary>
/// Unified interface for message bus operations.
/// Combines publishing and consuming capabilities.
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Gets the publisher for sending messages.
    /// </summary>
    IMessagePublisher Publisher { get; }

    /// <summary>
    /// Creates a consumer for receiving messages of a specific type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to consume.</typeparam>
    /// <param name="subscriptionName">The name of the subscription.</param>
    IMessageConsumer CreateConsumer<TMessage>(string subscriptionName) where TMessage : IMessage;
}
