using VisionaryCoder.Framework.Extensions;

namespace VisionaryCoder.Framework.Tests.Extensions;

[TestClass]
public class MathExtensionsTests
{
    #region ThrowIfZero Tests

    [TestMethod]
    public void ThrowIfZero_WithZeroInt_ShouldThrowDivideByZeroException()
    {
        // Arrange
        int value = 0;

        // Act & Assert
        DivideByZeroException? exception = Assert.ThrowsExactly<DivideByZeroException>(() => MathExtensions.ThrowIfZero(value));
        exception.Message.Should().Contain("Division by zero would occur");
    }

    [TestMethod]
    public void ThrowIfZero_WithZeroIntAndParamName_ShouldThrowWithParamName()
    {
        // Arrange
        int value = 0;
        string paramName = "divisor";

        // Act & Assert
        DivideByZeroException? exception = Assert.ThrowsExactly<DivideByZeroException>(() => MathExtensions.ThrowIfZero(value, paramName));
        exception.Message.Should().Contain("Division by zero would occur with parameter 'divisor'");
    }

    [TestMethod]
    public void ThrowIfZero_WithNonZeroInt_ShouldNotThrow()
    {
        // Arrange
        int value = 5;

        // Act & Assert
        Action action = () => MathExtensions.ThrowIfZero(value);
        action.Should().NotThrow();
    }

    [TestMethod]
    public void ThrowIfZero_WithZeroDouble_ShouldThrowDivideByZeroException()
    {
        // Arrange
        double value = 0.0;

        // Act & Assert
        DivideByZeroException? exception = Assert.ThrowsExactly<DivideByZeroException>(() => MathExtensions.ThrowIfZero(value));
        exception.Message.Should().Contain("Division by zero would occur");
    }

    [TestMethod]
    public void ThrowIfZero_WithNonZeroDouble_ShouldNotThrow()
    {
        // Arrange
        double value = 3.14;

        // Act & Assert
        Action action = () => MathExtensions.ThrowIfZero(value);
        action.Should().NotThrow();
    }

    [TestMethod]
    public void ThrowIfZero_WithZeroDecimal_ShouldThrowDivideByZeroException()
    {
        // Arrange
        decimal value = 0m;

        // Act & Assert
        DivideByZeroException? exception = Assert.ThrowsExactly<DivideByZeroException>(() => MathExtensions.ThrowIfZero(value));
        exception.Message.Should().Contain("Division by zero would occur");
    }

    [TestMethod]
    public void ThrowIfZero_WithNonZeroDecimal_ShouldNotThrow()
    {
        // Arrange
        decimal value = 1.5m;

        // Act & Assert
        Action action = () => MathExtensions.ThrowIfZero(value);
        action.Should().NotThrow();
    }

    #endregion

    #region IsZero Tests

