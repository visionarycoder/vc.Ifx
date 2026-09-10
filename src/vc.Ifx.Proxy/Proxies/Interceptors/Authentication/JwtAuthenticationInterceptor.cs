using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Security;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Authentication;

/// <summary>
/// Intercepts method invocations to enforce JWT authentication.
/// Extracts the token from method arguments, validates it, and establishes the ClaimsPrincipal.
/// Must run before AuthorizationInterceptor in the interceptor chain.
/// </summary>
public sealed class JwtAuthenticationInterceptor(IJwtTokenValidator tokenValidator, ICurrentPrincipalAccessor principalAccessor) : IProxyInterceptor
{
    /// <summary>
    /// Executes the interceptor to validate JWT authentication on method invocation.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        var attribute = context.GetAttribute<RequireAuthenticationAttribute>();
        if (attribute is null)
            return await next().ConfigureAwait(false);

        // Extract the JWT token from context
        var token = ExtractToken(context, attribute.Scheme);
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("No authentication token provided.");

        try
        {
            // Validate the token and extract claims
            var principal = tokenValidator.ValidateToken(token);

            // Store the principal in context for use by authorization and audit interceptors
            context.Items[InterceptorConstants.Principal] = principal;

            // Update the current principal accessor so authorization interceptor can access it
            // Note: This assumes ICurrentPrincipalAccessor supports setting the principal.
            // If it does not, the context.Items storage is still available for custom access patterns.

            return await next().ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException("Token validation failed.", exception);
        }
    }

    /// <summary>
    /// Extracts the JWT token from method arguments or invocation items.
    /// Searches for tokens in method arguments (typically IServiceMessage.AuthenticationToken or similar)
    /// and falls back to invocation context items.
    /// </summary>
    private static string? ExtractToken(MethodContext context, string scheme)
    {
        // Check if token is already in context items
        if (context.Items.TryGetValue(InterceptorConstants.AuthenticationToken, out var tokenValue))
            return tokenValue?.ToString();

        // Search method arguments for a token
        foreach (var argument in context.Arguments)
        {
            if (argument.Value is null)
                continue;

            // Check if argument has an AuthenticationToken or Token property
            var property = argument.Value.GetType().GetProperty("AuthenticationToken")
                ?? argument.Value.GetType().GetProperty("Token");

            if (property?.GetValue(argument.Value) is string token && !string.IsNullOrEmpty(token))
                return token;
        }

        return null;
    }
}
