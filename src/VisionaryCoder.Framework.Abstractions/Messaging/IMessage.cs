// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Abstractions.Messaging;

/// <summary>
/// Marker interface for all messages.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Gets the unique identifier for the message.
    /// </summary>
    string MessageId { get; }

    /// <summary>
    /// Gets the correlation identifier for tracking related messages.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Gets the timestamp when the message was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; }
}

/// <summary>
/// Base implementation of IMessage.
/// </summary>
public abstract record MessageBase : IMessage
{
    /// <inheritdoc />
    public string MessageId { get; init; } = Guid.NewGuid().ToString();

    /// <inheritdoc />
    public string? CorrelationId { get; init; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
