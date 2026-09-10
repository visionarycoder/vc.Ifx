using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Interceptors;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication;

/// <summary>Outbound authentication dependencies; inbound schemes remain host-owned.</summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, Action<JwtOptions> configureOptions)
    {
        Infrastructure(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        var options = new JwtOptions();
        configureOptions(options);
        Validate(options);
        services.TryAddSingleton(options);
        services.TryAddScoped<IUserContextProvider, NullUserContextProvider>();
        services.TryAddScoped<ITenantContextProvider, NullTenantContextProvider>();
        services.TryAddScoped<ITokenProvider, NullTokenProvider>();
        services.TryAddScoped<JwtAuthenticationInterceptor>();
        return services;
    }

    public static IServiceCollection ReplaceUserContextProvider<T>(this IServiceCollection services) where T : class, IUserContextProvider
    {
        Infrastructure(services);
        services.RemoveAll<IUserContextProvider>();
        services.AddScoped<IUserContextProvider, T>();
        return services;
    }

    public static IServiceCollection ReplaceTenantContextProvider<T>(this IServiceCollection services) where T : class, ITenantContextProvider
    {
        Infrastructure(services);
        services.RemoveAll<ITenantContextProvider>();
        services.AddScoped<ITenantContextProvider, T>();
        return services;
    }

    public static IServiceCollection ReplaceTokenProvider<T>(this IServiceCollection services) where T : class, ITokenProvider
    {
        Infrastructure(services);
        services.RemoveAll<ITokenProvider>();
        services.AddScoped<ITokenProvider, T>();
        return services;
    }

    public static IServiceCollection UseDefaultAuthenticationProviders(this IServiceCollection services) =>
        services.ReplaceUserContextProvider<DefaultUserContextProvider>()
            .ReplaceTenantContextProvider<DefaultTenantContextProvider>().ReplaceTokenProvider<DefaultTokenProvider>();

    public static IServiceCollection AddJwtAuthentication<TOptions>(this IServiceCollection services) where TOptions : class, new()
    {
        Infrastructure(services);
        if (new TOptions() is not JwtOptions options)
            throw new ArgumentException("Typed configuration must derive from JwtOptions.", nameof(TOptions));
        Validate(options);
        services.TryAddSingleton(options);
        services.AddUserContext().AddTenantContext().AddTokenProvider<DefaultTokenProvider>();
        services.TryAddScoped<JwtAuthenticationInterceptor>();
        return services;
    }

    public static IServiceCollection AddKeyVaultJwtAuthentication(this IServiceCollection services, Action<KeyVaultJwtOptions> configureOptions)
    {
        Infrastructure(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        var options = new KeyVaultJwtOptions();
        configureOptions(options);
        if (!options.IsValid())
            throw new ArgumentException("Key Vault JWT options are invalid.", nameof(configureOptions));
        services.TryAddSingleton(options);
        services.AddUserContext().AddTenantContext();
        services.TryAddScoped<KeyVaultJwtInterceptor>();
        return services;
    }

    public static IServiceCollection AddUserContext(this IServiceCollection services) => services.AddUserContext<DefaultUserContextProvider>();

    public static IServiceCollection AddUserContext<TProvider>(this IServiceCollection services) where TProvider : class, IUserContextProvider
    {
        Infrastructure(services);
        RemoveFallback<IUserContextProvider, NullUserContextProvider>(services);
        services.TryAddScoped<IUserContextProvider, TProvider>();
        services.TryAddScoped<UserContext>();
        return services;
    }

    public static IServiceCollection AddTenantContext(this IServiceCollection services) => services.AddTenantContext<DefaultTenantContextProvider>();

    public static IServiceCollection AddTenantContext<TProvider>(this IServiceCollection services) where TProvider : class, ITenantContextProvider
    {
        Infrastructure(services);
        RemoveFallback<ITenantContextProvider, NullTenantContextProvider>(services);
        services.TryAddScoped<ITenantContextProvider, TProvider>();
        services.TryAddScoped<TenantContext>();
        return services;
    }

    public static IServiceCollection AddTokenProvider<TTokenProvider>(this IServiceCollection services) where TTokenProvider : class, ITokenProvider
    {
        Infrastructure(services);
        RemoveFallback<ITokenProvider, NullTokenProvider>(services);
        services.TryAddScoped<ITokenProvider, TTokenProvider>();
        return services;
    }

    public static IServiceCollection AddAuthenticationInterceptors(this IServiceCollection services)
    {
        Infrastructure(services);
        if (services.Any(descriptor => descriptor.ServiceType == typeof(JwtOptions)))
            services.TryAddScoped<JwtAuthenticationInterceptor>();
        if (services.Any(descriptor => descriptor.ServiceType == typeof(KeyVaultJwtOptions)))
            services.TryAddScoped<KeyVaultJwtInterceptor>();
        return services;
    }

    public static IServiceCollection AddCompleteAuthentication(this IServiceCollection services, Action<JwtOptions> configureJwtOptions) =>
        services.AddJwtAuthentication(configureJwtOptions).AddUserContext().AddTenantContext()
            .AddTokenProvider<DefaultTokenProvider>().AddAuthenticationInterceptors();

    public static IServiceCollection AddAuthenticationWithValidation(this IServiceCollection services,
        Action<JwtOptions> configureOptions, bool validateSetup = true)
    {
        services.AddCompleteAuthentication(configureOptions);
        if (validateSetup && services.Any(descriptor =>
            (descriptor.ServiceType == typeof(IUserContextProvider) || descriptor.ServiceType == typeof(ITenantContextProvider)) &&
            descriptor.Lifetime == ServiceLifetime.Singleton))
            throw new InvalidOperationException("Request identity providers must not be singleton services.");
        return services;
    }

    private static void Validate(JwtOptions options)
    {
        if (!options.IsValid())
            throw new ArgumentException("JWT options are invalid.", nameof(options));
    }

    private static void RemoveFallback<TService, TFallback>(IServiceCollection services)
    {
        foreach (ServiceDescriptor descriptor in services.Where(descriptor =>
            descriptor.ServiceType == typeof(TService) && descriptor.ImplementationType == typeof(TFallback)).ToArray())
            services.Remove(descriptor);
    }

    private static void Infrastructure(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddLogging();
    }
}
