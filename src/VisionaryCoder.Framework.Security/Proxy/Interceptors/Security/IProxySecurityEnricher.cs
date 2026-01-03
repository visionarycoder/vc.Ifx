// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;

/// <summary>
/// Enriches proxy context with security information.
/// </summary>
public interface IProxySecurityEnricher
{
    /// <summary>
    /// Enriches the proxy context with security information.
    /// </summary>
    /// <param name="context">The proxy context to enrich.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default);
}