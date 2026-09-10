using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;

public class JwtBearerEnricher : IProxySecurityEnricher
{
    private readonly Func<Task<string?>> tokenProvider;

    public JwtBearerEnricher(ILogger<JwtBearerEnricher> logger, Func<Task<string?>> tokenProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    public async Task EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        string? token = await tokenProvider().ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        BearerHeaders.Set(context, "Authorization", token);
    }
}
