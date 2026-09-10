using System.Reflection;
using VisionaryCoder.Framework.Storage;

namespace VisionaryCoder.Framework.Tests.Storage;

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

        // Use reflection to call internal method for testing
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options, ["test", implementationType, null]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options, ["local", implementationType, null]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options, ["ftp", implementationType, testOptions]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options, ["local", type1, null]);
        method?.Invoke(options, ["ftp", type2, null]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options, ["provider", type1, null]);
        method?.Invoke(options, ["provider", type2, null]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options, ["string", implementationType, stringOptions]);
        method?.Invoke(options, ["int", implementationType, intOptions]);
        method?.Invoke(options, ["object", implementationType, objectOptions]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(options, ["local", typeof(TestStorageProvider), null]);
        method?.Invoke(options, ["ftp", typeof(AnotherTestProvider), null]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(options, ["local", type1, null]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(options, ["test", implementationType, null]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(options, ["exists", typeof(TestStorageProvider), null]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options, ["", typeof(TestStorageProvider), null]);

        // Assert
        options.Implementations.Should().ContainKey("");
    }

    [TestMethod]
    public void RegisterImplementation_WithWhitespaceName_ShouldStore()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options, ["  ", typeof(TestStorageProvider), null]);

        // Assert
        options.Implementations.Should().ContainKey("  ");
    }

    [TestMethod]
    public void RegisterImplementation_WithCaseSensitiveNames_ShouldStoreSeparately()
    {
        // Arrange
        var options = new StorageFactoryOptions();
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options, ["Provider", typeof(TestStorageProvider), null]);
        method?.Invoke(options, ["provider", typeof(AnotherTestProvider), null]);

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
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options, ["test", typeof(TestStorageProvider), null]);

        // Assert
        options.Implementations["test"].Options.Should().BeNull();
    }

    [TestMethod]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Arrange
        var options1 = new StorageFactoryOptions();
        var options2 = new StorageFactoryOptions();
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        method?.Invoke(options1, ["test", typeof(TestStorageProvider), null]);
        method?.Invoke(options2, ["other", typeof(AnotherTestProvider), null]);

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
    public void RegisterImplementation_ShouldBeInternal()
    {
        // Arrange & Act
        MethodInfo? method = typeof(StorageFactoryOptions).GetMethod("RegisterImplementation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        method.Should().NotBeNull();
        method!.IsAssembly.Should().BeTrue(); // Internal methods are marked as Assembly
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
