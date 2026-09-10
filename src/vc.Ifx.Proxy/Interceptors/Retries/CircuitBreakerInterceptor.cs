// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Retries;
/// <summary>
/// Interceptor that implements the circuit breaker pattern to prevent cascading failures.
/// </summary>
public sealed class CircuitBreakerInterceptor : IProxyInterceptor
{
    private readonly ILogger<CircuitBreakerInterceptor> logger;
    private readonly int failureThreshold;
    private readonly TimeSpan timeout;
    private readonly object lockObject = new();

    private CircuitBreakerState state = CircuitBreakerState.Closed;
    private int failureCount;
    private DateTimeOffset lastFailureTime;
    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerInterceptor"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="failureThreshold">Number of failures before opening the circuit.</param>
    /// <param name="timeout">Time to wait before attempting to close the circuit.</param>
    public CircuitBreakerInterceptor(ILogger<CircuitBreakerInterceptor> logger, int failureThreshold = 5, TimeSpan? timeout = null)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.failureThreshold = Math.Max(1, failureThreshold);
        this.timeout = timeout ?? TimeSpan.FromMinutes(1);
    }
    /// Gets the current circuit breaker state.
    public CircuitBreakerState State
    {
        get
        {
            lock (lockObject)
            {
                return state;
            }
        }
    }
    /// Invokes the interceptor with circuit breaker protection.
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="context">The proxy context.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        string operationName = context.OperationName ?? "Unknown";
        string correlationId = context.CorrelationId ?? "None";
        lock (lockObject)
        {
            switch (state)
            {
                case CircuitBreakerState.Open:
                    if (DateTimeOffset.UtcNow - lastFailureTime < timeout)
                    {
                        logger.LogWarning("Circuit breaker is OPEN for operation '{OperationName}'. Correlation ID: '{CorrelationId}'",
                            operationName, correlationId);

                        context.Metadata["CircuitBreakerState"] = state.ToString();
                        throw new TransientProxyException($"Circuit breaker is open for operation '{operationName}'");
                    }

                    // Timeout elapsed, try half-open
                    state = CircuitBreakerState.HalfOpen;
                    logger.LogInformation("Circuit breaker transitioning to HALF-OPEN for operation '{OperationName}'. Correlation ID: '{CorrelationId}'",
                        operationName, correlationId);
                    break;
                case CircuitBreakerState.HalfOpen:
                    // Allow one request through
                    break;
                case CircuitBreakerState.Closed:
                    // Normal operation
                    break;
            }
        }
        try
        {
            ProxyResponse<T> proxyResponse = await next(context, cancellationToken);
            lock (lockObject)
            {
                // Success - reset failure count and close circuit if needed
                if (state == CircuitBreakerState.HalfOpen)
                {
                    state = CircuitBreakerState.Closed;
                    logger.LogInformation("Circuit breaker closing after successful operation '{OperationName}'. Correlation ID: '{CorrelationId}'",
                        operationName, correlationId);
                }
                failureCount = 0;
                context.Metadata["CircuitBreakerState"] = state.ToString();
            }
            return proxyResponse;
        }
        catch (Exception)
        {
            lock (lockObject)
            {
                failureCount++;
                lastFailureTime = DateTimeOffset.UtcNow;
                if (state == CircuitBreakerState.HalfOpen)
                {
                    // Failed during half-open, go back to open
                    state = CircuitBreakerState.Open;
                    logger.LogWarning("Circuit breaker opening after failed test during HALF-OPEN state for operation '{OperationName}'. Correlation ID: '{CorrelationId}'",
                        operationName, correlationId);
                }
                else if (failureCount >= failureThreshold && state == CircuitBreakerState.Closed)
                {
                    state = CircuitBreakerState.Open;
                    // Threshold reached, open the circuit
                    logger.LogError("Circuit breaker opening after {FailureCount} failures for operation '{OperationName}'. Correlation ID: '{CorrelationId}'",
                        failureCount, operationName, correlationId);
                }
                context.Metadata["CircuitBreakerFailureCount"] = failureCount.ToString();
                context.Metadata["CircuitBreakerState"] = state.ToString();
            }
            throw;
        }
    }
}
