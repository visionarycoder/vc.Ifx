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

namespace Ifx.Proxy.Interceptor.Correlation;

/// <summary>
/// Provides correlation context for request tracking.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Gets or sets the correlation ID.
    /// </summary>
    string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets additional correlation data.
    /// </summary>
    Dictionary<string, string> Data { get; set; }

    /// <summary>
    /// Sets the correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID to set.</param>
    void SetCorrelationId(string correlationId);
}