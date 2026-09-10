// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Telemetry;
/// <summary>
/// Telemetry interceptor that creates activities and tracks proxy operations.
/// Order: -50 (executes early in the pipeline after security).
/// </summary>
public sealed class TelemetryInterceptor : IOrderedProxyInterceptor
{
    private readonly ILogger<TelemetryInterceptor> logger;
    private readonly ActivitySource activitySource;
    /// <inheritdoc />
    public int Order => -50;
    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryInterceptor"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="activitySource">The activity source for telemetry.</param>
    public TelemetryInterceptor(ILogger<TelemetryInterceptor> logger, ActivitySource? activitySource = null)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.activitySource = activitySource ?? new ActivitySource("VisionaryCoder.Framework.Proxy");
    }
    public async Task<ProxyResponse<T>> InvokeAsync<T>(
        ProxyContext context,
        ProxyDelegate<T> next,
        CancellationToken cancellationToken = default)
    {
        string requestType = context.Request?.GetType().Name ?? "Unknown";
        string operationName = $"Proxy.{requestType}";
        using Activity? activity = activitySource.StartActivity(operationName);

        // Enrich activity with context information
        activity?.SetTag("proxy.request_type", requestType);
        activity?.SetTag("proxy.result_type", context.ResultType?.Name);
        if (context.Items.TryGetValue("CorrelationId", out object? correlationId))
        {
            activity?.SetTag("proxy.correlation_id", correlationId?.ToString());
        }
        var stopwatch = Stopwatch.StartNew();
        try
        {
            logger.LogDebug("Starting telemetry for {RequestType}", requestType);

            ProxyResponse<T> result = await next(context, cancellationToken);
            stopwatch.Stop();
            activity?.SetTag("proxy.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("proxy.success", true);
            logger.LogDebug("Telemetry completed successfully for {RequestType} in {ElapsedMs}ms",
                requestType, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("proxy.success", false);
            activity?.SetTag("proxy.error", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Telemetry interceptor caught exception for {RequestType} after {ElapsedMs}ms",
                requestType, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
