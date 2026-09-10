---
title: Test Patterns Reference
doc_type: guide
status: active
last_updated: 2026-07-20
target_audience: ai
complexity: medium
estimated_tokens: 1736
prerequisites:
  - dotnet-unit-testing
related_skills:
  - dotnet-unit-testing
appliesTo: '**/*.{cs,csproj,xml,json,md}'
tags:
  - dotnet
  - unit
  - testing
---
# Test Patterns Reference

Concrete test patterns used in this codebase. Use these as templates when generating new tests.

## Pattern 1: DI Module / Component Registration

Tests that a DI module correctly registers its services without constructing the full dependency graph.

```csharp
[TestClass]
public class StorageAccessComponentTests
{
    [TestMethod]
    public void AddModule_Registers_IStorageAccess()
    {
        // Arrange
        var module = new StorageAccessComponent(
            new Mock<ILogger<StorageAccessComponent>>().Object);
        var services = new ServiceCollection();
        var configuration = new Mock<IConfiguration>().Object;

        // Act
        module.Add(services, configuration);

        // Assert (descriptor-based — avoids constructing implementation)
        var registered = services.Any(
            sd => sd.ServiceType == typeof(IStorageAccess));
        Assert.IsTrue(registered, "IStorageAccess should be registered.");
    }

    [TestMethod]
    [DataRow(null)]
    public void AddModule_LogsError_WhenServicesIsNull(
        IServiceCollection services)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<StorageAccessComponent>>();
        loggerMock
            .Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
            .Returns(true);
        var module = new StorageAccessComponent(loggerMock.Object);
        var configuration = new Mock<IConfiguration>().Object;

        // Act — should handle gracefully and log error
        module.Add(services, configuration);

        // Assert
        loggerMock.Verify(
            l => l.IsEnabled(LogLevel.Error),
            Times.AtLeastOnce);
    }

    [TestMethod]
    public void AddModule_DoesNotThrowException_WhenErrorOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<StorageAccessComponent>>();
        loggerMock
            .Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
            .Returns(true);
        var module = new StorageAccessComponent(loggerMock.Object);

        // Act & Assert — should not throw even with null arguments
        try
        {
            module.Add(null!, null!);
        }
        catch (Exception ex)
        {
            Assert.Fail(
                $"Module should handle exceptions gracefully. " +
                $"Exception: {ex.Message}");
        }
    }
}
```

**Key takeaway**: Use `ServiceCollection` descriptor checks (`services.Any(sd => ...)`) instead of building the full service provider. This avoids needing all transitive dependencies.

## Pattern 2: Helper Class with Edge Cases

Comprehensive coverage for utility/helper classes.

```csharp
[TestClass]
public class SomeHelperTests
{
    [TestMethod]
    public void Process_WithEmptyInput_ReturnsDefault()
    {
        var result = SomeHelper.Process([]);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void Process_WithSingleEntry_ReturnsThatEntry()
    {
        var result = SomeHelper.Process(["value1"]);
        Assert.AreEqual("value1", result);
    }

    [TestMethod]
    public void Process_WithMultipleEntries_AggregatesCorrectly()
    {
        var result = SomeHelper.Process(["a", "b", "c"]);
        Assert.AreEqual("a, b, c", result);
    }

    [TestMethod]
    public void Process_WithNullInput_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(
            () => SomeHelper.Process(null!));
    }

    [TestMethod]
    public void Process_WithLargeDataset_CompletesWithinTimeout()
    {
        var items = Enumerable.Range(0, 10_000)
            .Select(i => $"item{i}")
            .ToArray();

        var result = SomeHelper.Process(items);

        Assert.IsNotNull(result);
    }
}
```

**Key takeaway**: Cover empty → single → multiple → null → large. This pattern appears throughout `Ifx.UnitTests` and `Engine.Transforming.UnitTests`.

## Pattern 3: Async Service with Error Propagation

Testing services that chain async calls and propagate errors.

```csharp
[TestClass]
public class SomeServiceTests
{
    private Mock<IDependencyA> depAMock = null!;
    private Mock<IDependencyB> depBMock = null!;
    private SomeService sut = null!;

    [TestInitialize]
    public void Setup()
    {
        depAMock = new Mock<IDependencyA>();
        depBMock = new Mock<IDependencyB>();
        sut = new SomeService(depAMock.Object, depBMock.Object);
    }

    [TestMethod]
    public async Task DoWork_WhenDepAFails_PropagatesError()
    {
        // Arrange
        var responseA = new ResponseA();
        responseA.Errors = "Connection failed.";

        depAMock
            .Setup(a => a.ExecuteAsync(It.IsAny<RequestA>()))
            .ReturnsAsync(responseA);

        // Act
        var result = await sut.DoWork(new WorkRequest());

        // Assert
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Contains("Connection failed."));

        // Verify dep B was never called (short-circuit)
        depBMock.Verify(
            b => b.ExecuteAsync(It.IsAny<RequestB>()),
            Times.Never);
    }

    [TestMethod]
    public async Task DoWork_WhenDepAThrows_LogsAndReturnsGracefully()
    {
        // Arrange
        depAMock
            .Setup(a => a.ExecuteAsync(It.IsAny<RequestA>()))
            .ThrowsAsync(new InvalidOperationException("Boom"));

        // Act
        var result = await sut.DoWork(new WorkRequest());

        // Assert — verify graceful handling, not a thrown exception
        Assert.IsNotNull(result);
    }
}
```

**Key takeaway**: Test each dependency failure independently. Verify short-circuit behavior (downstream dependencies should not be called when upstream fails).

## Pattern 4: DataRow Parameterized Tests

```csharp
[TestMethod]
[DataRow("2016", true)]
[DataRow("9999", false)]
[DataRow("", false)]
[DataRow(null, false)]
public void IsValidInterface_ReturnsExpected(
    string input, bool expected)
{
    var result = InterfaceValidator.IsValid(input);
    Assert.AreEqual(expected, result);
}
```

**Key takeaway**: Use `[DataRow]` for truth-table-style tests. Keep each row's intent obvious from the parameters.

## Pattern 5: Logger Verification

Verifying that specific log levels were hit (without asserting exact message text):

```csharp
loggerMock.Verify(
    l => l.IsEnabled(LogLevel.Error),
    Times.AtLeastOnce);

loggerMock.Verify(
    l => l.IsEnabled(LogLevel.Information),
    Times.AtLeastOnce);
```

**Key takeaway**: Verify `IsEnabled` was called at the expected level. Avoid asserting exact log message strings — they're brittle and change often.
