// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Correlation;

/// <summary>
/// Context for tracking correlation across service boundaries.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Gets the correlation ID for the current request.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the causation ID (the ID of the command/message that caused this request).
    /// </summary>
    string? CausationId { get; }

    /// <summary>
    /// Gets the user ID associated with the request.
    /// </summary>
    string? UserId { get; }
}

/// <summary>
/// Default implementation of correlation context.
/// </summary>
public sealed class CorrelationContext : ICorrelationContext
{
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public string? CausationId { get; init; }
    public string? UserId { get; init; }
}
