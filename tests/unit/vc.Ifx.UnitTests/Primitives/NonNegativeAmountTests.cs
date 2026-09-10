using VisionaryCoder.Framework.Primitives;

namespace VisionaryCoder.Framework.Tests.Primitives;

[TestClass]
public class NonNegativeAmountTests
{
    #region Construction

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(123.45)]
    public void Constructor_WithNonNegativeValue_ShouldCreateAmount(double input)
    {
        decimal value = Convert.ToDecimal(input);

        var amount = new NonNegativeAmount<TestOrder>(value);

        amount.Value.Should().Be(value);
    }

    [TestMethod]
    public void Constructor_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        Action act = () => _ = new NonNegativeAmount<TestOrder>(-0.01m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Amount value cannot be negative*")
            .WithParameterName("value");
    }

    [TestMethod]
    public void Create_WithNonNegativeValue_ShouldCreateAmount()
    {
        var amount = NonNegativeAmount<TestOrder>.Create(12.34m);

        amount.Value.Should().Be(12.34m);
    }

    #endregion

    #region Parsing

    [TestMethod]
    public void Parse_WithNonNegativeDecimalText_ShouldReturnAmount()
    {
        var amount = NonNegativeAmount<TestOrder>.Parse("12.34");

        amount.Value.Should().Be(12.34m);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("-0.01")]
    [DataRow("abc")]
    public void Parse_WithInvalidText_ShouldThrowFormatException(string? input)
    {
        Action act = () => _ = NonNegativeAmount<TestOrder>.Parse(input);

        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid non-negative amount value*");
    }

    [TestMethod]
    public void TryParse_WithNonNegativeDecimalText_ShouldReturnTrue()
    {
        bool result = NonNegativeAmount<TestOrder>.TryParse("12.34", out NonNegativeAmount<TestOrder> amount);

        result.Should().BeTrue();
        amount.Value.Should().Be(12.34m);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("-0.01")]
    [DataRow("abc")]
    public void TryParse_WithInvalidText_ShouldReturnFalse(string? input)
    {
        bool result = NonNegativeAmount<TestOrder>.TryParse(input, out NonNegativeAmount<TestOrder> amount);

        result.Should().BeFalse();
        amount.Value.Should().Be(0m);
    }

    #endregion

    #region Conversion and metadata

    [TestMethod]
    public void ExplicitDecimalConversion_ShouldReturnValue()
    {
        var amount = new NonNegativeAmount<TestOrder>(12.34m);

        decimal value = (decimal)amount;

        value.Should().Be(12.34m);
    }

    [TestMethod]
    public void ToString_ShouldReturnInvariantText()
    {
        var amount = new NonNegativeAmount<TestOrder>(12.34m);

        amount.ToString().Should().Be("12.34");
    }

    [TestMethod]
    public void IPrimitiveValue_ShouldExposeValueMetadata()
    {
        IPrimitiveValue primitive = new NonNegativeAmount<TestOrder>(12.34m);

        primitive.ValueType.Should().Be(typeof(decimal));
        primitive.BoxedValue.Should().Be(12.34m);
    }

    [TestMethod]
    public void IPrimitiveValueOfT_ShouldExposeTypedValue()
    {
        IPrimitiveValue<decimal> primitive = new NonNegativeAmount<TestOrder>(12.34m);

        primitive.Value.Should().Be(12.34m);
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        var left = new NonNegativeAmount<TestOrder>(12.34m);
        var right = new NonNegativeAmount<TestOrder>(12.34m);

        left.Equals(right).Should().BeTrue();
        left.Equals((object)right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [TestMethod]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        var left = new NonNegativeAmount<TestOrder>(12.34m);
        var right = new NonNegativeAmount<TestOrder>(56.78m);

        left.Equals(right).Should().BeFalse();
        left.Equals((object)right).Should().BeFalse();
        left.Equals(12.34m).Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    #endregion
}

