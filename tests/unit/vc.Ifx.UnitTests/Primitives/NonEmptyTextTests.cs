using VisionaryCoder.Framework.Primitives;

namespace VisionaryCoder.Framework.Tests.Primitives;

[TestClass]
public class NonEmptyTextTests
{
    #region Construction

    [TestMethod]
    [DataRow("Alpha", "Alpha")]
    [DataRow("  Alpha  ", "Alpha")]
    public void Constructor_WithValidText_ShouldCreateTrimmedText(string input, string expected)
    {
        var text = new NonEmptyText<TestUser>(input);

        text.Value.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("\t")]
    public void Constructor_WithInvalidText_ShouldThrowArgumentException(string? input)
    {
        Action act = () => _ = new NonEmptyText<TestUser>(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Text value cannot be empty or whitespace*")
            .WithParameterName("value");
    }

    [TestMethod]
    public void Create_WithValidText_ShouldCreateText()
    {
        var text = NonEmptyText<TestUser>.Create(" value ");

        text.Value.Should().Be("value");
    }

    #endregion

    #region Parsing

    [TestMethod]
    public void Parse_WithValidText_ShouldReturnText()
    {
        var text = NonEmptyText<TestUser>.Parse(" value ");

        text.Value.Should().Be("value");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void Parse_WithInvalidText_ShouldThrowFormatException(string? input)
    {
        Action act = () => _ = NonEmptyText<TestUser>.Parse(input);

        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid non-empty text value*");
    }

    [TestMethod]
    public void TryParse_WithValidText_ShouldReturnTrue()
    {
        bool result = NonEmptyText<TestUser>.TryParse(" value ", out NonEmptyText<TestUser> text);

        result.Should().BeTrue();
        text.Value.Should().Be("value");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void TryParse_WithInvalidText_ShouldReturnFalse(string? input)
    {
        bool result = NonEmptyText<TestUser>.TryParse(input, out NonEmptyText<TestUser> text);

        result.Should().BeFalse();
        text.Value.Should().BeEmpty();
    }

    #endregion

    #region Conversion and metadata

    [TestMethod]
    public void ExplicitStringConversion_ShouldReturnValue()
    {
        var text = new NonEmptyText<TestUser>("Alpha");

        string value = (string)text;

        value.Should().Be("Alpha");
    }

    [TestMethod]
    public void ToString_ShouldReturnValue()
    {
        var text = new NonEmptyText<TestUser>("Alpha");

        text.ToString().Should().Be("Alpha");
    }

    [TestMethod]
    public void IPrimitiveValue_ShouldExposeValueMetadata()
    {
        IPrimitiveValue primitive = new NonEmptyText<TestUser>("Alpha");

        primitive.ValueType.Should().Be(typeof(string));
        primitive.BoxedValue.Should().Be("Alpha");
    }

    [TestMethod]
    public void IPrimitiveValueOfT_ShouldExposeTypedValue()
    {
        IPrimitiveValue<string> primitive = new NonEmptyText<TestUser>("Alpha");

        primitive.Value.Should().Be("Alpha");
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        var left = new NonEmptyText<TestUser>("Alpha");
        var right = new NonEmptyText<TestUser>("Alpha");

        left.Equals(right).Should().BeTrue();
        left.Equals((object)right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [TestMethod]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        var left = new NonEmptyText<TestUser>("Alpha");
        var right = new NonEmptyText<TestUser>("Beta");

        left.Equals(right).Should().BeFalse();
        left.Equals((object)right).Should().BeFalse();
        left.Equals("Alpha").Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    #endregion
}

