using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using VisionaryCoder.Framework.Proxy.Interceptors.Configuration.Local;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class LocalConfigurationContractTests
{
    [TestMethod]
    public async Task ExplicitRefreshReloadsFilesAndKeepsPrefixBoundaries()
    {
        using var files = new Files();
        files.Write("first");
        using var provider = files.Provider(prefix: "App:");
        provider.ProviderName.Should().Be("Local");
        provider.IsAvailable.Should().BeTrue();
        provider.GetValue("Value", new Setting()).Name.Should().Be("first");
        provider.GetSection<Setting>("Nested").Name.Should().Be("first");
        provider.GetValue("missing", new Setting { Name = "default" }).Name.Should().Be("default");
        provider.GetSection<Setting>("missing").Name.Should().BeEmpty();
        files.Write("second");
        provider.GetValue("Value", new Setting()).Name.Should().Be("first");
        provider.Refresh().Should().BeTrue();
        provider.GetValue("Value", new Setting()).Name.Should().Be("second");
        files.Write("third");
        (await provider.RefreshAsync()).Should().BeTrue();
        (await provider.GetValueAsync("Value", new Setting())).Name.Should().Be("third");
        (await provider.GetSectionAsync<Setting>("Nested")).Name.Should().Be("third");
        (await provider.GetAllValuesAsync()).Should().ContainKey("Value").And.NotContainKey("Wrong");
        provider.GetAllValues().Keys.Should().NotContain(key => key.Contains("AppExtra", StringComparison.Ordinal));
        Func<Task> canceled = () => provider.GetAllValuesAsync(new CancellationToken(true));
        await canceled.Should().ThrowAsync<OperationCanceledException>();
        Func<Task> canceledRefresh = () => provider.RefreshAsync(new CancellationToken(true));
        await canceledRefresh.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task ProviderIsReadOnlyAndValidatesDisposedAndCanceledCalls()
    {
        using var files = new Files();
        files.Write("value");
        var provider = files.Provider();
        try
        {
            Action set = () => provider.SetValue("key", new Setting());
            set.Should().Throw<NotSupportedException>();
            Action update = () => provider.UpdateSection("key", new Setting());
            update.Should().Throw<NotSupportedException>();
            Func<Task> setAsync = () => provider.SetValueAsync("key", new Setting());
            await setAsync.Should().ThrowAsync<NotSupportedException>();
            foreach (Func<Task> call in new Func<Task>[]
            {
                () => provider.GetValueAsync("key", new Setting(), new CancellationToken(true)),
                () => provider.GetSectionAsync<Setting>("key", new CancellationToken(true)),
                () => provider.SetValueAsync("key", new Setting(), new CancellationToken(true))
            })
                await call.Should().ThrowAsync<OperationCanceledException>();
            Action missingKey = () => provider.GetValue("", new Setting());
            missingKey.Should().Throw<ArgumentException>();
            Action missingSection = () => provider.GetSection<Setting>("");
            missingSection.Should().Throw<ArgumentException>();
        }
        finally { provider.Dispose(); }
        provider.Dispose();
        provider.IsAvailable.Should().BeFalse();
        Action disposed = () => provider.Refresh();
        disposed.Should().Throw<ObjectDisposedException>();
    }

    [TestMethod]
    public void OptionalFilesOverlaysAndDisabledCacheHaveExplicitBehavior()
    {
        using var files = new Files();
        using (var optional = files.Provider(optional: true))
            optional.IsAvailable.Should().BeTrue();
        files.Write("main");
        string overlay = Path.Combine(files.Directory, "overlay.json");
        File.WriteAllText(overlay, "{\"App\":{\"Nested\":{\"Name\":\"overlay\"}}}");
        using var provider = new LocalConfigurationProvider(new LocalConfigurationProviderOptions
        {
            BasePath = files.Directory, FilePath = files.Path, AdditionalFiles = [overlay],
            ReloadOnChange = false, EnableCaching = false
        }, NullLogger<LocalConfigurationProvider>.Instance);
        provider.GetSection<Setting>("App:Nested").Name.Should().Be("overlay");
        provider.GetValue("App:Value", new Setting()).Name.Should().Be("main");
        File.WriteAllText(files.Path, "invalid-json");
        provider.Refresh().Should().BeFalse();
    }

    [TestMethod]
    public void ConstructionAndOptionsRejectInvalidConfiguration()
    {
        using var files = new Files();
        Action missing = () => files.Provider();
        missing.Should().Throw<FileNotFoundException>();
        Action nullOptions = () => new LocalConfigurationProvider(null!, NullLogger<LocalConfigurationProvider>.Instance);
        nullOptions.Should().Throw<ArgumentNullException>();
        Action nullLogger = () => new LocalConfigurationProvider(new LocalConfigurationProviderOptions(), null!);
        nullLogger.Should().Throw<ArgumentNullException>();
        foreach (var options in new[]
        {
            new LocalConfigurationProviderOptions { FilePath = "" },
            new LocalConfigurationProviderOptions { AdditionalFiles = null! },
            new LocalConfigurationProviderOptions { AdditionalFiles = [""] },
            new LocalConfigurationProviderOptions { CacheExpiration = TimeSpan.Zero }
        })
        {
            Action invalid = () => options.Validate();
            invalid.Should().Throw<InvalidOperationException>();
        }
    }

    [TestMethod]
    public async Task CacheExpiresAndReloadRegistrationCanBeDisposed()
    {
        using var files = new Files();
        files.Write("initial");
        using var provider = new LocalConfigurationProvider(new LocalConfigurationProviderOptions
        {
            BasePath = files.Directory, FilePath = "settings.json", KeyPrefix = "App",
            CacheExpiration = TimeSpan.FromMilliseconds(1), ReloadOnChange = true
        }, NullLogger<LocalConfigurationProvider>.Instance);
        provider.GetValue("Value", new Setting()).Name.Should().Be("initial");
        provider.configuration["App:Value"] = JsonSerializer.Serialize(new Setting { Name = "updated" });
        await Task.Delay(20);
        provider.GetValue("Value", new Setting()).Name.Should().Be("updated");
        provider.Refresh().Should().BeTrue();
        provider.GetValue("Value", new Setting()).Name.Should().Be("initial");
    }

    public sealed class Setting
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class Files : IDisposable
    {
        public string Directory { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ifx-config-" + Guid.NewGuid().ToString("N"));
        public string Path => System.IO.Path.Combine(Directory, "settings.json");
        public Files() => System.IO.Directory.CreateDirectory(Directory);
        public void Write(string value) => File.WriteAllText(Path, JsonSerializer.Serialize(new
        {
            App = new { Value = JsonSerializer.Serialize(new Setting { Name = value }), Nested = new Setting { Name = value } },
            AppExtra = new { Wrong = "outside-prefix" }
        }));
        public LocalConfigurationProvider Provider(string? prefix = "App", bool optional = false) => new(new LocalConfigurationProviderOptions
        {
            BasePath = Directory, FilePath = "settings.json", KeyPrefix = prefix, Optional = optional, ReloadOnChange = false
        }, NullLogger<LocalConfigurationProvider>.Instance);
        public void Dispose() => System.IO.Directory.Delete(Directory, recursive: true);
    }
}
