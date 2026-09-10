using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors;
using VisionaryCoder.Framework.Proxy.Interceptors.Auditing;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Interceptors;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;
using VisionaryCoder.Framework.Proxy.Interceptors.Retries;
using VisionaryCoder.Framework.Proxy.Interceptors.Security;
using VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;
using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Tests.ProxyAspNetCore;

[TestClass]
public class RegistrationTests
{
    private static ServiceProvider Build(IServiceCollection services) => services.BuildServiceProvider(new ServiceProviderOptions
    {
        ValidateScopes = true, ValidateOnBuild = true
    });

    internal static void ConfigureJwt(JwtOptions options)
    {
        options.Authority = "https://issuer.example";
        options.Audience = "api";
        options.SigningKey = "test-signing-key-32-bytes-at-least";
    }

    [TestMethod]
    public void AuthenticationRegistrationResolvesScopedDependenciesAndReplacesOnlyFallbacks()
    {
        var services = new ServiceCollection();
        services.AddJwtAuthentication(ConfigureJwt).AddJwtAuthentication(ConfigureJwt);
        using (ServiceProvider fallback = Build(services))
        using (IServiceScope scope = fallback.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IUserContextProvider>().Should().BeOfType<NullUserContextProvider>();
            scope.ServiceProvider.GetRequiredService<ITenantContextProvider>().Should().BeOfType<NullTenantContextProvider>();
            scope.ServiceProvider.GetRequiredService<ITokenProvider>().Should().BeOfType<NullTokenProvider>();
            scope.ServiceProvider.GetRequiredService<JwtAuthenticationInterceptor>().Should().NotBeNull();
        }
        services.AddCompleteAuthentication(ConfigureJwt).AddCompleteAuthentication(ConfigureJwt);
        using ServiceProvider provider = Build(services);
        using IServiceScope first = provider.CreateScope();
        using IServiceScope second = provider.CreateScope();
        var user = first.ServiceProvider.GetRequiredService<IUserContextProvider>();
        user.Should().BeOfType<DefaultUserContextProvider>();
        user.Should().BeSameAs(first.ServiceProvider.GetRequiredService<IUserContextProvider>());
        user.Should().NotBeSameAs(second.ServiceProvider.GetRequiredService<IUserContextProvider>());
        first.ServiceProvider.GetRequiredService<ITenantContextProvider>().Should().BeOfType<DefaultTenantContextProvider>();
        first.ServiceProvider.GetRequiredService<ITokenProvider>().Should().BeOfType<DefaultTokenProvider>();
        first.ServiceProvider.GetRequiredService<UserContext>().Should().NotBeNull();
        first.ServiceProvider.GetRequiredService<TenantContext>().Should().NotBeNull();
        provider.GetRequiredService<IHttpContextAccessor>().Should().NotBeNull();
        provider.GetRequiredService<IHttpClientFactory>().Should().NotBeNull();
        Assert.ThrowsExactly<InvalidOperationException>(() => provider.GetRequiredService<IUserContextProvider>());
        services.Count(value => value.ServiceType == typeof(IUserContextProvider)).Should().Be(1);
        services.Should().NotContain(value => value.ServiceType == typeof(KeyVaultJwtInterceptor));
    }

