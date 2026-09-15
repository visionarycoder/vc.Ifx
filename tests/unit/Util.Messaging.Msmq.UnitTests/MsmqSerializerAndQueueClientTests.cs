using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Experimental.System.Messaging;
using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util.Messaging.Msmq;

namespace Util.Messaging.Msmq.UnitTests;

[TestClass]
public sealed class MsmqSerializerAndQueueClientTests
{
    #region Serializer Tests

    [TestMethod]
    public void Serialize_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        Action action = () => _ = MsmqEnvelopeSerializer.Serialize(null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("message");
    }

    [TestMethod]
    public void Serialize_WhenCorrelationIdIsInvalid_ShouldWriteNullEnvelopeCorrelationId()
    {
        TestMessage message = new() { CorrelationId = "not-a-guid" };

        SerializedMsmqMessage serializedMessage = MsmqEnvelopeSerializer.Serialize(message);
        MessageEnvelope envelope = MsmqEnvelopeSerializer.Deserialize(serializedMessage.Body);

        serializedMessage.CorrelationId.Should().BeNull();
        serializedMessage.Label.Should().Be(typeof(TestMessage).FullName);
        envelope.Message.Should().BeEquivalentTo(message);
        envelope.CorrelationId.Should().BeNull();
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Deserialize_WhenBodyIsMissing_ShouldThrowArgumentException(string? body)
    {
        Action action = () => _ = MsmqEnvelopeSerializer.Deserialize(body!);

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("body");
    }

    [TestMethod]
    public void Deserialize_WhenPayloadIsNull_ShouldThrowInvalidOperationException()
    {
        Action action = () => _ = MsmqEnvelopeSerializer.Deserialize("null");

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("MSMQ message payload could not be deserialized.");
    }

    [TestMethod]
    public void Deserialize_WhenMessageTypeCannotBeResolved_ShouldThrowInvalidOperationException()
    {
        string payload =
            """
            {
              "messageId": "f5738ec7-e9c9-41a5-9364-c6993c2588db",
              "correlationId": null,
              "enqueuedAtUtc": "2026-09-15T12:00:00Z",
              "messageType": "Missing.Type, Missing.Assembly",
              "message": {}
            }
            """;

        Action action = () => _ = MsmqEnvelopeSerializer.Deserialize(payload);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Unable to resolve message type 'Missing.Type, Missing.Assembly'.");
    }

    [TestMethod]
    public void Deserialize_WhenResolvedTypeDoesNotImplementIMessage_ShouldThrowInvalidOperationException()
    {
        string payload =
            """
            {
              "messageId": "f5738ec7-e9c9-41a5-9364-c6993c2588db",
              "correlationId": null,
              "enqueuedAtUtc": "2026-09-15T12:00:00Z",
              "messageType": "System.String, System.Private.CoreLib",
              "message": "hello"
            }
            """;

        Action action = () => _ = MsmqEnvelopeSerializer.Deserialize(payload);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Resolved message type 'System.String, System.Private.CoreLib' does not implement IMessage.");
    }

    #endregion

    #region Queue Client Tests

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task SendAsync_WhenQueuePathIsMissing_ShouldThrowArgumentException(string? queuePath)
    {
        MsmqQueueClient client = new();
        Func<Task> action = () => client.SendAsync(queuePath!, new SerializedMsmqMessage("{}", null, "label"), CancellationToken.None);

        var assertions = await action.Should().ThrowAsync<ArgumentException>();
        assertions.Which.ParamName.Should().Be("queuePath");
    }

    [TestMethod]
    public async Task SendAsync_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        MsmqQueueClient client = new();
        Func<Task> action = () => client.SendAsync(@".\Private$\orders", null!, CancellationToken.None);

        var assertions = await action.Should().ThrowAsync<ArgumentNullException>();
        assertions.Which.ParamName.Should().Be("message");
    }

