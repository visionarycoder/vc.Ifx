using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;

public sealed class JwtBearerInterceptor : IProxyInterceptor
{
    private readonly Func<CancellationToken, Task<string?>> tokenProvider;

    public JwtBearerInterceptor(ILogger<JwtBearerInterceptor> logger, Func<CancellationToken, Task<string?>> tokenProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        cancellationToken.ThrowIfCancellationRequested();
        string? token = await tokenProvider(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        BearerHeaders.Set(context, "Authorization", token);
        return await next(context, cancellationToken).ConfigureAwait(false);
    }
}
