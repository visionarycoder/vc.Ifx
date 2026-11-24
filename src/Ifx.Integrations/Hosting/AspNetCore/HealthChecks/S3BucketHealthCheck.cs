using Amazon.S3;
using Amazon.S3.Model;
using Ifx.Storage.S3;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ifx.Hosting.AspNetCore.HealthChecks;

/// <summary>Checks that the configured S3 bucket can be queried.</summary>
public sealed class S3BucketHealthCheck(IAmazonS3 client, S3StorageOptions options) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        _ = await client.GetBucketLocationAsync(
            new GetBucketLocationRequest { BucketName = options.BucketName },
            cancellationToken).ConfigureAwait(false);
        return HealthCheckResult.Healthy("The S3 bucket is reachable.");
    }
}
