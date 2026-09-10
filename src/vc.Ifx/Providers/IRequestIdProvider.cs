// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Providers;

/// <summary>
/// Provides request ID generation and management.
/// </summary>
public interface IRequestIdProvider
{
    /// <summary>
    /// Gets the current request ID.
    /// </summary>
    string RequestId { get; }

    /// Generates a new request ID.
    /// <returns>A new request ID.</returns>
    string GenerateNew();

    /// Sets the current request ID.
    /// <param name="requestId">The request ID to set.</param>
    void SetRequestId(string requestId);
}
