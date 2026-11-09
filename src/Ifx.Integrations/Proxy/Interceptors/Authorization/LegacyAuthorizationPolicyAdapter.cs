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

using Ifx.Proxy.Interceptor.Authorization.Policies;
using Ifx.Proxy.Interceptor.Security;

namespace Ifx.Proxy.Interceptor.Authorization;

/// <summary>Enforces all legacy policies through the canonical authorization port in the current DI scope.</summary>
internal sealed class LegacyAuthorizationPolicyAdapter(IEnumerable<IAuthorizationPolicy> policies) : IProxyAuthorizationPolicy
{
    private readonly IAuthorizationPolicy[] policies = policies.ToArray();

    public async Task<bool> IsAuthorizedAsync(ProxyContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        if (policies.Length == 0) return false;
        foreach (IAuthorizationPolicy policy in policies)
        {
            var result = await policy.EvaluateAsync(context, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (result is null || !result.IsAuthorized) return false;
        }
        return true;
    }
}
