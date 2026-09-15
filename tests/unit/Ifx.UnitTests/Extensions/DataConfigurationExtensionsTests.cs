using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ifx.Extensions;
using Ifx.Secrets;

namespace Ifx.Tests.Extensions;

/// <summary>
/// Unit tests for <see cref="DataConfigurationExtensions" />.
/// </summary>
[TestClass]
public sealed class DataConfigurationExtensionsTests
{
    #region AddConnectionString Tests

    [TestMethod]
    public void AddConnectionString_WithConfiguredConnectionString_RegistersSingletonString()
    {
        // Arrange
        const string connectionName = "primary";
        const string connectionString = "Server=test;Database=app;";
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(connectionName, connectionString);

        // Act
        IServiceCollection result = services.AddConnectionString(configuration, connectionName);

        // Assert
        result.Should().BeSameAs(services);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetRequiredService<string>().Should().Be(connectionString);
    }

    [TestMethod]
    public void AddConnectionString_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IConfiguration configuration = CreateConfiguration("primary", "Server=test;");

        // Act
        Action action = () => DataConfigurationExtensions.AddConnectionString(null!, configuration, "primary");

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should().Be("services");
    }

    [TestMethod]
    public void AddConnectionString_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        Action action = () => services.AddConnectionString(null!, "primary");

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should().Be("configuration");
    }

    [TestMethod]
    public void AddConnectionString_WithNullConnectionName_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("primary", "Server=test;");

        // Act
        Action action = () => services.AddConnectionString(configuration, null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should().Be("connectionName");
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    public void AddConnectionString_WithWhitespaceConnectionName_ThrowsArgumentException(string connectionName)
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("primary", "Server=test;");

        // Act
        Action action = () => services.AddConnectionString(configuration, connectionName);

        // Assert
        action.Should().ThrowExactly<ArgumentException>()
            .Which.ParamName.Should().Be("connectionName");
    }

    [TestMethod]
    public void AddConnectionString_WithMissingConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        // Act
        Action action = () => services.AddConnectionString(configuration, "missing");

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Connection string 'missing' is not configured.");
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    public void AddConnectionString_WithWhitespaceConnectionString_ThrowsInvalidOperationException(string? connectionString)
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("primary", connectionString);

        // Act
        Action action = () => services.AddConnectionString(configuration, "primary");

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Connection string 'primary' is not configured.");
    }

    #endregion

    #region AddNamedConnectionString Tests

    [TestMethod]
    public void AddNamedConnectionString_WithConfiguredConnectionString_RegistersKeyedSingletonString()
    {
        // Arrange
        const string connectionName = "primary";
        const string serviceName = "database";
        const string connectionString = "Server=test;Database=app;";
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(connectionName, connectionString);

        // Act
        IServiceCollection result = services.AddNamedConnectionString(configuration, connectionName, serviceName);

        // Assert
        result.Should().BeSameAs(services);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetRequiredKeyedService<string>(serviceName).Should().Be(connectionString);
    }

    [TestMethod]
    public void AddNamedConnectionString_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IConfiguration configuration = CreateConfiguration("primary", "Server=test;");

        // Act
        Action action = () => DataConfigurationExtensions.AddNamedConnectionString(null!, configuration, "primary", "database");

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should().Be("services");
    }

    [TestMethod]
    public void AddNamedConnectionString_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        Action action = () => services.AddNamedConnectionString(null!, "primary", "database");

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should().Be("configuration");
    }

    [TestMethod]
    public void AddNamedConnectionString_WithNullConnectionName_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("primary", "Server=test;");

        // Act
        Action action = () => services.AddNamedConnectionString(configuration, null!, "database");

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should().Be("connectionName");
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    public void AddNamedConnectionString_WithWhitespaceConnectionName_ThrowsArgumentException(string connectionName)
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("primary", "Server=test;");

        // Act
        Action action = () => services.AddNamedConnectionString(configuration, connectionName, "database");

        // Assert
        action.Should().ThrowExactly<ArgumentException>()
            .Which.ParamName.Should().Be("connectionName");
    }

    [TestMethod]
    public void AddNamedConnectionString_WithNullServiceName_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("primary", "Server=test;");

        // Act
        Action action = () => services.AddNamedConnectionString(configuration, "primary", null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should().Be("serviceName");
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    public void AddNamedConnectionString_WithWhitespaceServiceName_ThrowsArgumentException(string serviceName)
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("primary", "Server=test;");

        // Act
        Action action = () => services.AddNamedConnectionString(configuration, "primary", serviceName);

        // Assert
        action.Should().ThrowExactly<ArgumentException>()
            .Which.ParamName.Should().Be("serviceName");
    }

    [TestMethod]
    public void AddNamedConnectionString_WithMissingConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        // Act
        Action action = () => services.AddNamedConnectionString(configuration, "missing", "database");

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Connection string 'missing' is not configured.");
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    public void AddNamedConnectionString_WithWhitespaceConnectionString_ThrowsInvalidOperationException(string? connectionString)
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("primary", connectionString);

        // Act
        Action action = () => services.AddNamedConnectionString(configuration, "primary", "database");

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Connection string 'primary' is not configured.");
    }

    #endregion

    #region AddConnectionStringFromSecret Tests

    [TestMethod]
    public void AddConnectionStringFromSecret_WithResolvedSecret_RegistersSingletonString()
    {
        // Arrange
        const string secretName = "database-secret";
        const string connectionString = "Server=secret;Database=app;";
        IServiceCollection services = new ServiceCollection();
        var secretProvider = new Mock<ISecretProvider>(MockBehavior.Strict);
        secretProvider
            .Setup(provider => provider.GetAsync(secretName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectionString);

        services.AddSingleton(secretProvider.Object);
        services.AddConnectionStringFromSecret(secretName);

        // Act
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        string resolvedConnectionString = serviceProvider.GetRequiredService<string>();

        // Assert
        resolvedConnectionString.Should().Be(connectionString);
        secretProvider.Verify(provider => provider.GetAsync(secretName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void AddConnectionStringFromSecret_WithNullServices_ThrowsArgumentNullException()
    {
        // Act
        Action action = () => DataConfigurationExtensions.AddConnectionStringFromSecret(null!, "database-secret");

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should().Be("services");
    }

    [TestMethod]
    public void AddConnectionStringFromSecret_WithNullSecretName_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        Action action = () => services.AddConnectionStringFromSecret(null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .Which.ParamName.Should().Be("secretName");
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    public void AddConnectionStringFromSecret_WithWhitespaceSecretName_ThrowsArgumentException(string secretName)
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        Action action = () => services.AddConnectionStringFromSecret(secretName);

        // Assert
        action.Should().ThrowExactly<ArgumentException>()
            .Which.ParamName.Should().Be("secretName");
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    public void AddConnectionStringFromSecret_WithUnavailableSecret_ThrowsInvalidOperationException(string? secretValue)
    {
        // Arrange
        const string secretName = "database-secret";
        IServiceCollection services = new ServiceCollection();
        var secretProvider = new Mock<ISecretProvider>(MockBehavior.Strict);
        secretProvider
            .Setup(provider => provider.GetAsync(secretName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(secretValue);

        services.AddSingleton(secretProvider.Object);
        services.AddConnectionStringFromSecret(secretName);

        // Act
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        Action action = () => _ = serviceProvider.GetRequiredService<string>();

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Connection string secret 'database-secret' is not available or empty.");
        secretProvider.Verify(provider => provider.GetAsync(secretName, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    private static IConfiguration CreateConfiguration(string connectionName, string? connectionString)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{connectionName}"] = connectionString
            })
            .Build();
    }
}
