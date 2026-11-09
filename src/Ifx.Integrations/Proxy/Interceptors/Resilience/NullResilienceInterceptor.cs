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

namespace Ifx.Proxy.Interceptor.Resilience;
/// <summary>
/// Null object pattern implementation of resilience interceptor that performs no operations.
/// </summary>
public sealed class NullResilienceInterceptor : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 180;
    public Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        // Pass through without any resilience processing
        return next(context, cancellationToken);
    }
}
