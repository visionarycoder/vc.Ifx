using Microsoft.Extensions.Logging;
using System.Threading.RateLimiting;
using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Resilience;

/// <summary>Applies per-operation and identity sliding-window limits using the .NET rate limiter.</summary>
public sealed class RateLimitingInterceptor : IProxyInterceptor, IDisposable
{
    private readonly ILogger<RateLimitingInterceptor> logger;
    private readonly PartitionedRateLimiter<ProxyContext> limiter;

    /// <summary>Creates an owned limiter with ten segments per window and no waiting queue.</summary>
    public RateLimitingInterceptor(ILogger<RateLimitingInterceptor> logger, RateLimiterConfig? config = null)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        RateLimiterConfig configured = config ?? new RateLimiterConfig();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(configured.MaxRequests);
        if (configured.TimeWindow < TimeSpan.FromMilliseconds(10))
            throw new ArgumentOutOfRangeException(nameof(config), "The rate limiting window must be at least 10 milliseconds.");
        int permits = configured.MaxRequests;
        TimeSpan window = configured.TimeWindow;
        limiter = PartitionedRateLimiter.Create<ProxyContext, (string Operation, string Kind, string Identity)>(context =>
            RateLimitPartition.GetSlidingWindowLimiter(Partition(context), partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = permits,
                Window = window,
                SegmentsPerWindow = 10,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    }

    /// <inheritdoc />
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        cancellationToken.ThrowIfCancellationRequested();
        using RateLimitLease lease = limiter.AttemptAcquire(context);
        context.Metadata["RateLimited"] = !lease.IsAcquired;
        if (!lease.IsAcquired)
        {
            logger.LogWarning("Proxy rate limit exceeded for operation {OperationName}.", context.OperationName);
            throw new TransientProxyException("The proxy operation rate limit was exceeded.");
        }
        context.Metadata["RateLimitKey"] = Partition(context).ToString();
        return await next(context, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Disposes this interceptor's rate-limiter partitions and timers.</summary>
    public void Dispose() => limiter.Dispose();

    private static (string Operation, string Kind, string Identity) Partition(ProxyContext context)
    {
        string operation = context.OperationName ?? "Unknown";
        if (context.Metadata.TryGetValue("UserId", out object? userId) && userId is not null)
            return (operation, "User", userId.ToString() ?? string.Empty);
        if (context.Metadata.TryGetValue("ClientId", out object? clientId) && clientId is not null)
            return (operation, "Client", clientId.ToString() ?? string.Empty);
        return (operation, "Global", string.Empty);
    }
}
