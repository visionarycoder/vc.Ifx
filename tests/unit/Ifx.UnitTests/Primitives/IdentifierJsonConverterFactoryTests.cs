using System.Text.Json;

namespace Ifx.Tests.Primitives;

[TestClass]
public sealed class IdentifierJsonConverterFactoryTests
{
    private static readonly Guid ValidGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");

    private readonly JsonSerializerOptions options = new()
    {
        Converters = { new IdentifierJsonConverterFactory() }
    };

    [TestMethod]
    public void CanConvert_ShouldAcceptOnlyClosedIdentifierTypes()
    {
        var factory = new IdentifierJsonConverterFactory();

        factory.CanConvert(typeof(Identifier<TestUser>)).Should().BeTrue();
        factory.CanConvert(typeof(Identifier<>)).Should().BeFalse();
        factory.CanConvert(typeof(Guid)).Should().BeFalse();
        factory.CanConvert(typeof(string)).Should().BeFalse();
    }

    [TestMethod]
    public void CanConvert_WithNullType_ShouldThrowArgumentNullException()
    {
        var factory = new IdentifierJsonConverterFactory();

        Action act = () => factory.CanConvert(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("typeToConvert");
    }

    [TestMethod]
    public void CreateConverter_WithInvalidArguments_ShouldThrow()
    {
        var factory = new IdentifierJsonConverterFactory();

        Action nullOptions = () => factory.CreateConverter(typeof(Identifier<TestUser>), null!);
        Action invalidType = () => factory.CreateConverter(typeof(Guid), options);
        Action nullType = () => factory.CreateConverter(null!, options);

        nullOptions.Should().Throw<ArgumentNullException>().WithParameterName("options");
        invalidType.Should().Throw<ArgumentException>()
            .WithMessage("*closed Identifier type is required*")
            .WithParameterName("type");
        nullType.Should().Throw<ArgumentNullException>().WithParameterName("typeToConvert");
    }

    [TestMethod]
    public void Serializer_ShouldRoundTripIdentifier()
    {
        var original = Identifier<TestUser>.Create(ValidGuid);

        string json = JsonSerializer.Serialize(original, options);
        Identifier<TestUser> deserialized = JsonSerializer.Deserialize<Identifier<TestUser>>(json, options);

        json.Should().Be("\"12345678-1234-1234-1234-123456789012\"");
        deserialized.Should().Be(original);
    }

    [TestMethod]
    public void Serializer_ShouldRejectEmptyIdentifierValue()
    {
        Action act = () => JsonSerializer.Deserialize<Identifier<TestUser>>("\"00000000-0000-0000-0000-000000000000\"", options);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Identifier value cannot be empty*")
            .WithParameterName("value");
    }
}
