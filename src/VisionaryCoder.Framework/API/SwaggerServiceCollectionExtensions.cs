// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace VisionaryCoder.Framework.API;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Adds Swagger services with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiTitle">The API title for Swagger documentation.</param>
    /// <param name="apiVersion">The API version. Default: "v1".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        string apiTitle,
        string apiVersion = "v1")
    {
        ArgumentNullException.ThrowIfNull(apiTitle);
        ArgumentNullException.ThrowIfNull(apiVersion);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(apiVersion, new()
            {
                Title = apiTitle,
                Version = apiVersion,
                Description = $"{apiTitle} - API Documentation"
            });

            // Add XML comments if available
            try
            {
                var xmlFile = $"{System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (System.IO.File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }
            catch
            {
                // Ignore XML comments errors
            }
        });

        return services;
    }

    /// <summary>
    /// Adds Swagger services with advanced configuration including security.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiTitle">The API title for Swagger documentation.</param>
    /// <param name="apiVersion">The API version. Default: "v1".</param>
    /// <param name="enableAuth">Whether to enable Bearer token authentication in Swagger UI. Default: true.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSwaggerDocumentationWithAuth(
        this IServiceCollection services,
        string apiTitle,
        string apiVersion = "v1",
        bool enableAuth = true)
    {
        ArgumentNullException.ThrowIfNull(apiTitle);
        ArgumentNullException.ThrowIfNull(apiVersion);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(apiVersion, new()
            {
                Title = apiTitle,
                Version = apiVersion,
                Description = $"{apiTitle} - API Documentation"
            });

            if (enableAuth)
            {
                // Add JWT Bearer authentication
                options.AddSecurityDefinition("Bearer", new()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new()
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new()
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }

            // Add XML comments if available
            try
            {
                var xmlFile = $"{System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (System.IO.File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }
            catch
            {
                // Ignore XML comments errors
            }

            // Use human-readable enum values
            options.UseInlineDefinitionsForEnums();
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI middleware with default settings.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="apiVersion">The API version to display. Default: "v1".</param>
    /// <param name="routePrefix">The route prefix for Swagger UI. Default: "swagger" (accessible at /swagger).</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSwaggerDocumentation(
        this IApplicationBuilder app,
        string apiVersion = "v1",
        string routePrefix = "swagger")
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"API {apiVersion}");
            options.RoutePrefix = routePrefix;
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
        });

        return app;
    }
}
