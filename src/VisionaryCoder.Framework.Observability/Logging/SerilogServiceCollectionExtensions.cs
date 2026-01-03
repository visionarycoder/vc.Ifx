// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace VisionaryCoder.Framework.Logging;

/// <summary>
/// Extension methods for configuring Serilog structured logging.
/// </summary>
public static class SerilogServiceCollectionExtensions
{
    /// <summary>
    /// Adds Serilog with default configuration for console and file logging.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="configuration">Configuration instance for reading Serilog settings.</param>
    /// <param name="applicationName">Application name for log enrichment.</param>
    /// <returns>The application builder for chaining.</returns>
    public static WebApplicationBuilder AddSerilogLogging(
        this WebApplicationBuilder builder,
        IConfiguration? configuration = null,
        string? applicationName = null)
    {
        configuration ??= builder.Configuration;
        applicationName ??= builder.Environment.ApplicationName;

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                new CompactJsonFormatter(),
                "logs/log-.json",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }

    /// <summary>
    /// Adds Serilog with Azure Application Insights integration.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="instrumentationKey">Application Insights instrumentation key.</param>
    /// <param name="configuration">Configuration instance for reading Serilog settings.</param>
    /// <param name="applicationName">Application name for log enrichment.</param>
    /// <returns>The application builder for chaining.</returns>
    public static WebApplicationBuilder AddSerilogWithApplicationInsights(
        this WebApplicationBuilder builder,
        string instrumentationKey,
        IConfiguration? configuration = null,
        string? applicationName = null)
    {
        ArgumentNullException.ThrowIfNull(instrumentationKey);

        configuration ??= builder.Configuration;
        applicationName ??= builder.Environment.ApplicationName;

        var telemetryConfiguration = new TelemetryConfiguration
        {
            InstrumentationKey = instrumentationKey
        };

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                new CompactJsonFormatter(),
                "logs/log-.json",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .WriteTo.ApplicationInsights(
                telemetryConfiguration,
                TelemetryConverter.Traces,
                LogEventLevel.Information)
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }

    /// <summary>
    /// Adds Serilog with advanced configuration including async file writing and multiple sinks.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="configure">Configuration action for Serilog logger.</param>
    /// <returns>The application builder for chaining.</returns>
    public static WebApplicationBuilder AddSerilogAdvanced(
        this WebApplicationBuilder builder,
        Action<LoggerConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);

        configure(loggerConfiguration);

        Log.Logger = loggerConfiguration.CreateLogger();
        builder.Host.UseSerilog();

        return builder;
    }

    /// <summary>
    /// Adds Serilog request logging middleware with performance tracking.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="includeHealthChecks">Whether to log health check requests. Default: false.</param>
    /// <returns>The application builder for chaining.</returns>
    public static WebApplication UseSerilogRequestLogging(
        this WebApplication app,
        bool includeHealthChecks = false)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
            };

            if (!includeHealthChecks)
            {
                options.GetLevel = (httpContext, elapsed, ex) =>
                {
                    if (httpContext.Request.Path.StartsWithSegments("/health") ||
                        httpContext.Request.Path.StartsWithSegments("/healthz"))
                    {
                        return LogEventLevel.Verbose;
                    }

                    if (ex != null || httpContext.Response.StatusCode >= 500)
                    {
                        return LogEventLevel.Error;
                    }

                    if (httpContext.Response.StatusCode >= 400)
                    {
                        return LogEventLevel.Warning;
                    }

                    return LogEventLevel.Information;
                };
            }
        });

        return app;
    }
}
