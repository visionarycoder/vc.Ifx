using System.Text.RegularExpressions;
using Ifx.Abstractions;

namespace Ifx.Tests.Primitives;

[TestClass]
public sealed class IdentifierTests
{
    private static readonly Guid ValidGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
    private static readonly Guid OtherGuid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [TestMethod]
    public void Constructor_WithGuid_ShouldStoreValue()
    {
        var identifier = new Identifier<TestUser>(ValidGuid);

        identifier.Value.Should().Be(ValidGuid);
        identifier.ValueType.Should().Be(typeof(Guid));
        identifier.BoxedValue.Should().Be(ValidGuid);
    }

    [TestMethod]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        Action act = () => _ = new Identifier<TestUser>(Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Identifier value cannot be empty*")
            .WithParameterName("value");
    }

    [TestMethod]
    public void FactoryMethods_ShouldCreateIdentifiers()
    {
        Identifier<TestUser> created = Identifier<TestUser>.Create(ValidGuid);
        Identifier<TestUser> random = Identifier<TestUser>.New();
        Identifier<TestUser> sequential = Identifier<TestUser>.NewSequential();

        created.Value.Should().Be(ValidGuid);
        random.Value.Should().NotBe(Guid.Empty);
        sequential.Value.Should().NotBe(Guid.Empty);
        sequential.ToString()[14].Should().Be('7');
    }

    [TestMethod]
    public void Parse_WithValidText_ShouldReturnIdentifier()
    {
        Identifier<TestUser> identifier = Identifier<TestUser>.Parse(ValidGuid.ToString("D"));

        identifier.Value.Should().Be(ValidGuid);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("not-a-guid")]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    public void Parse_WithInvalidText_ShouldThrowFormatException(string? text)
    {
        Action act = () => _ = Identifier<TestUser>.Parse(text);

        act.Should().Throw<FormatException>().WithMessage("*Invalid identity value*");
    }

    [TestMethod]
    public void TryParse_WithValidText_ShouldReturnIdentifier()
    {
        bool result = Identifier<TestUser>.TryParse(ValidGuid.ToString("D"), out Identifier<TestUser> identifier);

        result.Should().BeTrue();
        identifier.Value.Should().Be(ValidGuid);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("not-a-guid")]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    public void TryParse_WithInvalidText_ShouldReturnFalse(string? text)
    {
        bool result = Identifier<TestUser>.TryParse(text, out Identifier<TestUser> identifier);

        result.Should().BeFalse();
        identifier.Value.Should().Be(Guid.Empty);
    }

    [TestMethod]
    public void ConversionAndFormattingMembers_ShouldExposeInvariantGuid()
    {
        var identifier = new Identifier<TestUser>(ValidGuid);
        Guid value = (Guid)identifier;

        value.Should().Be(ValidGuid);
        identifier.ToString().Should().Be("12345678-1234-1234-1234-123456789012");
        Regex.IsMatch(identifier.ToString(), "^[0-9a-f-]{36}$").Should().BeTrue();
    }

    [TestMethod]
    public void EqualityMembers_ShouldCompareValue()
    {
        var left = new Identifier<TestUser>(ValidGuid);
        var same = new Identifier<TestUser>(ValidGuid);
        var different = new Identifier<TestUser>(OtherGuid);

        left.Equals(same).Should().BeTrue();
        left.Equals((object)same).Should().BeTrue();
        left.GetHashCode().Should().Be(same.GetHashCode());
        (left == same).Should().BeTrue();
        (left != same).Should().BeFalse();
        left.Equals(different).Should().BeFalse();
        left.Equals((object)different).Should().BeFalse();
        left.Equals("not-an-identifier").Should().BeFalse();
        (left == different).Should().BeFalse();
        (left != different).Should().BeTrue();
    }

    [TestMethod]
    public void PrimitiveInterfaces_ShouldExposeTypedValue()
    {
        IPrimitiveValue primitive = new Identifier<TestUser>(ValidGuid);
        IPrimitiveValue<Guid> typedPrimitive = new Identifier<TestUser>(ValidGuid);

        primitive.ValueType.Should().Be(typeof(Guid));
        primitive.BoxedValue.Should().Be(ValidGuid);
        typedPrimitive.Value.Should().Be(ValidGuid);
    }
}
