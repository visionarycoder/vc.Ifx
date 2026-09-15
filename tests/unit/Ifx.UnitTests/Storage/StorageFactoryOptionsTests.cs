using Ifx.Storage;

namespace Ifx.Tests.Storage;

/// <summary>
/// Data-driven unit tests for <see cref="StorageFactoryOptions"/> class.
/// Tests storage factory configuration with various scenarios.
/// </summary>
[TestClass]
public class StorageFactoryOptionsTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_ShouldInitializeEmptyImplementations()
    {
        // Act
        var options = new StorageFactoryOptions();

        // Assert
        options.Implementations.Should().NotBeNull();
        options.Implementations.Should().BeEmpty();
    }

    #endregion

    #region Implementations Property Tests

    [TestMethod]
    public void Implementations_ShouldBeReadOnly()
    {
        // Arrange
        var options = new StorageFactoryOptions();

        // Assert
        options.Implementations.Should().BeAssignableTo<IReadOnlyDictionary<string, StorageImplementation>>();
    }

    [TestMethod]
    public void Implementations_AfterRegistration_ShouldContainImplementation()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        Type implementationType = typeof(TestStorageProvider);

        // Act
        options.RegisterImplementation("test", implementationType, null);

        // Assert
        options.Implementations.Should().ContainKey("test");
        options.Implementations["test"].ImplementationType.Should().Be(implementationType);
    }

    #endregion

    #region RegisterImplementation Tests

    [TestMethod]
    public void RegisterImplementation_WithValidParameters_ShouldAddImplementation()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        Type implementationType = typeof(TestStorageProvider);

        // Act
        options.RegisterImplementation("local", implementationType, null);

        // Assert
        options.Implementations.Should().HaveCount(1);
        options.Implementations.Should().ContainKey("local");
        options.Implementations["local"].ImplementationType.Should().Be(implementationType);
        options.Implementations["local"].Options.Should().BeNull();
    }

    [TestMethod]
    public void RegisterImplementation_WithOptions_ShouldStoreOptions()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        Type implementationType = typeof(TestStorageProvider);
        var testOptions = new TestOptions { Setting = "value" };

        // Act
        options.RegisterImplementation("ftp", implementationType, testOptions);

        // Assert
        options.Implementations["ftp"].Options.Should().BeSameAs(testOptions);
    }

    [TestMethod]
    public void RegisterImplementation_WithMultipleImplementations_ShouldStoreAll()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        Type type1 = typeof(TestStorageProvider);
        Type type2 = typeof(AnotherTestProvider);

        // Act
        options.RegisterImplementation("local", type1, null);
        options.RegisterImplementation("ftp", type2, null);

        // Assert
        options.Implementations.Should().HaveCount(2);
        options.Implementations["local"].ImplementationType.Should().Be(type1);
        options.Implementations["ftp"].ImplementationType.Should().Be(type2);
    }

    [TestMethod]
    public void RegisterImplementation_WithDuplicateName_ShouldOverwrite()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        Type type1 = typeof(TestStorageProvider);
        Type type2 = typeof(AnotherTestProvider);

        // Act
        options.RegisterImplementation("provider", type1, null);
        options.RegisterImplementation("provider", type2, null);

        // Assert
        options.Implementations.Should().HaveCount(1);
        options.Implementations["provider"].ImplementationType.Should().Be(type2);
    }

    [TestMethod]
    public void RegisterImplementation_WithDifferentOptionTypes_ShouldWork()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        Type implementationType = typeof(TestStorageProvider);
        string stringOptions = "string-option";
        int intOptions = 42;
        var objectOptions = new { Key = "Value" };

        // Act
        options.RegisterImplementation("string", implementationType, stringOptions);
        options.RegisterImplementation("int", implementationType, intOptions);
        options.RegisterImplementation("object", implementationType, objectOptions);

        // Assert
        options.Implementations["string"].Options.Should().Be(stringOptions);
        options.Implementations["int"].Options.Should().Be(intOptions);
        options.Implementations["object"].Options.Should().Be(objectOptions);
    }

    #endregion

    #region Dictionary Behavior Tests

    [TestMethod]
    public void Implementations_ShouldSupportKeyEnumeration()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        options.RegisterImplementation("local", typeof(TestStorageProvider), null);
        options.RegisterImplementation("ftp", typeof(AnotherTestProvider), null);

        // Act
        var keys = options.Implementations.Keys.ToList();

        // Assert
        keys.Should().HaveCount(2);
        keys.Should().Contain("local");
        keys.Should().Contain("ftp");
    }

    [TestMethod]
    public void Implementations_ShouldSupportValueEnumeration()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        Type type1 = typeof(TestStorageProvider);
        options.RegisterImplementation("local", type1, null);

        // Act
        var values = options.Implementations.Values.ToList();

        // Assert
        values.Should().HaveCount(1);
        values[0].ImplementationType.Should().Be(type1);
    }

    [TestMethod]
    public void Implementations_TryGetValue_ShouldWorkCorrectly()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        Type implementationType = typeof(TestStorageProvider);
        options.RegisterImplementation("test", implementationType, null);

        // Act
        bool exists = options.Implementations.TryGetValue("test", out StorageImplementation? implementation);
        bool notExists = options.Implementations.TryGetValue("missing", out StorageImplementation? missing);

        // Assert
        exists.Should().BeTrue();
        implementation.Should().NotBeNull();
        implementation!.ImplementationType.Should().Be(implementationType);
        notExists.Should().BeFalse();
        missing.Should().BeNull();
    }

    [TestMethod]
    public void Implementations_ContainsKey_ShouldWorkCorrectly()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        options.RegisterImplementation("exists", typeof(TestStorageProvider), null);

        // Act & Assert
        options.Implementations.ContainsKey("exists").Should().BeTrue();
        options.Implementations.ContainsKey("missing").Should().BeFalse();
    }

    #endregion

    #region Edge Cases Tests

    [TestMethod]
    public void RegisterImplementation_WithEmptyName_ShouldStore()
    {
        // Arrange
        var options = new StorageFactoryOptions();

        // Act
        options.RegisterImplementation("", typeof(TestStorageProvider), null);

        // Assert
        options.Implementations.Should().ContainKey("");
    }

    [TestMethod]
    public void RegisterImplementation_WithWhitespaceName_ShouldStore()
    {
        // Arrange
        var options = new StorageFactoryOptions();

        // Act
        options.RegisterImplementation("  ", typeof(TestStorageProvider), null);

        // Assert
        options.Implementations.Should().ContainKey("  ");
    }

    [TestMethod]
    public void RegisterImplementation_WithCaseSensitiveNames_ShouldStoreSeparately()
    {
        // Arrange
        var options = new StorageFactoryOptions();

        // Act
        options.RegisterImplementation("Provider", typeof(TestStorageProvider), null);
        options.RegisterImplementation("provider", typeof(AnotherTestProvider), null);

        // Assert
        options.Implementations.Should().HaveCount(2);
        options.Implementations["Provider"].ImplementationType.Should().Be(typeof(TestStorageProvider));
        options.Implementations["provider"].ImplementationType.Should().Be(typeof(AnotherTestProvider));
    }

    [TestMethod]
    public void RegisterImplementation_WithNullOptions_ShouldAccept()
    {
        // Arrange
        var options = new StorageFactoryOptions();

        // Act
        options.RegisterImplementation("test", typeof(TestStorageProvider), null);

        // Assert
        options.Implementations["test"].Options.Should().BeNull();
    }

    [TestMethod]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Arrange
        var options1 = new StorageFactoryOptions();
        var options2 = new StorageFactoryOptions();

        // Act
        options1.RegisterImplementation("test", typeof(TestStorageProvider), null);
        options2.RegisterImplementation("other", typeof(AnotherTestProvider), null);

        // Assert
        options1.Implementations.Should().HaveCount(1);
        options1.Implementations.Should().ContainKey("test");
        options2.Implementations.Should().HaveCount(1);
        options2.Implementations.Should().ContainKey("other");
    }

    #endregion

    #region Type System Tests

    [TestMethod]
    public void StorageFactoryOptions_ShouldBeSealed()
    {
        // Arrange & Act
        Type type = typeof(StorageFactoryOptions);

        // Assert
        type.IsSealed.Should().BeTrue();
    }

    [TestMethod]
    public void RegisterImplementation_ShouldBePublic()
    {
        // Arrange & Act
        var method = typeof(StorageFactoryOptions).GetMethod(nameof(StorageFactoryOptions.RegisterImplementation));

        // Assert
        method.Should().NotBeNull();
        method!.IsPublic.Should().BeTrue();
    }

    #endregion

    #region Test Helper Classes

    private class TestStorageProvider { }
    private class AnotherTestProvider { }
    private class TestOptions
    {
        public string Setting { get; set; } = string.Empty;
    }

    #endregion
}
