using System.Collections;
using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Tests.Secrets.Abstractions;

[TestClass]
public sealed class SecretProviderContractTests
{
    [TestMethod]
    public async Task BatchPreservesMissingEmptyAndWhitespaceValues()
    {
        var values = new Dictionary<string, string?>
        {
            ["missing"] = null,
            ["empty"] = "",
            ["whitespace"] = "  ",
            ["found"] = "secret"
        };
        ISecretProvider provider = new DelegateProvider((name, token) => Task.FromResult(values[name]));

        IDictionary<string, string?> result = await provider.GetMultipleAsync(values.Keys);

        result.Should().Equal(values);
    }

    [TestMethod]
    public async Task EmptyBatchReturnsIndependentMutableDictionariesWithoutRetrieval()
    {
        ISecretProvider provider = new DelegateProvider((name, token) => throw new InvalidOperationException());

        IDictionary<string, string?> first = await provider.GetMultipleAsync([]);
        IDictionary<string, string?> second = await provider.GetMultipleAsync([]);
        first.Add("added", "value");

        second.Should().BeEmpty();
        first.Should().NotBeSameAs(second);
    }

    [TestMethod]
    public async Task NullCollectionFailsWithParameterNameBeforeRetrieval()
    {
        ISecretProvider provider = new DelegateProvider((name, token) => throw new InvalidOperationException());

        Func<Task> action = () => provider.GetMultipleAsync(null!);

        await action.Should().ThrowAsync<ArgumentNullException>().WithParameterName("names");
    }

    [TestMethod]
    public async Task DuplicateNamesAreRetrievedAgainAndLastValueWins()
    {
        var calls = new List<string>();
        ISecretProvider provider = new DelegateProvider((name, token) =>
        {
            calls.Add(name);
            return Task.FromResult<string?>(calls.Count == 1 ? "old" : null);
        });

        IDictionary<string, string?> result = await provider.GetMultipleAsync(["key", "key"]);

        calls.Should().Equal("key", "key");
        result.Should().ContainSingle().Which.Value.Should().BeNull();
    }

    [TestMethod]
    public async Task KeysAreOrdinalAndNamesAreForwardedUnchanged()
    {
        string[] names = ["Key", "key", "", " ", "service:password", " a/b "];
        var calls = new List<string>();
        ISecretProvider provider = new DelegateProvider((name, token) =>
        {
            calls.Add(name);
            return Task.FromResult<string?>(name);
        });

        IDictionary<string, string?> result = await provider.GetMultipleAsync(names);

        calls.Should().Equal(names);
        result.Should().HaveCount(names.Length);
        foreach (string name in names)
        {
            result[name].Should().Be(name);
        }
    }

    [TestMethod]
    public async Task NullElementCannotBecomeADictionaryKey()
    {
        ISecretProvider provider = NullSecretProvider.Instance;
        Func<Task> action = () => provider.GetMultipleAsync([null!]);

        await action.Should().ThrowAsync<ArgumentNullException>().WithParameterName("key");
    }

    [TestMethod]
    public async Task BatchEnumeratesOnceAndDisposesEnumerator()
    {
        var names = new TrackedNames();
        ISecretProvider provider = NullSecretProvider.Instance;

        IDictionary<string, string?> result = await provider.GetMultipleAsync(names);

        names.EnumerationCount.Should().Be(1);
        names.Disposed.Should().BeTrue();
        result.Keys.Should().Equal("first", "second");
    }

    [TestMethod]
    public async Task BatchAwaitsEachRetrievalBeforeRequestingTheNext()
    {
        var pending = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = new List<string>();
        ISecretProvider provider = new DelegateProvider((name, token) =>
        {
            calls.Add(name);
            return name == "first" ? pending.Task : Task.FromResult<string?>("second-value");
        });

        Task<IDictionary<string, string?>> batch = provider.GetMultipleAsync(["first", "second"]);
        calls.Should().Equal("first");
        batch.IsCompleted.Should().BeFalse();
        pending.SetResult("first-value");
        IDictionary<string, string?> result = await batch;

        calls.Should().Equal("first", "second");
        result["first"].Should().Be("first-value");
        result["second"].Should().Be("second-value");
    }

    [TestMethod]
    public async Task BatchForwardsExactTokenToEveryRetrieval()
    {
        using var cancellation = new CancellationTokenSource();
        var tokens = new List<CancellationToken>();
        ISecretProvider provider = new DelegateProvider((name, token) =>
        {
            tokens.Add(token);
            return Task.FromResult<string?>(null);
        });

        await provider.GetMultipleAsync(["first", "second"], cancellation.Token);

        tokens.Should().Equal(cancellation.Token, cancellation.Token);
    }

    [TestMethod]
    public async Task CanceledProviderTaskStopsBatchAndPreservesToken()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var names = new TrackedNames();
        int calls = 0;
        ISecretProvider provider = new DelegateProvider((name, token) =>
        {
            calls++;
            return Task.FromCanceled<string?>(token);
        });

        Func<Task> action = () => provider.GetMultipleAsync(names, cancellation.Token);
        var exception = await action.Should().ThrowAsync<OperationCanceledException>();

        exception.Which.CancellationToken.Should().Be(cancellation.Token);
        calls.Should().Be(1);
        names.Disposed.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ProviderFailuresPropagateWithoutContinuingAndDisposeEnumeration(bool asynchronous)
    {
        var failure = new InvalidOperationException("provider failure");
        var names = new TrackedNames();
        int calls = 0;
        ISecretProvider provider = new DelegateProvider((name, token) =>
        {
            calls++;
            return asynchronous ? Task.FromException<string?>(failure) : throw failure;
        });

        Func<Task> action = () => provider.GetMultipleAsync(names);
        var exception = await action.Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Should().BeSameAs(failure);
        calls.Should().Be(1);
        names.Disposed.Should().BeTrue();
    }

    [TestMethod]
    public async Task EnumerationFailurePropagatesAfterSuccessfulRetrieval()
    {
        var failure = new InvalidOperationException("enumeration failure");
        var names = new TrackedNames(failure);
        var calls = new List<string>();
        ISecretProvider provider = new DelegateProvider((name, token) =>
        {
            calls.Add(name);
            return Task.FromResult<string?>("value");
        });

        Func<Task> action = () => provider.GetMultipleAsync(names);
        var exception = await action.Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Should().BeSameAs(failure);
        calls.Should().Equal("first");
        names.Disposed.Should().BeTrue();
    }

    private sealed class DelegateProvider(Func<string, CancellationToken, Task<string?>> retrieve) : ISecretProvider
    {
        public Task<string?> GetAsync(string name, CancellationToken cancellationToken = default) =>
            retrieve(name, cancellationToken);
    }

    private sealed class TrackedNames(Exception? failure = null) : IEnumerable<string>
    {
        public int EnumerationCount { get; private set; }
        public bool Disposed { get; private set; }

        public IEnumerator<string> GetEnumerator()
        {
            EnumerationCount++;
            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<string> Enumerate()
        {
            try
            {
                yield return "first";
                if (failure is not null)
                {
                    throw failure;
                }

                yield return "second";
            }
            finally
            {
                Disposed = true;
            }
        }
    }
}
