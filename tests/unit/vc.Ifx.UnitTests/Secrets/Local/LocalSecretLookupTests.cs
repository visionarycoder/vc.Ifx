using Microsoft.Extensions.Configuration;
using Moq;
using VisionaryCoder.Framework.Secrets;
using VisionaryCoder.Framework.Secrets.Azure.KeyVault;
using VisionaryCoder.Framework.Secrets.Local;

namespace VisionaryCoder.Framework.Tests.Secrets.Local;

[TestClass]
public sealed class LocalSecretLookupTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(":Secrets")]
    [DataRow("Secrets:")]
    [DataRow("Service::Secrets")]
    [DataRow("Service: :Secrets")]
    [DataRow("Secrets\0suffix")]
    public void ConstructorRejectsInvalidPrefixes(string? prefix)
    {
        var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
        Action action = () => new LocalSecretProvider(configuration.Object,
            new KeyVaultOptions { LocalSecretsPrefix = prefix! });

        action.Should().Throw<ArgumentException>().WithParameterName("LocalSecretsPrefix");
        configuration.VerifyNoOtherCalls();
    }

    [TestMethod]
    [DataRow(":Password")]
    [DataRow("Password:")]
    [DataRow("Database::Password")]
    [DataRow("Database: :Password")]
    [DataRow("Password\0suffix")]
    public async Task InvalidNamesFailBeforeAnySourceRead(string name)
    {
        var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
        var provider = new LocalSecretProvider(configuration.Object, new KeyVaultOptions());

        Func<Task> action = () => provider.GetAsync(name);

        await action.Should().ThrowAsync<ArgumentException>().WithParameterName("name");
        configuration.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task HierarchicalPrefixAndNameUseConfigurationComparison()
    {
        using ConfigurationRoot configuration = BuildConfiguration(new()
        {
            ["Services:Billing:Secrets:Database:Password"] = "nested-value"
        });
        var provider = new LocalSecretProvider(configuration,
            new KeyVaultOptions { LocalSecretsPrefix = "Services:Billing:Secrets" });

        (await provider.GetAsync("database:password")).Should().Be("nested-value");
    }

    [TestMethod]
    public async Task NonemptySegmentsAreNotTrimmedOrRewritten()
    {
        using ConfigurationRoot configuration = BuildConfiguration(new()
        {
            [" Custom Prefix : service :password__literal"] = "unchanged",
            ["Custom Prefix:service:password:literal"] = "wrong"
        });
        var provider = new LocalSecretProvider(configuration,
            new KeyVaultOptions { LocalSecretsPrefix = " Custom Prefix " });

        (await provider.GetAsync(" service :password__literal")).Should().Be("unchanged");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("value")]
    public async Task PrefixedValuesIncludingEmptyPreventFallback(string value)
    {
        string name = $"IFX_LOCAL_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(name, "environment");
        try
        {
            using ConfigurationRoot configuration = BuildConfiguration(new()
            {
                [$"Secrets:{name}"] = value,
                [name] = "direct"
            });
            var provider = new LocalSecretProvider(configuration, new KeyVaultOptions());

            (await provider.GetAsync(name)).Should().Be(value);
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("value")]
    public async Task NullPrefixedValueFallsThroughToDirectValueIncludingEmpty(string value)
    {
        string name = $"IFX_LOCAL_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(name, "environment");
        try
        {
            using ConfigurationRoot configuration = BuildConfiguration(new()
            {
                [$"Secrets:{name}"] = null,
                [name] = value
            });
            var provider = new LocalSecretProvider(configuration, new KeyVaultOptions());

            (await provider.GetAsync(name)).Should().Be(value);
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [TestMethod]
    public async Task MissingSecretReturnsNull()
    {
        using ConfigurationRoot configuration = BuildConfiguration([]);
        var provider = new LocalSecretProvider(configuration, new KeyVaultOptions());

        (await provider.GetAsync($"IFX_MISSING_{Guid.NewGuid():N}")).Should().BeNull();
    }

    [TestMethod]
    public async Task EnvironmentFallbackUsesExactHierarchicalNameAndReadsCurrentValue()
    {
        string name = $"IFX_LOCAL_{Guid.NewGuid():N}:Password";
        string alias = name.Replace(":", "__", StringComparison.Ordinal);
        Environment.SetEnvironmentVariable(alias, "must-not-alias");
        Environment.SetEnvironmentVariable(name, "first");
        try
        {
            using ConfigurationRoot configuration = BuildConfiguration([]);
            var provider = new LocalSecretProvider(configuration, new KeyVaultOptions());
            (await provider.GetAsync(name)).Should().Be("first");

            Environment.SetEnvironmentVariable(name, "second");
            (await provider.GetAsync(name)).Should().Be("second");

            Environment.SetEnvironmentVariable(name, null);
            (await provider.GetAsync(name)).Should().BeNull();
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
            Environment.SetEnvironmentVariable(alias, null);
        }
    }

    [TestMethod]
    public async Task ConfigurationMutationIsVisibleWithoutProviderRecreation()
    {
        using ConfigurationRoot configuration = BuildConfiguration(new()
        {
            ["Secrets:Password"] = "first",
            ["Password"] = "fallback"
        });
        var provider = new LocalSecretProvider(configuration, new KeyVaultOptions());
        (await provider.GetAsync("Password")).Should().Be("first");

        configuration["Secrets:Password"] = "second";
        (await provider.GetAsync("Password")).Should().Be("second");

        configuration["Secrets:Password"] = null;
        (await provider.GetAsync("Password")).Should().Be("fallback");
    }

    [TestMethod]
    public async Task ReloadReadsReplacedSourceDataAndRemovesOldSecrets()
    {
        string name = $"IFX_RELOAD_{Guid.NewGuid():N}";
        var values = new Dictionary<string, string?> { [$"Secrets:{name}"] = "first" };
        var source = new ReloadableSource(values);
        using var configuration = (ConfigurationRoot)new ConfigurationBuilder().Add(source).Build();
        var provider = new LocalSecretProvider(configuration, new KeyVaultOptions());
        (await provider.GetAsync(name)).Should().Be("first");

        values[$"Secrets:{name}"] = "reloaded";
        configuration.Reload();
        (await provider.GetAsync(name)).Should().Be("reloaded");

        values.Clear();
        configuration.Reload();
        (await provider.GetAsync(name)).Should().BeNull();
    }

    [TestMethod]
    public async Task MutableLegacyPrefixIsReadAndValidatedOnEachCall()
    {
        using ConfigurationRoot configuration = BuildConfiguration(new()
        {
            ["Secrets:Password"] = "original",
            ["Other:Secrets:Password"] = "changed"
        });
        var options = new KeyVaultOptions();
        var provider = new LocalSecretProvider(configuration, options);
        (await provider.GetAsync("Password")).Should().Be("original");

        options.LocalSecretsPrefix = "Other:Secrets";
        (await provider.GetAsync("Password")).Should().Be("changed");

        options.LocalSecretsPrefix = "Other::Secrets";
        Func<Task> action = () => provider.GetAsync("Password");
        await action.Should().ThrowAsync<ArgumentException>().WithParameterName("LocalSecretsPrefix");
    }

    [TestMethod]
    [DataRow(false, null)]
    [DataRow(false, "value")]
    [DataRow(true, null)]
    [DataRow(true, "value")]
    public async Task CancellationDuringSourceReadPreventsFurtherLookup(bool direct, string? value)
    {
        using var cancellation = new CancellationTokenSource();
        var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
        if (direct)
        {
            configuration.SetupGet(config => config["Secrets:Password"]).Returns((string?)null);
        }

        string key = direct ? "Password" : "Secrets:Password";
        configuration.SetupGet(config => config[key]).Returns(() =>
        {
            cancellation.Cancel();
            return value;
        });
        var provider = new LocalSecretProvider(configuration.Object, new KeyVaultOptions());

        Func<Task> action = () => provider.GetAsync("Password", cancellation.Token);
        var exception = await action.Should().ThrowAsync<OperationCanceledException>();

        exception.Which.CancellationToken.Should().Be(cancellation.Token);
        configuration.VerifyGet(config => config[key], Times.Once);
        if (direct)
        {
            configuration.VerifyGet(config => config["Secrets:Password"], Times.Once);
        }

        configuration.VerifyNoOtherCalls();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ConfigurationFailuresPropagateWithoutFallback(bool direct)
    {
        var failure = new InvalidOperationException("configuration failure");
        var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
        if (direct)
        {
            configuration.SetupGet(config => config["Secrets:Password"]).Returns((string?)null);
        }

        string key = direct ? "Password" : "Secrets:Password";
        configuration.SetupGet(config => config[key]).Throws(failure);
        var provider = new LocalSecretProvider(configuration.Object, new KeyVaultOptions());

        Func<Task> action = () => provider.GetAsync("Password");
        var exception = await action.Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Should().BeSameAs(failure);
    }

    [TestMethod]
    public async Task DefaultBatchRetainsNullsEmptyValuesAndLastDuplicate()
    {
        string missing = $"IFX_MISSING_{Guid.NewGuid():N}";
        using ConfigurationRoot configuration = BuildConfiguration(new()
        {
            ["Secrets:Database:Password"] = "password",
            ["Secrets:Empty"] = ""
        });
        ISecretProvider provider = new LocalSecretProvider(configuration, new KeyVaultOptions());

        IDictionary<string, string?> results = await provider.GetMultipleAsync(
            ["Database:Password", "Empty", missing, "Empty"]);

        results.Should().HaveCount(3);
        results["Database:Password"].Should().Be("password");
        results["Empty"].Should().BeEmpty();
        results[missing].Should().BeNull();
    }

    private static ConfigurationRoot BuildConfiguration(Dictionary<string, string?> values) =>
        (ConfigurationRoot)new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private sealed class ReloadableSource(Dictionary<string, string?> values) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new ReloadableProvider(values);
    }

    private sealed class ReloadableProvider(Dictionary<string, string?> values) : ConfigurationProvider
    {
        public override void Load() => Data = new Dictionary<string, string?>(values, StringComparer.OrdinalIgnoreCase);
    }
}
