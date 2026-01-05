// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using FluentAssertions;

namespace VisionaryCoder.Framework.Abstractions.Tests.Primitives;

// Test entities

[TestClass]
public class EntityIdTests
{
    #region Constructor and Create Tests

    [TestMethod]
    public void Constructor_WithValidIntValue_ShouldCreateEntityId()
    {
        // Act
        var id = new EntityId<TestUser, int>(42);

        // Assert
        id.Value.Should().Be(42);
    }

    [TestMethod]
    public void Constructor_WithValidStringValue_ShouldCreateEntityId()
    {
        // Arrange
        string value = "user-123";

        // Act
        var id = new EntityId<TestUser, string>(value);

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void Constructor_WithValidGuidValue_ShouldCreateEntityId()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var id = new EntityId<TestProduct, Guid>(value);

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void Create_WithValidValue_ShouldReturnEntityId()
    {
        // Act
        var id = EntityId<TestUser, int>.Create(100);

        // Assert
        id.Value.Should().Be(100);
    }

    [TestMethod]
    public void Create_WithDefaultInt_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => EntityId<TestUser, int>.Create(default);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ID cannot be the default value*")
            .WithParameterName("value");
    }

    [TestMethod]
    public void Create_WithDefaultGuid_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => EntityId<TestProduct, Guid>.Create(default);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ID cannot be the default value*");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    public void Create_WithEmptyOrWhitespaceString_ShouldThrowArgumentException(string value)
    {
        // Act
        Action act = () => EntityId<TestUser, string>.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ID cannot be empty/whitespace*");
    }

    [TestMethod]
    public void Create_WithValidString_ShouldReturnEntityId()
    {
        // Act
        var id = EntityId<TestUser, string>.Create("valid-id");

        // Assert
        id.Value.Should().Be("valid-id");
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_WithIntValue_ShouldReturnStringRepresentation()
    {
        // Arrange
        var id = new EntityId<TestUser, int>(42);

        // Act
        string result = id.ToString();

        // Assert
        result.Should().Be("42");
    }

    [TestMethod]
    public void ToString_WithStringValue_ShouldReturnValue()
    {
        // Arrange
        string value = "test-id-123";
        var id = new EntityId<TestUser, string>(value);

        // Act
        string result = id.ToString();

        // Assert
        result.Should().Be(value);
    }

    [TestMethod]
    public void ToString_WithGuidValue_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var id = new EntityId<TestProduct, Guid>(guid);

        // Act
        string result = id.ToString();

        // Assert
        result.Should().Be("12345678-1234-1234-1234-123456789012");
    }

    #endregion

    #region Conversion Tests

    [TestMethod]
    public void ImplicitConversion_FromInt_ShouldCreateEntityId()
    {
        // Act
        EntityId<TestUser, int> id = 42;

        // Assert
        id.Value.Should().Be(42);
    }

    [TestMethod]
    public void ImplicitConversion_FromString_ShouldCreateEntityId()
    {
        // Act
        EntityId<TestUser, string> id = "test-id";

        // Assert
        id.Value.Should().Be("test-id");
    }

    [TestMethod]
    public void ExplicitConversion_ToInt_ShouldReturnValue()
    {
        // Arrange
        var id = new EntityId<TestUser, int>(42);

        // Act
        int value = (int)id;

        // Assert
        value.Should().Be(42);
    }

    #endregion

    #region Parse Tests

    [TestMethod]
    public void Parse_WithValidIntString_ShouldReturnEntityId()
    {
        // Act
        var id = EntityId<TestUser, int>.Parse("42");

        // Assert
        id.Value.Should().Be(42);
    }

    [TestMethod]
    public void Parse_WithValidGuidString_ShouldReturnEntityId()
    {
        // Arrange
        string guidString = "12345678-1234-1234-1234-123456789012";

        // Act
        var id = EntityId<TestProduct, Guid>.Parse(guidString);

        // Assert
        id.Value.Should().Be(Guid.Parse(guidString));
    }

    [TestMethod]
    public void Parse_WithInvalidString_ShouldThrowFormatException()
    {
        // Act
        Action act = () => EntityId<TestUser, int>.Parse("invalid");

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid*");
    }

    [TestMethod]
    public void TryParse_WithValidString_ShouldReturnTrueAndId()
    {
        // Act
        bool result = EntityId<TestUser, int>.TryParse("42", out var id);

        // Assert
        result.Should().BeTrue();
        id.Value.Should().Be(42);
    }

    [TestMethod]
    public void TryParse_WithInvalidString_ShouldReturnFalse()
    {
        // Act
        bool result = EntityId<TestUser, int>.TryParse("invalid", out var id);

        // Assert
        result.Should().BeFalse();
        id.Value.Should().Be(0);
    }

    #endregion

    #region IEntityId Interface Tests

    [TestMethod]
    public void IEntityId_ValueType_ShouldReturnCorrectType()
    {
        // Arrange
        IEntityId id = new EntityId<TestUser, int>(42);

        // Assert
        id.ValueType.Should().Be(typeof(int));
    }

    [TestMethod]
    public void IEntityId_BoxedValue_ShouldReturnValue()
    {
        // Arrange
        IEntityId id = new EntityId<TestUser, int>(42);

        // Assert
        id.BoxedValue.Should().Be(42);
    }

    #endregion
}
