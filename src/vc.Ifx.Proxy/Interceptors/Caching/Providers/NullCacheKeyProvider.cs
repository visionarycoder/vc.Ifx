// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

/// <summary>
/// Null Object implementation of <see cref="ICacheKeyProvider"/> that provides safe fallback behavior.
/// Returns consistent but non-functional cache keys when no explicit provider is registered.
/// Follows SOLID principles by ensuring safe operation without implicit defaults.
/// </summary>
public sealed class NullCacheKeyProvider : ICacheKeyProvider
{
    /// <summary>
    /// Returns a null cache key to indicate caching should be bypassed.
    /// This ensures safe fallback behavior when no explicit key provider is configured.
    /// </summary>
    /// <param name="context">The proxy context (ignored in null implementation).</param>
    /// <returns>Always returns null to bypass caching.</returns>
    public string? GenerateKey(ProxyContext context)
    {
        // Return null to signal that caching should be bypassed
        // This is safer than generating potentially incorrect keys
        return null;
    }

    /// <summary>
    /// Indicates whether this provider can generate a cache key for the given context.
    /// Always returns false in the null implementation.
    /// </summary>
    /// <param name="context">The proxy context (ignored in null implementation).</param>
    /// <returns>Always returns false to indicate caching is not available.</returns>
    public bool CanGenerateKey(ProxyContext context)
    {
        return false;
    }
}
