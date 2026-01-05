// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Patterns.Events.Abstractions;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain that you want other parts of the domain to be aware of.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for the event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}