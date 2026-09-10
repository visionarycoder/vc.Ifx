// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Resilience;
/// <summary>
/// Interceptor that implements rate limiting to prevent abuse and ensure fair usage.
/// </summary>
public sealed class RateLimitingInterceptor : IProxyInterceptor
{
    private readonly ILogger<RateLimitingInterceptor> logger;
    private readonly RateLimiterConfig config;
    private readonly ConcurrentDictionary<string, Queue<DateTimeOffset>> requestHistory;
    private readonly object cleanupLock = new();
    private DateTimeOffset lastCleanup = DateTimeOffset.UtcNow;
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitingInterceptor"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="config">Rate limiter configuration.</param>
    public RateLimitingInterceptor(ILogger<RateLimitingInterceptor> logger, RateLimiterConfig? config = null)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.config = config ?? new RateLimiterConfig();
        requestHistory = new ConcurrentDictionary<string, Queue<DateTimeOffset>>();
    }
    /// Invokes the interceptor with rate limiting protection.
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="context">The proxy context.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        string operationName = context.OperationName ?? "Unknown";
        string correlationId = context.CorrelationId ?? "None";
        // Generate rate limit key (could be based on operation, user, IP, etc.)
        string rateLimitKey = GenerateRateLimitKey(context);
        // Check rate limit
        if (!IsRequestAllowed(rateLimitKey))
        {
            logger.LogWarning("Rate limit exceeded for key '{RateLimitKey}' on operation '{OperationName}'. Correlation ID: '{CorrelationId}'",
                rateLimitKey, operationName, correlationId);
            context.Metadata["RateLimited"] = true;
            throw new TransientProxyException($"Rate limit exceeded for operation '{operationName}'. Max {config.MaxRequests} requests per {config.TimeWindow}.");
        }
        // Record the request
        RecordRequest(rateLimitKey);
        // Periodic cleanup of old entries
        PerformCleanupIfNeeded();
        context.Metadata["RateLimited"] = false;
        context.Metadata["RateLimitKey"] = rateLimitKey;
        logger.LogDebug("Rate limit check passed for key '{RateLimitKey}' on operation '{OperationName}'. Correlation ID: '{CorrelationId}'",
            rateLimitKey, operationName, correlationId);
        return await next(context, cancellationToken);
    }
    private string GenerateRateLimitKey(ProxyContext context)
    {
        // Default key generation - can be customized based on requirements
        var keyParts = new List<string>
        {
            context.OperationName ?? "Unknown"
        };
        // Include user identifier if available
        if (context.Metadata.TryGetValue("UserId", out object? userId))
        {
            keyParts.Add($"User:{userId}");
        }
        else if (context.Metadata.TryGetValue("ClientId", out object? clientId))
        {
            keyParts.Add($"Client:{clientId}");
        }
        else
        {
            // Fallback to operation-level limiting
            keyParts.Add("Global");
        }
        return string.Join("|", keyParts);
    }
    private bool IsRequestAllowed(string key)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset cutoffTime = now - config.TimeWindow;
        Queue<DateTimeOffset> requestQueue = requestHistory.GetOrAdd(key, _ => new Queue<DateTimeOffset>());
        lock (requestQueue)
        {
            // Remove old requests outside the time window
            while (requestQueue.Count > 0 && requestQueue.Peek() <= cutoffTime)
            {
                requestQueue.Dequeue();
            }
            // Check if we're within the limit
            return requestQueue.Count < config.MaxRequests;
        }
    }
    private void RecordRequest(string key)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Queue<DateTimeOffset> requestQueue = requestHistory.GetOrAdd(key, _ => new Queue<DateTimeOffset>());
        lock (requestQueue)
        {
            requestQueue.Enqueue(now);
        }
        PerformCleanupIfNeeded();
    }
    private void PerformCleanupIfNeeded()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        // Perform cleanup every 5 minutes
        if (now - lastCleanup < TimeSpan.FromMinutes(5))
            return;
        lock (cleanupLock)
        {
            if (now - lastCleanup < TimeSpan.FromMinutes(5))
                return; // Double-check locking
            lastCleanup = now;
            DateTimeOffset cutoffTime = now - config.TimeWindow.Multiply(2); // Keep some extra history
            var keysToRemove = new List<string>();
            foreach (KeyValuePair<string, Queue<DateTimeOffset>> kvp in requestHistory)
            {
                Queue<DateTimeOffset> requestQueue = kvp.Value;
                lock (requestQueue)
                {
                    // Remove old requests
                    while (requestQueue.Count > 0 && requestQueue.Peek() <= cutoffTime)
                    {
                        requestQueue.Dequeue();
                    }
                    // Remove empty queues
                    if (requestQueue.Count == 0)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }
            // Remove empty queues
            foreach (string key in keysToRemove)
            {
                requestHistory.TryRemove(key, out _);
            }
            logger.LogDebug("Rate limiter cleanup completed. Removed {Count} empty entries", keysToRemove.Count);
        }
    }
}
