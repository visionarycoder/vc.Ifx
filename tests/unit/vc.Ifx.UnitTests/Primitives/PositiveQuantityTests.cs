using VisionaryCoder.Framework.Primitives;

namespace VisionaryCoder.Framework.Tests.Primitives;

[TestClass]
public class PositiveQuantityTests
{
    #region Construction

    [TestMethod]
    [DataRow(1)]
    [DataRow(42)]
    [DataRow(int.MaxValue)]
    public void Constructor_WithPositiveValue_ShouldCreateQuantity(int input)
    {
        var quantity = new PositiveQuantity<TestOrder>(input);

        quantity.Value.Should().Be(input);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(int.MinValue)]
    public void Constructor_WithNonPositiveValue_ShouldThrowArgumentOutOfRangeException(int input)
    {
        Action act = () => _ = new PositiveQuantity<TestOrder>(input);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Quantity value must be greater than zero*")
            .WithParameterName("value");
    }

    [TestMethod]
    public void Create_WithPositiveValue_ShouldCreateQuantity()
    {
        var quantity = PositiveQuantity<TestOrder>.Create(7);

        quantity.Value.Should().Be(7);
    }

    #endregion

    #region Parsing

    [TestMethod]
    public void Parse_WithPositiveIntegerText_ShouldReturnQuantity()
    {
        var quantity = PositiveQuantity<TestOrder>.Parse("7");

        quantity.Value.Should().Be(7);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("0")]
    [DataRow("-1")]
    [DataRow("1.5")]
    [DataRow("abc")]
    public void Parse_WithInvalidText_ShouldThrowFormatException(string? input)
    {
        Action act = () => _ = PositiveQuantity<TestOrder>.Parse(input);

        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid positive quantity value*");
    }

    [TestMethod]
    public void TryParse_WithPositiveIntegerText_ShouldReturnTrue()
    {
        bool result = PositiveQuantity<TestOrder>.TryParse("7", out PositiveQuantity<TestOrder> quantity);

        result.Should().BeTrue();
        quantity.Value.Should().Be(7);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("0")]
    [DataRow("-1")]
    [DataRow("abc")]
    public void TryParse_WithInvalidText_ShouldReturnFalse(string? input)
    {
        bool result = PositiveQuantity<TestOrder>.TryParse(input, out PositiveQuantity<TestOrder> quantity);

        result.Should().BeFalse();
        quantity.Value.Should().Be(0);
    }

    #endregion

    #region Conversion and metadata

    [TestMethod]
    public void ExplicitIntConversion_ShouldReturnValue()
    {
        var quantity = new PositiveQuantity<TestOrder>(7);

        int value = (int)quantity;

        value.Should().Be(7);
    }

    [TestMethod]
    public void ToString_ShouldReturnInvariantText()
    {
        var quantity = new PositiveQuantity<TestOrder>(7);

        quantity.ToString().Should().Be("7");
    }

    [TestMethod]
    public void IPrimitiveValue_ShouldExposeValueMetadata()
    {
        IPrimitiveValue primitive = new PositiveQuantity<TestOrder>(7);

        primitive.ValueType.Should().Be(typeof(int));
        primitive.BoxedValue.Should().Be(7);
    }

    [TestMethod]
    public void IPrimitiveValueOfT_ShouldExposeTypedValue()
    {
        IPrimitiveValue<int> primitive = new PositiveQuantity<TestOrder>(7);

        primitive.Value.Should().Be(7);
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        var left = new PositiveQuantity<TestOrder>(7);
        var right = new PositiveQuantity<TestOrder>(7);

        left.Equals(right).Should().BeTrue();
        left.Equals((object)right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [TestMethod]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        var left = new PositiveQuantity<TestOrder>(7);
        var right = new PositiveQuantity<TestOrder>(8);

        left.Equals(right).Should().BeFalse();
        left.Equals((object)right).Should().BeFalse();
        left.Equals(7).Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    #endregion
}

