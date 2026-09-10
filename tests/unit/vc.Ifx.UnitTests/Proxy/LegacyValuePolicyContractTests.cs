using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;
using VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Policies;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class LegacyValuePolicyContractTests
{
    [TestMethod]
    public void MutableIdentityModelsHaveExplicitValidityAndLookupSemantics()
    {
        var user = new UserContext();
        Assert.IsFalse(user.IsValid);
        user.UserId = "user";
        Assert.IsTrue(user.IsValid);
        user.ExpiresAt = DateTimeOffset.UtcNow.AddHours(1);
        Assert.IsTrue(user.IsValid);
        user.ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1);
        Assert.IsFalse(user.IsValid);
        user.AuthenticatedAt = DateTimeOffset.UnixEpoch;
        Assert.AreEqual(DateTimeOffset.UnixEpoch, user.AuthenticatedAt);
        user.Roles.Add("Reader"); user.Permissions.Add("Read");
        Assert.IsTrue(user.HasRole("reader")); Assert.IsFalse(user.HasRole("writer"));
        Assert.IsTrue(user.HasPermission("READ")); Assert.IsFalse(user.HasPermission("write"));
        Assert.IsTrue(user.HasAnyRole("writer", "reader")); Assert.IsFalse(user.HasAnyRole("writer"));
        Assert.IsTrue(user.HasAllRoles("reader")); Assert.IsFalse(user.HasAllRoles("reader", "writer"));
        var tenant = new TenantContext();
        Assert.IsFalse(tenant.IsValid);
        tenant.TenantId = "tenant"; Assert.IsTrue(tenant.IsValid);
        tenant.IsActive = false; Assert.IsFalse(tenant.IsValid);
        tenant.Domain = "example"; tenant.SubscriptionTier = "standard";
        Assert.AreEqual("example", tenant.Domain); Assert.AreEqual("standard", tenant.SubscriptionTier);
        tenant.EnabledFeatures.Add("Feature");
        Assert.IsTrue(tenant.HasFeature("feature")); Assert.IsFalse(tenant.HasFeature("missing"));
        tenant.ResourceLimits["count"] = 7;
        Assert.AreEqual(7, tenant.GetResourceLimit("count")); Assert.IsNull(tenant.GetResourceLimit("missing"));
        Assert.IsNull(tenant.GetSetting<string>(" ")); Assert.IsNull(tenant.GetSetting<string>("missing"));
        tenant.Settings["null"] = null!; Assert.IsNull(tenant.GetSetting<string>("null"));
        tenant.Settings["value"] = 7; Assert.AreEqual(7, tenant.GetSetting<int>("value"));
        Assert.ThrowsExactly<InvalidCastException>(() => tenant.GetSetting<string>("value"));
    }

    [TestMethod]
    public void JwtOptionFactoriesAndValidationPreserveLegacySurface()
    {
        var options = new JwtOptions();
        Assert.IsFalse(options.IsValid());
        options.Authority = "https://issuer.example"; Assert.IsFalse(options.IsValid());
        options.Audience = "audience"; Assert.IsTrue(options.IsValid());
        options.TokenEndpoint = "relative"; Assert.IsFalse(options.IsValid());
        options.TokenEndpoint = "https://issuer.example/token"; Assert.IsTrue(options.IsValid());
        options.Authority = "relative"; Assert.IsFalse(options.IsValid());
        options.Authority = "https://issuer.example"; options.RequestTimeout = TimeSpan.Zero; Assert.IsFalse(options.IsValid());
        Assert.IsNull(options.GetScopeString());
        options.Scopes = ["read", "write"]; Assert.AreEqual("read write", options.GetScopeString());
        var web = JwtOptions.CreateForWebApp("https://issuer.example", "audience", "client", "secret");
        Assert.IsTrue(web.IsValid()); Assert.AreEqual("client", web.ClientId); Assert.AreEqual("secret", web.ClientSecret);
        Assert.IsTrue(web.ValidateIssuer && web.ValidateAudience && web.ValidateLifetime && web.ValidateIssuerSigningKey && web.RequireHttpsMetadata);
        var api = JwtOptions.CreateForApiClient("https://issuer.example", "audience", "read");
        Assert.IsTrue(api.IsValid()); Assert.AreEqual("read", api.GetScopeString()); Assert.IsTrue(api.RefreshIfExpired);
    }

    [TestMethod]
    public async Task RolePolicyEvaluatesOnlyExpectedContextAndHonorsCancellation()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new RoleBasedAuthorizationPolicy(null!));
        var policy = new RoleBasedAuthorizationPolicy(["reader"]);
        Assert.IsFalse(policy.Evaluate(new object()));
        Assert.IsFalse((await policy.EvaluateAsync(new object())).IsAuthorized);
        Assert.IsFalse(policy.Evaluate(new ProxyContext()));
        Assert.IsFalse(policy.Evaluate(new ProxyContext { Metadata = new() { ["Roles"] = "not-a-collection" } }));
        Assert.IsTrue(policy.Evaluate(new ProxyContext { Metadata = new() { ["Roles"] = new[] { "READER" } } }));
        Assert.IsFalse((await policy.EvaluateAsync(new ProxyContext { Metadata = new() { ["Roles"] = new[] { "writer" } } })).IsAuthorized);
        await Assert.ThrowsAsync<OperationCanceledException>(() => policy.EvaluateAsync(new(), new(true)));
    }

    [TestMethod]
    public void DefaultCachePoliciesHonorPredicatesOverridesAndMethodBoundary()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new DefaultCachePolicyProvider(null!));
        var options = new CachingOptions { SlidingExpiration = TimeSpan.FromSeconds(2), MemorySizeLimit = 100 };
        Assert.AreEqual(TimeSpan.FromSeconds(2), options.SlidingExpiration); Assert.AreEqual(100L, options.MemorySizeLimit);
        var provider = new DefaultCachePolicyProvider(options);
        foreach (string? method in new[] { null, "GET", "HEAD", "POST", "PUT", "PATCH", "DELETE", "unknown" })
        {
            var context = new ProxyContext { Method = method };
            bool allowed = method is "GET" or "HEAD";
            Assert.AreEqual(allowed, provider.ShouldCache(context));
            Assert.AreEqual(allowed, provider.GetPolicy(context).IsCachingEnabled);
            Assert.AreEqual(allowed ? options.DefaultDuration : null, provider.GetExpiration(context));
            if (allowed)
            {
                Assert.IsTrue(provider.GetPolicy(context).ShouldCache(new object()));
                Assert.IsFalse(provider.GetPolicy(context).ShouldRefresh(new object()));
            }
        }
        var custom = new CachePolicy { Duration = TimeSpan.FromSeconds(7) };
        options.OperationPolicies["custom"] = custom;
        var customized = new ProxyContext { OperationName = "custom", Method = "POST" };
        Assert.AreSame(custom, provider.GetPolicy(customized));
        Assert.AreEqual(TimeSpan.FromSeconds(7), provider.GetExpiration(customized));
        custom.IsCachingEnabled = false; Assert.IsFalse(provider.ShouldCache(customized));
        options.ShouldCache = context => true; Assert.IsTrue(provider.ShouldCache(new() { Method = "GET" }));
        options.ShouldCache = context => false; Assert.IsFalse(provider.ShouldCache(new() { Method = "GET" }));
        Assert.ThrowsExactly<ArgumentNullException>(() => provider.GetPolicy(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => provider.ShouldCache(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => provider.GetExpiration(null!));
    }
}
