using FluentAssertions;
using VisionaryCoder.Framework.Events;

namespace VisionaryCoder.Framework.Tests.Events;

/// <summary>
/// Unit tests for IDomainEvent and DomainEvent with 100% coverage.
/// </summary>
[TestClass]
public class DomainEventTests
{
    [TestMethod]
    public void DomainEvent_DefaultConstructor_ShouldGenerateEventId()
    {
        // Act
        var domainEvent = new TestDomainEvent();

        // Assert
        domainEvent.EventId.Should().NotBeEmpty();
    }

    [TestMethod]
    public void DomainEvent_DefaultConstructor_ShouldSetOccurredAt()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var domainEvent = new TestDomainEvent();
        var after = DateTimeOffset.UtcNow;

        // Assert
        domainEvent.OccurredAt.Should().BeOnOrAfter(before);
        domainEvent.OccurredAt.Should().BeOnOrBefore(after);
    }

    [TestMethod]
    public void DomainEvent_WithCustomEventId_ShouldUseCustomId()
    {
        // Arrange
        var customId = Guid.NewGuid();

        // Act
        var domainEvent = new TestDomainEvent
        {
            EventId = customId
        };

        // Assert
        domainEvent.EventId.Should().Be(customId);
    }

    [TestMethod]
    public void DomainEvent_WithCustomOccurredAt_ShouldUseCustomTime()
    {
        // Arrange
        var customTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        var domainEvent = new TestDomainEvent
        {
            OccurredAt = customTime
        };

        // Assert
        domainEvent.OccurredAt.Should().Be(customTime);
    }

    [TestMethod]
    public void DomainEvent_MultipleInstances_ShouldHaveUniqueIds()
    {
        // Act
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();
        var event3 = new TestDomainEvent();

        // Assert
        var ids = new[] { event1.EventId, event2.EventId, event3.EventId };
        ids.Should().OnlyHaveUniqueItems();
    }

    [TestMethod]
    public void DomainEvent_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var event1 = new TestDomainEvent
        {
            EventId = eventId,
            OccurredAt = occurredAt,
            Data = "test"
        };

        var event2 = new TestDomainEvent
        {
            EventId = eventId,
            OccurredAt = occurredAt,
            Data = "test"
        };

        var event3 = new TestDomainEvent
        {
            EventId = eventId,
            OccurredAt = occurredAt,
            Data = "different"
        };

        // Assert
        event1.Should().Be(event2);
        event1.Should().NotBe(event3);
    }

    [TestMethod]
    public void DomainEvent_With_ShouldCreateCopyWithModifications()
    {
        // Arrange
        var original = new TestDomainEvent
        {
            EventId = Guid.NewGuid(),
            Data = "original"
        };

        // Act
        var modified = original with { Data = "modified" };

        // Assert
        modified.EventId.Should().Be(original.EventId);
        modified.OccurredAt.Should().Be(original.OccurredAt);
        modified.Data.Should().Be("modified");
        original.Data.Should().Be("original");
    }

    [TestMethod]
    public void DomainEvent_Inheritance_ShouldWorkCorrectly()
    {
        // Arrange & Act
        IDomainEvent domainEvent = new TestDomainEvent
        {
            Data = "test data"
        };

        // Assert
        domainEvent.Should().BeOfType<TestDomainEvent>();
        domainEvent.EventId.Should().NotBeEmpty();
        (domainEvent as TestDomainEvent)?.Data.Should().Be("test data");
    }

    [TestMethod]
    public void DomainEvent_WithComplexData_ShouldWork()
    {
        // Arrange
        var complexData = new { OrderId = Guid.NewGuid(), Total = 123.45m };

        // Act
        var domainEvent = new ComplexDomainEvent
        {
            OrderId = complexData.OrderId,
            Total = complexData.Total
        };

        // Assert
        domainEvent.OrderId.Should().Be(complexData.OrderId);
        domainEvent.Total.Should().Be(complexData.Total);
    }

    private record TestDomainEvent : DomainEvent
    {
        public string Data { get; init; } = string.Empty;
    }

    private record ComplexDomainEvent : DomainEvent
    {
        public Guid OrderId { get; init; }
        public decimal Total { get; init; }
    }
}
