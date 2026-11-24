# Ifx general integration source

General adapters grouped by capability: EF Core querying, FTP and S3 storage,
RabbitMQ and Kafka messaging, Redis-backed proxy caching, gRPC transport, SevenZip
archiving, ASP.NET Core health checks and OpenTelemetry OTLP hosting, and shared
interceptors. This source group is compiled into the `Ifx` assembly for both supported targets. It requires the ASP.NET Core shared framework. Azure and Windows provider sources are retained in their own source groups. Public namespaces are unchanged; applications reference the `Ifx` package.

See [ownership and migration](../../docs/architecture/root-library-consolidation.md).

## Provider setup

Redis proxy caching uses in-process memory as L1 and Redis as L2. A Redis Pub/Sub
channel invalidates matching L1 entries after writes and removals, and advances
the L1 generation after clears. Share the same `IConnectionMultiplexer` with both
tiers:

```csharp
IConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(
    configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("The Redis connection is not configured."));
services.AddSingleton<IConnectionMultiplexer>(redis);
services.AddRedisProxyCaching(
    redis,
    configureCache: options => options.KeyPrefix = "orders");
```

Pub/Sub invalidation is at-most-once. L1 entries retain their configured
expiration, so a disconnected subscriber can keep an entry until that expiration.

S3 storage scopes object keys to the configured prefix and uses the AWS SDK's
default credential and region chain. Register an `IAmazonS3` first when the
application supplies a custom client:

```csharp
services.AddS3ObjectStorage(new S3StorageOptions
{
    BucketName = configuration["Storage:S3:Bucket"]
        ?? throw new InvalidOperationException("The S3 bucket is not configured."),
    KeyPrefix = "orders"
});
```

The S3 adapter supports overwrite writes; it rejects create-only writes because
the current provider-neutral contract does not expose an atomic S3 condition.

Kafka derives deterministic topic names from the message assembly and type name.
Consumers commit offsets only after their handler succeeds. A handler failure is
logged and stops that consumer, leaving the message available for redelivery when
the consumer restarts:

```csharp
services.AddKafkaMessaging(configuration);
services.AddKafkaMessageHandler<InvoiceHandler, InvoiceCreated>("billing");
```

ASP.NET Core readiness checks are registered for Redis, S3, or Kafka individually
with `AddIfxDistributedCacheHealthCheck`, `AddIfxS3BucketHealthCheck`, and
`AddIfxKafkaBrokerHealthCheck`. These checks contact their external dependencies;
use them for readiness rather than process-only liveness. `AddIfxOpenTelemetry`
registers Ifx tracing and metrics, ASP.NET Core and HTTP client instrumentation,
and OTLP exporters. Configure exporter endpoints and credentials through the
OpenTelemetry environment variables or application configuration, not source code.