    [TestMethod]
    public void TypedConfigurationCustomProvidersAndExplicitReplacementWork()
    {
        var services = new ServiceCollection();
        services.AddJwtAuthentication<ValidJwtOptions>();
        services.AddUserContext<CustomUser>().AddTenantContext<CustomTenant>().AddTokenProvider<CustomToken>();
        services.ReplaceUserContextProvider<CustomUser>().ReplaceTenantContextProvider<CustomTenant>().ReplaceTokenProvider<CustomToken>();
        services.AddCompleteAuthentication(ConfigureJwt);
        using (ServiceProvider provider = Build(services))
        using (IServiceScope scope = provider.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IUserContextProvider>().Should().BeOfType<CustomUser>();
            scope.ServiceProvider.GetRequiredService<ITenantContextProvider>().Should().BeOfType<CustomTenant>();
            scope.ServiceProvider.GetRequiredService<ITokenProvider>().Should().BeOfType<CustomToken>();
        }
        services.UseDefaultAuthenticationProviders();
        using ServiceProvider defaults = Build(services);
        using IServiceScope defaultScope = defaults.CreateScope();
        defaultScope.ServiceProvider.GetRequiredService<ITokenProvider>().Should().BeOfType<DefaultTokenProvider>();
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddJwtAuthentication<object>());
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddJwtAuthentication<JwtOptions>());
    }

    [TestMethod]
    public void OptionalInterceptorsAndKeyVaultDependenciesAreExplicit()
    {
        var services = new ServiceCollection();
        services.AddAuthenticationInterceptors();
        using (ServiceProvider empty = Build(services))
            empty.GetService<JwtAuthenticationInterceptor>().Should().BeNull();
        services.AddKeyVaultJwtAuthentication(options => options.SecretName = "jwt").AddAuthenticationInterceptors();
        Assert.ThrowsExactly<AggregateException>(() => Build(services));
        services.AddScoped(provider => Mock.Of<ISecretProvider>());
        using ServiceProvider configured = Build(services);
        using IServiceScope scope = configured.CreateScope();
        scope.ServiceProvider.GetRequiredService<KeyVaultJwtInterceptor>().Should().NotBeNull();
    }

    [TestMethod]
    public void ConfigurationAndProviderLifetimeValidationFailEarly()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddJwtAuthentication(options => { }));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ServiceCollection().AddJwtAuthentication(null!));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddKeyVaultJwtAuthentication(options => options.SecretName = ""));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ServiceCollection().AddKeyVaultJwtAuthentication(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => AuthenticationExtensions.AddUserContext(null!));
        foreach (bool tenant in new[] { false, true })
        {
            var services = new ServiceCollection();
            if (tenant) services.AddSingleton<ITenantContextProvider, CustomTenant>();
            else services.AddSingleton<IUserContextProvider, CustomUser>();
            Assert.ThrowsExactly<InvalidOperationException>(() => services.AddAuthenticationWithValidation(ConfigureJwt));
            services.AddAuthenticationWithValidation(ConfigureJwt, false).Should().BeSameAs(services);
        }
        new ServiceCollection().AddAuthenticationWithValidation(ConfigureJwt).Should().NotBeNull();
    }

    [TestMethod]
    public async Task AllAggregateInterceptorsResolveOnceAndDenyBeforeRealTransport()
    {
        var services = new ServiceCollection();
        services.AddProxyInterceptors().AddProxyInterceptors(options => options.MaxRetries = 0);
        services.AddAuthorizationPolicy<AllowPolicy>().AddAuthorizationPolicy<DenyPolicy>().AddAuthorizationPolicy<DenyPolicy>();
        services.AddSecurityEnricher<FirstEnricher>().AddSecurityEnricher<SecondEnricher>().AddSecurityEnricher<FirstEnricher>();
        services.AddAuditSink<ExtraSink>().AddAuditSink<ExtraSink>();
        services.AddProxyPipeline();
        var transport = new Mock<IProxyTransport>(MockBehavior.Strict);
        services.AddSingleton(transport.Object);
        using ServiceProvider provider = Build(services);
        using IServiceScope scope = provider.CreateScope();
        IProxyInterceptor[] interceptors = scope.ServiceProvider.GetServices<IProxyInterceptor>().ToArray();
        interceptors.Should().HaveCount(7);
        interceptors.Select(value => value.GetType()).Should().OnlyHaveUniqueItems();
        interceptors.Should().NotContain(value => value is RetryInterceptor);
        scope.ServiceProvider.GetServices<IOrderedProxyInterceptor>().Should().HaveCount(7);
        scope.ServiceProvider.GetServices<IProxySecurityEnricher>().Should().HaveCount(2);
        scope.ServiceProvider.GetServices<IProxyAuthorizationPolicy>().Should().HaveCount(2);
        scope.ServiceProvider.GetServices<IAuditSink>().Should().HaveCount(2);
        provider.GetRequiredService<IOptions<ProxyOptions>>().Value.MaxRetries.Should().Be(0);
        var context = new ProxyContext();
        ProxyResponse<string> response = await scope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<string>(context);
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Be("Authorization failed");
        context.Headers.Keys.Should().Contain(["First", "Second"]);
        transport.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task AggregateSuccessfulPipelineReachesTransportExactlyOnce()
    {
        var services = new ServiceCollection();
        services.AddProxyInterceptors().AddProxyInterceptors().AddProxyPipeline();
        services.AddAuthorizationPolicy<AllowPolicy>();
        var transport = new Mock<IProxyTransport>(MockBehavior.Strict);
        transport.Setup(value => value.SendCoreAsync<string>(It.IsAny<ProxyContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProxyResponse<string>.Success("sent"));
        services.AddSingleton(transport.Object);
        using ServiceProvider provider = Build(services);
        using IServiceScope scope = provider.CreateScope();
        ProxyResponse<string> response = await scope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<string>(new ProxyContext());
        response.Data.Should().Be("sent");
        transport.Verify(value => value.SendCoreAsync<string>(It.IsAny<ProxyContext>(), It.IsAny<CancellationToken>()), Times.Once);
        transport.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task LegacyAuthorizationBridgeAlsoDeniesThroughAdapterPipeline()
    {
        var services = new ServiceCollection();
        services.AddProxyInterceptors().AddProxyPipeline();
        VisionaryCoder.Framework.Proxy.Interceptors.Authorization.AuthorizationExtensions.AddAuthorization(services);
        var transport = new Mock<IProxyTransport>(MockBehavior.Strict);
        services.AddSingleton(transport.Object);
        using ServiceProvider provider = Build(services);
        using IServiceScope scope = provider.CreateScope();
        (await scope.ServiceProvider.GetRequiredService<IProxyPipeline>().SendAsync<string>(new ProxyContext())).IsSuccess.Should().BeFalse();
        transport.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task BearerFactoriesAreScopedIdempotentAndDoNotCaptureRootSecrets()
    {
        var services = new ServiceCollection();
        services.AddScoped(provider =>
        {
            var secret = new Mock<ISecretProvider>();
            secret.Setup(value => value.GetAsync("jwt", It.IsAny<CancellationToken>())).ReturnsAsync("secret-token");
            return secret.Object;
        });
        services.AddJwtBearerInterceptorFromSecret("jwt").AddJwtBearerInterceptorFromSecret("jwt");
        services.AddJwtBearerEnricher(provider => provider.GetRequiredService<ISecretProvider>().GetAsync("jwt"));
        services.AddJwtBearerEnricher(provider => throw new InvalidOperationException("must not replace first"));
        using ServiceProvider root = Build(services);
        using IServiceScope first = root.CreateScope();
        using IServiceScope second = root.CreateScope();
        IProxyInterceptor interceptor = first.ServiceProvider.GetServices<IProxyInterceptor>().Single();
        interceptor.Should().BeSameAs(first.ServiceProvider.GetRequiredService<JwtBearerInterceptor>());
        interceptor.Should().NotBeSameAs(second.ServiceProvider.GetRequiredService<JwtBearerInterceptor>());
        var context = new ProxyContext();
        (await interceptor.InvokeAsync(context, (value, token) => Task.FromResult(ProxyResponse<string>.Success("ok")))).IsSuccess.Should().BeTrue();
        context.Headers["Authorization"].Should().Be("Bearer secret-token");
        await first.ServiceProvider.GetServices<IProxySecurityEnricher>().Single().EnrichAsync(context);
        Assert.ThrowsExactly<InvalidOperationException>(() => root.GetRequiredService<JwtBearerInterceptor>());
    }

    [TestMethod]
    public async Task ExplicitRetryCacheAndStaticBearerRegistrationRemainAvailable()
    {
        var services = new ServiceCollection();
        services.AddProxyCache<NullProxyCache>().AddCachingInterceptor().AddCachingInterceptor().AddRetryInterceptor().AddRetryInterceptor();
        services.AddJwtBearerInterceptorWithStaticToken("static-token").AddJwtBearerInterceptor(token => Task.FromResult<string?>("ignored"));
        using ServiceProvider provider = Build(services);
        using IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider.GetServices<IOrderedProxyInterceptor>().Should().HaveCount(2);
        var context = new ProxyContext();
        await scope.ServiceProvider.GetRequiredService<JwtBearerInterceptor>().InvokeAsync(context,
            (value, token) => Task.FromResult(ProxyResponse<string>.Success("ok")));
        context.Headers["Authorization"].Should().Be("Bearer static-token");
        Assert.ThrowsExactly<ArgumentNullException>(() => new ServiceCollection().AddJwtBearerInterceptor(null!));
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddJwtBearerInterceptorFromSecret(""));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => new ServiceCollection().AddJwtBearerInterceptorWithStaticToken("bad token"));
        Assert.ThrowsExactly<ArgumentNullException>(() => SecurityInterceptorExtensions.AddJwtBearerInterceptorWithStaticToken(null!, "ok"));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ServiceCollection().AddJwtBearerEnricher(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => ProxyInterceptorExtensions.AddSecurityInterceptor(null!));
    }

    public sealed class ValidJwtOptions : JwtOptions
    {
        public ValidJwtOptions() => ConfigureJwt(this);
    }

    public sealed class CustomUser(IHttpContextAccessor accessor, ILogger<DefaultUserContextProvider> logger)
        : DefaultUserContextProvider(accessor, logger);
    public sealed class CustomTenant(IHttpContextAccessor accessor, ILogger<DefaultTenantContextProvider> logger)
        : DefaultTenantContextProvider(accessor, logger);
    public sealed class CustomToken(HttpClient client, JwtOptions options, ILogger<DefaultTokenProvider> logger)
        : DefaultTokenProvider(client, options, logger);
    public sealed class AllowPolicy : IProxyAuthorizationPolicy
    {
        public Task<bool> IsAuthorizedAsync(ProxyContext context, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
    public sealed class DenyPolicy : IProxyAuthorizationPolicy
    {
        public Task<bool> IsAuthorizedAsync(ProxyContext context, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
    public sealed class FirstEnricher : IProxySecurityEnricher
    {
        public Task EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default)
        {
            context.Headers["First"] = "yes";
            return Task.CompletedTask;
        }
    }
    public sealed class SecondEnricher : IProxySecurityEnricher
    {
        public Task EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default)
        {
            context.Headers["Second"] = "yes";
            return Task.CompletedTask;
        }
    }
    public sealed class ExtraSink : IAuditSink
    {
        public Task WriteAsync(AuditRecord auditRecord, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
