using Microsoft.Extensions.Logging;
using Moq;

namespace VisionaryCoder.Framework.Tests;

/// <summary>
/// Data-driven unit tests for ServiceBase to ensure 100% code coverage.
/// Tests happy path, edge cases, and expected failures.
/// </summary>
[TestClass]
public class ServiceBaseTests
{
    #region Test Implementation

    /// <summary>
    /// Concrete implementation of ServiceBase for testing purposes.
    /// </summary>
    public class TestService(ILogger<TestService> logger) : ServiceBase<TestService>(logger)
    {
        /// <summary>
        /// Exposes the protected Logger property for testing.
        /// </summary>
        public ILogger<TestService> ExposedLogger => Logger;
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValidLogger_ShouldInitializeSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestService>>();

        // Act
        var service = new TestService(mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        service.ExposedLogger.Should().NotBeNull();
        service.ExposedLogger.Should().BeSameAs(mockLogger.Object);
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<TestService> action = () => new TestService(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger")
            .WithMessage("*cannot be null*");
    }

    #endregion

    #region Logger Property Tests

    [TestMethod]
    public void Logger_AfterConstruction_ShouldReturnSameInstanceAsConstructorParameter()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestService>>();
        var service = new TestService(mockLogger.Object);

        // Act
        ILogger<TestService> logger = service.ExposedLogger;

        // Assert
        logger.Should().NotBeNull();
        logger.Should().BeSameAs(mockLogger.Object);
    }

    [TestMethod]
    public void Logger_AfterConstruction_ShouldBeUsableForLogging()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestService>>();
        var service = new TestService(mockLogger.Object);

        // Act
        ILogger<TestService> logger = service.ExposedLogger;
        logger.LogInformation("Test message");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Inheritance Tests

    [TestMethod]
    public void ServiceBase_ShouldBeAbstract()
    {
        // Arrange & Act
        Type type = typeof(ServiceBase<>);

        // Assert
        type.IsAbstract.Should().BeTrue();
    }

    [TestMethod]
    public void ServiceBase_ShouldHaveGenericTypeConstraint()
    {
        // Arrange & Act
        Type type = typeof(ServiceBase<>);
        Type genericParameter = type.GetGenericArguments()[0];

        // Assert - ServiceBase<T> has 'where T : class' constraint
        genericParameter.GenericParameterAttributes.Should().HaveFlag(
            System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint,
            "ServiceBase<T> has a 'where T : class' constraint");
    }

    [TestMethod]
    public void DerivedService_ShouldInheritFromServiceBase()
    {
        // Arrange & Act
        Type testServiceType = typeof(TestService);
        Type? baseType = testServiceType.BaseType;

        // Assert
        baseType.Should().NotBeNull();
        baseType!.IsGenericType.Should().BeTrue();
        baseType.GetGenericTypeDefinition().Should().Be(typeof(ServiceBase<>));
    }

    #endregion

    #region Multiple Instances Tests

    [TestMethod]
    public void MultipleInstances_WithDifferentLoggers_ShouldMaintainSeparateLoggerReferences()
    {
        // Arrange
        var mockLogger1 = new Mock<ILogger<TestService>>();
        var mockLogger2 = new Mock<ILogger<TestService>>();

        // Act
        var service1 = new TestService(mockLogger1.Object);
        var service2 = new TestService(mockLogger2.Object);

        // Assert
        service1.ExposedLogger.Should().BeSameAs(mockLogger1.Object);
        service2.ExposedLogger.Should().BeSameAs(mockLogger2.Object);
        service1.ExposedLogger.Should().NotBeSameAs(service2.ExposedLogger);
    }

    [TestMethod]
    public void MultipleInstances_WithSameLogger_ShouldShareLoggerReference()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestService>>();

        // Act
        var service1 = new TestService(mockLogger.Object);
        var service2 = new TestService(mockLogger.Object);

        // Assert
        service1.ExposedLogger.Should().BeSameAs(mockLogger.Object);
        service2.ExposedLogger.Should().BeSameAs(mockLogger.Object);
        service1.ExposedLogger.Should().BeSameAs(service2.ExposedLogger);
    }

    #endregion

    #region Edge Case Tests

    [TestMethod]
    public void Constructor_CalledMultipleTimes_ShouldCreateIndependentInstances()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestService>>();

        // Act
        var service1 = new TestService(mockLogger.Object);
        var service2 = new TestService(mockLogger.Object);
        var service3 = new TestService(mockLogger.Object);

        // Assert
        service1.Should().NotBeSameAs(service2);
        service2.Should().NotBeSameAs(service3);
        service1.Should().NotBeSameAs(service3);
    }

    [TestMethod]
    public void Logger_ShouldBeAccessibleFromDerivedClass()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestService>>();
        var service = new TestService(mockLogger.Object);

        // Act
        bool canAccessLogger = service.ExposedLogger != null;

        // Assert
        canAccessLogger.Should().BeTrue();
    }

    #endregion

    #region Additional Derived Classes for Testing

    /// <summary>
    /// Second concrete implementation to test generic type parameter.
    /// </summary>
    public class AnotherTestService(ILogger<AnotherTestService> logger) : ServiceBase<AnotherTestService>(logger)
    {
        public ILogger<AnotherTestService> ExposedLogger => Logger;
    }

    [TestMethod]
    public void DifferentGenericTypes_ShouldHaveCorrectTypedLoggers()
    {
        // Arrange
        var mockLogger1 = new Mock<ILogger<TestService>>();
        var mockLogger2 = new Mock<ILogger<AnotherTestService>>();

        // Act
        var service1 = new TestService(mockLogger1.Object);
        var service2 = new AnotherTestService(mockLogger2.Object);

        // Assert - Verify the loggers are assignable to the correct interface types
        service1.ExposedLogger.Should().BeAssignableTo<ILogger<TestService>>();
        service2.ExposedLogger.Should().BeAssignableTo<ILogger<AnotherTestService>>();
        service1.ExposedLogger.Should().NotBeSameAs(service2.ExposedLogger as object);
    }

    #endregion
}
