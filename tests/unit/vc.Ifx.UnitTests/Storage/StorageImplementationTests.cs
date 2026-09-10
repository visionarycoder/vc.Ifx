using VisionaryCoder.Framework.Storage;

namespace VisionaryCoder.Framework.Tests.Storage;

/// <summary>
/// Data-driven unit tests for the <see cref="StorageImplementation"/> record.
/// Tests storage implementation registration with various scenarios.
/// </summary>
[TestClass]
public class StorageImplementationTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithImplementationType_ShouldSetProperties()
    {
        // Arrange
        Type implementationType = typeof(TestStorageProvider);

        // Act
        var implementation = new StorageImplementation(implementationType);

        // Assert
        implementation.ImplementationType.Should().Be(implementationType);
        implementation.Options.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_WithImplementationTypeAndOptions_ShouldSetBothProperties()
    {
        // Arrange
        Type implementationType = typeof(TestStorageProvider);
        var options = new TestOptions { Setting = "value" };

        // Act
        var implementation = new StorageImplementation(implementationType, options);

        // Assert
        implementation.ImplementationType.Should().Be(implementationType);
        implementation.Options.Should().BeSameAs(options);
    }

    [TestMethod]
    public void Constructor_WithNullOptions_ShouldAcceptNull()
    {
        // Arrange
        Type implementationType = typeof(TestStorageProvider);

        // Act
        var implementation = new StorageImplementation(implementationType, null);

        // Assert
        implementation.ImplementationType.Should().Be(implementationType);
        implementation.Options.Should().BeNull();
    }

    #endregion

    #region ImplementationType Property Tests

    [TestMethod]
    public void ImplementationType_ShouldReturnCorrectType()
    {
        // Arrange
        Type implementationType = typeof(TestStorageProvider);
        var implementation = new StorageImplementation(implementationType);

        // Assert
        implementation.ImplementationType.Should().Be(implementationType);
        implementation.ImplementationType.Name.Should().Be("TestStorageProvider");
    }

    [TestMethod]
    public void ImplementationType_WithDifferentTypes_ShouldWork()
    {
        // Arrange
        Type type1 = typeof(TestStorageProvider);
        Type type2 = typeof(AnotherTestProvider);

        // Act
        var implementation1 = new StorageImplementation(type1);
        var implementation2 = new StorageImplementation(type2);

        // Assert
        implementation1.ImplementationType.Should().NotBe(implementation2.ImplementationType);
    }

    #endregion

    #region Options Property Tests

    [TestMethod]
    public void Options_WhenNull_ShouldBeNull()
    {
        // Arrange
        var implementation = new StorageImplementation(typeof(TestStorageProvider));

        // Assert
        implementation.Options.Should().BeNull();
    }

    [TestMethod]
    public void Options_WithValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var options = new TestOptions { Setting = "test" };
        var implementation = new StorageImplementation(typeof(TestStorageProvider), options);

        // Assert
        implementation.Options.Should().BeSameAs(options);
    }

    [TestMethod]
    public void Options_WithDifferentTypes_ShouldWork()
    {
        // Arrange
        string stringOption = "string-option";
        int intOption = 42;
        var objectOption = new { Key = "Value" };

        // Act
        var impl1 = new StorageImplementation(typeof(TestStorageProvider), stringOption);
        var impl2 = new StorageImplementation(typeof(TestStorageProvider), intOption);
        var impl3 = new StorageImplementation(typeof(TestStorageProvider), objectOption);

        // Assert
        impl1.Options.Should().Be(stringOption);
        impl2.Options.Should().Be(intOption);
        impl3.Options.Should().Be(objectOption);
    }

    #endregion

    #region Record Equality Tests

    [TestMethod]
    public void Equals_WithSameTypeAndNullOptions_ShouldBeEqual()
    {
        // Arrange
        Type type = typeof(TestStorageProvider);
        var implementation1 = new StorageImplementation(type);
        var implementation2 = new StorageImplementation(type);

        // Assert
        implementation1.Should().Be(implementation2);
    }

    [TestMethod]
    public void Equals_WithSameTypeAndSameOptions_ShouldBeEqual()
    {
        // Arrange
        Type type = typeof(TestStorageProvider);
        var options = new TestOptions { Setting = "value" };
        var implementation1 = new StorageImplementation(type, options);
        var implementation2 = new StorageImplementation(type, options);

        // Assert
        implementation1.Should().Be(implementation2);
    }

    [TestMethod]
    public void Equals_WithDifferentTypes_ShouldNotBeEqual()
    {
        // Arrange
        var implementation1 = new StorageImplementation(typeof(TestStorageProvider));
        var implementation2 = new StorageImplementation(typeof(AnotherTestProvider));

        // Assert
        implementation1.Should().NotBe(implementation2);
    }

    [TestMethod]
    public void Equals_WithDifferentOptions_ShouldNotBeEqual()
    {
        // Arrange
        Type type = typeof(TestStorageProvider);
        var options1 = new TestOptions { Setting = "value1" };
        var options2 = new TestOptions { Setting = "value2" };
        var implementation1 = new StorageImplementation(type, options1);
        var implementation2 = new StorageImplementation(type, options2);

        // Assert
        implementation1.Should().NotBe(implementation2);
    }

    #endregion

    #region GetHashCode Tests

    [TestMethod]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        Type type = typeof(TestStorageProvider);
        var options = new TestOptions { Setting = "value" };
        var implementation1 = new StorageImplementation(type, options);
        var implementation2 = new StorageImplementation(type, options);

        // Assert
        implementation1.GetHashCode().Should().Be(implementation2.GetHashCode());
    }

    [TestMethod]
    public void GetHashCode_WithDifferentTypes_ShouldReturnDifferentHashCodes()
    {
        // Arrange
        var implementation1 = new StorageImplementation(typeof(TestStorageProvider));
        var implementation2 = new StorageImplementation(typeof(AnotherTestProvider));

        // Assert
        implementation1.GetHashCode().Should().NotBe(implementation2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_ShouldIncludeImplementationType()
    {
        // Arrange
        var implementation = new StorageImplementation(typeof(TestStorageProvider));

        // Act
        string? result = implementation.ToString();

        // Assert
        result.Should().Contain("TestStorageProvider");
    }

    [TestMethod]
    public void ToString_WithOptions_ShouldIncludeOptions()
    {
        // Arrange
        var options = new TestOptions { Setting = "test" };
        var implementation = new StorageImplementation(typeof(TestStorageProvider), options);

        // Act
        string? result = implementation.ToString();

        // Assert
        result.Should().Contain("TestStorageProvider");
        result.Should().Contain("Options");
    }

    #endregion

    #region Deconstruction Tests

    [TestMethod]
    public void Deconstruct_ShouldExtractBothProperties()
    {
        // Arrange
        Type type = typeof(TestStorageProvider);
        var options = new TestOptions { Setting = "value" };
        var implementation = new StorageImplementation(type, options);

        // Act
        (Type implementationType, object? extractedOptions) = implementation;

        // Assert
        implementationType.Should().Be(type);
        extractedOptions.Should().BeSameAs(options);
    }

    [TestMethod]
    public void Deconstruct_WithNullOptions_ShouldWork()
    {
        // Arrange
        Type type = typeof(TestStorageProvider);
        var implementation = new StorageImplementation(type);

        // Act
        (Type implementationType, object? options) = implementation;

        // Assert
        implementationType.Should().Be(type);
        options.Should().BeNull();
    }

    #endregion

    #region With Expression Tests

    [TestMethod]
    public void WithExpression_ModifyingImplementationType_ShouldCreateNewInstance()
    {
        // Arrange
        var original = new StorageImplementation(typeof(TestStorageProvider), "options");
        Type newType = typeof(AnotherTestProvider);

        // Act
        StorageImplementation modified = original with { ImplementationType = newType };

        // Assert
        modified.ImplementationType.Should().Be(newType);
        modified.Options.Should().Be("options");
        original.ImplementationType.Should().Be(typeof(TestStorageProvider));
    }

    [TestMethod]
    public void WithExpression_ModifyingOptions_ShouldCreateNewInstance()
    {
        // Arrange
        var original = new StorageImplementation(typeof(TestStorageProvider), "original");
        string newOptions = "modified";

        // Act
        StorageImplementation modified = original with { Options = newOptions };

        // Assert
        modified.Options.Should().Be(newOptions);
        modified.ImplementationType.Should().Be(typeof(TestStorageProvider));
        original.Options.Should().Be("original");
    }

    #endregion

    #region Edge Cases Tests

    [TestMethod]
    public void Constructor_WithAbstractType_ShouldAccept()
    {
        // Arrange
        Type abstractType = typeof(AbstractTestProvider);

        // Act
        var implementation = new StorageImplementation(abstractType);

        // Assert
        implementation.ImplementationType.Should().Be(abstractType);
    }

    [TestMethod]
    public void Constructor_WithInterfaceType_ShouldAccept()
    {
        // Arrange
        Type interfaceType = typeof(ITestProvider);

        // Act
        var implementation = new StorageImplementation(interfaceType);

        // Assert
        implementation.ImplementationType.Should().Be(interfaceType);
    }

    [TestMethod]
    public void Constructor_WithGenericType_ShouldWork()
    {
        // Arrange
        Type genericType = typeof(GenericTestProvider<string>);

        // Act
        var implementation = new StorageImplementation(genericType);

        // Assert
        implementation.ImplementationType.Should().Be(genericType);
    }

    #endregion

    #region Type System Tests

    [TestMethod]
    public void StorageImplementation_ShouldBeRecord()
    {
        // Arrange & Act
        Type type = typeof(StorageImplementation);

        // Assert
        type.GetMethod("<Clone>$", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Should().NotBeNull("records have a public <Clone>$ method");
    }

    [TestMethod]
    public void StorageImplementation_ShouldBeSealed()
    {
        // Arrange & Act
        Type type = typeof(StorageImplementation);

        // Assert
        type.IsSealed.Should().BeTrue();
    }

    #endregion

    #region Test Helper Classes

    private class TestStorageProvider { }
    private class AnotherTestProvider { }
    private abstract class AbstractTestProvider { }
    private interface ITestProvider { }
    private class GenericTestProvider<T> { }
    private class TestOptions
    {
        public string Setting { get; set; } = string.Empty;
    }

    #endregion
}
