using Azure;
using Azure.Storage.Queues.Models;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace VisionaryCoder.Framework.Tests.Messaging.AzureQueues;

[TestClass]
public sealed class OperationTests
{
    [TestMethod]
    public async Task SendsPreserveTextAndSerializeObjectsWithConfiguredLifetime()
    {
        var fake = new QueueFake();
        using var provider = QueueFixture.Provider(fake);
        using var cancellation = new CancellationTokenSource();
        provider.SendMessage("a<&\u00e9");
        await provider.SendMessageAsync("async", cancellation.Token);
        provider.SendMessage(new { Value = 42 });
        await provider.SendMessageAsync(new { Value = "async" }, cancellation.Token);
        CollectionAssert.AreEqual(new[] { "a<&\u00e9", "async", "{\"Value\":42}", "{\"Value\":\"async\"}" }, fake.Calls.Select(call => call.Text).ToArray());
        Assert.IsTrue(fake.Calls.All(call => call.Lifetime == TimeSpan.FromSeconds(-1) && call.Visibility is null));
        Assert.AreEqual(cancellation.Token, fake.Calls[1].Token);
        Assert.AreEqual(cancellation.Token, fake.Calls[3].Token);
        using var finite = QueueFixture.Provider(fake, QueueFixture.Options(ttl: 123));
        finite.SendMessage("finite");
        await finite.SendMessageAsync("finite");
        Assert.AreEqual(TimeSpan.FromSeconds(123), fake.Calls[^1].Lifetime);
        Assert.AreEqual(TimeSpan.FromSeconds(123), fake.Calls[^2].Lifetime);
    }

