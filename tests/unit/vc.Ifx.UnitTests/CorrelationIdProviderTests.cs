using VisionaryCoder.Framework.Providers;

namespace VisionaryCoder.Framework.Tests;

/// <summary>
/// Unit tests for CorrelationIdProvider to ensure 100% code coverage.
/// </summary>
[TestClass]
public class CorrelationIdProviderTests
{
    private CorrelationIdProvider provider = null!;

    [TestInitialize]
    public void Setup()
    {
        provider = new CorrelationIdProvider();
    }

    #region CorrelationId Property Tests

    [TestMethod]
    public void CorrelationId_WhenNoIdSet_ShouldGenerateNew()
    {
        // Act
        string correlationId = provider.CorrelationId;

        // Assert
        correlationId.Should().NotBeNullOrWhiteSpace();
        correlationId.Should().HaveLength(12);
        correlationId.Should().MatchRegex("^[A-Z0-9]{12}$");
    }

    [TestMethod]
    public void CorrelationId_WhenIdAlreadySet_ShouldReturnSameId()
    {
        // Arrange
        string firstCall = provider.CorrelationId;

        // Act
        string secondCall = provider.CorrelationId;

        // Assert
        secondCall.Should().Be(firstCall);
    }

    [TestMethod]
    public void CorrelationId_WhenSetExplicitly_ShouldReturnSetValue()
    {
        // Arrange
        string expectedId = "TEST123456AB";
        provider.SetCorrelationId(expectedId);

        // Act
        string result = provider.CorrelationId;

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
        result.Should().HaveLength(12);
        result.Should().MatchRegex("^[A-Z0-9]{12}$");
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
    public void GenerateNew_ShouldSetCurrentCorrelationId()
    {
        // Act
        string generated = provider.GenerateNew();
        string current = provider.CorrelationId;

        // Assert
        current.Should().Be(generated);
    }

    [TestMethod]
    public void GenerateNew_WhenCalledAfterSetCorrelationId_ShouldReplaceExistingId()
    {
        // Arrange
        provider.SetCorrelationId("ORIGINAL123");

        // Act
        string newId = provider.GenerateNew();
        string currentId = provider.CorrelationId;

        // Assert
        currentId.Should().Be(newId);
        currentId.Should().NotBe("ORIGINAL123");
    }

    #endregion

    #region SetCorrelationId Method Tests

    [TestMethod]
    public void SetCorrelationId_WithValidId_ShouldSetValue()
    {
        // Arrange
        string expectedId = "CUSTOM12345";

        // Act
        provider.SetCorrelationId(expectedId);

        // Assert
        provider.CorrelationId.Should().Be(expectedId);
    }

    [TestMethod]
    public void SetCorrelationId_WithNullValue_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action action = () => provider.SetCorrelationId(null!);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("correlationId");
    }

    [TestMethod]
    public void SetCorrelationId_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action action = () => provider.SetCorrelationId("");
        action.Should().Throw<ArgumentException>()
            .WithParameterName("correlationId");
    }

    [TestMethod]
    public void SetCorrelationId_WithWhitespace_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action action = () => provider.SetCorrelationId("   ");
        action.Should().Throw<ArgumentException>()
            .WithParameterName("correlationId");
    }

    [TestMethod]
    public void SetCorrelationId_ShouldAcceptAnyNonEmptyString()
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
            "Very-Long-Correlation-Id-With-Many-Characters"
        ];

        foreach (string testId in testIds)
        {
            // Act
            provider.SetCorrelationId(testId);

            // Assert
            provider.CorrelationId.Should().Be(testId, $"because '{testId}' should be valid");
        }
    }

    #endregion

    #region Thread Safety Tests

    [TestMethod]
    public void CorrelationId_InDifferentAsyncContexts_ShouldBeIndependent()
    {
        // This test verifies that AsyncLocal works correctly across async contexts
        var tasks = new List<Task<string>>();

        for (int i = 0; i < 10; i++)
        {
            int taskId = i;
            tasks.Add(Task.Run(async () =>
            {
                await Task.Delay(10); // Small delay to ensure async context switching
                var localProvider = new CorrelationIdProvider();
                string correlationId = $"TASK{taskId:D2}ID12";
                localProvider.SetCorrelationId(correlationId);
                await Task.Delay(10); // Another delay
                return localProvider.CorrelationId;
            }));
        }

        // Act & Assert
        Task.WaitAll(tasks.ToArray());

        for (int i = 0; i < tasks.Count; i++)
        {
            string expectedId = $"TASK{i:D2}ID12";
            tasks[i].Result.Should().Be(expectedId);
        }
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void ProviderLifecycle_ShouldWorkCorrectly()
    {
        // Arrange
        var provider = new CorrelationIdProvider();

        // Act & Assert - Initial state
        string initialId = provider.CorrelationId;
        initialId.Should().NotBeNullOrWhiteSpace();

        // Act & Assert - Set custom ID
        provider.SetCorrelationId("CUSTOM123");
        provider.CorrelationId.Should().Be("CUSTOM123");

        // Act & Assert - Generate new ID
        string newId = provider.GenerateNew();
        provider.CorrelationId.Should().Be(newId);
        newId.Should().NotBe("CUSTOM123");
        newId.Should().NotBe(initialId);

        // Act & Assert - Verify format consistency
        newId.Should().HaveLength(12);
        newId.Should().MatchRegex("^[A-Z0-9]{12}$");
    }

    #endregion
}
