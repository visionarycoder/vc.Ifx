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


// Copyright (c) 2025 Ifx. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Ifx.Proxy.Interceptor.Caching.Providers;

/// <summary>
/// Defines a contract for generating cache keys based on proxy context.
/// Implementations should create consistent, unique keys that properly identify cached content.
/// </summary>
public interface ICacheKeyProvider
{
    /// <summary>
    /// Generates a cache key for the given proxy context.
    /// The key should be unique and deterministic for the same context inputs.
    /// </summary>
    /// <param name="context">The proxy context containing request information.</param>
    /// <returns>A unique cache key string for the context, or null to bypass caching.</returns>
    string? GenerateKey(ProxyContext context);
}