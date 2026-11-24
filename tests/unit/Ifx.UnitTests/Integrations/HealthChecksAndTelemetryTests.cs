using Amazon.S3;
using Amazon.S3.Model;
using Confluent.Kafka;
using Ifx.Hosting.AspNetCore.HealthChecks;
using Ifx.Hosting.AspNetCore.Observability;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OpenTelemetry.Trace;
using vc.Ifx.Messaging.Kafka;
using Ifx.Storage.S3;

namespace Ifx.Tests.Integrations;

[TestClass]
public sealed class HealthChecksAndTelemetryTests
{
    [TestMethod]
    public async Task DistributedCacheHealthCheck_PerformsAReadAndReturnsHealthy()
    {
        string? requestedKey = null;
        var cache = new Mock<IDistributedCache>(MockBehavior.Strict);
        cache.Setup(value => value.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((key, _) => requestedKey = key)
            .ReturnsAsync((byte[]?)null);
        var check = new DistributedCacheHealthCheck(cache.Object);

        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.AreEqual(HealthStatus.Healthy, result.Status);
        Assert.IsTrue(requestedKey?.StartsWith("ifx:health:", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task S3BucketHealthCheck_QueriesConfiguredBucket()
    {
        var client = new Mock<IAmazonS3>(MockBehavior.Strict);
        client.Setup(value => value.GetBucketLocationAsync(
                It.Is<GetBucketLocationRequest>(request => request.BucketName == "orders"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetBucketLocationResponse());
        var check = new S3BucketHealthCheck(client.Object, new S3StorageOptions { BucketName = "orders" });

        HealthCheckResult result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.AreEqual(HealthStatus.Healthy, result.Status);
    }

    [TestMethod]
    public void HealthCheckAndTelemetryExtensions_RegisterExpectedServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIfxDistributedCacheHealthCheck(tags: ["ready"]);
        services.AddIfxS3BucketHealthCheck();
        services.AddIfxKafkaBrokerHealthCheck();
        services.AddIfxOpenTelemetry("ifx-tests");

        Assert.IsTrue(services.Any(descriptor => descriptor.ServiceType == typeof(TracerProvider)));
        using ServiceProvider provider = services.BuildServiceProvider();
        HealthCheckService healthChecks = provider.GetRequiredService<HealthCheckService>();
        Assert.IsNotNull(healthChecks);
    }

    [TestMethod]
    public void ExtensionsRejectNullCollectionsAndInvalidServiceName()
    {
        IServiceCollection services = new ServiceCollection();
        Assert.ThrowsExactly<ArgumentException>(() => services.AddIfxOpenTelemetry(" "));
        Assert.ThrowsExactly<ArgumentNullException>(() => IfxOpenTelemetryExtensions.AddIfxOpenTelemetry(null!, "service"));
        Assert.ThrowsExactly<ArgumentNullException>(() => IfxHealthCheckExtensions.AddIfxKafkaBrokerHealthCheck(null!));
    }
}
