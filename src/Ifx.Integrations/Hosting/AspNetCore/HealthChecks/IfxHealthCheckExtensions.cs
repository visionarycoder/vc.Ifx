using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using vc.Ifx.Messaging.Kafka;

namespace Ifx.Hosting.AspNetCore.HealthChecks;

/// <summary>Registers Ifx infrastructure readiness checks.</summary>
public static class IfxHealthCheckExtensions
{
    /// <summary>Registers a read-only connectivity check for the configured distributed cache.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The health-check name.</param>
    /// <param name="tags">Optional health-check tags.</param>
    /// <returns>The health-check builder.</returns>
    public static IHealthChecksBuilder AddIfxDistributedCacheHealthCheck(
        this IServiceCollection services,
        string name = "ifx-distributed-cache",
        IEnumerable<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return services.AddHealthChecks().AddCheck<DistributedCacheHealthCheck>(name, tags: tags);
    }

    /// <summary>Registers a read-only connectivity check for the configured S3 bucket.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The health-check name.</param>
    /// <param name="tags">Optional health-check tags.</param>
    /// <returns>The health-check builder.</returns>
    public static IHealthChecksBuilder AddIfxS3BucketHealthCheck(
        this IServiceCollection services,
        string name = "ifx-s3-bucket",
        IEnumerable<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return services.AddHealthChecks().AddCheck<S3BucketHealthCheck>(name, tags: tags);
    }

    /// <summary>Registers a broker-metadata connectivity check for Kafka.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The health-check name.</param>
    /// <param name="tags">Optional health-check tags.</param>
    /// <returns>The health-check builder.</returns>
    public static IHealthChecksBuilder AddIfxKafkaBrokerHealthCheck(
        this IServiceCollection services,
        string name = "ifx-kafka-broker",
        IEnumerable<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return services.AddHealthChecks().AddCheck<KafkaBrokerHealthCheck>(name, tags: tags);
    }
}
