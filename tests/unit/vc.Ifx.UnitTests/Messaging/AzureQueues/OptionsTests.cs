using Azure.Core;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging.Abstractions;
using VisionaryCoder.Framework.Messaging.Azure.Queue;

namespace VisionaryCoder.Framework.Tests.Messaging.AzureQueues;

[TestClass]
public sealed class OptionsTests
{
    [TestMethod]
    public void DefaultsAndSdkSettingsAreMapped()
    {
        var options = new AzureQueueStorageOptions { QueueName = "abc", ConnectionString = "UseDevelopmentStorage=true" };
        options.Validate();
        Assert.IsNull(options.StorageAccountUri);
        Assert.IsFalse(options.UseManagedIdentity);
        Assert.IsTrue(options.CreateQueueIfNotExists);
        Assert.AreEqual(604800, options.MessageTimeToLiveSeconds);
        Assert.AreEqual(30, options.VisibilityTimeoutSeconds);
        Assert.AreEqual(32, options.MaxMessagesToRetrieve);
        var sdk = options.CreateClientOptions();
        Assert.AreEqual(QueueMessageEncoding.Base64, sdk.MessageEncoding);
        Assert.AreEqual(RetryMode.Exponential, sdk.Retry.Mode);
        Assert.AreEqual(3, sdk.Retry.MaxRetries);
        Assert.AreEqual(TimeSpan.FromSeconds(1), sdk.Retry.Delay);
        Assert.AreEqual(TimeSpan.FromSeconds(30), sdk.Retry.NetworkTimeout);
        Assert.AreNotSame(sdk, options.CreateClientOptions());
        sdk = new AzureQueueStorageOptions { QueueName = new string('0', 63), ConnectionString = "fake", EncodeMessages = false, MaxRetryAttempts = 0, RetryDelayMilliseconds = 0, TimeoutMilliseconds = 1, MessageTimeToLiveSeconds = 1, VisibilityTimeoutSeconds = 1, MaxMessagesToRetrieve = 1 }.CreateClientOptions();
        Assert.AreEqual(QueueMessageEncoding.None, sdk.MessageEncoding);
        Assert.AreEqual(0, sdk.Retry.MaxRetries);
        Assert.AreEqual(TimeSpan.Zero, sdk.Retry.Delay);
        Assert.AreEqual(TimeSpan.FromMilliseconds(1), sdk.Retry.NetworkTimeout);
        new AzureQueueStorageOptions { QueueName = "a-0-b", ConnectionString = "fake", MessageTimeToLiveSeconds = -1, VisibilityTimeoutSeconds = 604800, MaxRetryAttempts = 10 }.Validate();
    }

    [TestMethod]
    public void InvalidNamesCredentialsAndRangesAreRejected()
    {
        foreach (string? name in new[] { null, "", " ", "ab", new string('a', 64), "-abc", "abc-", "a--b", "Abc", "a_b", "ab\u00e9", "ab\u0660", "valid\n" })
            Assert.Throws<ArgumentException>(() => new AzureQueueStorageOptions { QueueName = name!, ConnectionString = "fake" }.Validate());
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureQueueStorageOptions { QueueName = "abc" }.Validate());
        Assert.ThrowsExactly<ArgumentException>(() => new AzureQueueStorageOptions { QueueName = "abc", ConnectionString = " " }.Validate());
        foreach (string? uri in new[] { null, " ", "relative", "http://account/", "https://user:pass@account/", "https://account/?sig=secret", "https://account/#fragment" })
            Assert.Throws<ArgumentException>(() => new AzureQueueStorageOptions { QueueName = "abc", UseManagedIdentity = true, StorageAccountUri = uri }.Validate());
        AzureQueueStorageOptions[] invalid =
        [
            new() { QueueName = "abc", ConnectionString = "fake", TimeoutMilliseconds = 0 },
            new() { QueueName = "abc", ConnectionString = "fake", MessageTimeToLiveSeconds = -2 },
            new() { QueueName = "abc", ConnectionString = "fake", MessageTimeToLiveSeconds = 0 },
            new() { QueueName = "abc", ConnectionString = "fake", VisibilityTimeoutSeconds = 0 },
            new() { QueueName = "abc", ConnectionString = "fake", VisibilityTimeoutSeconds = 604801 },
            new() { QueueName = "abc", ConnectionString = "fake", MaxMessagesToRetrieve = 0 },
            new() { QueueName = "abc", ConnectionString = "fake", MaxMessagesToRetrieve = 33 },
            new() { QueueName = "abc", ConnectionString = "fake", MaxRetryAttempts = -1 },
            new() { QueueName = "abc", ConnectionString = "fake", MaxRetryAttempts = 11 },
            new() { QueueName = "abc", ConnectionString = "fake", RetryDelayMilliseconds = -1 }
        ];
        foreach (var options in invalid) Assert.ThrowsExactly<ArgumentOutOfRangeException>(options.Validate);
    }

    [TestMethod]
    public void LegacyAndInjectedConstructorsValidateWithoutNetworkCalls()
    {
        var logger = NullLogger<AzureQueueStorageProvider>.Instance;
        using var connection = new AzureQueueStorageProvider(QueueFixture.Options(create: true), logger);
        using var identity = new AzureQueueStorageProvider(new() { QueueName = "abc", UseManagedIdentity = true, StorageAccountUri = "https://example.queue.core.windows.net/" }, logger);
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureQueueStorageProvider(null!, logger));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureQueueStorageProvider(null!, logger, new QueueFake()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureQueueStorageProvider(QueueFixture.Options(), null!, new QueueFake()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AzureQueueStorageProvider(QueueFixture.Options(), logger, null!));
        Assert.ThrowsExactly<FormatException>(() => new AzureQueueStorageProvider(new() { QueueName = "bad", ConnectionString = "invalid-connection" }, logger));
        var fake = new QueueFake();
        using var injected = QueueFixture.Provider(fake, QueueFixture.Options(create: true));
        Assert.AreEqual(0, fake.Calls.Count);
    }
}
