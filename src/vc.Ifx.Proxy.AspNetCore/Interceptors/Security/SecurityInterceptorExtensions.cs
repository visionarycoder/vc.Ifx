using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;
using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;
/// <summary>
/// Extension methods for adding security interceptor services.
/// </summary>
public static class SecurityInterceptorExtensions
{
    /// <summary>
    /// Adds the JWT Bearer interceptor to the service collection with a token provider function.
    /// </summary>
    /// <param name="services">The service collection to add the interceptor to.</param>
    /// <param name="tokenProvider">Function that provides JWT tokens.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJwtBearerInterceptor(
        this IServiceCollection services,
        Func<CancellationToken, Task<string?>> tokenProvider)
    {
        services.AddSingleton<IProxyInterceptor>(provider =>
        {
            ILogger<JwtBearerInterceptor> logger = provider.GetRequiredService<ILogger<JwtBearerInterceptor>>();
            return new JwtBearerInterceptor(logger, tokenProvider);
        });
        return services;
    }
    /// <summary>
    /// Adds the JWT Bearer interceptor that retrieves tokens from a secret provider.
    /// </summary>
    /// <param name="services">The service collection to add the interceptor to.</param>
    /// <param name="secretName">The name of the secret containing the JWT token.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJwtBearerInterceptorFromSecret(
        this IServiceCollection services,
        string secretName)
    {
        services.AddSingleton<IProxyInterceptor>(provider =>
        {
            ISecretProvider secretProvider = provider.GetRequiredService<ISecretProvider>();
            ILogger<JwtBearerInterceptor> logger = provider.GetRequiredService<ILogger<JwtBearerInterceptor>>();
            Func<CancellationToken, Task<string?>> tokenProvider = async (cancellationToken) =>
            {
                return await secretProvider.GetAsync(secretName, cancellationToken);
            };
            return new JwtBearerInterceptor(logger, tokenProvider);
        });
        return services;
    }

    /// <summary>
    /// Adds the JWT Bearer interceptor with a static token (useful for development).
    /// </summary>
    /// <param name="services">The service collection to add the interceptor to.</param>
    /// <param name="staticToken">The static JWT token to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJwtBearerInterceptorWithStaticToken(
        this IServiceCollection services,
        string staticToken)
    {
        return services.AddJwtBearerInterceptor((cancellationToken) => Task.FromResult<string?>(staticToken));
    }
}
