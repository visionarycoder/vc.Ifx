using Ifx;
using Ifx.Component;
using Ifx.Pipeline;
using Ifx.Proxy.Exceptions;
using Ifx.Querying;
using Ifx.Querying.Serialization;
using Ifx.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Threading.RateLimiting;

// Copyright (c) 2025 Ifx. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Ifx.Proxy.Interceptor.Caching.Providers;

/// <summary>
/// Null Object implementation of <see cref="ICachePolicyProvider"/> that provides safe fallback behavior.
/// Returns no-cache policies when no explicit provider is registered.
/// Follows SOLID principles by ensuring safe operation without implicit defaults.
/// </summary>
public sealed class NullCachePolicyProvider : ICachePolicyProvider
{
    /// <summary>
    /// Returns a cache policy that disables caching entirely.
    /// This ensures safe fallback behavior when no explicit policy provider is configured.
    /// </summary>
    /// <param name="context">The proxy context (ignored in null implementation).</param>
    /// <returns>A cache policy with caching disabled to prevent any caching operations.</returns>
    public CachePolicy GetPolicy(ProxyContext context)
    {
        // Return a policy that explicitly disables caching
        return new CachePolicy
        {
            IsCachingEnabled = false,
            Duration = TimeSpan.Zero,
            ShouldCache = _ => false,
            ShouldRefresh = _ => false
        };
    }

    /// <summary>
    /// Returns null to indicate no expiration should be applied (caching disabled).
    /// This ensures safe fallback behavior when no explicit policy provider is configured.
    /// </summary>
    /// <param name="context">The proxy context (ignored in null implementation).</param>
    /// <returns>Always returns null to indicate caching is disabled.</returns>
    public TimeSpan? GetExpiration(ProxyContext context)
    {
        return null; // No expiration since caching is disabled
    }

    /// <summary>
    /// Always returns false to disable caching entirely.
    /// This ensures safe fallback behavior when no explicit policy provider is configured.
    /// </summary>
    /// <param name="context">The proxy context (ignored in null implementation).</param>
    /// <returns>Always returns false to disable caching.</returns>
    public bool ShouldCache(ProxyContext context)
    {
        return false;
    }
}