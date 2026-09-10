using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Authorization;
using VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Policies;
using VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Results;
using VisionaryCoder.Framework.Proxy.Interceptors.Security;
using AuthExtensions = VisionaryCoder.Framework.Proxy.Interceptors.Authorization.AuthorizationExtensions;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class AuthorizationBridgeContractTests
{
    private static ServiceCollection Services()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProxyPipeline();
        services.AddProxyTransport<CountingTransport>();
        services.AddProxyInterceptor<SecurityInterceptor>(ServiceLifetime.Scoped);
        return services;
    }

    [TestMethod]
    public async Task EveryLegacyRegistrationEnforcesDenyAndAllowThroughRealPipeline()
    {
        Action<IServiceCollection>[] registrations =
        [
            services => services.AddAuthorizationPolicy<AllowedPolicy>(),
            services => services.AddAuthorizationPolicy(new AllowedPolicy()),
            services => services.AddRoleBasedAuthorization("reader"),
            services => services.ReplaceAuthorizationPolicy<AllowedPolicy>(),
            services => services.UseDefaultRoleBasedAuthorization("reader")
        ];
        foreach (var register in registrations)
        {
            var services = Services();
            AuthExtensions.AddAuthorization(services);
            register(services);
            using var container = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
            using var scope = container.CreateScope();
            Assert.AreEqual(1, scope.ServiceProvider.GetServices<IProxyAuthorizationPolicy>().Count());
            Assert.IsFalse(scope.ServiceProvider.GetServices<IAuthorizationPolicy>().Any(policy => policy is NullAuthorizationPolicy));
            var pipeline = scope.ServiceProvider.GetRequiredService<IProxyPipeline>();
            var allowed = new ProxyContext { Metadata = new() { ["Roles"] = new[] { "reader" }, ["Allow"] = true } };
            Assert.IsTrue((await pipeline.SendAsync<int>(allowed)).IsSuccess);
            Assert.IsFalse((await pipeline.SendAsync<int>(new())).IsSuccess);
            Assert.AreEqual(1, ((CountingTransport)container.GetRequiredService<IProxyTransport>()).Calls);
        }
        var fallback = Services();
        AuthExtensions.AddAuthorization(fallback);
        AuthExtensions.AddAuthorization(fallback);
        using var defaults = fallback.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var defaultScope = defaults.CreateScope();
        Assert.IsFalse((await defaultScope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<int>(new())).IsSuccess);
        Assert.AreEqual(0, ((CountingTransport)defaults.GetRequiredService<IProxyTransport>()).Calls);
    }

    [TestMethod]
    public async Task ScopedLegacyPoliciesComposeWithCanonicalPoliciesAndNeverBypassOnNullOrRemoval()
    {
        var services = Services();
        AuthExtensions.AddAuthorization(services);
        services.RemoveAll<IAuthorizationPolicy>();
        services.AddScoped<IAuthorizationPolicy, AllowedPolicy>();
        services.AddSingleton<IProxyAuthorizationPolicy, CanonicalDeny>();
        using (var container = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }))
        using (var scope = container.CreateScope())
        {
            Assert.IsFalse((await scope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<int>(new() { Metadata = new() { ["Allow"] = true } })).IsSuccess);
            Assert.AreEqual(0, ((CountingTransport)container.GetRequiredService<IProxyTransport>()).Calls);
        }
        services.RemoveAll<IProxyAuthorizationPolicy>();
        services.ReplaceAuthorizationPolicy<NullResultPolicy>();
        using (var container = services.BuildServiceProvider())
        using (var scope = container.CreateScope())
            Assert.IsFalse((await scope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<int>(new())).IsSuccess);
        services.RemoveAll<IAuthorizationPolicy>();
        using (var container = services.BuildServiceProvider())
        using (var scope = container.CreateScope())
            Assert.IsFalse((await scope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<int>(new())).IsSuccess);
        services.AddAuthorizationPolicy(new AllowedPolicy());
        services.AddAuthorizationPolicy(new NullAuthorizationPolicy());
        using (var container = services.BuildServiceProvider())
        using (var scope = container.CreateScope())
            Assert.IsFalse((await scope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<int>(new() { Metadata = new() { ["Allow"] = true } })).IsSuccess);
    }

    [TestMethod]
    public async Task CancellationIsForwardedAndPreservedBeforeAndDuringLegacyEvaluation()
    {
        using var cancellation = new CancellationTokenSource();
        var policy = new CancelPolicy(cancellation);
        var services = Services();
        services.AddAuthorizationPolicy(policy);
        using var container = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        using var scope = container.CreateScope();
        var canonical = scope.ServiceProvider.GetRequiredService<IProxyAuthorizationPolicy>();
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => canonical.IsAuthorizedAsync(null!));
        await Assert.ThrowsAsync<OperationCanceledException>(() => canonical.IsAuthorizedAsync(new(), new(true)));
        Assert.AreEqual(0, policy.Calls);
        var pipeline = scope.ServiceProvider.GetRequiredService<IProxyPipeline>();
        await Assert.ThrowsAsync<OperationCanceledException>(() => pipeline.SendAsync<int>(new(), cancellation.Token));
        Assert.AreEqual(1, policy.Calls);
        Assert.AreEqual(cancellation.Token, policy.Token);
        Assert.AreEqual(0, ((CountingTransport)container.GetRequiredService<IProxyTransport>()).Calls);
    }

    [TestMethod]
    public void RegistrationGuardsRemainExplicit()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => AuthExtensions.AddAuthorization(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => AuthExtensions.AddAuthorizationPolicy<AllowedPolicy>(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => AuthExtensions.AddRoleBasedAuthorization(null!, "role"));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ServiceCollection().AddRoleBasedAuthorization(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => AuthExtensions.AddAuthorizationPolicy(null!, new AllowedPolicy()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ServiceCollection().AddAuthorizationPolicy(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => AuthExtensions.ReplaceAuthorizationPolicy<AllowedPolicy>(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => AuthExtensions.UseDefaultRoleBasedAuthorization(null!, "role"));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ServiceCollection().UseDefaultRoleBasedAuthorization(null!));
    }

    public sealed class CountingTransport : IProxyTransport
    {
        public int Calls { get; private set; }
        public Task<ProxyResponse<T>> SendCoreAsync<T>(ProxyContext context, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(ProxyResponse<T>.Success(default!));
        }
    }
    public sealed class AllowedPolicy : IAuthorizationPolicy
    {
        public string PolicyName => "allowed";
        public bool Evaluate(object context) => context is ProxyContext proxy && proxy.Metadata.GetValueOrDefault("Allow") is true;
        public Task<AuthorizationResult> EvaluateAsync(object context, CancellationToken cancellationToken = default)
            => Task.FromResult(Evaluate(context) ? AuthorizationResult.Success() : AuthorizationResult.Failure("denied"));
    }
    public sealed class NullResultPolicy : IAuthorizationPolicy
    {
        public string PolicyName => "null";
        public bool Evaluate(object context) => false;
        public Task<AuthorizationResult> EvaluateAsync(object context, CancellationToken cancellationToken = default) => Task.FromResult<AuthorizationResult>(null!);
    }
    public sealed class CanonicalDeny : IProxyAuthorizationPolicy
    {
        public Task<bool> IsAuthorizedAsync(ProxyContext context, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
    private sealed class CancelPolicy(CancellationTokenSource cancellation) : IAuthorizationPolicy
    {
        public int Calls { get; private set; }
        public CancellationToken Token { get; private set; }
        public string PolicyName => "cancellation";
        public bool Evaluate(object context) => throw new AssertFailedException();
        public Task<AuthorizationResult> EvaluateAsync(object context, CancellationToken cancellationToken = default)
        {
            Calls++; Token = cancellationToken; cancellation.Cancel();
            return Task.FromResult(AuthorizationResult.Success());
        }
    }
}
