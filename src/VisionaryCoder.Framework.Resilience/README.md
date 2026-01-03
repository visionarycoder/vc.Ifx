# VisionaryCoder.Framework.Resilience

Comprehensive resilience patterns for VisionaryCoder Framework using Polly and health checks for fault-tolerant applications.

## Features

### Retry Policies
- Exponential backoff with jitter
- Configurable retry counts and delays
- Transient fault detection
- Retry on specific exceptions
- Async retry support

### Circuit Breakers
- Automatic failure threshold detection
- Half-open state for recovery testing
- Configurable break duration
- Event notifications for state changes
- Per-endpoint circuit breaker isolation

### Rate Limiting
- Token bucket algorithm
- Sliding window rate limiting
- Per-user/per-role rate limits
- Queue-based rate limiting
- Distributed rate limiting support

### Bulkhead Isolation
- Resource isolation patterns
- Max parallel execution control
- Queue-based overflow handling
- Per-operation bulkheads
- Thread pool isolation

### Health Checks
- Database connectivity checks
- External service health monitoring
- Custom health check implementations
- Aggregated health status
- Health check UI integration

### Resilience Interceptors
- Automatic retry on proxy calls
- Circuit breaker per service
- Rate limiting enforcement
- Timeout policies
- Fallback mechanisms

## Installation

```bash
dotnet add package VisionaryCoder.Framework.Resilience
```

## Quick Start

### Retry Policy

```csharp
services.AddHttpClient("MyService")
    .AddRetryPolicy(options =>
    {
        options.MaxRetryAttempts = 3;
        options.BackoffType = BackoffType.Exponential;
        options.UseJitter = true;
    });

// Usage
var response = await httpClient.GetAsync("https://api.example.com/data");
// Automatically retries on transient failures
```

### Circuit Breaker

```csharp
services.AddHttpClient("ExternalService")
    .AddCircuitBreakerPolicy(options =>
    {
        options.FailureThreshold = 0.5; // Break after 50% failures
        options.SamplingDuration = TimeSpan.FromSeconds(30);
        options.BreakDuration = TimeSpan.FromSeconds(60);
        options.MinimumThroughput = 10; // Min requests before breaking
    });

// Circuit breaks automatically on repeated failures
// Enters half-open state after break duration
// Auto-recovers when service is healthy
```

### Rate Limiting

```csharp
services.AddRateLimiting(options =>
{
    options.AddTokenBucketLimiter("api", limiter =>
    {
        limiter.TokenLimit = 100;
        limiter.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        limiter.TokensPerPeriod = 100;
        limiter.QueueLimit = 10;
    });
});

app.UseRateLimiter();

// Use in controllers
[EnableRateLimiting("api")]
public class ApiController : ControllerBase
{
    // Rate limited endpoints
}
```

### Bulkhead Isolation

```csharp
services.AddHttpClient("CriticalService")
    .AddBulkheadPolicy(options =>
    {
        options.MaxParallelization = 10;
        options.MaxQueuingActions = 20;
    });

// Limits concurrent calls to prevent resource exhaustion
```

### Health Checks

```csharp
services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "database")
    .AddRedis(redisConnection, name: "cache")
    .AddCheck<CustomHealthCheck>("external-api")
    .AddCheck("memory", () =>
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var threshold = 1024 * 1024 * 1024; // 1GB
        return allocated < threshold
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Degraded("High memory usage");
    });

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### Resilience Interceptors

```csharp
// Configure resilience for proxy calls
services.AddProxyResilience(options =>
{
    options.EnableRetry = true;
    options.MaxRetryAttempts = 3;
    options.EnableCircuitBreaker = true;
    options.CircuitBreakerThreshold = 0.5;
    options.EnableRateLimiting = true;
    options.RateLimitPerSecond = 100;
    options.EnableTimeout = true;
    options.TimeoutSeconds = 30;
});

// Interceptors automatically applied to all proxy calls
services.AddProxyInterceptor<RetryInterceptor>(order: 10);
services.AddProxyInterceptor<CircuitBreakerInterceptor>(order: 20);
services.AddProxyInterceptor<RateLimitingInterceptor>(order: 30);
```

## Advanced Patterns

### Combined Resilience Strategy

```csharp
services.AddHttpClient("RobustService")
    .AddPolicyHandler(Policy.WrapAsync(
        // Outer: Timeout
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30)),
        
        // Middle: Circuit Breaker
        Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1)),
        
        // Inner: Retry with exponential backoff
        Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                Backoff.DecorrelatedJitterBackoffV2(
                    medianFirstRetryDelay: TimeSpan.FromSeconds(1),
                    retryCount: 3))
    ));
