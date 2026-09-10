using VisionaryCoder.Framework.Providers;

namespace VisionaryCoder.Framework.Tests;

/// <summary>
/// Unit tests for RequestIdProvider to ensure 100% code coverage.
/// </summary>
[TestClass]
public class RequestIdProviderTests
{
    private RequestIdProvider provider = null!;

    [TestInitialize]
    public void Setup()
    {
        provider = new RequestIdProvider();
    }

    #region RequestId Property Tests

    [TestMethod]
    public void RequestId_WhenNoIdSet_ShouldGenerateNew()
    {
        // Act
        string requestId = provider.RequestId;

        // Assert
        requestId.Should().NotBeNullOrWhiteSpace();
        requestId.Should().HaveLength(8);
        requestId.Should().MatchRegex("^[A-Z0-9]{8}$");
    }

    [TestMethod]
    public void RequestId_WhenIdAlreadySet_ShouldReturnSameId()
    {
        // Arrange
        string firstCall = provider.RequestId;

        // Act
        string secondCall = provider.RequestId;

        // Assert
        secondCall.Should().Be(firstCall);
    }

    [TestMethod]
    public void RequestId_WhenSetExplicitly_ShouldReturnSetValue()
    {
        // Arrange
        string expectedId = "TEST1234";
        provider.SetRequestId(expectedId);

        // Act
        string result = provider.RequestId;

        // Assert
        result.Should().Be(expectedId);
    }

    #endregion

    #region GenerateNew Method Tests

    [TestMethod]
    public void GenerateNew_ShouldReturnValidFormat()
    {
        // Act
        string result = provider.GenerateNew();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().HaveLength(8);
        result.Should().MatchRegex("^[A-Z0-9]{8}$");
    }

    [TestMethod]
    public void GenerateNew_ShouldReturnUpperCaseOnly()
    {
        // Act
        string result = provider.GenerateNew();

        // Assert
        result.Should().Be(result.ToUpperInvariant());
    }

    [TestMethod]
    public void GenerateNew_WhenCalledMultipleTimes_ShouldReturnDifferentValues()
    {
        // Act
        string first = provider.GenerateNew();
        string second = provider.GenerateNew();

        // Assert
        first.Should().NotBe(second);
    }

    [TestMethod]
    public void GenerateNew_ShouldSetCurrentRequestId()
    {
        // Act
        string generated = provider.GenerateNew();
        string current = provider.RequestId;

        // Assert
        current.Should().Be(generated);
    }

    [TestMethod]
    public void GenerateNew_WhenCalledAfterSetRequestId_ShouldReplaceExistingId()
    {
        // Arrange
        provider.SetRequestId("ORIGINAL");

        // Act
        string newId = provider.GenerateNew();
        string currentId = provider.RequestId;

        // Assert
        currentId.Should().Be(newId);
        currentId.Should().NotBe("ORIGINAL");
    }

    #endregion

    #region SetRequestId Method Tests

    [TestMethod]
    public void SetRequestId_WithValidId_ShouldSetValue()
    {
        // Arrange
        string expectedId = "CUSTOM12";

        // Act
        provider.SetRequestId(expectedId);

        // Assert
        provider.RequestId.Should().Be(expectedId);
    }

    [TestMethod]
    public void SetRequestId_WithNullValue_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action action = () => provider.SetRequestId(null!);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("requestId");
    }

    [TestMethod]
    public void SetRequestId_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action action = () => provider.SetRequestId("");
        action.Should().Throw<ArgumentException>()
            .WithParameterName("requestId");
    }

    [TestMethod]
    public void SetRequestId_WithWhitespace_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action action = () => provider.SetRequestId("   ");
        action.Should().Throw<ArgumentException>()
            .WithParameterName("requestId");
    }

    [TestMethod]
    public void SetRequestId_ShouldAcceptAnyNonEmptyString()
    {
        // Arrange
        string[] testIds =
        [
            "A",
            "123",
            "lowercase",
            "UPPERCASE",
            "Mixed-Case_123",
            "Special@Characters#!",
            "Very-Long-Request-Id-With-Many-Characters"
        ];

        foreach (string testId in testIds)
        {
            // Act
            provider.SetRequestId(testId);

            // Assert
            provider.RequestId.Should().Be(testId, $"because '{testId}' should be valid");
        }
    }

    #endregion

    #region Thread Safety Tests

    [TestMethod]
    public void RequestId_InDifferentAsyncContexts_ShouldBeIndependent()
    {
        // This test verifies that AsyncLocal works correctly across async contexts
        var tasks = new List<Task<string>>();

        for (int i = 0; i < 10; i++)
        {
            int taskId = i;
            tasks.Add(Task.Run(async () =>
            {
                await Task.Delay(10); // Small delay to ensure async context switching
                var localProvider = new RequestIdProvider();
                string requestId = $"REQ{taskId:D2}ID";
                localProvider.SetRequestId(requestId);
                await Task.Delay(10); // Another delay
                return localProvider.RequestId;
            }));
        }

        // Act & Assert
        Task.WaitAll(tasks.ToArray());

        for (int i = 0; i < tasks.Count; i++)
        {
            string expectedId = $"REQ{i:D2}ID";
            tasks[i].Result.Should().Be(expectedId);
        }
    }

    #endregion

    #region Comparison with CorrelationIdProvider

    [TestMethod]
    public void RequestId_ShouldBeDifferentFromCorrelationId()
    {
        // Arrange
        var correlationProvider = new CorrelationIdProvider();
        var requestProvider = new RequestIdProvider();

        // Act
        string correlationId = correlationProvider.CorrelationId;
        string requestId = requestProvider.RequestId;

        // Assert
        correlationId.Should().HaveLength(12);
        requestId.Should().HaveLength(8);
        correlationId.Should().NotBe(requestId);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void ProviderLifecycle_ShouldWorkCorrectly()
    {
        // Arrange
        var provider = new RequestIdProvider();

        // Act & Assert - Initial state
        string initialId = provider.RequestId;
        initialId.Should().NotBeNullOrWhiteSpace();

        // Act & Assert - Set custom ID
        provider.SetRequestId("CUSTOM123");
        provider.RequestId.Should().Be("CUSTOM123");

        // Act & Assert - Generate new ID
        string newId = provider.GenerateNew();
        provider.RequestId.Should().Be(newId);
        newId.Should().NotBe("CUSTOM123");
        newId.Should().NotBe(initialId);

        // Act & Assert - Verify format consistency
        newId.Should().HaveLength(8);
        newId.Should().MatchRegex("^[A-Z0-9]{8}$");
    }

    #endregion
}
