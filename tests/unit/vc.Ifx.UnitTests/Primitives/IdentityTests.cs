using VisionaryCoder.Framework.Primitives;

namespace VisionaryCoder.Framework.Tests.Primitives;

[TestClass]
public class IdentityTests
{
    private static readonly Guid ValidGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
    private static readonly Guid OtherGuid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    #region Construction

    [TestMethod]
    public void Constructor_WithValidGuid_ShouldCreateIdentity()
    {
        var identity = new Identity<TestUser>(ValidGuid);

        identity.Value.Should().Be(ValidGuid);
    }

    [TestMethod]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        Action act = () => _ = new Identity<TestUser>(Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Identity value cannot be empty*")
            .WithParameterName("value");
    }

    [TestMethod]
    public void Create_WithValidGuid_ShouldCreateIdentity()
    {
        var identity = Identity<TestUser>.Create(ValidGuid);

        identity.Value.Should().Be(ValidGuid);
    }

    [TestMethod]
    public void New_ShouldCreateNonEmptyIdentity()
    {
        var identity = Identity<TestUser>.New();

        identity.Value.Should().NotBe(Guid.Empty);
    }

    #endregion

    #region Parsing

    [TestMethod]
    public void Parse_WithValidGuidText_ShouldReturnIdentity()
    {
        var identity = Identity<TestUser>.Parse(ValidGuid.ToString("D"));

        identity.Value.Should().Be(ValidGuid);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("not-a-guid")]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    public void Parse_WithInvalidText_ShouldThrowFormatException(string? text)
    {
        Action act = () => _ = Identity<TestUser>.Parse(text);

        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid identity value*");
    }

    [TestMethod]
    public void TryParse_WithValidGuidText_ShouldReturnTrue()
    {
        bool result = Identity<TestUser>.TryParse(ValidGuid.ToString("D"), out Identity<TestUser> identity);

        result.Should().BeTrue();
        identity.Value.Should().Be(ValidGuid);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("not-a-guid")]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    public void TryParse_WithInvalidText_ShouldReturnFalse(string? text)
    {
        bool result = Identity<TestUser>.TryParse(text, out Identity<TestUser> identity);

        result.Should().BeFalse();
        identity.Should().Be(default(Identity<TestUser>));
    }

    #endregion

    #region Conversion and metadata

    [TestMethod]
    public void ExplicitGuidConversion_ShouldReturnValue()
    {
        var identity = new Identity<TestUser>(ValidGuid);

        Guid value = (Guid)identity;

        value.Should().Be(ValidGuid);
    }

    [TestMethod]
    public void ToString_ShouldReturnInvariantGuidText()
    {
        var identity = new Identity<TestUser>(ValidGuid);

        string text = identity.ToString();

        text.Should().Be("12345678-1234-1234-1234-123456789012");
    }

    [TestMethod]
    public void IPrimitiveValue_ShouldExposeValueMetadata()
    {
        IPrimitiveValue primitive = new Identity<TestUser>(ValidGuid);

        primitive.ValueType.Should().Be(typeof(Guid));
        primitive.BoxedValue.Should().Be(ValidGuid);
    }

    [TestMethod]
    public void IPrimitiveValueOfT_ShouldExposeTypedValue()
    {
        IPrimitiveValue<Guid> primitive = new Identity<TestUser>(ValidGuid);

        primitive.Value.Should().Be(ValidGuid);
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        var left = new Identity<TestUser>(ValidGuid);
        var right = new Identity<TestUser>(ValidGuid);

        left.Equals(right).Should().BeTrue();
        left.Equals((object)right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [TestMethod]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        var left = new Identity<TestUser>(ValidGuid);
        var right = new Identity<TestUser>(OtherGuid);

        left.Equals(right).Should().BeFalse();
        left.Equals((object)right).Should().BeFalse();
        left.Equals("not-an-identity").Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    #endregion
}

