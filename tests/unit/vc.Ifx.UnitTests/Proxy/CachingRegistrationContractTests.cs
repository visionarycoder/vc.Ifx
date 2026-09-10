using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;
using VisionaryCoder.Framework.Proxy.Interceptors.Logging;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class CachingRegistrationContractTests
{
    [TestMethod]
    public async Task ExplicitCachingProvidersParticipateInRealPipelineExactlyOnce()
    {
        Action<IServiceCollection>[] registrations =
        [
            services => services.AddCaching<MemoryProxyCache>(),
            services => services.AddCaching<DefaultCachePolicyProvider>(),
            services => services.AddCaching<DefaultCacheKeyProvider>(),
            services => services.AddCaching().UseDefaultCachingProviders()
        ];
        foreach (var register in registrations)
        {
            var services = new ServiceCollection();
            register(services);
            services.AddProxyPipeline().AddProxyTransport<AuthorizationBridgeContractTests.CountingTransport>();
            using var container = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
            using var scope = container.CreateScope();
            Assert.AreEqual(1, scope.ServiceProvider.GetServices<IProxyInterceptor>().Count());
            Assert.AreSame(scope.ServiceProvider.GetRequiredService<IProxyInterceptor>(), scope.ServiceProvider.GetRequiredService<IOrderedProxyInterceptor>());
            var pipeline = scope.ServiceProvider.GetRequiredService<IProxyPipeline>();
            var context = new ProxyContext { Method = "GET" };
            await pipeline.SendAsync<int>(context);
            await pipeline.SendAsync<int>(context);
            Assert.AreEqual(1, ((AuthorizationBridgeContractTests.CountingTransport)container.GetRequiredService<IProxyTransport>()).Calls);
        }
    }

    [TestMethod]
    public void OptionsAreRegisteredOnceAndLegacyDistributedAliasDoesNotInventBackend()
    {
        int configured = 0;
        var services = new ServiceCollection();
        services.AddDistributedCaching(options => { configured++; options.DefaultDuration = TimeSpan.FromSeconds(17); });
        using var container = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        Assert.AreEqual(TimeSpan.FromSeconds(17), container.GetRequiredService<IOptions<CachingOptions>>().Value.DefaultDuration);
        Assert.AreEqual(1, configured);
        Assert.IsInstanceOfType<NullProxyCache>(container.GetRequiredService<IProxyCache>());
        Assert.IsInstanceOfType<NullCacheKeyProvider>(container.GetRequiredService<ICacheKeyProvider>());
        Assert.IsInstanceOfType<NullCachePolicyProvider>(container.GetRequiredService<ICachePolicyProvider>());
        var custom = new ServiceCollection();
        custom.AddCaching<MemoryProxyCache>(options => options.KeyGenerator = context => "custom");
        using var customContainer = custom.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        Assert.AreEqual("custom", customContainer.GetRequiredService<ICacheKeyProvider>().GenerateKey(new()));
        Assert.AreEqual("custom", new DefaultCacheKeyProvider(new CachingOptions { KeyGenerator = context => "custom" }).GenerateKey<int>(new()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new DefaultCacheKeyProvider(null!));
        var quick = new ServiceCollection();
        quick.AddCaching(TimeSpan.FromSeconds(3), 10, true);
        using var quickContainer = quick.BuildServiceProvider();
        var quickOptions = quickContainer.GetRequiredService<CachingOptions>();
        Assert.AreEqual(TimeSpan.FromSeconds(3), quickOptions.DefaultDuration);
        Assert.AreEqual(10, quickOptions.MaxCacheSize);
        Assert.IsTrue(quickOptions.EnableEvictionLogging);
    }

    [TestMethod]
    public async Task LegacyCacheAndLoggingRegistrationResolveWithoutExtraLoggingSetup()
    {
        foreach (bool configured in new[] { false, true })
        {
            var services = new ServiceCollection();
            if (configured) services.AddCachingInterceptor(TimeSpan.FromSeconds(17), context => "key");
            else services.AddCachingInterceptor();
            services.AddLoggingInterceptor();
            using var container = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
            foreach (var interceptor in container.GetServices<IProxyInterceptor>())
                Assert.IsTrue((await interceptor.InvokeAsync<int>(new() { Method = "GET" }, (context, token) => Task.FromResult(ProxyResponse<int>.Success(1)))).IsSuccess);
        }
        Assert.ThrowsExactly<ArgumentNullException>(() => CachingInterceptorExtensions.AddCachingInterceptor(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingInterceptorExtensions.AddLoggingInterceptor(null!));
    }

    [TestMethod]
    public void RegistrationRejectsInvalidProvidersAndNullCollections()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddCaching<object>());
        Assert.ThrowsExactly<ArgumentNullException>(() => CachingExtensions.AddCaching(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => CachingExtensions.AddDistributedCaching(null!, options => { }));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ServiceCollection().AddDistributedCaching(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => CachingExtensions.ReplaceCacheKeyProvider<DefaultCacheKeyProvider>(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => CachingExtensions.ReplaceCachePolicyProvider<DefaultCachePolicyProvider>(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => CachingExtensions.ReplaceProxyCache<MemoryProxyCache>(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => CachingExtensions.UseDefaultCachingProviders(null!));
    }
}
