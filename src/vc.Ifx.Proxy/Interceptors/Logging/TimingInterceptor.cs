// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Logging;

/// <summary>
/// Interceptor that measures and logs the execution time of proxy operations.
/// </summary>
public sealed class TimingInterceptor(ILogger<TimingInterceptor> logger) : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 10;

    private readonly ILogger<TimingInterceptor> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Gets or sets the threshold in milliseconds above which operations are considered slow.
    /// </summary>
    public long SlowOperationThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the threshold in milliseconds above which operations are considered critical.
    /// </summary>
    public long CriticalOperationThresholdMs { get; set; } = 5000;

    /// <summary>
    /// Invokes the interceptor with timing measurement of the proxy operation.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="context">The proxy context.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        string operationName = context.OperationName ?? "Unknown";
        string correlationId = context.CorrelationId ?? "None";
        var stopwatch = Stopwatch.StartNew();
        DateTimeOffset startTime = DateTimeOffset.UtcNow;
        context.Metadata["StartTime"] = startTime;

        try
        {
            ProxyResponse<T> proxyResponse = await next(context, cancellationToken);
            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            long elapsedTicks = stopwatch.ElapsedTicks;

            context.Metadata["ExecutionTimeMs"] = elapsedMs;
            context.Metadata["ExecutionTimeTicks"] = elapsedTicks;
            context.Metadata["EndTime"] = DateTimeOffset.UtcNow;

            LogOperationTiming(operationName, correlationId, elapsedMs, proxyResponse.IsSuccess);
            return proxyResponse;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            context.Metadata["ExecutionTimeMs"] = elapsedMs;
            context.Metadata["EndTime"] = DateTimeOffset.UtcNow;

            logger.LogWarning("Proxy operation '{OperationName}' was cancelled after {ElapsedMs}ms. Correlation ID: '{CorrelationId}'",
                operationName, elapsedMs, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            context.Metadata["ExecutionTimeMs"] = stopwatch.ElapsedMilliseconds;
            context.Metadata["EndTime"] = DateTimeOffset.UtcNow;
            logger.LogError(ex, "Proxy operation '{OperationName}' failed after {ElapsedMs}ms. Correlation ID: '{CorrelationId}'",
                operationName, stopwatch.ElapsedMilliseconds, correlationId);
            throw;
        }
    }

    private void LogOperationTiming(string operationName, string correlationId, long elapsedMs, bool isSuccess)
    {
        string statusMessage = isSuccess ? "completed successfully" : "completed with failure";

        if (elapsedMs >= CriticalOperationThresholdMs)
        {
            logger.LogError("Critical performance: Proxy operation '{OperationName}' {Status} in {ElapsedMs}ms (>= {Threshold}ms). Correlation ID: '{CorrelationId}'",
                operationName, statusMessage, elapsedMs, CriticalOperationThresholdMs, correlationId);
        }
        else if (elapsedMs >= SlowOperationThresholdMs)
        {
            logger.LogWarning("Slow proxy operation '{OperationName}' {Status} in {ElapsedMs}ms (>= {Threshold}ms). Correlation ID: '{CorrelationId}'",
                operationName, statusMessage, elapsedMs, SlowOperationThresholdMs, correlationId);
        }
        else
        {
            logger.LogDebug("Proxy operation '{OperationName}' {Status} in {ElapsedMs}ms. Correlation ID: '{CorrelationId}'",
                operationName, statusMessage, elapsedMs, correlationId);
        }
    }
}
