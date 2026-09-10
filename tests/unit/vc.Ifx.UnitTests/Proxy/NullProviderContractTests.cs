using Microsoft.Extensions.Logging.Abstractions;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;
using VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Policies;
using VisionaryCoder.Framework.Proxy.Interceptors.Auditing;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;
using VisionaryCoder.Framework.Proxy.Interceptors.Correlation;
using VisionaryCoder.Framework.Proxy.Interceptors.Logging;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class NullProviderContractTests
{
    [TestMethod]
    public async Task NullIdentityAndAuthorizationProvidersNeverInventAuthority()
    {
        var users = new NullUserContextProvider();
        Assert.IsNull(await users.GetCurrentUserAsync());
        Assert.IsNull(await users.GetUserAsync("id"));
        Assert.IsFalse(await users.ValidateUserContextAsync(new()));
        var tenants = new NullTenantContextProvider();
        Assert.IsNull(tenants.GetTenantId());
        Assert.IsNull(await tenants.GetTenantIdAsync());
        Assert.IsNull(await tenants.GetTenantContextAsync());
        Assert.IsNull(await tenants.GetTenantContextAsync("id"));
        Assert.IsFalse(await tenants.ValidateTenantContextAsync(new()));
        var tokens = new NullTokenProvider();
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => tokens.GetTokenAsync());
        Assert.IsFalse((await tokens.GetTokenAsync(new VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt.TokenRequest())).IsSuccess);
        Assert.IsFalse(await tokens.ValidateTokenAsync("token"));
        Assert.IsFalse(tokens.ValidateToken("token"));
        Assert.IsNull(await tokens.RefreshTokenAsync("token"));
        Assert.AreEqual(0, tokens.ExtractClaims("token").Count);
        var authorization = new NullAuthorizationPolicy();
        Assert.AreEqual("NullAuthorization", authorization.PolicyName);
        Assert.IsFalse(authorization.Evaluate(new ProxyContext()));
        var denied = await authorization.EvaluateAsync(new ProxyContext());
        Assert.IsFalse(denied.IsAuthorized);
        Assert.AreEqual("NullAuthorizationPolicy", denied.Context["PolicyType"]);
    }

    [TestMethod]
    public async Task NullCacheAndPassThroughInterceptorsPreserveLegacyNoOpBehavior()
    {
        var keys = new NullCacheKeyProvider();
        Assert.IsNull(keys.GenerateKey(null!));
        Assert.IsFalse(keys.CanGenerateKey(null!));
        var policies = new NullCachePolicyProvider();
        var policy = policies.GetPolicy(null!);
        Assert.IsFalse(policy.IsCachingEnabled);
        Assert.AreEqual(TimeSpan.Zero, policy.Duration);
        Assert.IsFalse(policy.ShouldCache(new object()));
        Assert.IsFalse(policy.ShouldRefresh(new object()));
        Assert.IsFalse(policies.ShouldCache(null!));
        Assert.IsNull(policies.GetExpiration(null!));
        var cache = new NullProxyCache();
        Assert.IsNull(await cache.GetAsync<int>("key"));
        await cache.SetAsync("key", ProxyResponse<int>.Success(1), TimeSpan.Zero);
        Assert.IsFalse(await cache.ExistsAsync("key"));
        await cache.RemoveAsync("key");
        await cache.ClearAsync();
        IOrderedProxyInterceptor[] interceptors = [new NullAuditingInterceptor(), new NullAuditingInterceptor(7), new NullCorrelationInterceptor(), new NullLoggingInterceptor()];
        CollectionAssert.AreEqual(new[] { 300, 7, 0, 100 }, interceptors.Select(interceptor => interceptor.Order).ToArray());
        var response = ProxyResponse<int>.Success(1);
        var context = new ProxyContext();
        using var cancellation = new CancellationTokenSource();
        foreach (var interceptor in interceptors)
            Assert.AreSame(response, await interceptor.InvokeAsync<int>(context, (current, token) =>
            {
                Assert.AreSame(context, current); Assert.AreEqual(cancellation.Token, token); return Task.FromResult(response);
            }, cancellation.Token));
        await new LoggingAuditSink(NullLogger<LoggingAuditSink>.Instance).WriteAsync(new AuditRecord { OperationName = "operation", CorrelationId = "correlation" });
    }
}
