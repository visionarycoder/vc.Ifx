using FluentAssertions;
using VisionaryCoder.Framework.Patterns.CQRS;

namespace VisionaryCoder.Framework.Patterns.Tests.CQRS;

/// <summary>
/// Unit tests for the Unit struct.
/// </summary>
[TestClass]
public class UnitTests
{
    [TestMethod]
    public void Value_ShouldReturnDefaultInstance()
    {
        // Act
        Unit unit = Unit.Value;

        // Assert
        unit.Should().Be(default(Unit));
    }

    [TestMethod]
    public void Equals_WithSameUnit_ShouldReturnTrue()
    {
        // Arrange
        Unit unit1 = Unit.Value;
        Unit unit2 = Unit.Value;

        // Act
        bool result = unit1.Equals(unit2);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_WithObject_ShouldReturnTrue()
    {
        // Arrange
        Unit unit = Unit.Value;
        object obj = Unit.Value;

        // Act
        bool result = unit.Equals(obj);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_WithNonUnitObject_ShouldReturnFalse()
    {
        // Arrange
        Unit unit = Unit.Value;
        object obj = "not a unit";

        // Act
        bool result = unit.Equals(obj);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void EqualityOperator_ShouldReturnTrue()
    {
        // Arrange
        Unit unit1 = Unit.Value;
        Unit unit2 = Unit.Value;

        // Act
        bool result = unit1 == unit2;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void InequalityOperator_ShouldReturnFalse()
    {
        // Arrange
        Unit unit1 = Unit.Value;
        Unit unit2 = Unit.Value;

        // Act
        bool result = unit1 != unit2;

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GetHashCode_ShouldReturnZero()
    {
        // Arrange
        Unit unit = Unit.Value;

        // Act
        int hashCode = unit.GetHashCode();

        // Assert
        hashCode.Should().Be(0);
    }

    [TestMethod]
    public void ToString_ShouldReturnEmptyParentheses()
    {
        // Arrange
        Unit unit = Unit.Value;

        // Act
        string result = unit.ToString();

        // Assert
        result.Should().Be("()");
    }

    [TestMethod]
    public void DefaultConstructor_ShouldCreateValidUnit()
    {
        // Act
        Unit unit = new();

        // Assert
        unit.Should().Be(Unit.Value);
    }

    [TestMethod]
    public void MultipleInstances_ShouldBeEqual()
    {
        // Arrange
        Unit unit1 = new();
        Unit unit2 = default;
        Unit unit3 = Unit.Value;

        // Assert
        unit1.Should().Be(unit2);
        unit2.Should().Be(unit3);
        unit1.Should().Be(unit3);
    }
}
