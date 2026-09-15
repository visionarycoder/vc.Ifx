using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ifx.Tests.Primitives;

[TestClass]
public class EntityIdentifierJsonConverterFactoryTests
{
    private JsonSerializerOptions options = null!;

    [TestInitialize]
    public void Setup()
    {
        options = new JsonSerializerOptions();
        options.Converters.Add(new EntityIdentifierJsonConverterFactory());
    }

    #region CanConvert Tests

    [TestMethod]
    public void CanConvert_WithEntityIdType_ShouldReturnTrue()
    {
        // Arrange
        var factory = new EntityIdentifierJsonConverterFactory();
        Type type = typeof(EntityIdentifier<TestUser, int>);

        // Act
        bool result = factory.CanConvert(type);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(typeof(int))]
    [DataRow(typeof(string))]
    [DataRow(typeof(Guid))]
    [DataRow(typeof(TestUser))]
    public void CanConvert_WithNonEntityIdType_ShouldReturnFalse(Type type)
    {
        // Arrange
        var factory = new EntityIdentifierJsonConverterFactory();

        // Act
        bool result = factory.CanConvert(type);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Serialization Tests - Int

    [TestMethod]
    [DataRow(1)]
    [DataRow(42)]
    [DataRow(999)]
    [DataRow(-1)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void Serialize_WithIntEntityId_ShouldWriteNumber(int value)
    {
        // Arrange
        var id = new EntityIdentifier<TestUser, int>(value);

        // Act
        string json = JsonSerializer.Serialize(id, options);

        // Assert
        json.Should().Be(value.ToString());
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(999)]
    public void Deserialize_WithIntNumber_ShouldCreateEntityId(int value)
    {
        // Arrange
        string json = value.ToString();

        // Act
        EntityIdentifier<TestUser, int> id = JsonSerializer.Deserialize<EntityIdentifier<TestUser, int>>(json, options);

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void SerializeDeserialize_WithIntEntityId_ShouldRoundTrip()
    {
        // Arrange
        var original = new EntityIdentifier<TestUser, int>(42);

        // Act
        string json = JsonSerializer.Serialize(original, options);
        EntityIdentifier<TestUser, int> deserialized = JsonSerializer.Deserialize<EntityIdentifier<TestUser, int>>(json, options);

        // Assert
        deserialized.Should().Be(original);
    }

    #endregion

    #region Serialization Tests - String

    [TestMethod]
    [DataRow("user-123")]
    [DataRow("test-id")]
    [DataRow("a")]
    [DataRow("very-long-id-with-many-characters-and-numbers-123456789")]
    public void Serialize_WithStringEntityId_ShouldWriteString(string value)
    {
        // Arrange
        var id = new EntityIdentifier<TestUser, string>(value);

        // Act
        string json = JsonSerializer.Serialize(id, options);

        // Assert
        json.Should().Be($"\"{value}\"");
    }

    [TestMethod]
    [DataRow("user-123")]
    [DataRow("test")]
    public void Deserialize_WithString_ShouldCreateEntityId(string value)
    {
        // Arrange
        string json = $"\"{value}\"";

        // Act
        EntityIdentifier<TestUser, string> id = JsonSerializer.Deserialize<EntityIdentifier<TestUser, string>>(json, options);

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void SerializeDeserialize_WithStringEntityId_ShouldRoundTrip()
    {
        // Arrange
        var original = new EntityIdentifier<TestUser, string>("test-user-id-123");

        // Act
        string json = JsonSerializer.Serialize(original, options);
        EntityIdentifier<TestUser, string> deserialized = JsonSerializer.Deserialize<EntityIdentifier<TestUser, string>>(json, options);

        // Assert
        deserialized.Should().Be(original);
    }

    [TestMethod]
    public void Serialize_WithStringContainingSpecialCharacters_ShouldPreserveCharacters()
    {
        // Arrange
        var id = new EntityIdentifier<TestUser, string>("id\"with\\special/chars");

        // Act
        string json = JsonSerializer.Serialize(id, options);
        EntityIdentifier<TestUser, string> deserialized = JsonSerializer.Deserialize<EntityIdentifier<TestUser, string>>(json, options);

        // Assert
        deserialized.Value.Should().Be("id\"with\\special/chars");
    }

    #endregion

    #region Serialization Tests - Guid

    [TestMethod]
    public void Serialize_WithGuidEntityId_ShouldWriteGuidString()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var id = new EntityIdentifier<TestProduct, Guid>(guid);

        // Act
        string json = JsonSerializer.Serialize(id, options);

        // Assert
        json.Should().Be("\"12345678-1234-1234-1234-123456789012\"");
    }

    [TestMethod]
    public void Deserialize_WithGuidString_ShouldCreateEntityId()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        string json = "\"12345678-1234-1234-1234-123456789012\"";

        // Act
        EntityIdentifier<TestProduct, Guid> id = JsonSerializer.Deserialize<EntityIdentifier<TestProduct, Guid>>(json, options);

        // Assert
        id.Value.Should().Be(guid);
    }

    [TestMethod]
    public void SerializeDeserialize_WithGuidEntityId_ShouldRoundTrip()
    {
        // Arrange
        var original = new EntityIdentifier<TestProduct, Guid>(Guid.NewGuid());

        // Act
        string json = JsonSerializer.Serialize(original, options);
        EntityIdentifier<TestProduct, Guid> deserialized = JsonSerializer.Deserialize<EntityIdentifier<TestProduct, Guid>>(json, options);

        // Assert
        deserialized.Should().Be(original);
    }

    #endregion

    #region Serialization Tests - Long

    [TestMethod]
    [DataRow(1L)]
    [DataRow(9999999999L)]
    [DataRow(long.MaxValue)]
    [DataRow(long.MinValue)]
    public void Serialize_WithLongEntityId_ShouldWriteNumber(long value)
    {
        // Arrange
        var id = new EntityIdentifier<TestOrder, long>(value);

        // Act
        string json = JsonSerializer.Serialize(id, options);

        // Assert
        json.Should().Be(value.ToString());
    }

    [TestMethod]
    [DataRow(1L)]
    [DataRow(999L)]
    public void Deserialize_WithLongNumber_ShouldCreateEntityId(long value)
    {
        // Arrange
        string json = value.ToString();

        // Act
        EntityIdentifier<TestOrder, long> id = JsonSerializer.Deserialize<EntityIdentifier<TestOrder, long>>(json, options);

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void SerializeDeserialize_WithLongEntityId_ShouldRoundTrip()
    {
        // Arrange
        var original = new EntityIdentifier<TestOrder, long>(9999999999L);

        // Act
        string json = JsonSerializer.Serialize(original, options);
        EntityIdentifier<TestOrder, long> deserialized = JsonSerializer.Deserialize<EntityIdentifier<TestOrder, long>>(json, options);

        // Assert
        deserialized.Should().Be(original);
    }

    #endregion

    #region Serialization Tests - Short

    [TestMethod]
    [DataRow((short)1)]
    [DataRow((short)999)]
    [DataRow(short.MaxValue)]
    [DataRow(short.MinValue)]
    public void Serialize_WithShortEntityId_ShouldWriteNumber(short value)
    {
        // Arrange
        var id = new EntityIdentifier<TestOrder, short>(value);

        // Act
        string json = JsonSerializer.Serialize(id, options);

        // Assert
        json.Should().Be(value.ToString());
    }

    [TestMethod]
    [DataRow((short)1)]
    [DataRow((short)100)]
    public void Deserialize_WithShortNumber_ShouldCreateEntityId(short value)
    {
        // Arrange
        string json = value.ToString();

        // Act
        EntityIdentifier<TestOrder, short> id = JsonSerializer.Deserialize<EntityIdentifier<TestOrder, short>>(json, options);

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void SerializeDeserialize_WithShortEntityId_ShouldRoundTrip()
    {
        // Arrange
        var original = new EntityIdentifier<TestOrder, short>(12345);

        // Act
        string json = JsonSerializer.Serialize(original, options);
        EntityIdentifier<TestOrder, short> deserialized = JsonSerializer.Deserialize<EntityIdentifier<TestOrder, short>>(json, options);

        // Assert
        deserialized.Should().Be(original);
    }

    #endregion

    #region Complex Object Serialization Tests

    [TestMethod]
    public void Serialize_ObjectWithEntityIdProperty_ShouldSerializeCorrectly()
    {
        // Arrange
        var obj = new { UserId = new EntityIdentifier<TestUser, int>(42), Name = "Test" };

        // Act
        string json = JsonSerializer.Serialize(obj, options);

        // Assert
        json.Should().Contain("\"UserId\":42");
        json.Should().Contain("\"Name\":\"Test\"");
    }

    [TestMethod]
    public void Serialize_ArrayOfEntityIds_ShouldSerializeCorrectly()
    {
        // Arrange
        EntityIdentifier<TestUser, int>[] ids =
        [
            new EntityIdentifier<TestUser, int>(1),
            new EntityIdentifier<TestUser, int>(2),
            new EntityIdentifier<TestUser, int>(3)
        ];

        // Act
        string json = JsonSerializer.Serialize(ids, options);

        // Assert
        json.Should().Be("[1,2,3]");
    }

    [TestMethod]
    public void Deserialize_ArrayOfEntityIds_ShouldDeserializeCorrectly()
    {
        // Arrange
        string json = "[1,2,3]";

        // Act
        EntityIdentifier<TestUser, int>[]? ids = JsonSerializer.Deserialize<EntityIdentifier<TestUser, int>[]>(json, options);

        // Assert
        ids.Should().NotBeNull();
        ids.Should().HaveCount(3);
        ids![0].Value.Should().Be(1);
        ids[1].Value.Should().Be(2);
        ids[2].Value.Should().Be(3);
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    public void Deserialize_WithNullForString_ShouldCreateEntityIdWithEmptyString()
    {
        // Arrange
        string json = "null";

        // Act
        EntityIdentifier<TestUser, string> id = JsonSerializer.Deserialize<EntityIdentifier<TestUser, string>>(json, options);

        // Assert
        id.Value.Should().Be(string.Empty);
    }

    [TestMethod]
    public void Deserialize_WithInvalidJsonForInt_ShouldThrowJsonException()
    {
        // Arrange
        string json = "\"not-a-number\"";

        // Act
        Action act = () => JsonSerializer.Deserialize<EntityIdentifier<TestUser, int>>(json, options);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [TestMethod]
    public void Deserialize_WithInvalidJsonForGuid_ShouldThrowJsonException()
    {
        // Arrange
        string json = "\"not-a-guid\"";

        // Act
        Action act = () => JsonSerializer.Deserialize<EntityIdentifier<TestProduct, Guid>>(json, options);

        // Assert
        act.Should().Throw<JsonException>();
    }

    #endregion

    #region Edge Cases Tests

    [TestMethod]
    public void Serialize_WithUnicodeInString_ShouldPreserveUnicode()
    {
        // Arrange
        var id = new EntityIdentifier<TestUser, string>("用户-émile-123");

        // Act
        string json = JsonSerializer.Serialize(id, options);
        EntityIdentifier<TestUser, string> deserialized = JsonSerializer.Deserialize<EntityIdentifier<TestUser, string>>(json, options);

        // Assert
        deserialized.Value.Should().Be("用户-émile-123");
    }

    [TestMethod]
    public void Serialize_WithMaxIntValue_ShouldSerializeCorrectly()
    {
        // Arrange
        var id = new EntityIdentifier<TestUser, int>(int.MaxValue);

        // Act
        string json = JsonSerializer.Serialize(id, options);

        // Assert
        json.Should().Be("2147483647");
    }

    [TestMethod]
    public void Serialize_WithMinIntValue_ShouldSerializeCorrectly()
    {
        // Arrange
        var id = new EntityIdentifier<TestUser, int>(int.MinValue);

        // Act
        string json = JsonSerializer.Serialize(id, options);

        // Assert
        json.Should().Be("-2147483648");
    }

    [TestMethod]
    public void Serialize_WithEmptyGuid_ShouldSerializeAsZeroGuid()
    {
        // Arrange
        var id = new EntityIdentifier<TestProduct, Guid>(Guid.Empty);

        // Act
        string json = JsonSerializer.Serialize(id, options);

        // Assert
        json.Should().Be("\"00000000-0000-0000-0000-000000000000\"");
    }

    #endregion

    #region CreateConverter Tests

    [TestMethod]
    public void CreateConverter_WithValidEntityIdType_ShouldReturnConverter()
    {
        // Arrange
        var factory = new EntityIdentifierJsonConverterFactory();
        Type type = typeof(EntityIdentifier<TestUser, int>);

        // Act
        JsonConverter converter = factory.CreateConverter(type, options);

        // Assert
        converter.Should().NotBeNull();
    }

    [TestMethod]
    public void CreateConverter_WithDifferentEntityTypes_ShouldReturnDifferentConverters()
    {
        // Arrange
        var factory = new EntityIdentifierJsonConverterFactory();
        Type type1 = typeof(EntityIdentifier<TestUser, int>);
        Type type2 = typeof(EntityIdentifier<TestProduct, Guid>);

        // Act
        JsonConverter converter1 = factory.CreateConverter(type1, options);
        JsonConverter converter2 = factory.CreateConverter(type2, options);

        // Assert
        converter1.Should().NotBeNull();
        converter2.Should().NotBeNull();
        converter1.GetType().Should().NotBe(converter2.GetType());
    }

    #endregion
}
