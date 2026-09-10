using Azure;
using VisionaryCoder.Framework.Messaging.Azure.Queue;

namespace VisionaryCoder.Framework.Tests.Messaging.AzureQueues;

[TestClass]
public sealed class LifecycleTests
{
    private static Action[] SyncOperations(AzureQueueStorageProvider provider) =>
    [
        () => provider.QueueExists(), () => provider.SendMessage("text"), () => provider.SendMessage(new { Value = 1 }),
        () => provider.ReceiveMessages(), () => provider.PeekMessages(), () => provider.DeleteMessage("id", "pop"),
        () => provider.UpdateMessage("id", "pop"), () => provider.UpdateMessageWithReceipt("id", "pop"),
        () => provider.GetMessageCount(), provider.ClearMessages
    ];

    private static Func<Task>[] AsyncOperations(AzureQueueStorageProvider provider, CancellationToken token = default) =>
    [
        () => provider.QueueExistsAsync(token), () => provider.SendMessageAsync("text", token), () => provider.SendMessageAsync(new { Value = 1 }, token),
        () => provider.ReceiveMessagesAsync(cancellationToken: token), () => provider.PeekMessagesAsync(cancellationToken: token),
        () => provider.DeleteMessageAsync("id", "pop", token), () => provider.UpdateMessageAsync("id", "pop", cancellationToken: token),
        () => provider.UpdateMessageWithReceiptAsync("id", "pop", cancellationToken: token), () => provider.GetMessageCountAsync(token),
        () => provider.ClearMessagesAsync(token)
    ];

    [TestMethod]
    public async Task ExistsDoesNotCreateAndInitializationIsSharedAcrossSyncAndAsync()
    {
        foreach (bool asyncFirst in new[] { false, true })
        {
            var fake = new QueueFake { ExistsResult = false };
            using var provider = QueueFixture.Provider(fake, QueueFixture.Options(create: true));
            Assert.IsFalse(provider.QueueExists());
            Assert.IsFalse(await provider.QueueExistsAsync());
            Assert.IsFalse(fake.Calls.Any(call => call.Operation == "create"));
            fake.ExistsResult = true;
            Assert.IsTrue(provider.QueueExists());
            Assert.IsTrue(await provider.QueueExistsAsync());
            if (asyncFirst) await provider.ClearMessagesAsync(); else provider.ClearMessages();
            provider.SendMessage("text");
            await provider.SendMessageAsync("text");
            Assert.AreEqual(1, fake.Calls.Count(call => call.Operation == "create"));
        }
    }

    [TestMethod]
    public async Task ConcurrentInitializationRunsOnceAndWaiterCancellationDoesNotCancelOwner()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var fake = new QueueFake { Initialize = async token => { entered.SetResult(); await release.Task.WaitAsync(token); } };
        using var provider = QueueFixture.Provider(fake, QueueFixture.Options(create: true));
        Task owner = provider.SendMessageAsync("owner");
        await entered.Task;
        using var cancellation = new CancellationTokenSource();
        Task canceled = provider.ClearMessagesAsync(cancellation.Token);
        Task second = provider.SendMessageAsync("second");
        Task synchronous = Task.Run(() => provider.SendMessage("sync"));
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => canceled);
        release.SetResult();
        await Task.WhenAll(owner, second, synchronous);
        Assert.AreEqual(1, fake.Calls.Count(call => call.Operation == "create"));
        Assert.AreEqual(3, fake.Calls.Count(call => call.Operation == "send"));
        Assert.IsFalse(fake.Calls.Any(call => call.Operation == "clear"));
    }

    [TestMethod]
    public async Task InitializationFailureAndCancellationReleaseGateForLaterAttempt()
    {
        var fake = new QueueFake { Failure = new RequestFailedException(403, "denied") };
        using var provider = QueueFixture.Provider(fake, QueueFixture.Options(create: true));
        Assert.ThrowsExactly<RequestFailedException>(() => provider.ClearMessages());
        await Assert.ThrowsExactlyAsync<RequestFailedException>(() => provider.ClearMessagesAsync());
        using var cancellation = new CancellationTokenSource();
        fake.Failure = null;
        fake.Initialize = token => { Assert.AreEqual(cancellation.Token, token); cancellation.Cancel(); return Task.CompletedTask; };
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.ClearMessagesAsync(cancellation.Token));
        fake.Initialize = null;
        await provider.ClearMessagesAsync();
        provider.ClearMessages();
        Assert.AreEqual(4, fake.Calls.Count(call => call.Operation == "create"));
        Assert.AreEqual(2, fake.Calls.Count(call => call.Operation == "clear"));
    }

    [TestMethod]
    public async Task AllSdkFailuresPropagateWithoutProviderRetriesOrAcknowledgements()
    {
        foreach (int status in new[] { 404, 403, 409, 412, 429, 503 })
        {
            var failure = new RequestFailedException(status, "failure");
            var fake = new QueueFake { Failure = failure };
            using var provider = QueueFixture.Provider(fake);
            foreach (var operation in SyncOperations(provider))
                Assert.AreSame(failure, Assert.ThrowsExactly<RequestFailedException>(operation));
            foreach (var operation in AsyncOperations(provider))
                Assert.AreSame(failure, await Assert.ThrowsExactlyAsync<RequestFailedException>(operation));
            Assert.AreEqual(20, fake.Calls.Count);
            Assert.AreEqual(2, fake.Calls.Count(call => call.Operation == "delete"));
        }
    }

    [TestMethod]
    public async Task PreCanceledOperationsNeverCallSdkAndCompletionCancellationIsObserved()
    {
        var fake = new QueueFake();
        using var provider = QueueFixture.Provider(fake, QueueFixture.Options(create: true));
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        foreach (var operation in AsyncOperations(provider, canceled.Token))
            await Assert.ThrowsAsync<OperationCanceledException>(operation);
        Assert.AreEqual(0, fake.Calls.Count);
        for (int index = 0; index < 10; index++)
        {
            using var cancellation = new CancellationTokenSource();
            var completed = new QueueFake { OnCall = call => { Assert.AreEqual(cancellation.Token, call.Token); cancellation.Cancel(); } };
            using var noCreate = QueueFixture.Provider(completed);
            await Assert.ThrowsAsync<OperationCanceledException>(AsyncOperations(noCreate, cancellation.Token)[index]);
            Assert.AreEqual(1, completed.Calls.Count);
        }
    }

    [TestMethod]
    public async Task DisposalIsIdempotentAndAllOperationsRejectUseAfterDisposal()
    {
        var fake = new QueueFake();
        var provider = QueueFixture.Provider(fake);
        provider.Dispose(); provider.Dispose();
        foreach (var operation in SyncOperations(provider)) Assert.ThrowsExactly<ObjectDisposedException>(operation);
        foreach (var operation in AsyncOperations(provider)) await Assert.ThrowsExactlyAsync<ObjectDisposedException>(operation);
        Assert.AreEqual(0, fake.Calls.Count);
        Assert.IsTrue(fake.Exists().Value);
    }
}
