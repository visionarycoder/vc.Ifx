using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ifx.Hosting.AspNetCore.HealthChecks;

/// <summary>Checks that the configured distributed cache can complete a read.</summary>
public sealed class DistributedCacheHealthCheck(IDistributedCache cache) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        _ = await cache.GetAsync($"ifx:health:{Guid.NewGuid():N}", cancellationToken).ConfigureAwait(false);
        return HealthCheckResult.Healthy("The distributed cache is reachable.");
    }
}
