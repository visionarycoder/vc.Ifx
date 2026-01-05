// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Messaging.Abstractions;

/// <summary>
/// Base interface for all messages in the messaging framework.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Gets the unique identifier for this message.
    /// </summary>
    string MessageId { get; }

    /// <summary>
    /// Gets the timestamp when the message was created.
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the correlation identifier for tracing related messages.
    /// </summary>
    string? CorrelationId { get; }
}
