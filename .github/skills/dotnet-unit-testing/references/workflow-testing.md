---
title: Workflow Testing Guide
doc_type: guide
status: active
last_updated: 2026-07-20
target_audience: ai
complexity: high
estimated_tokens: 2269
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
# Workflow Testing Guide

Patterns specific to testing `IWorkflow` implementations in the `Manager.Transport.Service` project.

## Workflow Anatomy

Every workflow class follows this pattern:

```csharp
internal class Workflow{NNNN}(
    ILogger<TransportManager> logger,
    IStorageAccess storageAccess,
    IConfiguration configuration,
    /* additional dependencies */) : IWorkflow
{
    public string InterfaceNumber => InterfaceNumbers.INTERFACE_{NNNN};

    public async Task<ProcessInterfaceResponse> ExecuteAsync(
        ProcessInterfaceRequest request)
    {
        // 1. Create response from request
        // 2. Read input (StorageAccess.ReadAsync)
        // 3. Transform data (optional)
        // 4. Write output (StorageAccess.WriteAsync)
        // 5. Return response with success message or errors
    }
}
```

## Test Class Template for Workflows

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Moq;

using Wa.Wsdot.Fin.Idl.Access.Storage.Contract;
using Wa.Wsdot.Fin.Idl.Access.Storage.Contract.IO;
using Wa.Wsdot.Fin.Idl.Ifx;
using Wa.Wsdot.Fin.Idl.Ifx.Services.Messaging;
using Wa.Wsdot.Fin.Idl.Manager.Transport.Contract.IO;
using Wa.Wsdot.Fin.Idl.Manager.Transport.Service;
using Wa.Wsdot.Fin.Idl.Manager.Transport.Service.Workflows.Interfaces;


namespace Manager.Transport.UnitTests.Service.Workflows;

[TestClass]
public class Workflow{NNNN}Tests
{
    private Mock<ILogger<TransportManager>> loggerMock = null!;
    private Mock<IStorageAccess> storageAccessMock = null!;
    private Mock<IEmailService> emailServiceMock = null!;
    private IConfiguration configuration = null!;
    private Workflow{NNNN} workflow = null!;

