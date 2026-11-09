using Ifx;
using Ifx.Component;
using Ifx.Pipeline;
using Ifx.Proxy.Exceptions;
using Ifx.Querying;
using Ifx.Querying.Serialization;
using Ifx.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Threading.RateLimiting;

using Ifx.Proxy.Interceptor.Authentication;
using Ifx.Proxy.Interceptor.Authentication.Providers;

namespace Ifx.Proxy.Interceptor.Security;
/// <summary>
/// Security enricher that adds user information to the proxy context.
/// </summary>
/// <param name="userProvider">The user context provider.</param>
public class UserContextEnricher(IUserContextProvider userProvider) : IProxySecurityEnricher
{
    private readonly IUserContextProvider userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
    /// <summary>
    /// Gets the execution order for this enricher.
    /// </summary>
    public int Order => 100;
    /// <summary>
    /// Enriches the context with current user information.
    /// </summary>
    /// <param name="context">The proxy context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the enrichment operation.</returns>
    public async Task EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default)
    {
        UserContext? userContext = await userProvider.GetCurrentUserAsync(cancellationToken);
        if (userContext == null || string.IsNullOrWhiteSpace(userContext.UserId))
        {
            return;
        }

        context.Metadata["UserId"] = userContext.UserId;
        context.Metadata["UserName"] = userContext.UserName;
        context.Metadata["Roles"] = userContext.Roles;
        context.Metadata["Permissions"] = userContext.Permissions;

        if (!string.IsNullOrWhiteSpace(userContext.Email))
        {
            context.Metadata["UserEmail"] = userContext.Email;
        }
    }
}
