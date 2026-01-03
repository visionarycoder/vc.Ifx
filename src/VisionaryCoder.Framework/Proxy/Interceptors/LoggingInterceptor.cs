// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Interceptors;

/// <summary>
/// Interceptor that provides comprehensive logging for proxy operations including success, failure, and exception scenarios.
/// Captures operation names, correlation IDs, and detailed error information for monitoring and debugging.
/// </summary>
public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 100; // Logging typically runs later in the pipeline to capture final results

    private readonly ILogger<LoggingInterceptor> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Invokes the logging interceptor with comprehensive operation tracking.
    /// Logs operation start, success/failure status, and any exceptions that occur.
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
        DateTimeOffset startTime = DateTimeOffset.UtcNow;

        logger.LogDebug("Starting proxy operation '{OperationName}' with correlation ID '{CorrelationId}' at {StartTime}",
            operationName, correlationId, startTime);

        try
        {
            ProxyResponse<T> response = await next(context, cancellationToken);
            TimeSpan duration = DateTimeOffset.UtcNow - startTime;

            if (response.IsSuccess)
            {
                logger.LogInformation("Proxy operation '{OperationName}' completed successfully in {Duration}ms. Correlation ID: '{CorrelationId}'",
                    operationName, duration.TotalMilliseconds, correlationId);

                // Add success metrics to context
                context.Metadata["LoggedAt"] = DateTimeOffset.UtcNow;
                context.Metadata["LogLevel"] = "Information";
            }
            else
            {
                logger.LogWarning("Proxy operation '{OperationName}' completed with failure in {Duration}ms. Error: '{ErrorMessage}'. Correlation ID: '{CorrelationId}'",
                    operationName, duration.TotalMilliseconds, response.ErrorMessage, correlationId);

                // Add failure metrics to context
                context.Metadata["LoggedAt"] = DateTimeOffset.UtcNow;
                context.Metadata["LogLevel"] = "Warning";
            }

            return response;
        }
        catch (ProxyException ex)
        {
            TimeSpan duration = DateTimeOffset.UtcNow - startTime;
            logger.LogError(ex, "Proxy operation '{OperationName}' failed with proxy exception in {Duration}ms. Correlation ID: '{CorrelationId}'",
                operationName, duration.TotalMilliseconds, correlationId);

            context.Metadata["LoggedAt"] = DateTimeOffset.UtcNow;
            context.Metadata["LogLevel"] = "Error";
            context.Metadata["ExceptionType"] = "ProxyException";

            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            TimeSpan duration = DateTimeOffset.UtcNow - startTime;
            logger.LogWarning("Proxy operation '{OperationName}' was cancelled after {Duration}ms. Correlation ID: '{CorrelationId}'",
                operationName, duration.TotalMilliseconds, correlationId);

            context.Metadata["LoggedAt"] = DateTimeOffset.UtcNow;
            context.Metadata["LogLevel"] = "Warning";
            context.Metadata["ExceptionType"] = "OperationCanceled";

            throw;
        }
        catch (Exception ex)
        {
            TimeSpan duration = DateTimeOffset.UtcNow - startTime;
            logger.LogError(ex, "Proxy operation '{OperationName}' failed with unexpected exception in {Duration}ms. Correlation ID: '{CorrelationId}'",
                operationName, duration.TotalMilliseconds, correlationId);

            context.Metadata["LoggedAt"] = DateTimeOffset.UtcNow;
            context.Metadata["LogLevel"] = "Error";
            context.Metadata["ExceptionType"] = ex.GetType().Name;

            throw;
        }
    }
}
