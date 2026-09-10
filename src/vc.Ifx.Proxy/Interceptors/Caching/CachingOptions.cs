// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Caching.Memory;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching;

/// <summary>
/// Configuration options for caching behavior including default policies and operation-specific overrides.
/// Provides comprehensive control over cache duration, priority, size limits, and custom behaviors.
/// </summary>
public sealed class CachingOptions
{
    /// <summary>
    /// Gets or sets the default cache duration for items when no specific policy is defined.
    /// </summary>
    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the default cache priority for memory pressure scenarios.
    /// Higher priority items are less likely to be evicted under memory pressure.
    /// </summary>
    public CacheItemPriority DefaultPriority { get; set; } = CacheItemPriority.Normal;

    /// <summary>
    /// Gets or sets a value indicating whether to enable logging when cache items are evicted.
    /// Useful for monitoring and debugging cache performance.
    /// </summary>
    public bool EnableEvictionLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets operation-specific cache policies that override default behavior.
    /// Keys should match operation names, values define the specific caching policy.
    /// </summary>
    public Dictionary<string, CachePolicy> OperationPolicies { get; set; } = new();

    /// <summary>
    /// Gets or sets the maximum size of the cache in number of entries.
    /// When null, no entry-based size limit is enforced.
    /// </summary>
    public int? MaxCacheSize { get; set; }

    /// <summary>
    /// Gets or sets a custom cache key generator function.
    /// When provided, overrides the default key generation strategy.
    /// </summary>
    public Func<ProxyContext, string>? KeyGenerator { get; set; }

    /// <summary>
    /// Gets or sets a predicate to determine if a response should be cached based on context.
    /// When provided, this predicate is evaluated in addition to policy-based decisions.
    /// </summary>
    public Func<ProxyContext, bool>? ShouldCache { get; set; }

    /// <summary>
    /// Gets or sets the sliding expiration for cache entries.
    /// When set, extends the cache lifetime when items are accessed.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets the memory size limit for the cache in bytes.
    /// When specified, limits total memory usage of cached items.
    /// </summary>
    public long? MemorySizeLimit { get; set; }
}
