using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Tests.Secrets.Abstractions;

[TestClass]
public sealed class NullSecretProviderContractTests
{
    [TestMethod]
    public void InstanceIsSharedAndImplementsProviderContract()
    {
        NullSecretProvider.Instance.Should().BeSameAs(NullSecretProvider.Instance);
        NullSecretProvider.Instance.Should().BeAssignableTo<ISecretProvider>();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("service:secret")]
    public async Task RetrievalCompletesImmediatelyWithoutValidationOrCancellation(string? name)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Task<string?> pending = NullSecretProvider.Instance.GetAsync(name!, cancellation.Token);

        pending.IsCompletedSuccessfully.Should().BeTrue();
        (await pending).Should().BeNull();
    }

    [TestMethod]
    public async Task InterfaceBatchRetainsMissingValuesWithAnAlreadyCanceledToken()
    {
        ISecretProvider provider = NullSecretProvider.Instance;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        IDictionary<string, string?> result = await provider.GetMultipleAsync(
            ["key", "KEY", "key", ""], cancellation.Token);

        result.Keys.Should().Equal("key", "KEY", "");
        result.Values.Should().OnlyContain(value => value == null);
        (await provider.GetMultipleAsync([], cancellation.Token)).Should().BeEmpty();
    }

    [TestMethod]
    public async Task ConcurrentCallsShareNoMutableState()
    {
        Task<string?>[] calls = Enumerable.Range(0, 32)
            .Select(index => NullSecretProvider.Instance.GetAsync($"secret-{index}"))
            .ToArray();

        string?[] results = await Task.WhenAll(calls);

        results.Should().HaveCount(32).And.OnlyContain(value => value == null);
    }
}
