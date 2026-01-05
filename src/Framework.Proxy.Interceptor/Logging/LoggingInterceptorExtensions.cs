// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Proxy.Abstractions;

namespace VisionaryCoder.Framework.Proxy.Interceptor.Logging;
/// <summary>
/// Extension methods for adding logging interceptor services.
/// </summary>
public static class LoggingInterceptorExtensions
{
    /// <summary>
    /// Adds the logging interceptor to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the interceptor to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoggingInterceptor(this IServiceCollection services)
    {
        services.AddSingleton<IProxyInterceptor, LoggingInterceptor>();
        return services;
    }
}
