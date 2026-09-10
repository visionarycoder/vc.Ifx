using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Interceptors;

/// <summary>Acquires outbound bearer credentials without silently swallowing required-token failures.</summary>
public class JwtAuthenticationInterceptor : IProxyInterceptor
{
    private readonly ITokenProvider tokenProvider;
    private readonly ILogger<JwtAuthenticationInterceptor> logger;
    private readonly JwtOptions options;

    /// <summary>Creates an interceptor from explicitly supplied token-provider settings.</summary>
    public JwtAuthenticationInterceptor(ITokenProvider tokenProvider, ILogger<JwtAuthenticationInterceptor> logger, JwtOptions options)
    {
        this.tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        if (!options.IsValid())
            throw new ArgumentException("JWT options are invalid.", nameof(options));
    }

    /// <inheritdoc />
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        cancellationToken.ThrowIfCancellationRequested();
        TokenRequest request = TokenRequest.CreateClientCredentials(options.ClientId, options.ClientSecret, options.Scopes, options.Audience);
        if (!request.IsValid())
            throw new InvalidOperationException("Token request configuration is invalid.");
        TokenResult result;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(options.RequestTimeout);
            result = await tokenProvider.GetTokenAsync(request, timeout.Token).ConfigureAwait(false);
            if (result.IsSuccess && ShouldRefreshToken(result))
                result = await tokenProvider.RefreshTokenAsync(result.RefreshToken!, timeout.Token).ConfigureAwait(false) ?? result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is TimeoutException or OperationCanceledException)
        {
            logger.LogWarning(exception, "JWT acquisition timed out.");
            if (ShouldFail("FailOnTimeout"))
                throw new TimeoutException("JWT acquisition timed out.", exception);
            return await next(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "JWT acquisition failed.");
            if (ShouldFail("FailOnError"))
                throw;
            return await next(context, cancellationToken).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.AccessToken) || result.IsExpired)
        {
            if (ShouldFail("FailOnTokenError"))
                throw new InvalidOperationException("A required JWT token is unavailable or expired.");
        }
        else
        {
            ProxyHeaders.Set(context, options.HeaderName, $"{result.TokenType} {result.AccessToken}");
            if (!string.IsNullOrEmpty(result.CorrelationId))
                ProxyHeaders.Set(context, "X-Correlation-ID", result.CorrelationId);
            if (!string.IsNullOrEmpty(result.Scope))
                ProxyHeaders.Set(context, "X-Token-Scope", result.Scope);
        }
        return await next(context, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Uses the host's token provider to validate a token.</summary>
    protected virtual bool IsTokenValid(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return false;
        try { return tokenProvider.ValidateToken(token); }
        catch (Exception exception)
        {
            logger.LogDebug(exception, "JWT pre-validation failed.");
            return false;
        }
    }

    /// <summary>Requests refresh only when enabled, supported by the token, and close to expiry.</summary>
    protected virtual bool ShouldRefreshToken(TokenResult tokenResult)
        => options.RefreshIfExpired && !string.IsNullOrEmpty(tokenResult.RefreshToken)
            && tokenResult.IsCloseToExpiry(TimeSpan.FromMinutes(2));

    private bool ShouldFail(string name) => !options.CustomProperties.TryGetValue(name, out object? value) || value is not false;
}