    [TestMethod]
    public async Task SendAsync_WhenCancellationIsAlreadyRequested_ShouldThrowOperationCanceledException()
    {
        MsmqQueueClient client = new();
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        Func<Task> action = () => client.SendAsync(
            @".\Private$\orders",
            new SerializedMsmqMessage("{}", null, "label"),
            cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task ReceiveAsync_WhenQueuePathIsMissing_ShouldThrowArgumentException()
    {
        MsmqQueueClient client = new();
        Func<Task> action = async () => _ = await client.ReceiveAsync("", TimeSpan.FromMilliseconds(1), CancellationToken.None);

        var assertions = await action.Should().ThrowAsync<ArgumentException>();
        assertions.Which.ParamName.Should().Be("queuePath");
    }

    [TestMethod]
    public async Task ReceiveAsync_WhenTimeoutIsNotPositive_ShouldThrowArgumentOutOfRangeException()
    {
        MsmqQueueClient client = new();
        Func<Task> action = async () => _ = await client.ReceiveAsync(@".\Private$\orders", TimeSpan.Zero, CancellationToken.None);

        var assertions = await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
        assertions.Which.ParamName.Should().Be("receiveTimeout");
    }

    [TestMethod]
    public async Task ReceiveAsync_WhenCancellationIsAlreadyRequested_ShouldThrowOperationCanceledException()
    {
        MsmqQueueClient client = new();
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        Func<Task> action = async () => _ = await client.ReceiveAsync(@".\Private$\orders", TimeSpan.FromMilliseconds(1), cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public void CreateMessage_WhenCorrelationIdIsProvided_ShouldStampBodyLabelAndCorrelationId()
    {
        SerializedMsmqMessage serializedMessage = new("""{"value":"hello"}""", Guid.NewGuid().ToString("D"), "orders");

        using Message message = InvokePrivateStatic<Message>("CreateMessage", serializedMessage);
        using StreamReader reader = new(message.BodyStream!, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        message.BodyStream!.Position = 0;

        reader.ReadToEnd().Should().Be("""{"value":"hello"}""");
        message.Label.Should().Be("orders");
        message.Recoverable.Should().BeTrue();
        message.CorrelationId.Should().Be($"{serializedMessage.CorrelationId}\\0");
    }

    [TestMethod]
    public void CreateMessage_WhenCorrelationIdAlreadyContainsTerminator_ShouldPreserveValue()
    {
        SerializedMsmqMessage serializedMessage = new("""{"value":"hello"}""", $"{Guid.NewGuid():D}\\0", "orders");

        using Message message = InvokePrivateStatic<Message>("CreateMessage", serializedMessage);

        message.CorrelationId.Should().Be(serializedMessage.CorrelationId);
    }

    [TestMethod]
    public void CreateMessage_WhenCorrelationIdIsMissing_ShouldLeaveCorrelationIdUnset()
    {
        SerializedMsmqMessage serializedMessage = new("""{"value":"hello"}""", " ", "orders");

        using Message message = InvokePrivateStatic<Message>("CreateMessage", serializedMessage);

        message.Label.Should().Be("orders");
        string.IsNullOrWhiteSpace(message.CorrelationId).Should().BeTrue();
    }

    [TestMethod]
    public void ReadBody_WhenBodyStreamExists_ShouldReadMessageBody()
    {
        using Message message = new()
        {
            BodyStream = new MemoryStream(Encoding.UTF8.GetBytes("hello"))
        };

        string body = InvokePrivateStatic<string>("ReadBody", message);

        body.Should().Be("hello");
    }

    #endregion

    private static T InvokePrivateStatic<T>(string methodName, params object?[] arguments)
    {
        MethodInfo method = typeof(MsmqQueueClient).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Method '{methodName}' was not found.");

        try
        {
            return (T)method.Invoke(null, arguments)!;
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private sealed record TestMessage : MessageBase
    {
        public string? Value { get; init; }
    }
}
