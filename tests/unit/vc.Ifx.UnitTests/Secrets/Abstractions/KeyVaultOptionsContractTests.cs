using VisionaryCoder.Framework.Secrets.Azure.KeyVault;

namespace VisionaryCoder.Framework.Tests.Secrets.Abstractions;

[TestClass]
public sealed class KeyVaultOptionsContractTests
{
    [TestMethod]
    public void DefaultsRemainCompatible()
    {
        var options = new KeyVaultOptions();

        options.VaultUri.Should().BeNull();
        options.CacheTtl.Should().Be(TimeSpan.FromMinutes(15));
        options.UseLocalSecrets.Should().BeFalse();
        options.LocalSecretsPrefix.Should().Be("Secrets");
        options.MaxRetries.Should().Be(3);
        options.RetryDelay.Should().Be(TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void SettersPreserveAssignedValuesWithoutNormalization()
    {
        var vaultUri = new Uri("https://example.vault.azure.net/");
        var options = new KeyVaultOptions
        {
            VaultUri = vaultUri,
            CacheTtl = TimeSpan.FromMinutes(2),
            UseLocalSecrets = true,
            LocalSecretsPrefix = "Service:Secrets",
            MaxRetries = 5,
            RetryDelay = TimeSpan.FromSeconds(3)
        };

        options.VaultUri.Should().BeSameAs(vaultUri);
        options.CacheTtl.Should().Be(TimeSpan.FromMinutes(2));
        options.UseLocalSecrets.Should().BeTrue();
        options.LocalSecretsPrefix.Should().Be("Service:Secrets");
        options.MaxRetries.Should().Be(5);
        options.RetryDelay.Should().Be(TimeSpan.FromSeconds(3));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void PassiveOptionsLeaveProviderValidationToProviderStage(string? prefix)
    {
        var options = new KeyVaultOptions
        {
            VaultUri = new Uri("relative", UriKind.Relative),
            CacheTtl = TimeSpan.FromTicks(-1),
            LocalSecretsPrefix = prefix!,
            MaxRetries = -1,
            RetryDelay = TimeSpan.FromTicks(-1)
        };

        options.VaultUri.IsAbsoluteUri.Should().BeFalse();
        options.CacheTtl.Should().Be(TimeSpan.FromTicks(-1));
        options.LocalSecretsPrefix.Should().Be(prefix);
        options.MaxRetries.Should().Be(-1);
        options.RetryDelay.Should().Be(TimeSpan.FromTicks(-1));
        options.VaultUri = null;
        options.VaultUri.Should().BeNull();
    }

    [TestMethod]
    public void InstancesAreIndependentAndUseReferenceEquality()
    {
        var first = new KeyVaultOptions();
        var second = new KeyVaultOptions();

        first.Should().NotBe(second);
        first.Should().Be(first);
        first.MaxRetries = 99;
        second.MaxRetries.Should().Be(3);
        typeof(KeyVaultOptions).Assembly.GetName().Name.Should().Be("vc.Ifx.Secrets.Abstractions");
    }
}
