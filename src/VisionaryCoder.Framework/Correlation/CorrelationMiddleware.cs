// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace VisionaryCoder.Framework.Correlation;

/// <summary>
/// Middleware for managing correlation IDs.
/// Extracts correlation ID from headers or generates a new one.
/// </summary>
public sealed class CorrelationMiddleware
{
    private readonly RequestDelegate next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CausationIdHeaderName = "X-Causation-ID";

    public CorrelationMiddleware(RequestDelegate next)
    {
        this.next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationContextAccessor accessor)
    {
        // Get or generate correlation ID
        string correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        string? causationId = context.Request.Headers[CausationIdHeaderName].FirstOrDefault();
        string? userId = context.User?.Identity?.Name;

        // Set correlation context
        accessor.CorrelationContext = new CorrelationContext
        {
            CorrelationId = correlationId,
            CausationId = causationId,
            UserId = userId
        };

        // Add correlation ID to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);
            if (!string.IsNullOrEmpty(causationId))
            {
                context.Response.Headers.TryAdd(CausationIdHeaderName, causationId);
            }
            return Task.CompletedTask;
        });

        await next(context);
    }
}
