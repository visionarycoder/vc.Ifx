using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Moq;
using VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Azure;
using AzureProvider = VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Azure.AzureConfigurationProvider;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class AzureConfigurationContractTests
{
    private static AzureConfigurationProviderOptions Options(bool cache = true, bool refresh = true, string? prefix = null) => new()
    {
        Endpoint = new("https://example.azconfig.io"), KeyPrefix = prefix, EnableCaching = cache, EnableRefresh = refresh
    };

    [TestMethod]
    public void OptionsMapWithoutRemoteStartupAndValidateSecurityBoundaries()
    {
        var options = Options();
        Assert.AreEqual("Production", options.Label);
        Assert.AreEqual("App:Sentinel", options.SentinelKey);
        Assert.IsFalse(options.UseConnectionString);
        Assert.IsNull(options.ConnectionString);
        var sdk = new AzureAppConfigurationOptions();
        Assert.AreSame(sdk, options.ApplyTo(sdk));
        Options(prefix: "app:", refresh: false).ApplyTo(new());
        new AzureConfigurationProviderOptions { UseConnectionString = true, ConnectionString = "Endpoint=https://example.azconfig.io;Id=id;Secret=YWJj", KeyPrefix = "" }.ApplyTo(new());
        Assert.ThrowsExactly<ArgumentNullException>(() => AzureConfigurationProviderOptionsExtensions.Validate(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => options.ApplyTo(null!));
        AzureConfigurationProviderOptions[] invalid =
        [
            new(), new() { UseConnectionString = true }, new() { UseConnectionString = true, ConnectionString = " " },
            new() { Endpoint = new("relative", UriKind.Relative) }, new() { Endpoint = new("http://example") },
            new() { Endpoint = new("https://user:pass@example") }, new() { Endpoint = new("https://example/?secret=x") },
            new() { Endpoint = new("https://example/#x") }, new() { Endpoint = options.Endpoint, Label = " " },
            new() { Endpoint = options.Endpoint, SentinelKey = " " }, new() { Endpoint = options.Endpoint, CacheExpiration = TimeSpan.Zero }
        ];
        foreach (var invalidOptions in invalid) Assert.ThrowsExactly<InvalidOperationException>(invalidOptions.Validate);
        var logger = NullLogger<AzureProvider>.Instance;
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureProvider(null!, logger));
        Assert.ThrowsExactly<InvalidOperationException>(() => new AzureProvider(new() { UseConnectionString = true, ConnectionString = "broken" }, logger));
        using var root = new ConfigurationRoot([]);
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureProvider(options, null!, root));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureProvider(options, logger, (IConfigurationRoot)null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureProvider(options, logger, (Func<IConfigurationBuilder, IConfigurationRoot>)null!));
        var owned = new ReloadSource();
        using (var fromFactory = new AzureProvider(options, logger, builder =>
        {
            Assert.AreEqual(1, builder.Sources.Count);
            return new ConfigurationRoot([owned]);
        })) Assert.IsTrue(fromFactory.IsAvailable);
        Assert.IsTrue(owned.Disposed);
    }

    [TestMethod]
    public async Task RefreshActuallyReloadsAndCacheTracksExternalRootNotifications()
    {
        var source = new ReloadSource();
        source.Values["app:item"] = "{\"Value\":1}";
        source.Values["app:section:Value"] = "7";
        using var root = new ConfigurationRoot([source]);
        using var provider = new AzureProvider(Options(prefix: "app:"), NullLogger<AzureProvider>.Instance, root);
        Assert.AreEqual("Azure", provider.ProviderName);
        Assert.IsTrue(provider.IsAvailable);
        var fallback = new Box { Value = -1 };
        var first = provider.GetValue("item", fallback);
        Assert.AreEqual(1, first.Value);
        root["app:item"] = "{\"Value\":2}";
        Assert.AreSame(first, provider.GetValue("item", fallback));
        source.Values["app:item"] = "{\"Value\":3}";
        Assert.IsTrue(await provider.RefreshAsync());
        Assert.AreEqual(3, provider.GetValue("item", fallback).Value);
        source.Values["app:item"] = "{\"Value\":4}";
        root.Reload();
        Assert.AreEqual(4, provider.GetValue("item", fallback).Value);
        source.Values["app:item"] = "{\"Value\":5}";
        Assert.IsTrue(provider.Refresh());
        Assert.AreEqual(5, provider.GetValue("item", fallback).Value);
        Assert.AreEqual(7, provider.GetSection<Box>("section").Value);
        Assert.AreEqual(0, provider.GetSection<Box>("absent").Value);
        Assert.AreSame(fallback, provider.GetValue("absent", fallback));
        root["app:section:Value"] = "bad";
        Assert.ThrowsExactly<InvalidOperationException>(() => provider.GetSection<Box>("section"));
        Assert.Throws<ArgumentException>(() => provider.GetValue(" ", fallback));
        Assert.Throws<ArgumentException>(() => provider.GetSection<Box>(" "));
        Assert.ThrowsExactly<NotSupportedException>(() => provider.SetValue("item", fallback));
        source.Fail = true;
        Assert.IsFalse(provider.Refresh());
        Assert.IsFalse(await provider.RefreshAsync());
        provider.Dispose(); provider.Dispose();
        Assert.IsFalse(provider.IsAvailable);
        Assert.ThrowsExactly<ObjectDisposedException>(() => provider.Refresh());
        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => provider.RefreshAsync());
        Assert.ThrowsExactly<ObjectDisposedException>(() => provider.GetValue("item", fallback));
        Assert.ThrowsExactly<ObjectDisposedException>(() => provider.GetSection<Box>("section"));
        Assert.IsFalse(source.Disposed);
    }

    [TestMethod]
    public async Task DisabledCacheAndRefreshAndOwnedRootAreExplicit()
    {
        var source = new ReloadSource();
        source.Values["item"] = "{\"Value\":1}";
        var root = new ConfigurationRoot([source]);
        using var provider = new AzureProvider(Options(cache: false, refresh: false), NullLogger<AzureProvider>.Instance, root, ownsConfiguration: true);
        Assert.AreEqual(1, provider.GetValue("item", new Box()).Value);
        root["item"] = "{\"Value\":2}";
        Assert.AreEqual(2, provider.GetValue("item", new Box()).Value);
        Assert.IsTrue(provider.Refresh());
        Assert.IsTrue(await provider.RefreshAsync());
        Assert.AreEqual(1, source.Loads);
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.RefreshAsync(canceled.Token));
        provider.Dispose();
        Assert.IsTrue(source.Disposed);
        var unreadable = new Mock<IConfigurationRoot>();
        unreadable.Setup(root => root.GetReloadToken()).Returns(new CancellationChangeToken(CancellationToken.None));
        unreadable.Setup(root => root.GetSection(It.IsAny<string>())).Throws<InvalidOperationException>();
        using var broken = new AzureProvider(Options(), NullLogger<AzureProvider>.Instance, unreadable.Object);
        Assert.IsFalse(broken.IsAvailable);
    }

    [TestMethod]
    public async Task RefreshCancellationWaitsWithoutCancelingAnotherReload()
    {
        var entered = new ManualResetEventSlim();
        var release = new ManualResetEventSlim();
        var source = new ReloadSource();
        using var root = new ConfigurationRoot([source]);
        using var provider = new AzureProvider(Options(), NullLogger<AzureProvider>.Instance, root);
        source.OnLoad = () => { entered.Set(); release.Wait(); };
        Task<bool> owner = Task.Run(provider.Refresh);
        Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(5)));
        using var cancellation = new CancellationTokenSource();
        Task<bool> waiter = provider.RefreshAsync(cancellation.Token);
        cancellation.Cancel();
        try { await Assert.ThrowsAsync<OperationCanceledException>(() => waiter); }
        finally { release.Set(); }
        Assert.IsTrue(await owner);
        source.OnLoad = cancellation.Cancel;
        using var after = new CancellationTokenSource();
        source.OnLoad = after.Cancel;
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.RefreshAsync(after.Token));
    }

    public sealed class Box { public int Value { get; set; } }
    private sealed class ReloadSource : Microsoft.Extensions.Configuration.ConfigurationProvider, IDisposable
    {
        public Dictionary<string, string?> Values { get; } = [];
        public bool Fail { get; set; }
        public bool Disposed { get; private set; }
        public int Loads { get; private set; }
        public Action? OnLoad { get; set; }
        public override void Load()
        {
            Loads++;
            OnLoad?.Invoke();
            if (Fail) throw new InvalidOperationException("reload failure");
            Data = new Dictionary<string, string?>(Values, StringComparer.OrdinalIgnoreCase);
        }
        public void Dispose() => Disposed = true;
    }
}
