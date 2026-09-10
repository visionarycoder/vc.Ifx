// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Providers;

namespace VisionaryCoder.Framework.Tests.Providers;

[TestClass]
public sealed class CorrelationIdProviderTests
{
    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(50)]
    public void GenerateNew_CalledMultipleTimes_ShouldReturnDifferentIds(int count)
    {
        // Arrange
        var provider = new CorrelationIdProvider();
        var ids = new HashSet<string>();

        // Act
        for (int i = 0; i < count; i++)
        {
            ids.Add(provider.GenerateNew());
        }

        // Assert
        ids.Should().HaveCount(count, "each generated correlation ID should be unique");
    }

    [TestMethod]
    public void CorrelationId_InitialValue_ShouldNotBeNullOrEmpty()
    {
        // Arrange & Act
        var provider = new CorrelationIdProvider();

        // Assert
        provider.CorrelationId.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void GenerateNew_ShouldReturnValidGuid()
    {
        // Arrange
        var provider = new CorrelationIdProvider();

        // Act
        string newId = provider.GenerateNew();

        // Assert
        newId.Should().HaveLength(12, "correlation ID should be 12-character hex string");
        newId.Should().MatchRegex("^[0-9A-F]{12}$", "correlation ID should be uppercase hex");
    }

    [TestMethod]
    [DataRow("correlation-123")]
    [DataRow("trace-abc-xyz")]
    [DataRow("parent-correlation-id")]
    public void SetCorrelationId_WithVariousValues_ShouldStoreCorrectly(string correlationId)
    {
        // Arrange
        var provider = new CorrelationIdProvider();

        // Act
        provider.SetCorrelationId(correlationId);

        // Assert
        provider.CorrelationId.Should().Be(correlationId);
    }

    [TestMethod]
    public void CorrelationId_AfterGenerateNew_ShouldReflectNewValue()
    {
        // Arrange
        var provider = new CorrelationIdProvider();
        string initialId = provider.CorrelationId;

        // Act
        string newId = provider.GenerateNew();

        // Assert
        provider.CorrelationId.Should().NotBe(initialId);
        provider.CorrelationId.Should().Be(newId);
    }

    [TestMethod]
    public void GenerateNew_ShouldUpdateCorrelationIdProperty()
    {
        // Arrange
        var provider = new CorrelationIdProvider();
        provider.SetCorrelationId("old-correlation-id");

        // Act
        string generated = provider.GenerateNew();

        // Assert
        provider.CorrelationId.Should().Be(generated);
        provider.CorrelationId.Should().NotBe("old-correlation-id");
    }

    [TestMethod]
    public void SetCorrelationId_CalledMultipleTimes_ShouldUpdateEachTime()
    {
        // Arrange
        var provider = new CorrelationIdProvider();
        string[] ids = ["corr-1", "corr-2", "corr-3", "corr-4"];

        // Act & Assert
        foreach (string id in ids)
        {
            provider.SetCorrelationId(id);
            provider.CorrelationId.Should().Be(id);
        }
    }

    [TestMethod]
    public void GenerateNew_Format_ShouldBeUpperCaseHex()
    {
        // Arrange
        var provider = new CorrelationIdProvider();

        // Act
        string newId = provider.GenerateNew();

        // Assert
        newId.Should().MatchRegex(@"^[0-9A-F]{12}$",
            "correlation ID should be 12-character uppercase hex string");
    }

    [TestMethod]
    public void CorrelationIdProvider_MultipleCalls_ShouldMaintainThreadSafety()
    {
        // Arrange
        var provider = new CorrelationIdProvider();
        var ids = new System.Collections.Concurrent.ConcurrentBag<string>();

        // Act
        Parallel.For(0, 100, _ =>
        {
            ids.Add(provider.GenerateNew());
        });

        // Assert
        ids.Distinct().Should().HaveCount(100, "all generated IDs should be unique even in parallel execution");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    public void SetCorrelationId_WithWhitespace_ShouldThrowArgumentException(string correlationId)
    {
        // Arrange
        var provider = new CorrelationIdProvider();

        // Act
        Action act = () => provider.SetCorrelationId(correlationId);

        // Assert
        act.Should().Throw<ArgumentException>("whitespace is not valid for correlation ID");
    }

    [TestMethod]
    public void CorrelationId_AfterSetAndGenerate_ShouldBeDifferent()
    {
        // Arrange
        var provider = new CorrelationIdProvider();
        string customId = "custom-correlation-id";

        // Act
        provider.SetCorrelationId(customId);
        string setId = provider.CorrelationId;
        string generatedId = provider.GenerateNew();

        // Assert
        setId.Should().Be(customId);
        generatedId.Should().NotBe(customId);
        provider.CorrelationId.Should().Be(generatedId);
    }

    [TestMethod]
    public void CorrelationIdProvider_UsesAsyncLocalStorage_StatePersistsAcrossInstancesInSameContext()
    {
        // Arrange
        var provider1 = new CorrelationIdProvider();
        string id1 = provider1.GenerateNew();

        // Act
        var provider2 = new CorrelationIdProvider();
        string id2 = provider2.CorrelationId;

        // Assert
        id2.Should().Be(id1,
            "providers share AsyncLocal storage within the same async context");
    }
}
