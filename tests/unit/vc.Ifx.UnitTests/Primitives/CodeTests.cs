using VisionaryCoder.Framework.Primitives;

namespace VisionaryCoder.Framework.Tests.Primitives;

[TestClass]
public class CodeTests
{
    #region Construction

    [TestMethod]
    [DataRow("abc", "ABC")]
    [DataRow("  abc-123  ", "ABC-123")]
    [DataRow("A.B_C-1", "A.B_C-1")]
    public void Constructor_WithValidCode_ShouldNormalizeValue(string input, string expected)
    {
        var code = new Code<TestUser>(input);

        code.Value.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("-ABC")]
    [DataRow("ABC 123")]
    [DataRow("ABC/123")]
    public void Constructor_WithInvalidCode_ShouldThrowArgumentException(string? input)
    {
        Action act = () => _ = new Code<TestUser>(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Code value must start*")
            .WithParameterName("value");
    }

    [TestMethod]
    public void Create_WithValidCode_ShouldCreateCode()
    {
        var code = Code<TestUser>.Create("abc");

        code.Value.Should().Be("ABC");
    }

    #endregion

    #region Parsing

    [TestMethod]
    public void Parse_WithValidCode_ShouldReturnCode()
    {
        var code = Code<TestUser>.Parse("abc");

        code.Value.Should().Be("ABC");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("-ABC")]
    [DataRow("ABC 123")]
    public void Parse_WithInvalidCode_ShouldThrowFormatException(string? input)
    {
        Action act = () => _ = Code<TestUser>.Parse(input);

        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid code value*");
    }

    [TestMethod]
    public void TryParse_WithValidCode_ShouldReturnTrue()
    {
        bool result = Code<TestUser>.TryParse(" abc ", out Code<TestUser> code);

        result.Should().BeTrue();
        code.Value.Should().Be("ABC");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("-ABC")]
    [DataRow("ABC 123")]
    public void TryParse_WithInvalidCode_ShouldReturnFalse(string? input)
    {
        bool result = Code<TestUser>.TryParse(input, out Code<TestUser> code);

        result.Should().BeFalse();
        code.Value.Should().BeEmpty();
    }

    #endregion

    #region Conversion and metadata

    [TestMethod]
    public void ExplicitStringConversion_ShouldReturnValue()
    {
        var code = new Code<TestUser>("ABC");

        string value = (string)code;

        value.Should().Be("ABC");
    }

    [TestMethod]
    public void ToString_ShouldReturnValue()
    {
        var code = new Code<TestUser>("abc");

        code.ToString().Should().Be("ABC");
    }

    [TestMethod]
    public void IPrimitiveValue_ShouldExposeValueMetadata()
    {
        IPrimitiveValue primitive = new Code<TestUser>("ABC");

        primitive.ValueType.Should().Be(typeof(string));
        primitive.BoxedValue.Should().Be("ABC");
    }

    [TestMethod]
    public void IPrimitiveValueOfT_ShouldExposeTypedValue()
    {
        IPrimitiveValue<string> primitive = new Code<TestUser>("ABC");

        primitive.Value.Should().Be("ABC");
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equals_WithSameNormalizedValue_ShouldReturnTrue()
    {
        var left = new Code<TestUser>("abc");
        var right = new Code<TestUser>("ABC");

        left.Equals(right).Should().BeTrue();
        left.Equals((object)right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [TestMethod]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        var left = new Code<TestUser>("ABC");
        var right = new Code<TestUser>("DEF");

        left.Equals(right).Should().BeFalse();
        left.Equals((object)right).Should().BeFalse();
        left.Equals("ABC").Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    #endregion
}

