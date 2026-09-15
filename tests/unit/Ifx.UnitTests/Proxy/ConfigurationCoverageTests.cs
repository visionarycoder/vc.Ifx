using System.Text.Json;
using Ifx.Proxy.Interceptor.Configuration;
using Ifx.Proxy.Interceptor.Configuration.Local;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using IfxConfigurationProvider = Ifx.Proxy.Interceptor.Configuration.ConfigurationProvider;

namespace Ifx.Tests.Proxy;

[TestClass]
public sealed class ConfigurationCoverageTests
{
    [TestMethod]
    public async Task BaseAsyncHelpersDelegateToSynchronousMembersAndTrimPrefixes()
    {
        using var provider = new StubConfigurationProvider(new StubConfigurationOptions { KeyPrefix = "App", CacheExpiration = TimeSpan.FromMinutes(1) });

        (await provider.GetValueAsync("App:Value", new ConfigSetting())).Name.Should().Be("configured");
        (await provider.GetSectionAsync<ConfigSetting>("App:Section")).Name.Should().Be("section");
        (await provider.SetValueAsync("App:Value", new ConfigSetting { Name = "updated" })).Should().BeTrue();
        (await provider.GetValueAsync("App:Value", new ConfigSetting())).Name.Should().Be("updated");

        IDictionary<string, object?> values = await provider.GetAllValuesAsync();
        values["Value"].Should().Be(JsonSerializer.Serialize(new ConfigSetting { Name = "updated" }));
        values.Should().ContainKey("Section:Name").WhoseValue.Should().Be("section");
        values.Should().ContainKey(string.Empty);
        values.Should().NotContainKey("Other:Ignored");

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => provider.GetValueAsync("App:Value", new ConfigSetting(), cancellation.Token));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => provider.SetValueAsync("App:Value", new ConfigSetting(), cancellation.Token));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => provider.GetSectionAsync<ConfigSetting>("App:Section", cancellation.Token));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => provider.GetAllValuesAsync(cancellation.Token));
    }

    [TestMethod]
    public void BaseConstructorRejectsNullOptions()
    {
        Action action = () => new StubConfigurationProvider(null!);

        action.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [TestMethod]
    public async Task LocalProviderCoversAdditionalFilesAvailabilityAndReadOnlyPaths()
    {
        using var files = new LocalFiles();
        files.WriteMain("main");
        files.WriteOverlay("overlay");
        using var provider = new LocalConfigurationProvider(new LocalConfigurationProviderOptions
        {
            BasePath = files.DirectoryPath,
            FilePath = "settings.json",
            AdditionalFiles = ["overlay.json"],
            ReloadOnChange = true,
            EnableCaching = true
        }, NullLogger<LocalConfigurationProvider>.Instance);

        provider.ProviderName.Should().Be("Local");
        provider.IsAvailable.Should().BeTrue();
        provider.GetValue("Value", new ConfigSetting()).Name.Should().Be("main");
        provider.GetValue("Value", new ConfigSetting()).Name.Should().Be("main");
        provider.GetSection<ConfigSetting>("Nested").Name.Should().Be("overlay");
        provider.GetValue("Missing", new ConfigSetting { Name = "fallback" }).Name.Should().Be("fallback");
        provider.GetSection<ConfigSetting>("Missing").Name.Should().BeEmpty();
        provider.Refresh().Should().BeTrue();
        (await provider.RefreshAsync()).Should().BeTrue();
        File.Delete(files.MainPath);
        provider.IsAvailable.Should().BeFalse();
        provider.GetValue("Value", new ConfigSetting()).Name.Should().Be("main");
        Assert.ThrowsExactly<NotSupportedException>(() => provider.SetValue("Value", new ConfigSetting()));
        Assert.ThrowsExactly<NotSupportedException>(() => provider.UpdateSection("Nested", new ConfigSetting()));
    }

    [TestMethod]
    public void LocalProviderWithoutPrefixSupportsOptionalAvailabilityAndFailedReload()
    {
        using var files = new LocalFiles();
        files.WriteMain("root");
        var provider = new LocalConfigurationProvider(new LocalConfigurationProviderOptions
        {
            BasePath = files.DirectoryPath,
            FilePath = "settings.json",
            KeyPrefix = null,
            Optional = true,
            EnableCaching = false,
            ReloadOnChange = false
        }, NullLogger<LocalConfigurationProvider>.Instance);

        provider.IsAvailable.Should().BeTrue();
        provider.GetValue("Value", new ConfigSetting()).Name.Should().Be("root");
        File.Delete(files.MainPath);
        provider.IsAvailable.Should().BeTrue();
        File.WriteAllText(files.MainPath, "invalid-json");
        provider.Refresh().Should().BeFalse();
        provider.Dispose();
        provider.IsAvailable.Should().BeFalse();
    }

    [TestMethod]
    public void LocalProviderOptionsValidationRejectsFileAndAdditionalFileFailures()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => new LocalConfigurationProviderOptions { FilePath = "" }.Validate());
        Assert.ThrowsExactly<InvalidOperationException>(() => new LocalConfigurationProviderOptions { AdditionalFiles = null! }.Validate());
        Assert.ThrowsExactly<InvalidOperationException>(() => new LocalConfigurationProviderOptions { AdditionalFiles = [" "] }.Validate());
    }

    [TestMethod]
    public void LocalProviderFullKeyReturnsOriginalKeyForNullOrEmptyPrefixes()
    {
        using var files = new LocalFiles();
        files.WriteMain("root");
        using var nullPrefix = new LocalConfigurationProvider(new LocalConfigurationProviderOptions
        {
            BasePath = files.DirectoryPath,
            FilePath = "settings.json",
            KeyPrefix = null
        }, NullLogger<LocalConfigurationProvider>.Instance);
        using var emptyPrefix = new LocalConfigurationProvider(new LocalConfigurationProviderOptions
        {
            BasePath = files.DirectoryPath,
            FilePath = "settings.json",
            KeyPrefix = string.Empty
        }, NullLogger<LocalConfigurationProvider>.Instance);
        using var prefixed = new LocalConfigurationProvider(new LocalConfigurationProviderOptions
        {
            BasePath = files.DirectoryPath,
            FilePath = "settings.json",
            KeyPrefix = "App:"
        }, NullLogger<LocalConfigurationProvider>.Instance);

        InvokeGetFullKey(nullPrefix, "Value").Should().Be("Value");
        InvokeGetFullKey(emptyPrefix, "Value").Should().Be("Value");
        InvokeGetFullKey(prefixed, "Value").Should().Be("App:Value");
    }

    private sealed class StubConfigurationOptions : ConfigurationProviderOptions;

    private sealed class StubConfigurationProvider : IfxConfigurationProvider
    {
        public StubConfigurationProvider(StubConfigurationOptions options)
            : base(options, NullLogger<IfxConfigurationProvider>.Instance)
        {
            configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["App"] = null,
                ["App:Value"] = JsonSerializer.Serialize(new ConfigSetting { Name = "configured" }),
                ["App:Section:Name"] = "section",
                ["Other:Ignored"] = "ignored"
            }).Build();
        }

        public override T GetValue<T>(string key, T defaultValue) =>
            configuration[key] is string text
                ? JsonSerializer.Deserialize<T>(text) ?? defaultValue
                : defaultValue;

        public override bool SetValue<T>(string key, T value)
        {
            configuration[key] = JsonSerializer.Serialize(value);
            return true;
        }

        public override T GetSection<T>(string sectionName) =>
            configuration.GetSection(sectionName).Get<T>() ?? new T();

        public override Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Refresh());
        }
    }

    private sealed class LocalFiles : IDisposable
    {
        public LocalFiles()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), "ifx-local-config-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirectoryPath);
        }

        public string DirectoryPath { get; }

        public string MainPath => Path.Combine(DirectoryPath, "settings.json");

        public void WriteMain(string value) =>
            File.WriteAllText(MainPath, JsonSerializer.Serialize(new
            {
                Value = JsonSerializer.Serialize(new ConfigSetting { Name = value })
            }));

        public void WriteOverlay(string value) =>
            File.WriteAllText(Path.Combine(DirectoryPath, "overlay.json"), JsonSerializer.Serialize(new
            {
                Nested = new ConfigSetting { Name = value }
            }));

        public void Dispose() => Directory.Delete(DirectoryPath, recursive: true);
    }

    public sealed class ConfigSetting
    {
        public string Name { get; set; } = string.Empty;
    }

    private static string InvokeGetFullKey(LocalConfigurationProvider provider, string key) => (string)typeof(LocalConfigurationProvider)
        .GetMethod("GetFullKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
        .Invoke(provider, [key])!;
}
