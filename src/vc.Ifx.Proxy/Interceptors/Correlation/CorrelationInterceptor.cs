// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Correlation;
/// <summary>
/// Correlation interceptor that manages correlation IDs for proxy operations.
/// Order: 0 (executes in the middle of the pipeline).
/// </summary>
public sealed class CorrelationInterceptor : IOrderedProxyInterceptor
{
    private readonly ILogger<CorrelationInterceptor> logger;
    private readonly ICorrelationContext correlationContext;
    private readonly ICorrelationIdGenerator idGenerator;
    /// <inheritdoc />
    public int Order => 0;
    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationInterceptor"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="correlationContext">The correlation context.</param>
    /// <param name="idGenerator">The correlation ID generator.</param>
    public CorrelationInterceptor(
        ILogger<CorrelationInterceptor> logger,
        ICorrelationContext correlationContext,
        ICorrelationIdGenerator idGenerator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
        this.idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
    }
    public async Task<ProxyResponse<T>> InvokeAsync<T>(
        ProxyContext context,
        ProxyDelegate<T> next,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        cancellationToken.ThrowIfCancellationRequested();
        string? previousId = correlationContext.CorrelationId;
        Dictionary<string, string> previousData = correlationContext.Data;
        string? correlationId = previousId;
        try
        {
            correlationContext.Data = new Dictionary<string, string>(previousData ?? []);
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = idGenerator.GenerateId();
                correlationContext.SetCorrelationId(correlationId);
                logger.LogDebug("Generated new correlation ID: {CorrelationId}", correlationId);
            }
            else
                logger.LogDebug("Using existing correlation ID: {CorrelationId}", correlationId);
            context.Items["CorrelationId"] = correlationId;
            context.CorrelationId = correlationId;
            using IDisposable? scope = logger.BeginScope("CorrelationId: {CorrelationId}", correlationId);
            return await next(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in correlation interceptor with CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
        finally
        {
            correlationContext.CorrelationId = previousId;
            correlationContext.Data = previousData!;
        }
    }
}
