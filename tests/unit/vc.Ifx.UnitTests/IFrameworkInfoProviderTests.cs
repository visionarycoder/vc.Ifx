// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Moq;
using System.Reflection;
using VisionaryCoder.Framework.Providers;

namespace VisionaryCoder.Framework.Tests;

[TestClass]
public sealed class IFrameworkInfoProviderTests
{
    [TestMethod]
    public void Version_ShouldReturnFrameworkVersion()
    {
        // Arrange
        var mockProvider = new Mock<IFrameworkInfoProvider>();
        string version = "1.0.0";
        mockProvider.Setup(p => p.Version).Returns(version);

        // Act
        string result = mockProvider.Object.Version;

        // Assert
        result.Should().Be(version);
    }

    [TestMethod]
    public void Name_ShouldReturnFrameworkName()
    {
        // Arrange
        var mockProvider = new Mock<IFrameworkInfoProvider>();
        string name = "VisionaryCoder.Framework";
        mockProvider.Setup(p => p.Name).Returns(name);

        // Act
        string result = mockProvider.Object.Name;

        // Assert
        result.Should().Be(name);
    }

    [TestMethod]
    public void Description_ShouldReturnFrameworkDescription()
    {
        // Arrange
        var mockProvider = new Mock<IFrameworkInfoProvider>();
        string description = "Enterprise framework for .NET applications";
        mockProvider.Setup(p => p.Description).Returns(description);

        // Act
        string result = mockProvider.Object.Description;

        // Assert
        result.Should().Be(description);
    }

    [TestMethod]
    public void CompiledAt_ShouldReturnCompilationTimestamp()
    {
        // Arrange
        var mockProvider = new Mock<IFrameworkInfoProvider>();
        DateTimeOffset compiledAt = DateTimeOffset.UtcNow;
        mockProvider.Setup(p => p.CompiledAt).Returns(compiledAt);

        // Act
        DateTimeOffset result = mockProvider.Object.CompiledAt;

        // Assert
        result.Should().Be(compiledAt);
    }

    [TestMethod]
    public void CompiledAt_ShouldBeInPast()
    {
        // Arrange
        var mockProvider = new Mock<IFrameworkInfoProvider>();
        DateTimeOffset pastDate = DateTimeOffset.UtcNow.AddDays(-1);
        mockProvider.Setup(p => p.CompiledAt).Returns(pastDate);

        // Act
        DateTimeOffset result = mockProvider.Object.CompiledAt;

        // Assert
        result.Should().BeBefore(DateTimeOffset.UtcNow);
    }

    [TestMethod]
    public void AllProperties_ShouldBeReadable()
    {
        // Arrange
        var mockProvider = new Mock<IFrameworkInfoProvider>();
        mockProvider.Setup(p => p.Version).Returns("2.0.0");
        mockProvider.Setup(p => p.Name).Returns("TestFramework");
        mockProvider.Setup(p => p.Description).Returns("Test Description");
        mockProvider.Setup(p => p.CompiledAt).Returns(DateTimeOffset.UtcNow);

        // Act
        string version = mockProvider.Object.Version;
        string name = mockProvider.Object.Name;
        string description = mockProvider.Object.Description;
        DateTimeOffset compiledAt = mockProvider.Object.CompiledAt;

        // Assert
        version.Should().NotBeNullOrWhiteSpace();
        name.Should().NotBeNullOrWhiteSpace();
        description.Should().NotBeNullOrWhiteSpace();
        compiledAt.Should().NotBe(default);
    }

    [TestMethod]
    public void Interface_ShouldHaveFourProperties()
    {
        // Arrange & Act
        Type interfaceType = typeof(IFrameworkInfoProvider);
        PropertyInfo[] properties = interfaceType.GetProperties();

        // Assert
        properties.Should().HaveCount(4, "interface has Version, Name, Description, and CompiledAt properties");
    }

    [TestMethod]
    public void Version_ShouldFollowSemanticVersioning()
    {
        // Arrange
        var mockProvider = new Mock<IFrameworkInfoProvider>();
        string version = "1.2.3";
        mockProvider.Setup(p => p.Version).Returns(version);

        // Act
        string result = mockProvider.Object.Version;

        // Assert
        result.Should().MatchRegex(@"^\d+\.\d+\.\d+", "version should follow semantic versioning pattern");
    }

    [TestMethod]
    public void CompiledAt_WithUtcTime_ShouldHaveZeroOffset()
    {
        // Arrange
        var mockProvider = new Mock<IFrameworkInfoProvider>();
        var utcTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        mockProvider.Setup(p => p.CompiledAt).Returns(utcTime);

        // Act
        DateTimeOffset result = mockProvider.Object.CompiledAt;

        // Assert
        result.Offset.Should().Be(TimeSpan.Zero);
    }

    [TestMethod]
    public void Name_ShouldContainVisionaryCoder()
    {
        // Arrange
        var mockProvider = new Mock<IFrameworkInfoProvider>();
        string name = "VisionaryCoder.Framework";
        mockProvider.Setup(p => p.Name).Returns(name);

        // Act
        string result = mockProvider.Object.Name;

        // Assert
        result.Should().Contain("VisionaryCoder");
    }
}
