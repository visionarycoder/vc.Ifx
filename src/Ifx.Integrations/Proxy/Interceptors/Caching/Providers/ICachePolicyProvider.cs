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