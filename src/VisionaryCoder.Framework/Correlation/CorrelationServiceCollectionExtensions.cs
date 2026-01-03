// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VisionaryCoder.Framework.Correlation;

/// <summary>
/// Extension methods for configuring correlation services.
/// </summary>
public static class CorrelationServiceCollectionExtensions
{
    /// <summary>
    /// Adds correlation tracking services.
    /// </summary>
    public static IServiceCollection AddCorrelation(this IServiceCollection services)
    {
        services.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        services.TryAddScoped(sp =>
        {
            var accessor = sp.GetRequiredService<ICorrelationContextAccessor>();
            return accessor.CorrelationContext ?? new CorrelationContext();
        });

        return services;
    }

    /// <summary>
    /// Adds correlation middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseCorrelation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationMiddleware>();
    }
}
