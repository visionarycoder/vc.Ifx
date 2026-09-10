using System.Reflection;
using VisionaryCoder.Framework.Providers;

namespace VisionaryCoder.Framework.Tests;

/// <summary>
/// Unit tests for FrameworkInfoProvider to ensure 100% code coverage.
/// </summary>
[TestClass]
public class FrameworkInfoProviderTests
{
    private FrameworkInfoProvider provider = null!;

    [TestInitialize]
    public void Setup()
    {
        provider = new FrameworkInfoProvider();
    }

    #region Property Tests

    [TestMethod]
    public void Version_ShouldStartWithFrameworkConstantsVersion()
    {
        // Act
        string version = provider.Version;

        // Assert
        version.Should().StartWith(Constants.Version, "version should start with the semantic version");
        version.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void Name_ShouldReturnCorrectFrameworkName()
    {
        // Act
        string name = provider.Name;

        // Assert
        name.Should().Be("VisionaryCoder Framework");
    }

    [TestMethod]
    public void Description_ShouldReturnCorrectDescription()
    {
        // Act
        string description = provider.Description;

        // Assert
        description.Should().Be("A comprehensive framework for building enterprise-grade applications with proxy interceptor architecture.");
        description.Should().NotBeNullOrWhiteSpace();
        description.Should().Contain("framework");
        description.Should().Contain("proxy interceptor");
    }

    [TestMethod]
    public void CompiledAt_ShouldBeValidDateTimeOffset()
    {
        // Act
        DateTimeOffset compiledAt = provider.CompiledAt;

        // Assert
        compiledAt.Should().NotBe(default);
        compiledAt.Should().BeBefore(DateTimeOffset.UtcNow.AddMinutes(1)); // Should be compiled before now
        compiledAt.Should().BeAfter(DateTimeOffset.UtcNow.AddYears(-10)); // Should not be too old
    }

    [TestMethod]
    public void CompiledAt_ShouldBeConsistentAcrossInstances()
    {
        // Arrange
        var provider1 = new FrameworkInfoProvider();
        var provider2 = new FrameworkInfoProvider();

        // Act
        DateTimeOffset compiledAt1 = provider1.CompiledAt;
        DateTimeOffset compiledAt2 = provider2.CompiledAt;

        // Assert
        // CompiledAt should be the same for all instances since it's based on assembly creation time
        compiledAt1.Should().Be(compiledAt2);
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void FrameworkInfoProvider_ShouldImplementIFrameworkInfoProvider()
    {
        // Assert
        provider.Should().BeAssignableTo<IFrameworkInfoProvider>();
    }

    [TestMethod]
    public void FrameworkInfoProvider_ShouldImplementAllInterfaceProperties()
    {
        // Arrange
        Type interfaceType = typeof(IFrameworkInfoProvider);
        Type implementationType = typeof(FrameworkInfoProvider);

        // Act & Assert
        foreach (PropertyInfo property in interfaceType.GetProperties())
        {
            PropertyInfo? implementationProperty = implementationType.GetProperty(property.Name);
            implementationProperty.Should().NotBeNull($"Property {property.Name} should be implemented");
            implementationProperty!.PropertyType.Should().Be(property.PropertyType);
        }
    }

    #endregion

    #region Compilation Time Tests

    [TestMethod]
    public void GetCompilationTime_ShouldReturnAssemblyCreationTime()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var fileInfo = new FileInfo(assembly.Location);
        DateTime expectedTime = fileInfo.CreationTime;

        // Act
        DateTimeOffset actualTime = provider.CompiledAt;

        // Assert
        // Since CompiledAt uses the same logic as our test, they should match
        actualTime.DateTime.Should().BeCloseTo(expectedTime, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void CompiledAt_ShouldBeReadOnlyProperty()
    {
        // Arrange
        PropertyInfo? propertyInfo = typeof(FrameworkInfoProvider).GetProperty(nameof(FrameworkInfoProvider.CompiledAt));

        // Assert
        propertyInfo.Should().NotBeNull();
        propertyInfo!.CanRead.Should().BeTrue();
        propertyInfo.CanWrite.Should().BeFalse();
        propertyInfo.SetMethod.Should().BeNull();
    }

    #endregion

    #region Value Consistency Tests

    [TestMethod]
    public void Properties_ShouldReturnConsistentValues()
    {
        // Arrange
        var provider1 = new FrameworkInfoProvider();
        var provider2 = new FrameworkInfoProvider();

        // Act & Assert
        provider1.Version.Should().Be(provider2.Version);
        provider1.Name.Should().Be(provider2.Name);
        provider1.Description.Should().Be(provider2.Description);
        provider1.CompiledAt.Should().Be(provider2.CompiledAt);
    }

    [TestMethod]
    public void Properties_ShouldNotReturnNullOrEmpty()
    {
        // Act & Assert
        provider.Version.Should().NotBeNullOrEmpty();
        provider.Name.Should().NotBeNullOrEmpty();
        provider.Description.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Edge Case Tests

    [TestMethod]
    public void MultipleInstances_ShouldNotAffectEachOther()
    {
        // Arrange
        var providers = new List<FrameworkInfoProvider>();
        for (int i = 0; i < 5; i++)
        {
            providers.Add(new FrameworkInfoProvider());
        }

        // Act & Assert
        string firstVersion = providers[0].Version;
        string firstName = providers[0].Name;
        string firstDescription = providers[0].Description;
        DateTimeOffset firstCompiledAt = providers[0].CompiledAt;

        foreach (FrameworkInfoProvider p in providers.Skip(1))
        {
            p.Version.Should().Be(firstVersion);
            p.Name.Should().Be(firstName);
            p.Description.Should().Be(firstDescription);
            p.CompiledAt.Should().Be(firstCompiledAt);
        }
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void FrameworkInfoProvider_ShouldProvideValidMetadata()
    {
        // Act & Assert
        provider.Version.Should().MatchRegex(@"^\d+\.\d+\.\d+.*");
        provider.Name.Should().Contain("VisionaryCoder");
        provider.Description.Should().Contain("framework");
        provider.CompiledAt.Should().BeWithin(TimeSpan.FromDays(365)).Before(DateTimeOffset.UtcNow);
    }

    [TestMethod]
    public void FrameworkInfoProvider_AsInterface_ShouldWorkCorrectly()
    {
        // Arrange
        IFrameworkInfoProvider interfaceProvider = provider;

        // Act & Assert
        interfaceProvider.Version.Should().Be(provider.Version);
        interfaceProvider.Name.Should().Be(provider.Name);
        interfaceProvider.Description.Should().Be(provider.Description);
        interfaceProvider.CompiledAt.Should().Be(provider.CompiledAt);
    }

    #endregion
}
