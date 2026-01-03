# VisionaryCoder.Framework.Observability

Comprehensive observability solution for VisionaryCoder Framework with structured logging, distributed tracing, and metrics collection.

## Features

### Structured Logging with Serilog
- JSON-formatted logs for machine parsing
- Multiple sinks: Console, File, Application Insights
- Async logging for performance
- Context enrichment (environment, process, thread, correlation IDs)

### Distributed Tracing with OpenTelemetry
- W3C Trace Context propagation
- Automatic instrumentation for ASP.NET Core, HTTP clients, EF Core
- Export to OTLP, Jaeger, Zipkin
- Correlation across service boundaries

### Metrics Collection
- Custom business metrics
- Performance counters
- HTTP request metrics
- Database query metrics
- Cache hit/miss ratios

### Application Insights Integration
- Seamless Azure integration
- Real-time monitoring dashboards
- Anomaly detection
- Smart diagnostics

## Installation

```bash
dotnet add package VisionaryCoder.Framework.Observability
```

## Quick Start

### Configure Serilog

```csharp
// Program.cs
using VisionaryCoder.Framework.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext()
          .Enrich.WithMachineName()
          .Enrich.WithEnvironmentName()
          .Enrich.WithThreadId()
          .WriteTo.Console(new CompactJsonFormatter())
          .WriteTo.File(new CompactJsonFormatter(), "logs/app-.log", rollingInterval: RollingInterval.Day)
          .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);
});
```

### Configure OpenTelemetry

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddEntityFrameworkCoreInstrumentation()
               .AddSource("MyApp")
               .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddRuntimeInstrumentation()
               .AddOtlpExporter();
    });
```

### Structured Logging

```csharp
public class OrderService
{
    private readonly ILogger<OrderService> logger;
    
    public OrderService(ILogger<OrderService> logger)
    {
        this.logger = logger;
    }
    
    public async Task ProcessOrderAsync(Order order)
    {
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["OrderId"] = order.Id,
            ["CustomerId"] = order.CustomerId
        }))
        {
            logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", 
                                 order.Id, order.CustomerId);
            
            try
            {
                await ProcessPaymentAsync(order);
                logger.LogInformation("Order {OrderId} processed successfully", order.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
                throw;
            }
        }
    }
}
```

### Distributed Tracing

```csharp
public class OrderService
{
    private readonly ActivitySource activitySource;
    
    public OrderService()
    {
        activitySource = new ActivitySource("MyApp.OrderService");
    }
    
    public async Task ProcessOrderAsync(Order order)
    {
        using var activity = activitySource.StartActivity("ProcessOrder");
        activity?.SetTag("order.id", order.Id);
        activity?.SetTag("customer.id", order.CustomerId);
        
        // Work happens here - automatically traced
        await ProcessPaymentAsync(order);
        
        activity?.SetTag("order.total", order.Total);
    }
}
```

### Custom Metrics

```csharp
public class OrderMetrics
{
    private readonly Meter meter;
    private readonly Counter<long> ordersProcessed;
    private readonly Histogram<double> orderValue;
    
    public OrderMetrics(IMeterFactory meterFactory)
    {
        meter = meterFactory.Create("MyApp.Orders");
        ordersProcessed = meter.CreateCounter<long>("orders.processed");
        orderValue = meter.CreateHistogram<double>("orders.value");
    }
    
    public void RecordOrderProcessed(Order order)
    {
        ordersProcessed.Add(1, new KeyValuePair<string, object?>("status", order.Status));
        orderValue.Record(order.Total);
    }
}
```

## Configuration

### appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "connectionString": "InstrumentationKey=..."
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  },
  "OpenTelemetry": {
    "ServiceName": "MyApp",
    "ServiceVersion": "1.0.0",
    "Otlp": {
      "Endpoint": "http://localhost:4317"
    }
  }
}
```

## Logging Interceptors

```csharp
// Automatic logging for proxy calls
services.AddProxyLogging(options =>
{
    options.LogRequests = true;
    options.LogResponses = true;
    options.LogExceptions = true;
    options.IncludeRequestBody = false; // Privacy
    options.IncludeResponseBody = false; // Performance
});
```

## Telemetry Interceptors

```csharp
// Automatic tracing for proxy calls
services.AddProxyTelemetry(options =>
{
    options.RecordExceptions = true;
    options.RecordHttpAttributes = true;
    options.MaxAttributeLength = 1000;
});
```

## Best Practices

### Logging
- Use structured logging with properties, not string interpolation
- Log at appropriate levels (Trace, Debug, Info, Warning, Error, Critical)
- Include correlation IDs in all log entries
- Never log sensitive data (passwords, tokens, PII)
- Use log scopes to group related operations

### Tracing
- Create spans for significant operations
- Add meaningful tags to spans
- Keep span names consistent and low-cardinality
- Don't create too many spans (performance impact)
- Use baggage for cross-service context

### Metrics
- Use consistent naming conventions
- Keep metric cardinality low (avoid user IDs as tags)
- Choose appropriate metric types (counter, gauge, histogram)
- Aggregate metrics before export
- Monitor metric collection overhead

### Performance
- Use async sinks for logging
- Enable sampling for high-volume traces
- Batch metric exports
- Monitor observability pipeline health
- Set appropriate retention policies

## Correlation IDs

```csharp
// Automatic correlation ID propagation
app.UseCorrelation();

// Access correlation ID
public class MyService
{
    private readonly ICorrelationContext correlationContext;
    
    public MyService(ICorrelationContext correlationContext)
    {
        this.correlationContext = correlationContext;
    }
    
    public void DoWork()
    {
        var correlationId = correlationContext.CorrelationId;
        // Use in external calls, logs, etc.
    }
}
```

## Monitoring Dashboards

### Key Metrics to Monitor
- Request rate and latency (p50, p95, p99)
- Error rate and types
- Database query performance
- Cache hit ratios
- Background job success rates
- Resource utilization (CPU, memory, connections)

### Alerting Rules
- Error rate > 1%
- p99 latency > 1s
- Database connection pool > 80%
- Memory usage > 85%
- Failed background jobs

## Dependencies

This package depends on:
- `VisionaryCoder.Framework` - Base types
- Serilog 4.2.0+
- OpenTelemetry 1.14.0+

## Version Compatibility

| Framework.Observability | .NET Version | Serilog | OpenTelemetry |
|------------------------|--------------|---------|---------------|
| 1.0.0                  | .NET 10 LTS  | 4.2.0   | 1.14.0        |

## License

MIT License - see LICENSE file for details

## Support

- GitHub Issues: https://github.com/visionarycoder/Framework/issues
- Documentation: https://github.com/visionarycoder/Framework/wiki
