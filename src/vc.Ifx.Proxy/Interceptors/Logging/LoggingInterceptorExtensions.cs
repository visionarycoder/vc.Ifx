using Microsoft.Extensions.DependencyInjection;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Logging;
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
        ArgumentNullException.ThrowIfNull(services);
        services.AddLogging();
        services.AddSingleton<IProxyInterceptor, LoggingInterceptor>();
        return services;
    }
}
