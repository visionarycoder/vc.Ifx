// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Correlation;

/// <summary>
/// Provides correlation context for request tracking.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Gets or sets the correlation ID.
    /// </summary>
    string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets additional correlation data.
    /// </summary>
    Dictionary<string, string> Data { get; set; }

    /// <summary>
    /// Sets the correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID to set.</param>
    void SetCorrelationId(string correlationId);
}