    [TestMethod]
    public void IsZero_WithZeroInt_ShouldReturnTrue()
    {
        // Arrange
        int value = 0;

        // Act
        bool result = value.IsZero();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsZero_WithNonZeroInt_ShouldReturnFalse()
    {
        // Arrange
        int value = 42;

        // Act
        bool result = value.IsZero();

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsZero_WithZeroDouble_ShouldReturnTrue()
    {
        // Arrange
        double value = 0.0;

        // Act
        bool result = value.IsZero();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsZero_WithNonZeroDouble_ShouldReturnFalse()
    {
        // Arrange
        double value = 1.23;

        // Act
        bool result = value.IsZero();

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsZero_WithZeroDecimal_ShouldReturnTrue()
    {
        // Arrange
        decimal value = 0m;

        // Act
        bool result = value.IsZero();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsZero_WithNonZeroDecimal_ShouldReturnFalse()
    {
        // Arrange
        decimal value = 5.67m;

        // Act
        bool result = value.IsZero();

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsZero_WithZeroFloat_ShouldReturnTrue()
    {
        // Arrange
        float value = 0.0f;

        // Act
        bool result = value.IsZero();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsZero_WithNonZeroFloat_ShouldReturnFalse()
    {
        // Arrange
        float value = 2.5f;

        // Act
        bool result = value.IsZero();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SafeDivide Tests (with default value)

    [TestMethod]
    public void SafeDivide_WithNonZeroDenominator_ShouldReturnQuotient()
    {
        // Arrange
        int numerator = 10;
        int denominator = 2;
        int defaultValue = 999;

        // Act
        int result = MathExtensions.SafeDivide(numerator, denominator, defaultValue);

        // Assert
        result.Should().Be(5);
    }

    [TestMethod]
    public void SafeDivide_WithZeroDenominator_ShouldReturnDefaultValue()
    {
        // Arrange
        int numerator = 10;
        int denominator = 0;
        int defaultValue = 999;

        // Act
        int result = MathExtensions.SafeDivide(numerator, denominator, defaultValue);

        // Assert
        result.Should().Be(999);
    }

    [TestMethod]
    public void SafeDivide_WithDoubles_ShouldWorkCorrectly()
    {
        // Arrange
        double numerator = 15.0;
        double denominator = 3.0;
        double defaultValue = -1.0;

        // Act
        double result = MathExtensions.SafeDivide(numerator, denominator, defaultValue);

        // Assert
        result.Should().Be(5.0);
    }

    [TestMethod]
    public void SafeDivide_WithZeroDoublesDenominator_ShouldReturnDefaultValue()
    {
        // Arrange
        double numerator = 15.0;
        double denominator = 0.0;
        double defaultValue = -1.0;

        // Act
        double result = MathExtensions.SafeDivide(numerator, denominator, defaultValue);

        // Assert
        result.Should().Be(-1.0);
    }

    [TestMethod]
    public void SafeDivide_WithDecimals_ShouldWorkCorrectly()
    {
        // Arrange
        decimal numerator = 20.5m;
        decimal denominator = 4.1m;
        decimal defaultValue = 0m;

        // Act
        decimal result = MathExtensions.SafeDivide(numerator, denominator, defaultValue);

        // Assert
        result.Should().Be(5m);
    }

    #endregion

    #region SafeDivide Tests (without default value - returns zero)

    [TestMethod]
    public void SafeDivide_WithoutDefault_WithNonZeroDenominator_ShouldReturnQuotient()
    {
        // Arrange
        int numerator = 20;
        int denominator = 4;

        // Act
        int result = MathExtensions.SafeDivide(numerator, denominator);

        // Assert
        result.Should().Be(5);
    }

    [TestMethod]
    public void SafeDivide_WithoutDefault_WithZeroDenominator_ShouldReturnZero()
    {
        // Arrange
        int numerator = 10;
        int denominator = 0;

        // Act
        int result = MathExtensions.SafeDivide(numerator, denominator);

        // Assert
        result.Should().Be(0);
    }

    [TestMethod]
    public void SafeDivide_WithoutDefault_WithDoubles_ShouldWorkCorrectly()
    {
        // Arrange
        double numerator = 12.0;
        double denominator = 0.0;

        // Act
        double result = MathExtensions.SafeDivide(numerator, denominator);

        // Assert
        result.Should().Be(0.0);
    }

    #endregion

    #region TryDivide Tests

    [TestMethod]
    public void TryDivide_WithNonZeroDenominator_ShouldReturnTrueAndCorrectResult()
    {
        // Arrange
        int numerator = 15;
        int denominator = 3;

        // Act
        bool success = MathExtensions.TryDivide(numerator, denominator, out int result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(5);
    }

    [TestMethod]
    public void TryDivide_WithZeroDenominator_ShouldReturnFalseAndDefaultResult()
    {
        // Arrange
        int numerator = 10;
        int denominator = 0;

        // Act
        bool success = MathExtensions.TryDivide(numerator, denominator, out int result);

        // Assert
        success.Should().BeFalse();
        result.Should().Be(0); // default for int
    }

    [TestMethod]
    public void TryDivide_WithDoubles_ShouldWorkCorrectly()
    {
        // Arrange
        double numerator = 21.0;
        double denominator = 7.0;

        // Act
        bool success = MathExtensions.TryDivide(numerator, denominator, out double result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(3.0);
    }

    [TestMethod]
    public void TryDivide_WithZeroDoubleDenominator_ShouldReturnFalse()
    {
        // Arrange
        double numerator = 10.5;
        double denominator = 0.0;

        // Act
        bool success = MathExtensions.TryDivide(numerator, denominator, out double result);

        // Assert
        success.Should().BeFalse();
        result.Should().Be(0.0);
    }

    [TestMethod]
    public void TryDivide_WithDecimals_ShouldWorkCorrectly()
    {
        // Arrange
        decimal numerator = 24.6m;
        decimal denominator = 6.15m;

        // Act
        bool success = MathExtensions.TryDivide(numerator, denominator, out decimal result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(4m);
    }

    #endregion

    #region DefaultIfZero Tests

    [TestMethod]
    public void DefaultIfZero_WithZeroValue_ShouldReturnDefault()
    {
        // Arrange
        int value = 0;
        int defaultValue = 42;

        // Act
        int result = value.DefaultIfZero(defaultValue);

        // Assert
        result.Should().Be(42);
    }

    [TestMethod]
    public void DefaultIfZero_WithNonZeroValue_ShouldReturnOriginalValue()
    {
        // Arrange
        int value = 15;
        int defaultValue = 42;

        // Act
        int result = value.DefaultIfZero(defaultValue);

        // Assert
        result.Should().Be(15);
    }

    [TestMethod]
    public void DefaultIfZero_WithZeroDouble_ShouldReturnDefault()
    {
        // Arrange
        double value = 0.0;
        double defaultValue = 3.14;

        // Act
        double result = value.DefaultIfZero(defaultValue);

        // Assert
        result.Should().Be(3.14);
    }

    [TestMethod]
    public void DefaultIfZero_WithNonZeroDouble_ShouldReturnOriginalValue()
    {
        // Arrange
        double value = 2.5;
        double defaultValue = 3.14;

        // Act
        double result = value.DefaultIfZero(defaultValue);

        // Assert
        result.Should().Be(2.5);
    }

    [TestMethod]
    public void DefaultIfZero_WithZeroDecimal_ShouldReturnDefault()
    {
        // Arrange
        decimal value = 0m;
        decimal defaultValue = 9.99m;

        // Act
        decimal result = value.DefaultIfZero(defaultValue);

        // Assert
        result.Should().Be(9.99m);
    }

    [TestMethod]
    public void DefaultIfZero_WithNonZeroDecimal_ShouldReturnOriginalValue()
    {
        // Arrange
        decimal value = 7.77m;
        decimal defaultValue = 9.99m;

        // Act
        decimal result = value.DefaultIfZero(defaultValue);

        // Assert
        result.Should().Be(7.77m);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void DivideByZeroExtensions_ComplexScenario_ShouldWorkCorrectly()
    {
        // Arrange
        int[] values = [10, 0, 5, 20];
        int divisor = 2;
        var results = new List<int>();

        // Act
        foreach (int value in values)
        {
            if (!value.IsZero())
            {
                MathExtensions.ThrowIfZero(divisor); // This should not throw for divisor = 2
                int result = MathExtensions.SafeDivide(value, divisor);
                results.Add(result);
            }
            else
            {
                results.Add(value.DefaultIfZero(99));
            }
        }

        // Assert
        results.Should().ContainInOrder(5, 99, 2, 10); // 10/2=5, 0->99, 5/2=2, 20/2=10
    }

    [TestMethod]
    public void DivideByZeroExtensions_WithDifferentNumericTypes_ShouldWorkConsistently()
    {
        // Test with int
        int intResult = MathExtensions.SafeDivide(10, 0, -1);
        intResult.Should().Be(-1);

        // Test with double
        double doubleResult = MathExtensions.SafeDivide(10.0, 0.0, -1.0);
        doubleResult.Should().Be(-1.0);

        // Test with decimal
        decimal decimalResult = MathExtensions.SafeDivide(10m, 0m, -1m);
        decimalResult.Should().Be(-1m);

        // Test with float
        float floatResult = MathExtensions.SafeDivide(10f, 0f, -1f);
        floatResult.Should().Be(-1f);

        // All should consistently return the default value when dividing by zero
        intResult.Should().Be(-1);
        doubleResult.Should().Be(-1.0);
        decimalResult.Should().Be(-1m);
        floatResult.Should().Be(-1f);
    }

    [TestMethod]
    public void DivideByZeroExtensions_TryDividePattern_ShouldHandleEdgeCases()
    {
        // Test successful division
        bool success1 = MathExtensions.TryDivide(100, 25, out int result1);
        success1.Should().BeTrue();
        result1.Should().Be(4);

        // Test zero division
        bool success2 = MathExtensions.TryDivide(100, 0, out int result2);
        success2.Should().BeFalse();
        result2.Should().Be(0);

        // Test zero numerator (valid division)
        bool success3 = MathExtensions.TryDivide(0, 5, out int result3);
        success3.Should().BeTrue();
        result3.Should().Be(0);
    }

    #endregion
}
