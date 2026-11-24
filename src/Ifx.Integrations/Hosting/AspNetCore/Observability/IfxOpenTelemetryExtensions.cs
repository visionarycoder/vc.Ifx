using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Ifx.Hosting.AspNetCore.Observability;

/// <summary>Registers Ifx tracing, metrics, standard ASP.NET instrumentation, and OTLP exporters.</summary>
public static class IfxOpenTelemetryExtensions
{
    /// <summary>Configures tracing and metrics for the Ifx sources and exports them through OTLP.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The resource service name.</param>
    /// <param name="configureResource">Optional resource customization.</param>
    /// <param name="configureTracing">Optional additional tracing customization.</param>
    /// <param name="configureMetrics">Optional additional metrics customization.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIfxOpenTelemetry(
        this IServiceCollection services,
        string serviceName,
        Action<ResourceBuilder>? configureResource = null,
        Action<TracerProviderBuilder>? configureTracing = null,
        Action<MeterProviderBuilder>? configureMetrics = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(serviceName);
                configureResource?.Invoke(resource);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource("Ifx.Proxy")
                    .AddSource("PipelineInvoker")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                configureTracing?.Invoke(tracing);
                tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter("PipelineInvoker.Metrics")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                configureMetrics?.Invoke(metrics);
                metrics.AddOtlpExporter();
            });

        return services;
    }
}
