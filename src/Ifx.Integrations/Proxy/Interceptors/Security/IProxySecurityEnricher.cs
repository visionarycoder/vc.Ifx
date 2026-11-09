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

namespace Ifx.Proxy.Interceptor.Security;

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