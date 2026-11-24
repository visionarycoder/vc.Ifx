using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using vc.Ifx.Messaging.Kafka;

namespace Ifx.Hosting.AspNetCore.HealthChecks;

/// <summary>Checks that Kafka returns broker metadata.</summary>
public sealed class KafkaBrokerHealthCheck(IOptions<KafkaOptions> options) : IHealthCheck
{
    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        using IAdminClient client = new AdminClientBuilder(options.Value.Validate().CreateAdminClientConfig()).Build();
        Metadata metadata = client.GetMetadata(TimeSpan.FromSeconds(5));
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(metadata.Brokers.Count > 0
            ? HealthCheckResult.Healthy("Kafka broker metadata is available.")
            : HealthCheckResult.Unhealthy("Kafka returned no broker metadata."));
    }
}
