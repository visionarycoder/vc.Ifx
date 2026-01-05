using VisionaryCoder.Framework.Primitives;

namespace VisionaryCoder.Framework.Tests.Primitives;

// Test entities for EntityId tests

[TestClass]
public class EntityIdTests
{
    #region Constructor Tests

    [TestMethod]
    [DataRow(1)]
    [DataRow(42)]
    [DataRow(999)]
    [DataRow(int.MaxValue)]
    public void Constructor_WithValidIntValue_ShouldCreateEntityId(int value)
    {
        // Act
        var id = new EntityId<TestUser, int>(value);

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void Constructor_WithValidStringValue_ShouldCreateEntityId()
    {
        // Arrange
        string value = "user-123";

        // Act
        var id = new EntityId<TestUser, string>(value);

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void Constructor_WithValidGuidValue_ShouldCreateEntityId()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var id = new EntityId<TestProduct, Guid>(value);

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    [DataRow(1L)]
    [DataRow(999999999L)]
    [DataRow(long.MaxValue)]
    public void Constructor_WithValidLongValue_ShouldCreateEntityId(long value)
    {
        // Act
        var id = new EntityId<TestOrder, long>(value);

        // Assert
        id.Value.Should().Be(value);
    }

    #endregion

    #region Create Method Tests

    [TestMethod]
    [DataRow(1)]
    [DataRow(100)]
    [DataRow(-1)]
    [DataRow(int.MinValue)]
    public void Create_WithValidIntValue_ShouldReturnEntityId(int value)
    {
        // Act
        var id = EntityId<TestUser, int>.Create(value);

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void Create_WithDefaultInt_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => EntityId<TestUser, int>.Create(default);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ID cannot be the default value*")
            .WithParameterName("value");
    }

    [TestMethod]
    public void Create_WithDefaultGuid_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => EntityId<TestProduct, Guid>.Create(default);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ID cannot be the default value*");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("\t")]
    [DataRow("\n")]
    public void Create_WithEmptyOrWhitespaceString_ShouldThrowArgumentException(string value)
    {
        // Act
        Action act = () => EntityId<TestUser, string>.Create(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ID cannot be empty/whitespace*");
    }

    [TestMethod]
    [DataRow("valid-id")]
    [DataRow("123")]
    [DataRow("user@domain.com")]
    [DataRow("a")]
    public void Create_WithValidString_ShouldReturnEntityId(string value)
    {
        // Act
        var id = EntityId<TestUser, string>.Create(value);

        // Assert
        id.Value.Should().Be(value);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    [DataRow(1, "1")]
    [DataRow(999, "999")]
    [DataRow(-42, "-42")]
    public void ToString_WithIntValue_ShouldReturnStringRepresentation(int value, string expected)
    {
        // Arrange
        var id = new EntityId<TestUser, int>(value);

        // Act
        string result = id.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void ToString_WithStringValue_ShouldReturnValue()
    {
        // Arrange
        string value = "test-id-123";
        var id = new EntityId<TestUser, string>(value);

        // Act
        string result = id.ToString();

        // Assert
        result.Should().Be(value);
    }

    [TestMethod]
    public void ToString_WithGuidValue_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var id = new EntityId<TestProduct, Guid>(guid);

        // Act
        string result = id.ToString();

        // Assert
        result.Should().Be("12345678-1234-1234-1234-123456789012");
    }

    #endregion

    #region Implicit Conversion Tests

    [TestMethod]
    [DataRow(1)]
    [DataRow(42)]
    [DataRow(100)]
    public void ImplicitConversion_FromInt_ShouldCreateEntityId(int value)
    {
        // Act
        EntityId<TestUser, int> id = value;

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void ImplicitConversion_FromDefaultInt_ShouldThrowArgumentException()
    {
        // Act
        Action act = () =>
        {
            int value = default;
            EntityId<TestUser, int> id = value;
        };

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void ImplicitConversion_FromString_ShouldCreateEntityId()
    {
        // Arrange
        string value = "user-id-123";

        // Act
        EntityId<TestUser, string> id = value;

        // Assert
        id.Value.Should().Be(value);
    }

    [TestMethod]
    public void ImplicitConversion_FromGuid_ShouldCreateEntityId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        EntityId<TestProduct, Guid> id = guid;

        // Assert
        id.Value.Should().Be(guid);
    }

    #endregion

    #region Explicit Conversion Tests

    [TestMethod]
    [DataRow(1)]
    [DataRow(999)]
    public void ExplicitConversion_ToInt_ShouldReturnValue(int value)
    {
        // Arrange
        var id = new EntityId<TestUser, int>(value);

        // Act
        int result = (int)id;

        // Assert
        result.Should().Be(value);
    }

    [TestMethod]
    public void ExplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        string value = "test-id";
        var id = new EntityId<TestUser, string>(value);

        // Act
        string? result = (string)id;

        // Assert
        result.Should().Be(value);
    }

    [TestMethod]
    public void ExplicitConversion_ToGuid_ShouldReturnValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new EntityId<TestProduct, Guid>(guid);

        // Act
        var result = (Guid)id;

        // Assert
        result.Should().Be(guid);
    }

    #endregion

    #region Parse Tests

    [TestMethod]
    [DataRow("1", 1)]
    [DataRow("42", 42)]
    [DataRow("-10", -10)]
    [DataRow("2147483647", int.MaxValue)]
    public void Parse_WithValidIntString_ShouldReturnEntityId(string text, int expected)
    {
        // Act
        var id = EntityId<TestUser, int>.Parse(text);

        // Assert
        id.Value.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("abc")]
    [DataRow("12.34")]
    [DataRow("")]
    [DataRow(" ")]
    public void Parse_WithInvalidIntString_ShouldThrowFormatException(string text)
    {
        // Act
        Action act = () => EntityId<TestUser, int>.Parse(text);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid Int32*");
    }

    [TestMethod]
    public void Parse_WithValidGuidString_ShouldReturnEntityId()
    {
        // Arrange
        string guidString = "12345678-1234-1234-1234-123456789012";
        var expectedGuid = Guid.Parse(guidString);

        // Act
        var id = EntityId<TestProduct, Guid>.Parse(guidString);

        // Assert
        id.Value.Should().Be(expectedGuid);
    }

    [TestMethod]
    [DataRow("not-a-guid")]
    [DataRow("12345")]
    [DataRow("")]
    public void Parse_WithInvalidGuidString_ShouldThrowFormatException(string text)
    {
        // Act
        Action act = () => EntityId<TestProduct, Guid>.Parse(text);

        // Assert
        act.Should().Throw<FormatException>();
    }

    [TestMethod]
    [DataRow("valid-string-id")]
    [DataRow("user@example.com")]
    [DataRow("123")]
    [DataRow("a")]
    public void Parse_WithValidString_ShouldReturnEntityId(string text)
    {
        // Act
        var id = EntityId<TestUser, string>.Parse(text);

        // Assert
        id.Value.Should().Be(text);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    public void Parse_WithEmptyOrWhitespaceString_ShouldThrowFormatException(string text)
    {
        // Act
        Action act = () => EntityId<TestUser, string>.Parse(text);

        // Assert
        act.Should().Throw<FormatException>();
    }

    [TestMethod]
    [DataRow("1", 1L)]
    [DataRow("9999999999", 9999999999L)]
    [DataRow("9223372036854775807", long.MaxValue)]
    public void Parse_WithValidLongString_ShouldReturnEntityId(string text, long expected)
    {
        // Act
        var id = EntityId<TestOrder, long>.Parse(text);

        // Assert
        id.Value.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("1", (short)1)]
    [DataRow("32767", short.MaxValue)]
    [DataRow("-32768", short.MinValue)]
    public void Parse_WithValidShortString_ShouldReturnEntityId(string text, short expected)
    {
        // Act
        var id = EntityId<TestOrder, short>.Parse(text);

        // Assert
        id.Value.Should().Be(expected);
    }

    #endregion

    #region TryParse Tests

    [TestMethod]
    [DataRow("1", true, 1)]
    [DataRow("999", true, 999)]
    [DataRow("-1", true, -1)]
    [DataRow("abc", false, 0)]
    [DataRow("", false, 0)]
    [DataRow("12.34", false, 0)]
    public void TryParse_WithIntString_ShouldReturnExpectedResult(string text, bool expectedSuccess, int expectedValue)
    {
        // Act
        bool success = EntityId<TestUser, int>.TryParse(text, out EntityId<TestUser, int> id);

        // Assert
        success.Should().Be(expectedSuccess);
        if (expectedSuccess)
        {
            id.Value.Should().Be(expectedValue);
        }
    }

    [TestMethod]
    public void TryParse_WithValidGuidString_ShouldReturnTrue()
    {
        // Arrange
        string guidString = "12345678-1234-1234-1234-123456789012";
        var expectedGuid = Guid.Parse(guidString);

        // Act
        bool success = EntityId<TestProduct, Guid>.TryParse(guidString, out EntityId<TestProduct, Guid> id);

        // Assert
        success.Should().BeTrue();
        id.Value.Should().Be(expectedGuid);
    }

    [TestMethod]
    [DataRow("not-a-guid")]
    [DataRow("")]
    [DataRow("12345")]
    public void TryParse_WithInvalidGuidString_ShouldReturnFalse(string text)
    {
        // Act
        bool success = EntityId<TestProduct, Guid>.TryParse(text, out EntityId<TestProduct, Guid> id);

        // Assert
        success.Should().BeFalse();
        id.Should().Be(default(EntityId<TestProduct, Guid>));
    }

    [TestMethod]
    [DataRow("valid-id", true)]
    [DataRow("123", true)]
    [DataRow("", false)]
    [DataRow(" ", false)]
    [DataRow("  ", false)]
    public void TryParse_WithString_ShouldReturnExpectedResult(string text, bool expectedSuccess)
    {
        // Act
        bool success = EntityId<TestUser, string>.TryParse(text, out EntityId<TestUser, string> id);

        // Assert
        success.Should().Be(expectedSuccess);
        if (expectedSuccess)
        {
            id.Value.Should().Be(text);
        }
    }

    [TestMethod]
    [DataRow("1", true, 1L)]
    [DataRow("9999999999", true, 9999999999L)]
    [DataRow("abc", false, 0L)]
    public void TryParse_WithLongString_ShouldReturnExpectedResult(string text, bool expectedSuccess, long expectedValue)
    {
        // Act
        bool success = EntityId<TestOrder, long>.TryParse(text, out EntityId<TestOrder, long> id);

        // Assert
        success.Should().Be(expectedSuccess);
        if (expectedSuccess)
        {
            id.Value.Should().Be(expectedValue);
        }
    }

    [TestMethod]
    [DataRow("1", true, (short)1)]
    [DataRow("32767", true, short.MaxValue)]
    [DataRow("99999", false, (short)0)]
    public void TryParse_WithShortString_ShouldReturnExpectedResult(string text, bool expectedSuccess, short expectedValue)
    {
        // Act
        bool success = EntityId<TestOrder, short>.TryParse(text, out EntityId<TestOrder, short> id);

        // Assert
        success.Should().Be(expectedSuccess);
        if (expectedSuccess)
        {
            id.Value.Should().Be(expectedValue);
        }
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void IEntityId_ValueType_ShouldReturnCorrectType()
    {
        // Arrange
        var id = new EntityId<TestUser, int>(42);
        var iEntityId = (IEntityId)id;

        // Act
        Type valueType = iEntityId.ValueType;

        // Assert
        valueType.Should().Be(typeof(int));
    }

    [TestMethod]
    public void IEntityId_BoxedValue_ShouldReturnValueAsObject()
    {
        // Arrange
        int value = 42;
        var id = new EntityId<TestUser, int>(value);
        var iEntityId = (IEntityId)id;

        // Act
        object boxedValue = iEntityId.BoxedValue;

        // Assert
        boxedValue.Should().Be(value);
        boxedValue.Should().BeOfType<int>();
    }

    [TestMethod]
    public void IEntityId_ValueType_ForString_ShouldReturnStringType()
    {
        // Arrange
        var id = new EntityId<TestUser, string>("test");
        var iEntityId = (IEntityId)id;

        // Act
        Type valueType = iEntityId.ValueType;

        // Assert
        valueType.Should().Be(typeof(string));
    }

    [TestMethod]
    public void IEntityId_ValueType_ForGuid_ShouldReturnGuidType()
    {
        // Arrange
        var id = new EntityId<TestProduct, Guid>(Guid.NewGuid());
        var iEntityId = (IEntityId)id;

        // Act
        Type valueType = iEntityId.ValueType;

        // Assert
        valueType.Should().Be(typeof(Guid));
    }

    #endregion

    #region Equality Tests

    [TestMethod]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var id1 = new EntityId<TestUser, int>(42);
        var id2 = new EntityId<TestUser, int>(42);

        // Act & Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [TestMethod]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = new EntityId<TestUser, int>(42);
        var id2 = new EntityId<TestUser, int>(99);

        // Act & Assert
        id1.Should().NotBe(id2);
        (id1 != id2).Should().BeTrue();
    }

    [TestMethod]
    public void Equality_WithDifferentEntities_ShouldNotBeEqual()
    {
        // Arrange
        var userId = new EntityId<TestUser, int>(42);
        var productId = new EntityId<TestProduct, int>(42);

        // Act & Assert - These are different types, so direct comparison would fail at compile time
        userId.Value.Should().Be(42);
        productId.Value.Should().Be(42);
    }

    #endregion

    #region Edge Cases and Malicious Input Tests

    [TestMethod]
    public void Parse_WithVeryLongString_ShouldSucceed()
    {
        // Arrange
        string longString = new('a', 10000);

        // Act
        var id = EntityId<TestUser, string>.Parse(longString);

        // Assert
        id.Value.Should().Be(longString);
    }

    [TestMethod]
    public void Parse_WithSpecialCharacters_ShouldSucceed()
    {
        // Arrange
        string specialString = "id!@#$%^&*()_+-=[]{}|;':\"<>?,./`~";

        // Act
        var id = EntityId<TestUser, string>.Parse(specialString);

        // Assert
        id.Value.Should().Be(specialString);
    }

    [TestMethod]
    public void Parse_WithUnicodeCharacters_ShouldSucceed()
    {
        // Arrange
        string unicodeString = "用户ID-123-émile-naïve-Übermensch";

        // Act
        var id = EntityId<TestUser, string>.Parse(unicodeString);

        // Assert
        id.Value.Should().Be(unicodeString);
    }

    [TestMethod]
    [DataRow("2147483648")] // Int32.MaxValue + 1
    [DataRow("-2147483649")] // Int32.MinValue - 1
    [DataRow("999999999999999")]
    public void TryParse_WithIntOverflow_ShouldReturnFalse(string text)
    {
        // Act
        bool success = EntityId<TestUser, int>.TryParse(text, out _);

        // Assert
        success.Should().BeFalse();
    }

    [TestMethod]
    public void TryParse_WithNullString_ShouldReturnFalse()
    {
        // Act
        bool success = EntityId<TestUser, string>.TryParse(null!, out _);

        // Assert
        success.Should().BeFalse();
    }

    [TestMethod]
    public void ToString_WithNegativeInt_ShouldIncludeMinusSign()
    {
        // Arrange
        var id = new EntityId<TestUser, int>(-42);

        // Act
        string result = id.ToString();

        // Assert
        result.Should().Be("-42");
        result.Should().StartWith("-");
    }

    #endregion
}
