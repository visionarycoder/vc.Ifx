// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Abstractions;
using VisionaryCoder.Framework.Proxy.Abstractions.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Interceptor.Logging;
/// <summary>
/// Interceptor that logs proxy operations for monitoring and debugging purposes.
/// </summary>
public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : IOrderedProxyInterceptor
{
    /// <inheritdoc />
    public int Order => 100; // Logging typically runs later in the pipeline
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

        logger.LogDebug("Starting proxy operation '{OperationName}' with correlation ID '{CorrelationId}'",
            operationName, correlationId);
        try
        {
            ProxyResponse<T> proxyResponse = await next(context, cancellationToken).ConfigureAwait(false);

            if (proxyResponse.IsSuccess)
            {
                logger.LogInformation("Proxy operation '{OperationName}' completed successfully. Correlation ID: '{CorrelationId}'",
                    operationName, correlationId);
            }
            else
            {
                logger.LogWarning("Proxy operation '{OperationName}' completed with failure. Error: '{ErrorMessage}'. Correlation ID: '{CorrelationId}'",
                    operationName, proxyResponse.ErrorMessage, correlationId);
            }
            return proxyResponse;
        }
        catch (ProxyException ex)
        {
            logger.LogError(ex, "Proxy operation '{OperationName}' failed with proxy exception. Correlation ID: '{CorrelationId}'", operationName, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Proxy operation '{OperationName}' failed with unexpected exception. Correlation ID: '{CorrelationId}'",
                operationName, correlationId);
            throw;
        }
    }
}