    [TestInitialize]
    public void Setup()
    {
        loggerMock = new Mock<ILogger<TransportManager>>();
        storageAccessMock = new Mock<IStorageAccess>();
        emailServiceMock = new Mock<IEmailService>();

        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Add interface-specific config keys here
            })
            .Build();

        workflow = new Workflow{NNNN}(
            loggerMock.Object,
            storageAccessMock.Object,
            configuration,
            emailServiceMock.Object
            /* additional dependencies */);
    }

    // --- Tests follow coverage order ---
}
```

## Required Test Coverage for Workflows

### 1. InterfaceNumber Property

```csharp
[TestMethod]
public void InterfaceNumber_ReturnsExpectedValue()
{
    Assert.AreEqual(
        InterfaceNumbers.INTERFACE_{NNNN}.Number,
        workflow.InterfaceNumber);
}
```

### 2. Happy Path

```csharp
[TestMethod]
public async Task ExecuteAsync_WhenAllSucceeds_ReturnsSuccessMessage()
{
    // Arrange
    storageAccessMock
        .Setup(s => s.ReadAsync(It.IsAny<ReadRequest>()))
        .ReturnsAsync(new ReadResponse
        {
            FileContents = [0x01, 0x02, 0x03],
            Metadata = new FileMetadata
            {
                FileName = "input.txt"
            }
        });

    storageAccessMock
        .Setup(s => s.WriteAsync(It.IsAny<WriteRequest>()))
        .ReturnsAsync(new WriteResponse
        {
            Metadata = new FileMetadata
            {
                FileName = "output.txt",
                Exists = true
            }
        });

    var request = ServiceMessageFactory<ProcessInterfaceRequest>.Create();
    request.InterfaceNumber = InterfaceNumbers.INTERFACE_{NNNN};

    // Act
    var response = await workflow.ExecuteAsync(request);

    // Assert
    Assert.IsNotNull(response);
    Assert.IsFalse(response.HasErrors, response.Errors);

    storageAccessMock.Verify(
        s => s.ReadAsync(It.IsAny<ReadRequest>()), Times.Once);
    storageAccessMock.Verify(
        s => s.WriteAsync(It.IsAny<WriteRequest>()), Times.Once);
}
```

### 3. Read Failure — Short-Circuit

```csharp
[TestMethod]
public async Task ExecuteAsync_WhenReadFails_ReturnsErrorsAndSkipsWrite()
{
    // Arrange
    var readResponse = new ReadResponse();
    readResponse.Errors = "Source file not found.";

    storageAccessMock
        .Setup(s => s.ReadAsync(It.IsAny<ReadRequest>()))
        .ReturnsAsync(readResponse);

    var request = ServiceMessageFactory<ProcessInterfaceRequest>.Create();
    request.InterfaceNumber = InterfaceNumbers.INTERFACE_{NNNN};

    // Act
    var response = await workflow.ExecuteAsync(request);

    // Assert
    Assert.IsTrue(response.HasErrors);
    Assert.IsTrue(response.Errors.Contains("Source file not found."));

    storageAccessMock.Verify(
        s => s.WriteAsync(It.IsAny<WriteRequest>()), Times.Never);
}
```

### 4. Write Failure

```csharp
[TestMethod]
public async Task ExecuteAsync_WhenWriteFails_ReturnsErrors()
{
    // Arrange — read succeeds
    storageAccessMock
        .Setup(s => s.ReadAsync(It.IsAny<ReadRequest>()))
        .ReturnsAsync(new ReadResponse
        {
            FileContents = [0x01],
            Metadata = new FileMetadata { FileName = "test.txt" }
        });

    // Arrange — write fails
    var writeResponse = new WriteResponse();
    writeResponse.Errors = "SFTP upload failed.";

    storageAccessMock
        .Setup(s => s.WriteAsync(It.IsAny<WriteRequest>()))
        .ReturnsAsync(writeResponse);

    var request = ServiceMessageFactory<ProcessInterfaceRequest>.Create();

    // Act
    var response = await workflow.ExecuteAsync(request);

    // Assert
    Assert.IsTrue(response.HasErrors);
    Assert.IsTrue(response.Errors.Contains("SFTP upload failed."));
}
```

### 5. Data Flow Verification

```csharp
[TestMethod]
public async Task ExecuteAsync_PassesReadContentsToWrite()
{
    // Arrange
    var expectedBytes = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
    WriteRequest? capturedRequest = null;

    storageAccessMock
        .Setup(s => s.ReadAsync(It.IsAny<ReadRequest>()))
        .ReturnsAsync(new ReadResponse
        {
            FileContents = expectedBytes,
            Metadata = new FileMetadata { FileName = "input.dat" }
        });

    storageAccessMock
        .Setup(s => s.WriteAsync(It.IsAny<WriteRequest>()))
        .Callback<WriteRequest>(req => capturedRequest = req)
        .ReturnsAsync(new WriteResponse
        {
            Metadata = new FileMetadata()
        });

    var request = ServiceMessageFactory<ProcessInterfaceRequest>.Create();

    // Act
    await workflow.ExecuteAsync(request);

    // Assert
    Assert.IsNotNull(capturedRequest);
    CollectionAssert.AreEqual(expectedBytes, capturedRequest.RawContent);
}
```

### 6. Request Configuration

```csharp
[TestMethod]
public async Task ExecuteAsync_SetsActivityKeyOnReadRequest()
{
    // Arrange
    ReadRequest? capturedRequest = null;

    storageAccessMock
        .Setup(s => s.ReadAsync(It.IsAny<ReadRequest>()))
        .Callback<ReadRequest>(req => capturedRequest = req)
        .ReturnsAsync(new ReadResponse
        {
            FileContents = [0x01],
            Metadata = new FileMetadata { FileName = "test.txt" }
        });

    storageAccessMock
        .Setup(s => s.WriteAsync(It.IsAny<WriteRequest>()))
        .ReturnsAsync(new WriteResponse
        {
            Metadata = new FileMetadata()
        });

    var request = ServiceMessageFactory<ProcessInterfaceRequest>.Create();

    // Act
    await workflow.ExecuteAsync(request);

    // Assert
    Assert.IsNotNull(capturedRequest);
    Assert.AreEqual(
        InterfaceNumbers.INTERFACE_{NNNN}.Number,
        capturedRequest.ActivityKey);
}
```

## Workflows with Additional Dependencies

Some workflows have extra dependencies beyond the base set. Adapt the template:

| Dependency | How to Handle in Tests |
|-----------|----------------------|
| `ITransformingEngine` | `new Mock<ITransformingEngine>()` — setup `TransformFile` |
| `IAdvantageAccess` | `new Mock<IAdvantageAccess>()` — setup data access methods |
| `ISecretProvider` | `new Mock<ISecretProvider>()` — `Setup(x => x.GetValue(...)).Returns("value")` |
| `ArchiveExecutionRequestFactory` | Real instance: `new ArchiveExecutionRequestFactory(mockLogger.Object, configuration)` |
| `IValidatingEngine` | `new Mock<IValidatingEngine>()` |

## Common Pitfalls

1. **Don't reference `Manager.Transport.Service` from `Client.Portal.WebApi` tests** — violates component boundaries
2. **Don't assert exact log messages** — they change; verify `IsEnabled(LogLevel.X)` instead
3. **Don't forget `InternalsVisibleTo`** — workflow classes are `internal`
4. **Watch for `InterfaceDefinition` / `string` ambiguity** — use `.Number` in `Assert.AreEqual`
5. **Use `ServiceMessageFactory<T>.Create()`** — not `new T()` — for request/response objects that need correlation IDs
