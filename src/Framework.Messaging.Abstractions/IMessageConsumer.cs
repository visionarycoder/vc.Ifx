// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Messaging.Abstractions;

/// <summary>
/// Interface for consuming messages from a message bus.
/// </summary>
public interface IMessageConsumer
{
    /// <summary>
    /// Starts consuming messages.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops consuming messages.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StopAsync(CancellationToken cancellationToken = default);
}