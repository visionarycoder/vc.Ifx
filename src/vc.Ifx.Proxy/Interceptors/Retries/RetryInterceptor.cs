// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Retries;
/// <summary>
/// Retry interceptor that implements exponential backoff retry logic.
/// Order: 200 (executes very late in the pipeline).
/// Only retries RetryableTransportException.
/// </summary>
public sealed class RetryInterceptor : IOrderedProxyInterceptor
{
    private readonly ILogger<RetryInterceptor> logger;
    private readonly ProxyOptions options;
    /// <inheritdoc />
    public int Order => 200;
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryInterceptor"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The proxy options.</param>
    public RetryInterceptor(ILogger<RetryInterceptor> logger, IOptionsSnapshot<ProxyOptions> options)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        int attempt = 0;
        int maxRetries = options.MaxRetryAttempts;
        TimeSpan baseDelay = options.RetryDelay;
        while (true)
        {
            try
            {
                ProxyResponse<T> result = await next(context, cancellationToken);
                if (attempt > 0)
                {
                    logger.LogInformation("Operation succeeded after {Attempt} retries", attempt);
                }
                return result;
            }
            catch (RetryableTransportException ex) when (attempt < maxRetries)
            {
                attempt++;
                TimeSpan delay = CalculateDelay(baseDelay, attempt);
                LoggerExtensions.LogWarning(logger, ex, "Retryable exception on attempt {Attempt}/{MaxAttempts}, retrying in {Delay}ms",
                    attempt, maxRetries + 1, delay.TotalMilliseconds);
                await Task.Delay(delay, context.CancellationToken);
            }
            catch (RetryableTransportException ex) when (attempt >= maxRetries)
            {
                LoggerExtensions.LogError(logger, ex, "Operation failed after {MaxAttempts} attempts, giving up", maxRetries + 1);
                throw;
            }
            catch (BusinessException ex)
            {
                logger.LogDebug("Business exception encountered, not retrying: {Message}", ex.Message);
            }
            catch (NonRetryableTransportException ex)
            {
                logger.LogDebug("Non-retryable transport exception encountered, not retrying: {Message}", ex.Message);
            }
            catch (ProxyCanceledException ex)
            {
                logger.LogDebug("Operation was cancelled, not retrying: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected exception encountered, not retrying");
            }
        }
    }
    private static TimeSpan CalculateDelay(TimeSpan baseDelay, int attempt)
    {
        // Exponential backoff: baseDelay * (2 ^ (attempt - 1))
        // With jitter to avoid thundering herd
        var exponentialDelay = TimeSpan.FromMilliseconds(
            baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
        // Add jitter (±25% random variation)
        double jitter = Random.Shared.NextDouble() * 0.5 - 0.25; // -0.25 to +0.25
        var jitteredDelay = TimeSpan.FromMilliseconds(
            exponentialDelay.TotalMilliseconds * (1 + jitter));
        // Cap at maximum reasonable delay (e.g., 30 seconds)
        var maxDelay = TimeSpan.FromSeconds(30);
        return jitteredDelay > maxDelay ? maxDelay : jitteredDelay;
    }
}
