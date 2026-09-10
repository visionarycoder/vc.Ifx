// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

/// <summary>
/// Defines a contract for cache policy providers that determine caching behavior.
/// Implementations should provide consistent caching rules based on context.
/// </summary>
public interface ICachePolicyProvider
{
    /// <summary>
    /// Gets the complete cache policy for the given proxy context.
    /// </summary>
    /// <param name="context">The proxy context containing request information.</param>
    /// <returns>The cache policy to apply for this operation.</returns>
    CachePolicy GetPolicy(ProxyContext context);

    /// <summary>
    /// Gets the cache expiration duration for the given proxy context.
    /// </summary>
    /// <param name="context">The proxy context containing request information.</param>
    /// <returns>The cache expiration time, or null if no caching should be applied.</returns>
    TimeSpan? GetExpiration(ProxyContext context);

    /// <summary>
    /// Determines whether the operation should be cached based on the proxy context.
    /// </summary>
    /// <param name="context">The proxy context containing request information.</param>
    /// <returns>True if the operation should be cached; otherwise, false.</returns>
    bool ShouldCache(ProxyContext context);
}