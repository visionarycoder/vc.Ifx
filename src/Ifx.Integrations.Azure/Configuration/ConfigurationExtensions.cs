using Azure.Identity;
using Ifx;
using Ifx.Component;
using Ifx.Pipeline;
using Ifx.Proxy.Exceptions;
using Ifx.Querying;
using Ifx.Querying.Serialization;
using Ifx.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Threading.RateLimiting;

using Ifx.Proxy.Interceptor.Configuration.Azure;

namespace Ifx.Proxy.Interceptor.Configuration;

/// <summary>Registers legacy configuration options or explicitly adds an Azure configuration source.</summary>
public static class ConfigurationExtensions
{
    public static string ConfigurationKey { get; set; } = "AzureAppConfiguration";

    public static IServiceCollection AddAzureAppConfiguration(this IServiceCollection services, IConfiguration configuration, Action<ConfigurationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ConfigurationOptions options = configuration.GetSection(ConfigurationKey).Get<ConfigurationOptions>() ?? new ConfigurationOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        return services;
    }

    public static IConfigurationBuilder AddAzureAppConfiguration(this IConfigurationBuilder builder, ConfigurationOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        if (options.Endpoint is null && !options.UseConnectionString)
            return builder;
        var providerOptions = new AzureConfigurationProviderOptions
        {
            Endpoint = options.Endpoint,
            UseConnectionString = options.UseConnectionString,
            ConnectionString = options.ConnectionString,
            Label = options.Label,
            SentinelKey = options.SentinelKey,
            CacheExpiration = options.CacheExpiration
        };
        AzureConfigurationProviderOptionsExtensions.Validate(providerOptions);
        return builder.AddAzureAppConfiguration(sdk => providerOptions.ApplyTo(sdk));
    }
}
