using FluentAssertions;
using VisionaryCoder.Framework.Messaging;

namespace VisionaryCoder.Framework.Tests.Messaging;

/// <summary>
/// Unit tests for IMessage and MessageBase with 100% coverage.
/// </summary>
[TestClass]
public class MessageTests
{
    [TestMethod]
    public void MessageBase_DefaultConstructor_ShouldGenerateMessageId()
    {
        // Act
        var message = new TestMessage();

        // Assert
        message.MessageId.Should().NotBeNullOrEmpty();
        Guid.TryParse(message.MessageId, out _).Should().BeTrue();
    }

    [TestMethod]
    public void MessageBase_DefaultConstructor_ShouldSetCreatedAt()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var message = new TestMessage();
        var after = DateTimeOffset.UtcNow;

        // Assert
        message.CreatedAt.Should().BeOnOrAfter(before);
        message.CreatedAt.Should().BeOnOrBefore(after);
    }

    [TestMethod]
    public void MessageBase_CorrelationId_ShouldBeNullByDefault()
    {
        // Act
        var message = new TestMessage();

        // Assert
        message.CorrelationId.Should().BeNull();
    }

    [TestMethod]
    public void MessageBase_WithCorrelationId_ShouldSetCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var message = new TestMessage
        {
            CorrelationId = correlationId
        };

        // Assert
        message.CorrelationId.Should().Be(correlationId);
    }

    [TestMethod]
    public void MessageBase_WithCustomMessageId_ShouldUseCustomId()
    {
        // Arrange
        var customId = "CUSTOM-123";

        // Act
        var message = new TestMessage
        {
            MessageId = customId
        };

        // Assert
        message.MessageId.Should().Be(customId);
    }

    [TestMethod]
    public void MessageBase_WithCustomCreatedAt_ShouldUseCustomTime()
    {
        // Arrange
        var customTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        var message = new TestMessage
        {
            CreatedAt = customTime
        };

        // Assert
        message.CreatedAt.Should().Be(customTime);
    }

    [TestMethod]
    public void MessageBase_MultipleInstances_ShouldHaveUniqueIds()
    {
        // Act
        var message1 = new TestMessage();
        var message2 = new TestMessage();
        var message3 = new TestMessage();

        // Assert
        var ids = new[] { message1.MessageId, message2.MessageId, message3.MessageId };
        ids.Should().OnlyHaveUniqueItems();
    }

    [TestMethod]
    public void MessageBase_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var correlationId = Guid.NewGuid().ToString();
        var createdAt = DateTimeOffset.UtcNow;

        var message1 = new TestMessage
        {
            MessageId = id,
            CorrelationId = correlationId,
            CreatedAt = createdAt,
            Data = "test"
        };

        var message2 = new TestMessage
        {
            MessageId = id,
            CorrelationId = correlationId,
            CreatedAt = createdAt,
            Data = "test"
        };

        var message3 = new TestMessage
        {
            MessageId = id,
            CorrelationId = correlationId,
            CreatedAt = createdAt,
            Data = "different"
        };

        // Assert
        message1.Should().Be(message2);
        message1.Should().NotBe(message3);
    }

    [TestMethod]
    public void MessageBase_With_ShouldCreateCopyWithModifications()
    {
        // Arrange
        var original = new TestMessage
        {
            MessageId = "ID1",
            CorrelationId = "CORR1",
            Data = "original"
        };

        // Act
        var modified = original with { Data = "modified" };

        // Assert
        modified.MessageId.Should().Be(original.MessageId);
        modified.CorrelationId.Should().Be(original.CorrelationId);
        modified.CreatedAt.Should().Be(original.CreatedAt);
        modified.Data.Should().Be("modified");
        original.Data.Should().Be("original");
    }

    [TestMethod]
    public void MessageBase_Inheritance_ShouldWorkCorrectly()
    {
        // Arrange & Act
        IMessage message = new TestMessage
        {
            Data = "test data"
        };

        // Assert
        message.Should().BeOfType<TestMessage>();
        message.MessageId.Should().NotBeNullOrEmpty();
        (message as TestMessage)?.Data.Should().Be("test data");
    }

    private record TestMessage : MessageBase
    {
        public string Data { get; init; } = string.Empty;
    }
}
