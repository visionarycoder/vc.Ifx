using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Interceptors;

/// <summary>Attaches outbound tokens from a trusted secret provider with explicit failure policy.</summary>
public class KeyVaultJwtInterceptor : IProxyInterceptor
{
    private readonly ISecretProvider secretProvider;
    private readonly ILogger<KeyVaultJwtInterceptor> logger;
    private readonly KeyVaultJwtOptions options;

    /// <summary>Creates an interceptor; this does not retrieve secrets.</summary>
    public KeyVaultJwtInterceptor(ISecretProvider secretProvider, ILogger<KeyVaultJwtInterceptor> logger, KeyVaultJwtOptions options)
    {
        this.secretProvider = secretProvider ?? throw new ArgumentNullException(nameof(secretProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        if (!options.IsValid())
            throw new ArgumentException("Key Vault JWT options are invalid.", nameof(options));
    }

    /// <inheritdoc />
    public async Task<ProxyResponse<T>> InvokeAsync<T>(ProxyContext context, ProxyDelegate<T> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        cancellationToken.ThrowIfCancellationRequested();
        string? token;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(options.RequestTimeout);
            token = await secretProvider.GetAsync(options.SecretName, timeout.Token).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(token) && options.ValidateToken && !IsTokenValid(token))
            {
                token = options.AutoRefresh && !string.IsNullOrEmpty(options.RefreshSecretName)
                    ? await TryRefreshTokenAsync(timeout.Token).ConfigureAwait(false)
                    : null;
                if (token is not null && !IsTokenValid(token))
                    token = null;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is TimeoutException or OperationCanceledException)
        {
            logger.LogWarning(exception, "Secret token retrieval timed out.");
            if (options.FailOnTimeout)
                throw new TimeoutException("Secret token retrieval timed out.", exception);
            return await next(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Secret token retrieval failed.");
            if (options.FailOnError)
                throw;
            return await next(context, cancellationToken).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(token))
            HandleTokenFailure();
        else
        {
            ProxyHeaders.Set(context, options.HeaderName, FormatTokenForHeader(token));
            AddMetadataHeaders(context);
        }
        return await next(context, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Checks encoding and lifetime only; trust is supplied by the secret store, not this inspection.</summary>
    protected virtual bool IsTokenValid(string token)
    {
        try
        {
            string raw = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? token[7..] : token;
            JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(raw);
            DateTime now = DateTime.UtcNow;
            return jwt.ValidTo > now && jwt.ValidFrom <= now;
        }
        catch (Exception exception)
        {
            logger.LogDebug(exception, "Stored token inspection failed.");
            return false;
        }
    }

    /// <summary>Override with a real issuer refresh flow. The default never pretends a secret is a refreshed token.</summary>
    protected virtual Task<string?> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(null);
    }

    /// <summary>Formats the outbound token without duplicating a Bearer prefix.</summary>
    protected virtual string FormatTokenForHeader(string token)
        => options.HeaderName.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
            && !token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? $"Bearer {token}" : token;

    /// <summary>Adds explicitly opted-in metadata headers.</summary>
    protected virtual void AddMetadataHeaders(ProxyContext context)
    {
        if (options.IncludeMetadata)
        {
            ProxyHeaders.Set(context, "X-Token-Source", "KeyVault");
            ProxyHeaders.Set(context, "X-Token-Secret", options.SecretName);
            if (!string.IsNullOrEmpty(options.CorrelationId))
                ProxyHeaders.Set(context, "X-Correlation-ID", options.CorrelationId);
        }
    }

    /// <summary>Applies the missing/invalid-token policy outside the retrieval exception handler.</summary>
    protected virtual void HandleTokenFailure()
    {
        if (options.FailOnMissingToken)
            throw new InvalidOperationException("A required secret token is unavailable or invalid.");
    }
}
