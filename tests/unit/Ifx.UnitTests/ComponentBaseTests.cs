using Microsoft.Extensions.Logging;
using Moq;

namespace Ifx.Tests;

/// <summary>
/// Data-driven unit tests for ComponentBase to ensure 100% code coverage.
/// Tests happy path, edge cases, and expected failures.
/// </summary>
[TestClass]
public class ComponentBaseTests
{
    [TestMethod]
    public void DisposeIsDeterministicAndTheLoggerIsBorrowed()
    {
        var logger = new Mock<ILogger<LifecycleComponent>>();
        var disposableLogger = logger.As<IDisposable>();
        var service = new LifecycleComponent(logger.Object);
        service.CheckUsable();
        service.Dispose();
        var exception = Assert.ThrowsExactly<ObjectDisposedException>(() => service.CheckUsable());
        Assert.AreEqual(nameof(LifecycleComponent), exception.ObjectName);
        service.Dispose();
        Assert.AreEqual(2, service.DisposeCalls);
        Assert.IsTrue(service.LastDisposing);
        disposableLogger.Verify(value => value.Dispose(), Times.Never);
    }

    [TestMethod]
    public void ProtectedDisposalHookRemainsCompatibleWithoutABaseFinalizer()
    {
        var service = new LifecycleComponent(Mock.Of<ILogger<LifecycleComponent>>());
        service.CheckUsable();
        service.InvokeDispose(false);
        Assert.IsFalse(service.LastDisposing);
        Assert.ThrowsExactly<ObjectDisposedException>(() => service.CheckUsable());
        service.Dispose();
        Assert.IsTrue(service.LastDisposing);
        Assert.IsNull(typeof(ComponentBase<>).GetMethod("Finalize",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly));
    }

    public sealed class LifecycleComponent(ILogger<LifecycleComponent> logger) : ComponentBase<LifecycleComponent>(logger)
    {
        public int DisposeCalls { get; private set; }
        public bool LastDisposing { get; private set; }
        public void CheckUsable() => ThrowIfDisposed();
        public void InvokeDispose(bool disposing) => Dispose(disposing);
        protected override void Dispose(bool disposing)
        {
            DisposeCalls++;
            LastDisposing = disposing;
            base.Dispose(disposing);
        }
    }

    #region Test Implementation

    /// <summary>
    /// Concrete implementation of ComponentBase for testing purposes.
    /// </summary>
    public class TestComponent(ILogger<TestComponent> logger) : ComponentBase<TestComponent>(logger)
    {
        /// <summary>
        /// Exposes the protected Logger property for testing.
        /// </summary>
        public ILogger<TestComponent> ExposedLogger => Logger;
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValidLogger_ShouldInitializeSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestComponent>>();

        // Act
        var service = new TestComponent(mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        service.ExposedLogger.Should().NotBeNull();
        service.ExposedLogger.Should().BeSameAs(mockLogger.Object);
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<TestComponent> action = () => new TestComponent(null!);

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
        var mockLogger = new Mock<ILogger<TestComponent>>();
        var service = new TestComponent(mockLogger.Object);

        // Act
        ILogger<TestComponent> logger = service.ExposedLogger;

        // Assert
        logger.Should().NotBeNull();
        logger.Should().BeSameAs(mockLogger.Object);
    }

    [TestMethod]
    public void Logger_AfterConstruction_ShouldBeUsableForLogging()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestComponent>>();
        var service = new TestComponent(mockLogger.Object);

        // Act
        ILogger<TestComponent> logger = service.ExposedLogger;
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
        Type type = typeof(ComponentBase<>);

        // Assert
        type.IsAbstract.Should().BeTrue();
    }

    [TestMethod]
    public void ServiceBase_ShouldHaveGenericTypeConstraint()
    {
        // Arrange & Act
        Type type = typeof(ComponentBase<>);
        Type genericParameter = type.GetGenericArguments()[0];

        // Assert - ComponentBase<T> has 'where T : class' constraint
        genericParameter.GenericParameterAttributes.Should().HaveFlag(
            System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint,
            "ComponentBase<T> has a 'where T : class' constraint");
    }

    [TestMethod]
    public void DerivedService_ShouldInheritFromServiceBase()
    {
        // Arrange & Act
        Type testServiceType = typeof(TestComponent);
        Type? baseType = testServiceType.BaseType;

        // Assert
        baseType.Should().NotBeNull();
        baseType!.IsGenericType.Should().BeTrue();
        baseType.GetGenericTypeDefinition().Should().Be(typeof(ComponentBase<>));
    }

    #endregion

    #region Multiple Instances Tests

    [TestMethod]
    public void MultipleInstances_WithDifferentLoggers_ShouldMaintainSeparateLoggerReferences()
    {
        // Arrange
        var mockLogger1 = new Mock<ILogger<TestComponent>>();
        var mockLogger2 = new Mock<ILogger<TestComponent>>();

        // Act
        var service1 = new TestComponent(mockLogger1.Object);
        var service2 = new TestComponent(mockLogger2.Object);

        // Assert
        service1.ExposedLogger.Should().BeSameAs(mockLogger1.Object);
        service2.ExposedLogger.Should().BeSameAs(mockLogger2.Object);
        service1.ExposedLogger.Should().NotBeSameAs(service2.ExposedLogger);
    }

    [TestMethod]
    public void MultipleInstances_WithSameLogger_ShouldShareLoggerReference()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestComponent>>();

        // Act
        var service1 = new TestComponent(mockLogger.Object);
        var service2 = new TestComponent(mockLogger.Object);

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
        var mockLogger = new Mock<ILogger<TestComponent>>();

        // Act
        var service1 = new TestComponent(mockLogger.Object);
        var service2 = new TestComponent(mockLogger.Object);
        var service3 = new TestComponent(mockLogger.Object);

        // Assert
        service1.Should().NotBeSameAs(service2);
        service2.Should().NotBeSameAs(service3);
        service1.Should().NotBeSameAs(service3);
    }

    [TestMethod]
    public void Logger_ShouldBeAccessibleFromDerivedClass()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestComponent>>();
        var service = new TestComponent(mockLogger.Object);

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
    public class AnotherTestComponent(ILogger<AnotherTestComponent> logger) : ComponentBase<AnotherTestComponent>(logger)
    {
        public ILogger<AnotherTestComponent> ExposedLogger => Logger;
    }

    [TestMethod]
    public void DifferentGenericTypes_ShouldHaveCorrectTypedLoggers()
    {
        // Arrange
        var mockLogger1 = new Mock<ILogger<TestComponent>>();
        var mockLogger2 = new Mock<ILogger<AnotherTestComponent>>();

        // Act
        var service1 = new TestComponent(mockLogger1.Object);
        var service2 = new AnotherTestComponent(mockLogger2.Object);

        // Assert - Verify the loggers are assignable to the correct interface types
        service1.ExposedLogger.Should().BeAssignableTo<ILogger<TestComponent>>();
        service2.ExposedLogger.Should().BeAssignableTo<ILogger<AnotherTestComponent>>();
        service1.ExposedLogger.Should().NotBeSameAs(service2.ExposedLogger as object);
    }

    #endregion
}
