// Copyright (c) 2025 Ifx. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Ifx.Proxy.Interceptor.Correlation;

/// <summary>
/// Generates correlation IDs for request tracking.
/// </summary>
public interface ICorrelationIdGenerator
{
    /// <summary>
    /// Generates a new correlation ID.
    /// </summary>
    /// <returns>A unique correlation ID.</returns>
    string GenerateId();
}