// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

/// <summary>
/// Default implementation of <see cref="ICachePolicyProvider"/> that provides caching policies
/// based on HTTP method, operation name, and configured options.
/// Supports operation-specific policies and intelligent defaults.
/// </summary>
public class DefaultCachePolicyProvider(CachingOptions options) : ICachePolicyProvider
{
    private readonly CachingOptions options = options ?? throw new ArgumentNullException(nameof(options));

    /// <summary>
    /// Gets the complete cache policy based on the operation and HTTP method.
    /// Combines operation-specific policies with default fallbacks.
    /// </summary>
    /// <param name="context">The proxy context containing request information.</param>
    /// <returns>The cache policy to apply for this operation.</returns>
    public CachePolicy GetPolicy(ProxyContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Check if caching should be applied at all
        if (!ShouldCache(context))
        {
            return new CachePolicy { IsCachingEnabled = false };
        }

        // Return operation-specific policy if configured
        if (options.OperationPolicies.TryGetValue(context.OperationName ?? string.Empty, out CachePolicy? policy))
        {
            return policy;
        }

        // Return default policy based on HTTP method and options
        return CreateDefaultPolicy(context);
    }

    /// <summary>
    /// Determines whether the request should be cached based on the context and options.
    /// </summary>
    /// <param name="context">The proxy context containing request information.</param>
    /// <returns>True if the request should be cached; otherwise, false.</returns>
    public bool ShouldCache(ProxyContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Apply custom predicate if provided
        if (options.ShouldCache?.Invoke(context) == false)
        {
            return false;
        }

        // Check operation-specific policy
        if (options.OperationPolicies.TryGetValue(context.OperationName ?? string.Empty, out CachePolicy? policy))
        {
            return policy.IsCachingEnabled;
        }

        // Default caching rules based on HTTP method
        return IsMethodCacheable(context.Method);
    }

    /// <summary>
    /// Gets the cache expiration duration for the given context.
    /// </summary>
    /// <param name="context">The proxy context containing request information.</param>
    /// <returns>The cache expiration duration, or null if caching is disabled.</returns>
    public TimeSpan? GetExpiration(ProxyContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!ShouldCache(context))
        {
            return null;
        }

        // Return operation-specific duration
        if (options.OperationPolicies.TryGetValue(context.OperationName ?? string.Empty, out CachePolicy? policy))
        {
            return policy.Duration;
        }

        // Return default duration
        return options.DefaultDuration;
    }

    /// <summary>
    /// Creates a default cache policy for operations without specific configuration.
    /// </summary>
    /// <param name="context">The proxy context.</param>
    /// <returns>A default cache policy.</returns>
    private CachePolicy CreateDefaultPolicy(ProxyContext context)
    {
        return new CachePolicy
        {
            IsCachingEnabled = true,
            Duration = options.DefaultDuration,
            Priority = options.DefaultPriority,
            ShouldCache = _ => IsMethodCacheable(context.Method),
            ShouldRefresh = _ => false
        };
    }

    /// <summary>
    /// Determines if an HTTP method is cacheable by default.
    /// </summary>
    /// <param name="method">The HTTP method to evaluate.</param>
    /// <returns>True if the method is cacheable by default.</returns>
    private static bool IsMethodCacheable(string? method)
    {
        return method switch
        {
            "GET" or "HEAD" => true,
            "POST" => false, // Generally not cached unless explicitly configured
            "PUT" or "DELETE" or "PATCH" => false,
            _ => false
        };
    }
}
