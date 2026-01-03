// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.CQRS.Behaviors;

/// <summary>
/// Pipeline behavior that measures and logs execution time of commands and queries.
/// Useful for identifying performance bottlenecks.
/// </summary>
/// <typeparam name="TRequest">The type of request being timed.</typeparam>
/// <typeparam name="TResponse">The type of response being returned.</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> logger;
    private readonly TimeSpan warningThreshold;

    /// <summary>
    /// Initializes a new instance with default warning threshold of 500ms.
    /// </summary>
    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        : this(logger, TimeSpan.FromMilliseconds(500))
    {
    }

    /// <summary>
    /// Initializes a new instance with specified warning threshold.
    /// </summary>
    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, TimeSpan warningThreshold)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.warningThreshold = warningThreshold;
    }

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            TResponse response = await next();
            stopwatch.Stop();

            if (stopwatch.Elapsed > warningThreshold)
            {
                logger.LogWarning(
                    "Long Running Request: {RequestName} took {ElapsedMilliseconds}ms (threshold: {ThresholdMilliseconds}ms)",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    warningThreshold.TotalMilliseconds);
            }
            else
            {
                logger.LogDebug(
                    "{RequestName} completed in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception)
        {
            stopwatch.Stop();
            logger.LogWarning(
                "{RequestName} failed after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