```

### Custom Health Check

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnection connection;
    
    public DatabaseHealthCheck(IDbConnection connection)
    {
        this.connection = connection;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await connection.OpenAsync(cancellationToken);
            
            var latency = await MeasureLatencyAsync(cancellationToken);
            
            if (latency < TimeSpan.FromMilliseconds(100))
                return HealthCheckResult.Healthy($"Latency: {latency.TotalMilliseconds}ms");
            
            if (latency < TimeSpan.FromMilliseconds(500))
                return HealthCheckResult.Degraded($"High latency: {latency.TotalMilliseconds}ms");
            
            return HealthCheckResult.Unhealthy($"Excessive latency: {latency.TotalMilliseconds}ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
```

### Fallback Strategy

```csharp
services.AddHttpClient("ServiceWithFallback")
    .AddPolicyHandler(Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .FallbackAsync(
            fallbackValue: new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetCachedData())
            },
            onFallbackAsync: async (result, context) =>
            {
                logger.LogWarning("Falling back to cached data");
                await NotifyFallbackAsync(context);
            }));
```

## Configuration

### appsettings.json

```json
{
  "Resilience": {
    "Retry": {
      "MaxAttempts": 3,
      "BackoffType": "Exponential",
      "InitialDelay": "00:00:01",
      "MaxDelay": "00:00:30",
      "UseJitter": true
    },
    "CircuitBreaker": {
      "FailureThreshold": 0.5,
      "SamplingDuration": "00:00:30",
      "BreakDuration": "00:01:00",
      "MinimumThroughput": 10
    },
    "RateLimit": {
      "PermitLimit": 100,
      "Window": "00:01:00",
      "QueueLimit": 10
    },
    "Timeout": {
      "Default": "00:00:30",
      "Critical": "00:00:10",
      "Background": "00:05:00"
    },
    "Bulkhead": {
      "MaxParallelization": 10,
      "MaxQueuedActions": 20
    }
  },
  "HealthChecks": {
    "ReadinessChecks": ["database", "cache"],
    "LivenessChecks": ["self"],
    "StartupChecks": ["database", "external-api"]
  }
}
```

## Best Practices

### Retry Policies
- **Use exponential backoff** - Prevents overwhelming recovering services
- **Add jitter** - Distributes retry attempts to avoid thundering herd
- **Limit retry attempts** - Prevent infinite retry loops
- **Detect transient failures** - Only retry recoverable errors
- **Log retry attempts** - Track retry patterns for monitoring

### Circuit Breakers
- **Set appropriate thresholds** - Balance between fault tolerance and latency
- **Monitor state transitions** - Alert on circuit breaks
- **Per-endpoint isolation** - Don't let one failing service break others
- **Test half-open state** - Ensure proper recovery detection
- **Document break reasons** - Help with troubleshooting

### Rate Limiting
- **Use appropriate algorithms** - Token bucket for burst, sliding window for smooth
- **Set realistic limits** - Based on capacity planning
- **Provide clear error messages** - Include retry-after headers
- **Monitor rejection rates** - Adjust limits based on metrics
- **Implement backpressure** - Queue requests when possible

### Health Checks
- **Separate readiness from liveness** - Different purposes
- **Keep checks fast** - < 1 second response time
- **Test dependencies** - Include external services
- **Return detailed status** - Help with diagnostics
- **Cache check results** - Avoid overwhelming dependencies

### General Resilience
- **Defense in depth** - Combine multiple patterns
- **Fail fast** - Don't hide cascading failures
- **Isolate failures** - Use bulkheads and circuit breakers
- **Monitor everything** - Track all resilience metrics
- **Test failure scenarios** - Use chaos engineering

## Monitoring & Metrics

### Key Metrics
- Retry attempt count and success rate
- Circuit breaker state and transition frequency
- Rate limit rejection count
- Bulkhead queue length and rejection count
- Health check success rate and latency

### Dashboards
```csharp
services.AddMetrics(options =>
{
    options.TrackRetries = true;
    options.TrackCircuitBreakerState = true;
    options.TrackRateLimitRejections = true;
    options.TrackHealthCheckDuration = true;
});
```

## Testing

### Chaos Engineering
```csharp
// In test/staging environments
services.AddChaosEngineering(options =>
{
    options.EnableLatencyInjection = true;
    options.EnableExceptionInjection = true;
    options.EnableResultInjection = true;
    options.InjectionRate = 0.1; // 10% of requests
});
```

## Dependencies

This package depends on:
- `VisionaryCoder.Framework` - Base types
- Polly 8.6.4+ - Resilience policies
- Microsoft.Extensions.Http.Resilience
- AspNetCore.HealthChecks.*

## Version Compatibility

| Framework.Resilience | .NET Version | Polly |
|---------------------|--------------|-------|
| 1.0.0               | .NET 10 LTS  | 8.6.4 |

## License

MIT License - see LICENSE file for details

## Support

- GitHub Issues: https://github.com/visionarycoder/Framework/issues
- Documentation: https://github.com/visionarycoder/Framework/wiki
