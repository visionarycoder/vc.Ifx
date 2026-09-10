using Azure.Core;
using Microsoft.Extensions.Logging.Abstractions;
using VisionaryCoder.Framework.Data.Azure.Table;

namespace VisionaryCoder.Framework.Tests.Data.AzureTables;

[TestClass]
public sealed class OptionsTests
{
    [TestMethod]
    public void DefaultsAndSdkPolicyMappingAreExplicit()
    {
        var options = new AzureTableStorageOptions { TableName = "Valid123", ConnectionString = "UseDevelopmentStorage=true" };
        options.Validate();
        Assert.IsTrue(options.CreateTableIfNotExists);
        Assert.IsTrue(options.EnableOptimisticConcurrency);
        Assert.IsFalse(options.UseManagedIdentity);
        Assert.IsNull(options.StorageAccountUri);
        Assert.AreEqual(100, options.MaxEntitiesPerBatch);
        Assert.AreEqual(1000, options.MaxEntitiesPerQuery);
        var sdk = options.CreateClientOptions();
        Assert.AreEqual(RetryMode.Exponential, sdk.Retry.Mode);
        Assert.AreEqual(3, sdk.Retry.MaxRetries);
        Assert.AreEqual(TimeSpan.FromSeconds(1), sdk.Retry.Delay);
        Assert.AreEqual(TimeSpan.FromSeconds(30), sdk.Retry.NetworkTimeout);
        Assert.AreNotSame(sdk, options.CreateClientOptions());
        var custom = new AzureTableStorageOptions { TableName = new string('A', 63), ConnectionString = "fake", MaxRetryAttempts = 0, RetryDelayMilliseconds = 0, TimeoutMilliseconds = 1, MaxEntitiesPerBatch = 1, MaxEntitiesPerQuery = 1 };
        sdk = custom.CreateClientOptions();
        Assert.AreEqual(0, sdk.Retry.MaxRetries);
        Assert.AreEqual(TimeSpan.Zero, sdk.Retry.Delay);
        Assert.AreEqual(TimeSpan.FromMilliseconds(1), sdk.Retry.NetworkTimeout);
    }

    [TestMethod]
    public void InvalidNamesCredentialsAndRangesAreRejected()
    {
        foreach (var name in new[] { "", " ", "ab", new string('a', 64), "1abc", "a-b", "a_b", "T\u00e9st", "TABLES", "Valid\n" })
            Assert.Throws<ArgumentException>(() => new AzureTableStorageOptions { TableName = name, ConnectionString = "fake" }.Validate());
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureTableStorageOptions { TableName = null!, ConnectionString = "fake" }.Validate());
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureTableStorageOptions { TableName = "Valid" }.Validate());
        Assert.ThrowsExactly<ArgumentException>(() => new AzureTableStorageOptions { TableName = "Valid", ConnectionString = " " }.Validate());
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureTableStorageOptions { TableName = "Valid", UseManagedIdentity = true }.Validate());
        foreach (var uri in new[] { " ", "relative", "http://account/", "ftp://account/", "https://user:pass@account/", "https://account/?secret=x", "https://account/#fragment" })
            Assert.Throws<ArgumentException>(() => new AzureTableStorageOptions { TableName = "Valid", UseManagedIdentity = true, StorageAccountUri = uri }.Validate());
        var invalid = new AzureTableStorageOptions[]
        {
            new() { TableName = "Valid", ConnectionString = "fake", TimeoutMilliseconds = 0 },
            new() { TableName = "Valid", ConnectionString = "fake", MaxEntitiesPerQuery = 0 },
            new() { TableName = "Valid", ConnectionString = "fake", MaxEntitiesPerQuery = 1001 },
            new() { TableName = "Valid", ConnectionString = "fake", MaxEntitiesPerBatch = 0 },
            new() { TableName = "Valid", ConnectionString = "fake", MaxEntitiesPerBatch = 101 },
            new() { TableName = "Valid", ConnectionString = "fake", MaxRetryAttempts = -1 },
            new() { TableName = "Valid", ConnectionString = "fake", RetryDelayMilliseconds = -1 }
        };
        foreach (var options in invalid) Assert.ThrowsExactly<ArgumentOutOfRangeException>(options.Validate);
    }

    [TestMethod]
    public void ConstructorsAreNetworkFreeAndValidateCollaborators()
    {
        var logger = NullLogger<AzureTableStorageProvider>.Instance;
        using var connection = new AzureTableStorageProvider(TableFixture.Options(create: true), logger);
        using var identity = new AzureTableStorageProvider(new() { TableName = "Entities", UseManagedIdentity = true, StorageAccountUri = "https://account.table.core.windows.net" }, logger);
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureTableStorageProvider(null!, logger));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureTableStorageProvider(null!, logger, new ServiceFake(new())));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureTableStorageProvider(TableFixture.Options(), null!, new ServiceFake(new())));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureTableStorageProvider(TableFixture.Options(), logger, null!));
        Assert.ThrowsExactly<ArgumentException>(() => new AzureTableStorageProvider(TableFixture.Options(), logger, new ServiceFake(new()) { ReturnNull = true }));
        Assert.Throws<Exception>(() => new AzureTableStorageProvider(new() { TableName = "Valid", ConnectionString = "malformed" }, logger));
    }
}
