using Microsoft.Extensions.DependencyInjection;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies;

/// <summary>
/// Extension methods for registering boundary-intercepted services in the dependency injection container.
/// </summary>
public static class BoundaryProxyServiceCollectionExtensions
{
    /// <summary>
    /// Registers a scoped service with automatic boundary interception applied.
    /// The implementation is registered directly, and a proxy is registered for the contract interface.
    /// All registered interceptors are automatically injected into the proxy.
    /// </summary>
    /// <typeparam name="TContract">The contract interface type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type that must implement TContract.</typeparam>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddBoundaryInterceptedScoped<TContract, TImplementation>(this IServiceCollection services)
        where TContract : class
        where TImplementation : class, TContract
    {
        // Register the implementation directly so it can be resolved as a dependency
        services.AddScoped<TImplementation>();

        // Register a proxy for the contract interface that wraps the implementation
        services.AddScoped<TContract>(serviceProvider =>
        {
            var target = serviceProvider.GetRequiredService<TImplementation>();
            var interceptors = serviceProvider.GetServices<IProxyInterceptor>();
            return BoundaryProxy<TContract>.Create(target, interceptors);
        });
        return services;
    }

}