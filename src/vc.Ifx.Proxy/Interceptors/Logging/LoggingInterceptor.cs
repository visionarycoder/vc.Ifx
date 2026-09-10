// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Logging;

/// <summary>
/// Interceptor that provides comprehensive logging for proxy operations including success, failure, and exception scenarios.
/// </summary>
public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 100;

    private readonly ILogger<LoggingInterceptor> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Invokes the interceptor with comprehensive logging of the proxy operation.
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
        DateTimeOffset startTime = DateTimeOffset.UtcNow;

        logger.LogDebug("Starting proxy operation '{OperationName}' with correlation ID '{CorrelationId}'",
            operationName, correlationId);
        try
        {
            ProxyResponse<T> proxyResponse = await next(context, cancellationToken);
            TimeSpan duration = DateTimeOffset.UtcNow - startTime;

            if (proxyResponse.IsSuccess)
            {
                logger.LogInformation("Proxy operation '{OperationName}' completed successfully in {Duration}ms. Correlation ID: '{CorrelationId}'",
                    operationName, duration.TotalMilliseconds, correlationId);
                context.Metadata["LoggedAt"] = DateTimeOffset.UtcNow;
                context.Metadata["LogLevel"] = "Information";
            }
            else
            {
                logger.LogWarning("Proxy operation '{OperationName}' completed with failure in {Duration}ms. Error: '{ErrorMessage}'. Correlation ID: '{CorrelationId}'",
                    operationName, duration.TotalMilliseconds, proxyResponse.ErrorMessage, correlationId);
                context.Metadata["LoggedAt"] = DateTimeOffset.UtcNow;
                context.Metadata["LogLevel"] = "Warning";
            }

            return proxyResponse;
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
