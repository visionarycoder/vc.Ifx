using VisionaryCoder.Framework.Primitives;

namespace VisionaryCoder.Framework.Tests.Primitives;

[TestClass]
public class PercentageTests
{
    #region Construction

    [TestMethod]
    [DataRow(0)]
    [DataRow(50)]
    [DataRow(100)]
    public void Constructor_WithBoundedValue_ShouldCreatePercentage(double input)
    {
        decimal value = Convert.ToDecimal(input);

        var percentage = new Percentage<TestOrder>(value);

        percentage.Value.Should().Be(value);
    }

    [TestMethod]
    [DataRow(-0.01)]
    [DataRow(100.01)]
    public void Constructor_WithOutOfRangeValue_ShouldThrowArgumentOutOfRangeException(double input)
    {
        decimal value = Convert.ToDecimal(input);

        Action act = () => _ = new Percentage<TestOrder>(value);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Percentage value must be between 0 and 100*")
            .WithParameterName("value");
    }

    [TestMethod]
    public void Create_WithBoundedValue_ShouldCreatePercentage()
    {
        var percentage = Percentage<TestOrder>.Create(12.34m);

        percentage.Value.Should().Be(12.34m);
    }

    #endregion

    #region Parsing

    [TestMethod]
    public void Parse_WithBoundedDecimalText_ShouldReturnPercentage()
    {
        var percentage = Percentage<TestOrder>.Parse("12.34");

        percentage.Value.Should().Be(12.34m);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("-0.01")]
    [DataRow("100.01")]
    [DataRow("abc")]
    public void Parse_WithInvalidText_ShouldThrowFormatException(string? input)
    {
        Action act = () => _ = Percentage<TestOrder>.Parse(input);

        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid percentage value*");
    }

    [TestMethod]
    public void TryParse_WithBoundedDecimalText_ShouldReturnTrue()
    {
        bool result = Percentage<TestOrder>.TryParse("12.34", out Percentage<TestOrder> percentage);

        result.Should().BeTrue();
        percentage.Value.Should().Be(12.34m);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("-0.01")]
    [DataRow("100.01")]
    [DataRow("abc")]
    public void TryParse_WithInvalidText_ShouldReturnFalse(string? input)
    {
        bool result = Percentage<TestOrder>.TryParse(input, out Percentage<TestOrder> percentage);

        result.Should().BeFalse();
        percentage.Value.Should().Be(0m);
    }

    #endregion

    #region Conversion and metadata

    [TestMethod]
    public void ExplicitDecimalConversion_ShouldReturnValue()
    {
        var percentage = new Percentage<TestOrder>(12.34m);

        decimal value = (decimal)percentage;

        value.Should().Be(12.34m);
    }

    [TestMethod]
    public void ToString_ShouldReturnInvariantText()
    {
        var percentage = new Percentage<TestOrder>(12.34m);

        percentage.ToString().Should().Be("12.34");
    }

    [TestMethod]
    public void IPrimitiveValue_ShouldExposeValueMetadata()
    {
        IPrimitiveValue primitive = new Percentage<TestOrder>(12.34m);

        primitive.ValueType.Should().Be(typeof(decimal));
        primitive.BoxedValue.Should().Be(12.34m);
    }

    [TestMethod]
    public void IPrimitiveValueOfT_ShouldExposeTypedValue()
    {
        IPrimitiveValue<decimal> primitive = new Percentage<TestOrder>(12.34m);

        primitive.Value.Should().Be(12.34m);
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        var left = new Percentage<TestOrder>(12.34m);
        var right = new Percentage<TestOrder>(12.34m);

        left.Equals(right).Should().BeTrue();
        left.Equals((object)right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [TestMethod]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        var left = new Percentage<TestOrder>(12.34m);
        var right = new Percentage<TestOrder>(56.78m);

        left.Equals(right).Should().BeFalse();
        left.Equals((object)right).Should().BeFalse();
        left.Equals(12.34m).Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    #endregion
}

