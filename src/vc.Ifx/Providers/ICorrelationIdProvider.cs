// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Providers;

/// <summary>
/// Provides correlation ID generation and management.
/// </summary>
public interface ICorrelationIdProvider
{

    /// <summary>
    /// Gets the current correlation ID.
    /// </summary>
    string CorrelationId { get; }

    /// Generates a new correlation ID.
    /// <returns>A new correlation ID.</returns>
    string GenerateNew();

    /// Sets the current correlation ID.
    /// <param name="correlationId">The correlation ID to set.</param>
    void SetCorrelationId(string correlationId);

}
