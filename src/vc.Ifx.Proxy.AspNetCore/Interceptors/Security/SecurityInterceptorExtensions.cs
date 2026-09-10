using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;
using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;

public static class SecurityInterceptorExtensions
{
    public static IServiceCollection AddJwtBearerInterceptor(this IServiceCollection services,
        Func<CancellationToken, Task<string?>> tokenProvider)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);
        return Register(services, provider => new JwtBearerInterceptor(
            provider.GetRequiredService<ILogger<JwtBearerInterceptor>>(), tokenProvider));
    }

    public static IServiceCollection AddJwtBearerInterceptorFromSecret(this IServiceCollection services, string secretName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);
        return Register(services, provider =>
        {
            ISecretProvider secrets = provider.GetRequiredService<ISecretProvider>();
            return new JwtBearerInterceptor(provider.GetRequiredService<ILogger<JwtBearerInterceptor>>(),
                token => secrets.GetAsync(secretName, token));
        });
    }

    public static IServiceCollection AddJwtBearerInterceptorWithStaticToken(this IServiceCollection services, string staticToken)
    {
        BearerHeaders.Validate(staticToken);
        return services.AddJwtBearerInterceptor(token => Task.FromResult<string?>(staticToken));
    }

    private static IServiceCollection Register(IServiceCollection services, Func<IServiceProvider, JwtBearerInterceptor> factory)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddLogging();
        if (services.Any(descriptor => descriptor.ServiceType == typeof(JwtBearerInterceptor)))
            return services;
        services.TryAddScoped(factory);
        services.AddScoped<IProxyInterceptor>(provider => provider.GetRequiredService<JwtBearerInterceptor>());
        return services;
    }
}
