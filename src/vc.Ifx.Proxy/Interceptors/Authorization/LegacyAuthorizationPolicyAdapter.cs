using VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Policies;
using VisionaryCoder.Framework.Proxy.Interceptors.Security;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authorization;

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
