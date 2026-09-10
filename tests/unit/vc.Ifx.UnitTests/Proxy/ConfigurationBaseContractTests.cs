using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VisionaryCoder.Framework.Proxy.Interceptors.Configuration;
using ConfigExtensions = VisionaryCoder.Framework.Proxy.Interceptors.Configuration.ConfigurationExtensions;
using ProviderBase = VisionaryCoder.Framework.Proxy.Interceptors.Configuration.ConfigurationProvider;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class ConfigurationBaseContractTests
{
    [TestMethod]
    public void OptionalLocalConfigurationUsesWorkingDirectoryWhenBasePathIsAbsent()
    {
        using var provider = new VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Local.LocalConfigurationProvider(
            new VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Local.LocalConfigurationProviderOptions
            { BasePath = null, FilePath = $"missing-{Guid.NewGuid():N}.json", Optional = true, ReloadOnChange = false },
            NullLogger<VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Local.LocalConfigurationProvider>.Instance);
        Assert.IsTrue(provider.IsAvailable);
        var fallback = new Box();
        Assert.AreSame(fallback, provider.GetValue("missing", fallback));
    }

    [TestMethod]
    [DoNotParallelize]
    public void LegacyConfigurationRegistrationMapsOptionsAndBuildsSdkSourcesWithoutLoading()
    {
        string original = ConfigExtensions.ConfigurationKey;
        try
        {
            ConfigExtensions.ConfigurationKey = "Config";
            using var root = new ConfigurationRoot([]);
            var services = new ServiceCollection();
            int calls = 0;
            Assert.AreSame(services, ConfigExtensions.AddAzureAppConfiguration(services, root, options => calls++));
            using var container = services.BuildServiceProvider();
            var options = container.GetRequiredService<ConfigurationOptions>();
            Assert.IsNull(options.Endpoint);
            Assert.AreEqual("Production", options.Label);
            Assert.AreEqual("App:Sentinel", options.SentinelKey);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.CacheExpiration);
            Assert.IsNull(options.ConnectionString);
            Assert.IsFalse(options.UseConnectionString);
            Assert.AreEqual(1, calls);
            var builder = new ConfigurationBuilder();
            Assert.AreSame(builder, ConfigExtensions.AddAzureAppConfiguration(builder, options));
            Assert.AreEqual(0, builder.Sources.Count);
            foreach (var configured in new[]
            {
                new ConfigurationOptions { Endpoint = new("https://example.azconfig.io"), Label = "Test" },
                new ConfigurationOptions { UseConnectionString = true, ConnectionString = "Endpoint=https://example.azconfig.io;Id=id;Secret=YWJj" }
            })
            {
                var configuredBuilder = new ConfigurationBuilder();
                Assert.AreSame(configuredBuilder, ConfigExtensions.AddAzureAppConfiguration(configuredBuilder, configured));
                var source = configuredBuilder.Sources.Single().Build(configuredBuilder);
                if (source is IDisposable disposable) disposable.Dispose();
            }
            using var configuredRoot = (ConfigurationRoot)new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Config:Endpoint"] = "https://example.azconfig.io", ["Config:Label"] = "configured"
            }).Build();
            var configuredServices = new ServiceCollection();
            ConfigExtensions.AddAzureAppConfiguration(configuredServices, configuredRoot);
            using var configuredContainer = configuredServices.BuildServiceProvider();
            Assert.AreEqual("configured", configuredContainer.GetRequiredService<ConfigurationOptions>().Label);
            Assert.ThrowsExactly<ArgumentNullException>(() => ConfigExtensions.AddAzureAppConfiguration((IServiceCollection)null!, root));
            Assert.ThrowsExactly<ArgumentNullException>(() => ConfigExtensions.AddAzureAppConfiguration(services, null!));
            Assert.ThrowsExactly<ArgumentNullException>(() => ConfigExtensions.AddAzureAppConfiguration((IConfigurationBuilder)null!, options));
            Assert.ThrowsExactly<ArgumentNullException>(() => ConfigExtensions.AddAzureAppConfiguration(builder, (ConfigurationOptions)null!));
            Assert.ThrowsExactly<InvalidOperationException>(() => ConfigExtensions.AddAzureAppConfiguration(builder, new ConfigurationOptions { UseConnectionString = true }));
        }
        finally { ConfigExtensions.ConfigurationKey = original; }
    }

    [TestMethod]
    public async Task BaseAsyncCompatibilityMethodsAndCacheTypeExpirationAreExplicit()
    {
        using var root = new ConfigurationRoot([]);
        using var provider = new TestProvider(new BaseOptions(), root);
        Assert.AreEqual("", provider.ProviderName);
        var value = new Box();
        Assert.AreSame(value, await provider.GetValueAsync("value", value));
        Assert.IsNotNull(await provider.GetSectionAsync<Box>("section"));
        Assert.IsTrue(await provider.SetValueAsync("value", value));
        Assert.IsFalse(provider.Read<Box>("missing", out var missing));
        provider.Store("value", value);
        Assert.IsTrue(provider.Read<Box>("value", out var stored));
        Assert.AreSame(value, stored);
        Assert.IsFalse(provider.Read<List<int>>("value", out var otherType));
        Assert.IsTrue(provider.Refresh());
        Assert.IsFalse(provider.Read<Box>("value", out var cleared));
        using var expired = new TestProvider(new BaseOptions { CacheExpiration = TimeSpan.FromTicks(1) }, root);
        expired.Store("value", value);
        await Task.Delay(5);
        Assert.IsFalse(expired.Read<Box>("value", out var expiredValue));
        var thrown = false;
        using var failing = new TestProvider(new BaseOptions(), root, new CallbackLogger(() =>
        {
            if (!thrown) { thrown = true; throw new InvalidOperationException("logging failure"); }
        }));
        Assert.IsFalse(failing.Refresh());
        provider.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => provider.GetAllValues());
        Assert.ThrowsExactly<ObjectDisposedException>(() => provider.Refresh());
    }

    [TestMethod]
    public async Task BaseRefreshReturnsFalseOnContentionAndSnapshotFallbackIsDeliberate()
    {
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        int enteredOnce = 0;
        using var root = (ConfigurationRoot)new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["key"] = "value", ["app:child"] = "nested", ["appOther"] = "outside"
        }).Build();
        using var provider = new TestProvider(new BaseOptions(), root, new CallbackLogger(() =>
        {
            if (Interlocked.Increment(ref enteredOnce) == 1) { entered.Set(); release.Wait(); }
        }));
        Task<bool> owner = Task.Run(provider.Refresh);
        Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(5)));
        try { Assert.IsFalse(provider.Refresh()); }
        finally { release.Set(); }
        Assert.IsTrue(await owner);
        Assert.AreEqual("value", provider.GetAllValues()["key"]);
        using var prefixed = new TestProvider(new BaseOptions { KeyPrefix = "app:" }, root);
        var snapshot = prefixed.GetAllValues();
        Assert.AreEqual("nested", snapshot["child"]);
        Assert.IsFalse(snapshot.ContainsKey("appOther"));
        var broken = new Mock<IConfiguration>();
        broken.Setup(config => config.GetChildren()).Throws<InvalidOperationException>();
        using var failedSnapshot = new TestProvider(new BaseOptions(), broken.Object);
        Assert.AreEqual(0, failedSnapshot.GetAllValues().Count);
        var blankSection = new Mock<IConfigurationSection>();
        blankSection.SetupGet(section => section.Path).Returns("");
        blankSection.SetupGet(section => section.Key).Returns("");
        blankSection.Setup(section => section.GetChildren()).Returns([]);
        broken.Setup(config => config.GetChildren()).Returns([blankSection.Object]);
        Assert.AreEqual(0, failedSnapshot.GetAllValues().Count);
    }

    public sealed class Box { }
    private sealed class BaseOptions : ConfigurationProviderOptions { }
    private sealed class TestProvider : ProviderBase
    {
        public TestProvider(ConfigurationProviderOptions options, IConfiguration root, ILogger<ProviderBase>? logger = null)
            : base(options, logger ?? NullLogger<ProviderBase>.Instance) => configuration = root;
        public void Store(string key, object value) => AddToCache(key, value);
        public bool Read<T>(string key, out T value) => TryGetFromCache(key, out value);
        public override T GetValue<T>(string key, T defaultValue) => defaultValue;
        public override bool SetValue<T>(string key, T value) => true;
        public override T GetSection<T>(string sectionName) => new();
        public override Task<bool> RefreshAsync(CancellationToken cancellationToken = default) => Task.FromResult(Refresh());
    }
    private sealed class CallbackLogger(Action callback) : ILogger<ProviderBase>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => callback();
    }
}
