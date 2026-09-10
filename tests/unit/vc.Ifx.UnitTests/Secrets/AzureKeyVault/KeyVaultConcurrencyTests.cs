using Azure;
using Azure.Security.KeyVault.Secrets;
using VisionaryCoder.Framework.Secrets.Azure.KeyVault;

namespace VisionaryCoder.Framework.Tests.Secrets.AzureKeyVault;

[TestClass]
public sealed class KeyVaultConcurrencyTests
{
    [TestMethod]
    public async Task ConcurrentDefaultTokenMissesShareOneSdkOperation()
    {
        using var context = new KeyVaultTestContext();
        var completion = new TaskCompletionSource<Response<KeyVaultSecret>>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.Client.Retrieve = (name, version, token) => completion.Task;
        KeyVaultSecretProvider provider = context.Create();

        Task<string?>[] calls = Enumerable.Range(0, 20).Select(index => provider.GetAsync("name")).ToArray();
        context.Client.Calls.Should().ContainSingle();
        completion.SetResult(KeyVaultTestContext.Response("shared"));

        (await Task.WhenAll(calls)).Should().OnlyContain(value => value == "shared");
        (await provider.GetAsync("name")).Should().Be("shared");
        context.Client.Calls.Should().ContainSingle();
    }

    [TestMethod]
    public async Task SharedFailureIsRemovedSoSubsequentRequestCanRecover()
    {
        using var context = new KeyVaultTestContext();
        var completion = new TaskCompletionSource<Response<KeyVaultSecret>>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.Client.Retrieve = (name, version, token) => completion.Task;
        KeyVaultSecretProvider provider = context.Create();
        Task<string?> first = provider.GetAsync("name");
        Task<string?> second = provider.GetAsync("name");
        var failure = new InvalidOperationException("sdk failure");
        completion.SetException(failure);

        Func<Task> firstAction = () => first;
        Func<Task> secondAction = () => second;
        (await firstAction.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(failure);
        (await secondAction.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(failure);
        context.Client.Retrieve = (name, version, token) => Task.FromResult(KeyVaultTestContext.Response("recovered"));
        (await provider.GetAsync("name")).Should().Be("recovered");
        context.Client.Calls.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task DifferentKeysDoNotSerializeOneAnother()
    {
        using var context = new KeyVaultTestContext();
        var completion = new TaskCompletionSource<Response<KeyVaultSecret>>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.Client.Retrieve = (name, version, token) => name == "slow"
            ? completion.Task : Task.FromResult(KeyVaultTestContext.Response("fast"));
        KeyVaultSecretProvider provider = context.Create();
        Task<string?> slow = provider.GetAsync("slow");

        (await provider.GetAsync("fast")).Should().Be("fast");
        slow.IsCompleted.Should().BeFalse();
        completion.SetResult(KeyVaultTestContext.Response("slow"));
        (await slow).Should().Be("slow");
    }

    [TestMethod]
    public async Task CanceledTokenWinsBeforeCacheHitOrValidation()
    {
        using var context = new KeyVaultTestContext();
        KeyVaultSecretProvider provider = context.Create();
        await provider.GetAsync("name");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Func<Task> hit = () => provider.GetAsync("name", cancellation.Token);
        Func<Task> invalid = () => provider.GetAsync(null!, cancellation.Token);
        (await hit.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(cancellation.Token);
        await invalid.Should().ThrowAsync<OperationCanceledException>();
        context.Client.Calls.Should().ContainSingle();
    }

    [TestMethod]
    public async Task CancelableSdkOperationReceivesTokenAndDoesNotCancelOtherCallers()
    {
        using var context = new KeyVaultTestContext();
        using var cancellation = new CancellationTokenSource();
        var shared = new TaskCompletionSource<Response<KeyVaultSecret>>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.Client.Retrieve = async (name, version, token) =>
        {
            if (token.CanBeCanceled)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
            }

            return await shared.Task;
        };
        KeyVaultSecretProvider provider = context.Create();
        Task<string?> uncanceled = provider.GetAsync("name");
        Task<string?> canceled = provider.GetAsync("name", cancellation.Token);
        cancellation.Cancel();

        Func<Task> action = () => canceled;
        (await action.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(cancellation.Token);
        uncanceled.IsCompleted.Should().BeFalse();
        shared.SetResult(KeyVaultTestContext.Response("success"));
        (await uncanceled).Should().Be("success");
        context.Client.Calls.Should().HaveCount(2);
        context.Client.Calls.Last().Token.Should().Be(cancellation.Token);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task CancellationAfterSdkCompletionWinsOverSuccessOr404(bool missing)
    {
        using var context = new KeyVaultTestContext();
        using var cancellation = new CancellationTokenSource();
        context.Client.Retrieve = (name, version, token) =>
        {
            cancellation.Cancel();
            return missing ? Task.FromException<Response<KeyVaultSecret>>(new RequestFailedException(404, "missing"))
                : Task.FromResult(KeyVaultTestContext.Response());
        };
        Func<Task> action = () => context.Create().GetAsync("name", cancellation.Token);

        (await action.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(cancellation.Token);
    }

    [TestMethod]
    public async Task BatchPreservesDuplicatesMissingEmptyAndOrdinalKeys()
    {
        using var context = new KeyVaultTestContext();
        context.Options.CacheTtl = TimeSpan.Zero;
        context.Client.Retrieve = (name, version, token) => name == "missing"
            ? throw new RequestFailedException(404, "missing")
            : Task.FromResult(KeyVaultTestContext.Response(name == "empty" ? "" : name));
        KeyVaultSecretProvider provider = context.Create();

        IDictionary<string, string?> values = await provider.GetMultipleAsync(["name", "NAME", "missing", "empty", "name"]);

        values.Keys.Should().Equal("name", "NAME", "missing", "empty");
        values["missing"].Should().BeNull();
        values["empty"].Should().BeEmpty();
        values["name"].Should().Be("name");
        context.Client.Calls.Should().HaveCount(5);
        (await provider.GetMultipleAsync([])).Should().BeEmpty();
        Func<Task> nullInput = () => provider.GetMultipleAsync(null!);
        await nullInput.Should().ThrowAsync<ArgumentNullException>().WithParameterName("names");
    }

    [TestMethod]
    public async Task BatchFailureStopsBeforeLaterRequests()
    {
        using var context = new KeyVaultTestContext();
        context.Client.Retrieve = (name, version, token) => throw new RequestFailedException(403, "forbidden");
        Func<Task> action = () => context.Create().GetMultipleAsync(["first", "second"]);

        await action.Should().ThrowAsync<RequestFailedException>();
        context.Client.Calls.Should().ContainSingle().Which.Name.Should().Be("first");
    }
}
