using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;

public class WebJwtInterceptor : IProxyInterceptor
{
    private readonly ITokenProvider tokenProvider;
    private readonly string audience;
    private readonly string[] scopes;
    private readonly bool refreshIfExpired;
    private readonly string headerName;

    public WebJwtInterceptor(ITokenProvider tokenProvider, ILogger<WebJwtInterceptor> logger, WebJwtOptions options)
    {
        this.tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Audience);
        ArgumentNullException.ThrowIfNull(options.Scopes);
        // Credentials may target Authorization or an explicitly named private header, never framing/routing headers.
        if (options.HeaderName is null || !System.Text.RegularExpressions.Regex.IsMatch(
            options.HeaderName, @"\A(?:Authorization|X-[A-Za-z0-9-]+)\z",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant))
            throw new ArgumentException("Use Authorization or an X- prefixed credential header.", nameof(options));
        audience = options.Audience;
        scopes = options.Scopes.ToArray();
        refreshIfExpired = options.RefreshIfExpired;
        headerName = options.HeaderName;
    }

    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        cancellationToken.ThrowIfCancellationRequested();
        TokenResult result = await tokenProvider.GetTokenAsync(new TokenRequest
        {
            Audience = audience,
            Scopes = scopes.ToArray(),
            RefreshIfExpired = refreshIfExpired
        }, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (result is null || !result.IsSuccess || result.IsExpired ||
            !string.Equals(result.TokenType, "Bearer", StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Token acquisition failed.");
        BearerHeaders.Validate(result.AccessToken);
        if (!await tokenProvider.ValidateTokenAsync(result.AccessToken, cancellationToken).ConfigureAwait(false))
            throw new UnauthorizedAccessException("Token validation failed.");
        cancellationToken.ThrowIfCancellationRequested();
        BearerHeaders.Set(context, headerName, result.AccessToken);
        return await next(context, cancellationToken).ConfigureAwait(false);
    }
}
