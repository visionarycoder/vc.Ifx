// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace VisionaryCoder.Framework.Configuration;

/// <summary>
/// Extension methods for strongly-typed configuration.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Registers a configuration section as a strongly-typed options object with validation.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="sectionName">The configuration section name. If null, uses typeof(TOptions).Name.</param>
    /// <param name="validateOnStart">Whether to validate configuration on application start. Default: true.</param>
    /// <returns>The options builder for further configuration.</returns>
    public static OptionsBuilder<TOptions> AddOptionsWithValidation<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null,
        bool validateOnStart = true)
        where TOptions : class
    {
        sectionName ??= typeof(TOptions).Name;

        var builder = services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations();

        if (validateOnStart)
        {
            builder.ValidateOnStart();
        }

        return builder;
    }

    /// <summary>
    /// Registers a configuration section as a strongly-typed options object with custom validation.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="sectionName">The configuration section name. If null, uses typeof(TOptions).Name.</param>
    /// <param name="validator">Custom validation function.</param>
    /// <param name="validateOnStart">Whether to validate configuration on application start. Default: true.</param>
    /// <returns>The options builder for further configuration.</returns>
    public static OptionsBuilder<TOptions> AddOptionsWithValidation<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName,
        Func<TOptions, bool> validator,
        bool validateOnStart = true)
        where TOptions : class
    {
        sectionName ??= typeof(TOptions).Name;

        var builder = services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(validator, $"{typeof(TOptions).Name} validation failed");

        if (validateOnStart)
        {
            builder.ValidateOnStart();
        }

        return builder;
    }

    /// <summary>
    /// Gets a strongly-typed configuration section.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="sectionName">The configuration section name. If null, uses typeof(TOptions).Name.</param>
    /// <returns>The bound configuration object.</returns>
    public static TOptions GetOptions<TOptions>(
        this IConfiguration configuration,
        string? sectionName = null)
        where TOptions : class, new()
    {
        sectionName ??= typeof(TOptions).Name;

        var options = new TOptions();
        configuration.GetSection(sectionName).Bind(options);
        return options;
    }
}
