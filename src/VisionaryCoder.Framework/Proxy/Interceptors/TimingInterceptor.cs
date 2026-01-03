// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Proxy.Interceptors;

/// <summary>
/// Interceptor that measures and logs the execution time of proxy operations.
/// Provides performance monitoring and can identify slow operations for optimization.
/// </summary>
public sealed class TimingInterceptor(ILogger<TimingInterceptor> logger) : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 10; // Run early to capture complete execution time

    private readonly ILogger<TimingInterceptor> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Gets or sets the threshold in milliseconds above which operations are considered slow.
    /// Operations exceeding this threshold will be logged as warnings.
    /// </summary>
    public long SlowOperationThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the threshold in milliseconds above which operations are considered critical.
    /// Operations exceeding this threshold will be logged as errors.
    /// </summary>
    public long CriticalOperationThresholdMs { get; set; } = 5000;

    /// <summary>
    /// Invokes the timing interceptor to measure proxy operation execution time.
    /// Captures precise timing information and logs based on performance thresholds.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="context">The proxy context containing operation information.</param>
    /// <param name="next">The next delegate in the pipeline to execute.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the async operation with the response.</returns>
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        string operationName = context.OperationName ?? "Unknown";
        string correlationId = context.CorrelationId ?? "None";
        var stopwatch = Stopwatch.StartNew();

        // Record start time for detailed metrics
        DateTimeOffset startTime = DateTimeOffset.UtcNow;
        context.Metadata["StartTime"] = startTime;

        try
        {
            ProxyResponse<T> response = await next(context, cancellationToken);
            stopwatch.Stop();

            long elapsedMs = stopwatch.ElapsedMilliseconds;
            long elapsedTicks = stopwatch.ElapsedTicks;

            // Store comprehensive timing metrics in context
            context.Metadata["ExecutionTimeMs"] = elapsedMs;
            context.Metadata["ExecutionTimeTicks"] = elapsedTicks;
            context.Metadata["EndTime"] = DateTimeOffset.UtcNow;

            // Log based on performance thresholds
            LogOperationTiming(operationName, correlationId, elapsedMs, response.IsSuccess);

            return response;
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
            long elapsedMs = stopwatch.ElapsedMilliseconds;

            context.Metadata["ExecutionTimeMs"] = elapsedMs;
            context.Metadata["EndTime"] = DateTimeOffset.UtcNow;

            logger.LogError(ex, "Proxy operation '{OperationName}' failed after {ElapsedMs}ms. Correlation ID: '{CorrelationId}'",
                operationName, elapsedMs, correlationId);

            throw;
        }
    }

    /// <summary>
    /// Logs operation timing based on configured performance thresholds.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="correlationId">The correlation ID for tracking.</param>
    /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
    /// <param name="isSuccess">Whether the operation was successful.</param>
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
            logger.LogWarning("Slow performance: Proxy operation '{OperationName}' {Status} in {ElapsedMs}ms (>= {Threshold}ms). Correlation ID: '{CorrelationId}'",
                operationName, statusMessage, elapsedMs, SlowOperationThresholdMs, correlationId);
        }
        else
        {
            logger.LogDebug("Proxy operation '{OperationName}' {Status} in {ElapsedMs}ms. Correlation ID: '{CorrelationId}'",
                operationName, statusMessage, elapsedMs, correlationId);
        }
    }
}
