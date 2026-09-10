---
title: Telemetry Interceptor
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Export OpenTelemetry metrics and traces
tags:
  - telemetry
  - observability
  - opentelemetry
  - otel
  - metrics
  - traces
audience: developer
source_paths:
  - Telemetry/TelemetryInterceptor.cs
---

# Telemetry Interceptor

Exports OpenTelemetry metrics and traces for comprehensive observability.

## Purpose

Provide standardized observability by emitting:
- **Traces** — Detailed execution flow across services
- **Metrics** — Counters, histograms, gauges for performance monitoring
- **Logs** — Structured logging integrated with other signals

## How It Works

```
Method invoked
    ↓
TelemetryInterceptor creates trace span
    ↓
Records execution metrics (duration, success/failure)
    ↓
Emits structured logs with correlation context
    ↓
Exports to OpenTelemetry Collector/backend
```

## Files

| File | Purpose |
|---|---|
| `TelemetryInterceptor.cs` | Exports OpenTelemetry signals |

## Integration

### 1. Configure OpenTelemetry

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4318");
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4318");
            });
    });
```

### 2. Register the Interceptor

```csharp
services.AddScoped<IInvocationInterceptor, TelemetryInterceptor>();
```

### 3. Traces and Metrics Automatically Collected

```csharp
public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(PaymentRequest request);
}

var result = await paymentService.ProcessAsync(request);
// TelemetryInterceptor automatically emits:
// - Trace span with method name, status, duration
// - Metrics: success count, latency histogram
// - Structured logs with correlation ID
```

## OpenTelemetry Signals

### Traces
- Method name and arguments (redacted if needed)
- Execution status (success/failure)
- Duration in milliseconds
- Correlation ID for distributed tracing

### Metrics
- Method call counter (by status)
- Call duration histogram (percentiles)
- Request size gauge
- Response size gauge

### Logs
- Structured log with all context
- Correlation ID for tracing
- Authentication/authorization data (if applicable)
- Exception details (if failed)

## Execution Order

Register `TelemetryInterceptor` **late in the chain** (to capture all upstream interceptor work):

```csharp
services.AddScoped<IInvocationInterceptor, CorrelationInterceptor>();     // 1st
services.AddScoped<IInvocationInterceptor, JwtAuthenticationInterceptor>();   // 2nd
services.AddScoped<IInvocationInterceptor, ValidationInterceptor>();       // 3rd
// ... other interceptors ...
services.AddScoped<IInvocationInterceptor, TelemetryInterceptor>();        // Late
```

## Correlation with Other Signals

Works seamlessly with other interceptors:

| Interceptor | Contributes To Telemetry |
|---|---|
| CorrelationInterceptor | Trace correlation ID |
| StatusInterceptor | Execution status and duration |
| AuditInterceptor | User identity and operation type |
| ValidationInterceptor | Input validation context |
| JwtAuthenticationInterceptor | Principal information |

## Backend Configuration

For local development, use Jaeger or Grafana Loki:

```yaml
# docker-compose.yml
services:
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "6831:6831/udp"
      - "16686:16686"  # Jaeger UI

  otel-collector:
    image: otel/opentelemetry-collector:latest
    ports:
      - "4317:4317"  # gRPC
      - "4318:4318"  # HTTP
    config: /etc/otel-collector-config.yaml
```

## Related

- [Correlation Interceptor](../Correlation/README.md) — Correlation ID for traces
- [Status Interceptor](../Status/README.md) — Status and duration metrics
- [Audit Interceptor](../Auditing/README.md) — User actions in traces
- [Core Infrastructure](../Core/README.md) — MethodContext, InvocationItemNames

## See Also

- [README.md](../README.md#observability-4-interceptors) — Observability category
- [Execution Order](../README.md#execution-order) — Full recommended pipeline
- [TOC.md](../TOC.md#otlp) — Complete table of contents
- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)

// When you call an intercepted method, it automatically creates traces
var result = await paymentService.ProcessAsync(request);
// OTEL now has:
// - Trace span for the method call
// - Duration metric
// - Status (success/failure)
```

## Typical Setup

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(opt => opt.Endpoint = new Uri("http://localhost:4317")))
    .WithMetrics(metrics =>
        metrics
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(opt => opt.Endpoint = new Uri("http://localhost:4317")));

// The interceptor emits to OpenTelemetry automatically
services.AddScoped<IInvocationInterceptor, TelemetryInterceptor>();
```

## Performance

- Minimal overhead when no exporter is configured
- Async exporting to collectors (doesn't block your code)
- Built-in sampling support for high-volume scenarios

## Further Reading

- [OpenTelemetry.io](https://opentelemetry.io/)
- [Configuring OpenTelemetry in .NET](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
- [Application Insights Integration](https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable)
- [Auditing](../Auditing/README.md) — For compliance-focused observation


