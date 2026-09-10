// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Caching.Memory;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching;

/// <summary>
/// Represents a caching policy that defines how items should be cached, including duration, priority, and conditions.
/// This policy is used by caching interceptors and providers to make consistent caching decisions.
/// </summary>
public class CachePolicy
{
    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled.
    /// When disabled, no caching operations will be performed.
    /// </summary>
    public bool IsCachingEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache duration.
    /// Specifies how long items should remain in the cache before expiring.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the cache priority.
    /// Determines the relative importance of cached items when memory pressure occurs.
    /// </summary>
    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

    /// <summary>
    /// Gets or sets a function to determine if a response should be cached.
    /// This predicate allows fine-grained control over what gets cached based on the response content.
    /// </summary>
    public Func<object, bool> ShouldCache { get; set; } = _ => true;

    /// <summary>
    /// Gets or sets a function to determine if a cached response should be refreshed.
    /// This predicate allows conditional cache refresh based on the cached content.
    /// </summary>
    public Func<object, bool> ShouldRefresh { get; set; } = _ => false;
}
