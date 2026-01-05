using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Tests.Secrets;

[TestClass]
public class NullSecretProviderTests
{
    [TestMethod]
    public void Instance_ShouldReturnSameInstanceEveryTime()
    {
        // Act
        NullSecretProvider instance1 = NullSecretProvider.Instance;
        NullSecretProvider instance2 = NullSecretProvider.Instance;
        NullSecretProvider instance3 = NullSecretProvider.Instance;

        // Assert
        instance1.Should().BeSameAs(instance2, "Instance should be a singleton");
        instance2.Should().BeSameAs(instance3, "Instance should be a singleton");
    }

    [TestMethod]
    public void Instance_ShouldNotBeNull()
    {
        // Act
        NullSecretProvider instance = NullSecretProvider.Instance;

        // Assert
        instance.Should().NotBeNull("singleton instance should always be available");
    }

    [TestMethod]
    public async Task GetAsync_WithAnyName_ShouldReturnNull()
    {
        // Arrange
        NullSecretProvider provider = NullSecretProvider.Instance;

        // Act
        string? result = await provider.GetAsync("any-secret-name");

        // Assert
        result.Should().BeNull("NullSecretProvider always returns null");
    }

    [TestMethod]
    [DataRow("ApiKey")]
    [DataRow("ConnectionString")]
    [DataRow("DatabasePassword")]
    [DataRow("")]
    [DataRow(" ")]
    public async Task GetAsync_WithVariousNames_ShouldAlwaysReturnNull(string secretName)
    {
        // Arrange
        NullSecretProvider provider = NullSecretProvider.Instance;

        // Act
        string? result = await provider.GetAsync(secretName);

        // Assert
        result.Should().BeNull($"NullSecretProvider should return null for '{secretName}'");
    }

    [TestMethod]
    public async Task GetAsync_WithCancellationToken_ShouldStillReturnNull()
    {
        // Arrange
        NullSecretProvider provider = NullSecretProvider.Instance;
        using var cts = new CancellationTokenSource();

        // Act
        string? result = await provider.GetAsync("secret-name", cts.Token);

        // Assert
        result.Should().BeNull("NullSecretProvider returns null regardless of cancellation token");
    }

    [TestMethod]
    public async Task GetAsync_WithCanceledToken_ShouldNotThrow()
    {
        // Arrange
        NullSecretProvider provider = NullSecretProvider.Instance;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await provider.GetAsync("secret-name", cts.Token);

        // Assert
        await act.Should().NotThrowAsync("NullSecretProvider doesn't check cancellation");
    }

    [TestMethod]
    public async Task GetAsync_CalledMultipleTimes_ShouldAlwaysReturnNull()
    {
        // Arrange
        NullSecretProvider provider = NullSecretProvider.Instance;
        string secretName = "test-secret";

        // Act
        string? result1 = await provider.GetAsync(secretName);
        string? result2 = await provider.GetAsync(secretName);
        string? result3 = await provider.GetAsync(secretName);

        // Assert
        result1.Should().BeNull();
        result2.Should().BeNull();
        result3.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAsync_MultipleConcurrentCalls_ShouldAllReturnNull()
    {
        // Arrange
        NullSecretProvider provider = NullSecretProvider.Instance;
        var tasks = new List<Task<string?>>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(provider.GetAsync($"secret-{i}"));
        }
        string?[] results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBe(null, "all results should be null");
    }

    [TestMethod]
    public async Task GetAsync_WithNullName_ShouldReturnNull()
    {
        // Arrange
        NullSecretProvider provider = NullSecretProvider.Instance;

        // Act
        string? result = await provider.GetAsync(null!);

        // Assert
        result.Should().BeNull("NullSecretProvider doesn't validate input");
    }

    [TestMethod]
    public void NullSecretProvider_ShouldImplementISecretProvider()
    {
        // Arrange
        NullSecretProvider provider = NullSecretProvider.Instance;

        // Assert
        provider.Should().BeAssignableTo<ISecretProvider>();
    }
}
