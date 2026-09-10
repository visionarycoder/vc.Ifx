// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Providers;

namespace VisionaryCoder.Framework.Tests.Providers;

[TestClass]
public sealed class RequestIdProviderTests
{
    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(10)]
    public void GenerateNew_CalledMultipleTimes_ShouldReturnDifferentIds(int count)
    {
        // Arrange
        var provider = new RequestIdProvider();
        var ids = new HashSet<string>();

        // Act
        for (int i = 0; i < count; i++)
        {
            ids.Add(provider.GenerateNew());
        }

        // Assert
        ids.Should().HaveCount(count, "each generated ID should be unique");
    }

    [TestMethod]
    public void RequestId_InitialValue_ShouldNotBeNullOrEmpty()
    {
        // Arrange & Act
        var provider = new RequestIdProvider();

        // Assert
        provider.RequestId.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void GenerateNew_ShouldReturnValidGuid()
    {
        // Arrange
        var provider = new RequestIdProvider();

        // Act
        string newId = provider.GenerateNew();

        // Assert
        newId.Should().HaveLength(8, "request ID should be 8-character hex string");
        newId.Should().MatchRegex("^[0-9A-F]{8}$", "request ID should be uppercase hex");
    }

    [TestMethod]
    public void SetRequestId_WithValidValue_ShouldUpdateCurrentId()
    {
        // Arrange
        var provider = new RequestIdProvider();
        string newId = "test-request-id-123";

        // Act
        provider.SetRequestId(newId);

        // Assert
        provider.RequestId.Should().Be(newId);
    }

    [TestMethod]
    [DataRow("custom-id-1")]
    [DataRow("request-abc-xyz")]
    [DataRow("12345")]
    public void SetRequestId_WithVariousValues_ShouldStoreCorrectly(string requestId)
    {
        // Arrange
        var provider = new RequestIdProvider();

        // Act
        provider.SetRequestId(requestId);

        // Assert
        provider.RequestId.Should().Be(requestId);
    }

    [TestMethod]
    public void RequestId_AfterGenerateNew_ShouldReflectNewValue()
    {
        // Arrange
        var provider = new RequestIdProvider();
        string initialId = provider.RequestId;

        // Act
        string newId = provider.GenerateNew();

        // Assert
        provider.RequestId.Should().NotBe(initialId);
        provider.RequestId.Should().Be(newId);
    }

    [TestMethod]
    public void GenerateNew_ShouldUpdateRequestIdProperty()
    {
        // Arrange
        var provider = new RequestIdProvider();
        provider.SetRequestId("old-id");

        // Act
        string generated = provider.GenerateNew();

        // Assert
        provider.RequestId.Should().Be(generated);
        provider.RequestId.Should().NotBe("old-id");
    }

    [TestMethod]
    public void SetRequestId_CalledMultipleTimes_ShouldUpdateEachTime()
    {
        // Arrange
        var provider = new RequestIdProvider();
        string[] ids = ["id-1", "id-2", "id-3"];

        // Act & Assert
        foreach (string id in ids)
        {
            provider.SetRequestId(id);
            provider.RequestId.Should().Be(id);
        }
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    public void SetRequestId_WithWhitespace_ShouldThrowArgumentException(string requestId)
    {
        // Arrange
        var provider = new RequestIdProvider();

        // Act
        Action act = () => provider.SetRequestId(requestId);

        // Assert
        act.Should().Throw<ArgumentException>("whitespace is not valid for request ID");
    }

    [TestMethod]
    public void GenerateNew_Format_ShouldBeUpperCaseHex()
    {
        // Arrange
        var provider = new RequestIdProvider();

        // Act
        string newId = provider.GenerateNew();

        // Assert
        newId.Should().MatchRegex(@"^[0-9A-F]{8}$",
            "request ID should be 8-character uppercase hex string");
    }

    [TestMethod]
    public void RequestIdProvider_MultipleCalls_ShouldMaintainThreadSafety()
    {
        // Arrange
        var provider = new RequestIdProvider();
        var ids = new System.Collections.Concurrent.ConcurrentBag<string>();

        // Act
        Parallel.For(0, 100, _ =>
        {
            ids.Add(provider.GenerateNew());
        });

        // Assert
        ids.Distinct().Should().HaveCount(100, "all generated IDs should be unique even in parallel execution");
    }
}
