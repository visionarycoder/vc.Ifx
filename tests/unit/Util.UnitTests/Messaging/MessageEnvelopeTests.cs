using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util.Messaging;

namespace Util.UnitTests.Messaging;

[TestClass]
public sealed class MessageEnvelopeTests
{
    #region Create Tests

    [TestMethod]
    public void Create_WhenMessageIsProvided_ShouldStampEnvelope()
    {
        TestMessage message = new();
        Guid correlationId = Guid.NewGuid();
        DateTimeOffset before = DateTimeOffset.UtcNow;

        MessageEnvelope envelope = MessageEnvelope.Create(message, correlationId);

        DateTimeOffset after = DateTimeOffset.UtcNow;

        envelope.Message.Should().BeSameAs(message);
        envelope.CorrelationId.Should().Be(correlationId);
        envelope.MessageId.Should().NotBe(Guid.Empty);
        envelope.EnqueuedAtUtc.Should().BeOnOrAfter(before);
        envelope.EnqueuedAtUtc.Should().BeOnOrBefore(after);
    }

    [TestMethod]
    public void Create_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        Action action = () => MessageEnvelope.Create(null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("message");
    }

    #endregion

    private sealed record TestMessage : IMessage
    {
        public string MessageId { get; init; } = Guid.NewGuid().ToString();

        public string? CorrelationId { get; init; }

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