    [TestMethod]
    public async Task PayloadBoundariesAreUtf8AndIncludeBase64Expansion()
    {
        var fake = new QueueFake();
        using var encoded = QueueFixture.Provider(fake);
        encoded.SendMessage(new string('x', 49152));
        await encoded.SendMessageAsync(new string('\u00e9', 24576));
        Assert.ThrowsExactly<ArgumentException>(() => encoded.SendMessage(new string('x', 49153)));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => encoded.SendMessageAsync(new string('\u00e9', 24577)));
        Assert.ThrowsExactly<EncoderFallbackException>(() => encoded.SendMessage("\ud800"));
        encoded.SendMessage("\0");
        using var raw = QueueFixture.Provider(fake, QueueFixture.Options(encode: false));
        raw.SendMessage(new string('x', 65536));
        await raw.SendMessageAsync(new string('\u00e9', 32768));
        Assert.ThrowsExactly<ArgumentException>(() => raw.SendMessage(new string('x', 65537)));
        Assert.ThrowsExactly<XmlException>(() => raw.SendMessage("invalid\0"));
        foreach (string? text in new[] { null, "", " \t" })
        {
            Assert.Throws<ArgumentException>(() => raw.SendMessage(text!));
            await Assert.ThrowsAsync<ArgumentException>(() => raw.SendMessageAsync(text!));
        }
        Assert.ThrowsExactly<ArgumentNullException>(() => encoded.SendMessage<object>(null!));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => encoded.SendMessageAsync<object>(null!));
        var cycle = new Cycle(); cycle.Next = cycle;
        Assert.ThrowsExactly<JsonException>(() => encoded.SendMessage(cycle));
        await Assert.ThrowsExactlyAsync<JsonException>(() => encoded.SendMessageAsync(cycle));
        Assert.AreEqual(5, fake.Calls.Count);
    }

    [TestMethod]
    public async Task ReceiveAndPeekPreserveSdkBodiesReceiptsCountsAndEmptyResults()
    {
        var message = QueuesModelFactory.QueueMessage("id", "pop", new BinaryData("decoded"), 9, DateTimeOffset.UnixEpoch);
        var peeked = QueuesModelFactory.PeekedMessage("id", new BinaryData("decoded"), 9);
        var fake = new QueueFake { Messages = [message], Peeked = [peeked], Count = 12 };
        using var provider = QueueFixture.Provider(fake);
        using var cancellation = new CancellationTokenSource();
        Assert.AreSame(fake.Messages, provider.ReceiveMessages());
        Assert.AreSame(fake.Messages, await provider.ReceiveMessagesAsync(32, cancellation.Token));
        Assert.AreSame(fake.Peeked, provider.PeekMessages(1));
        Assert.AreSame(fake.Peeked, await provider.PeekMessagesAsync(cancellationToken: cancellation.Token));
        CollectionAssert.AreEqual(new int?[] { 7, 32, 1, 7 }, fake.Calls.Select(call => call.Count).ToArray());
        Assert.AreEqual(TimeSpan.FromSeconds(17), fake.Calls[0].Visibility);
        Assert.AreEqual(TimeSpan.FromSeconds(17), fake.Calls[1].Visibility);
        Assert.AreEqual(cancellation.Token, fake.Calls[1].Token);
        Assert.AreEqual(cancellation.Token, fake.Calls[3].Token);
        Assert.AreEqual(12, provider.GetMessageCount());
        Assert.AreEqual(12, await provider.GetMessageCountAsync(cancellation.Token));
        fake.Messages = []; fake.Peeked = []; fake.Count = 0;
        Assert.AreEqual(0, provider.ReceiveMessages(1).Length);
        Assert.AreEqual(0, (await provider.ReceiveMessagesAsync()).Length);
        Assert.AreEqual(0, provider.PeekMessages().Length);
        Assert.AreEqual(0, (await provider.PeekMessagesAsync(32)).Length);
        Assert.AreEqual(0, provider.GetMessageCount());
        Assert.AreEqual(0, await provider.GetMessageCountAsync());
        Assert.IsFalse(fake.Calls.Any(call => call.Operation == "delete"));
    }

    [TestMethod]
    public async Task UpdatesReturnRenewedReceiptAndDeleteRequiresExplicitAcknowledgement()
    {
        var fake = new QueueFake();
        using var provider = QueueFixture.Provider(fake);
        using var cancellation = new CancellationTokenSource();
        Assert.AreSame(fake.Receipt, provider.UpdateMessageWithReceipt("id", "old"));
        Assert.AreSame(fake.Receipt, await provider.UpdateMessageWithReceiptAsync("id", "old", "", TimeSpan.FromDays(7), cancellation.Token));
        provider.UpdateMessage("id", "old", "changed", TimeSpan.FromSeconds(1));
        await provider.UpdateMessageAsync("id", "old", cancellationToken: cancellation.Token);
        Assert.IsNull(fake.Calls[0].Text);
        Assert.AreEqual(TimeSpan.Zero, fake.Calls[0].Visibility);
        Assert.AreEqual("", fake.Calls[1].Text);
        Assert.AreEqual(TimeSpan.FromDays(7), fake.Calls[1].Visibility);
        Assert.AreEqual(cancellation.Token, fake.Calls[1].Token);
        Assert.AreEqual("changed", fake.Calls[2].Text);
        provider.DeleteMessage("id", fake.Receipt.PopReceipt);
        await provider.DeleteMessageAsync("id", "renewed", cancellation.Token);
        Assert.AreEqual("id", fake.Calls[^1].Id);
        Assert.AreEqual("renewed", fake.Calls[^1].Receipt);
        Assert.AreEqual(cancellation.Token, fake.Calls[^1].Token);
        provider.ClearMessages();
        await provider.ClearMessagesAsync(cancellation.Token);
        Assert.AreEqual("clear", fake.Calls[^1].Operation);
        Assert.AreEqual(cancellation.Token, fake.Calls[^1].Token);
    }

    [TestMethod]
    public async Task InvalidOperationArgumentsNeverInitializeOrCallSdk()
    {
        var fake = new QueueFake();
        using var provider = QueueFixture.Provider(fake, QueueFixture.Options(create: true));
        foreach (int count in new[] { 0, 33 })
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => provider.ReceiveMessages(count));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => provider.PeekMessages(count));
            await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => provider.ReceiveMessagesAsync(count));
            await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => provider.PeekMessagesAsync(count));
        }
        foreach (var pair in new (string Id, string Receipt)[] { (null!, "pop"), ("", "pop"), ("id", null!), ("id", " ") })
        {
            Assert.Throws<ArgumentException>(() => provider.DeleteMessage(pair.Id, pair.Receipt));
            await Assert.ThrowsAsync<ArgumentException>(() => provider.DeleteMessageAsync(pair.Id, pair.Receipt));
            Assert.Throws<ArgumentException>(() => provider.UpdateMessage(pair.Id, pair.Receipt));
            await Assert.ThrowsAsync<ArgumentException>(() => provider.UpdateMessageAsync(pair.Id, pair.Receipt));
        }
        foreach (var visibility in new[] { TimeSpan.FromTicks(-1), TimeSpan.FromDays(7).Add(TimeSpan.FromTicks(1)) })
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => provider.UpdateMessage("id", "pop", visibilityTimeout: visibility));
            await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => provider.UpdateMessageAsync("id", "pop", visibilityTimeout: visibility));
        }
        Assert.ThrowsExactly<ArgumentException>(() => provider.UpdateMessage("id", "pop", new string('a', 49153)));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => provider.UpdateMessageAsync("id", "pop", new string('a', 49153)));
        Assert.AreEqual(0, fake.Calls.Count);
    }

    public sealed class Cycle { public Cycle? Next { get; set; } }
}
