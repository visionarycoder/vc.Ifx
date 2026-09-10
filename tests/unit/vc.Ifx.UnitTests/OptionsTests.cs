namespace VisionaryCoder.Framework.Tests;

/// <summary>
/// Unit tests for Options to ensure 100% code coverage.
/// </summary>
[TestClass]
public class OptionsTests
{
    #region Constructor and Default Values Tests

    [TestMethod]
    public void DefaultConstructor_ShouldSetCorrectDefaultValues()
    {
        // Act
        var options = new Options();

        // Assert
        options.EnableCorrelationId.Should().BeTrue();
        options.EnableRequestId.Should().BeTrue();
        options.EnableStructuredLogging.Should().BeTrue();
        options.DefaultHttpTimeoutSeconds.Should().Be(Constants.Timeouts.DefaultHttpTimeoutSeconds);
        options.DefaultCacheExpirationMinutes.Should().Be(Constants.Timeouts.DefaultCacheExpirationMinutes);
    }

    [TestMethod]
    public void DefaultValues_ShouldMatchFrameworkConstants()
    {
        // Act
        var options = new Options();

        // Assert
        options.DefaultHttpTimeoutSeconds.Should().Be(30);
        options.DefaultCacheExpirationMinutes.Should().Be(15);
    }

    #endregion

    #region Property Set/Get Tests

    [TestMethod]
    public void EnableCorrelationId_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new Options();

        // Act & Assert - Set to false
        options.EnableCorrelationId = false;
        options.EnableCorrelationId.Should().BeFalse();

        // Act & Assert - Set back to true
        options.EnableCorrelationId = true;
        options.EnableCorrelationId.Should().BeTrue();
    }

    [TestMethod]
    public void EnableRequestId_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new Options();

        // Act & Assert - Set to false
        options.EnableRequestId = false;
        options.EnableRequestId.Should().BeFalse();

        // Act & Assert - Set back to true
        options.EnableRequestId = true;
        options.EnableRequestId.Should().BeTrue();
    }

    [TestMethod]
    public void EnableStructuredLogging_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new Options();

        // Act & Assert - Set to false
        options.EnableStructuredLogging = false;
        options.EnableStructuredLogging.Should().BeFalse();

        // Act & Assert - Set back to true
        options.EnableStructuredLogging = true;
        options.EnableStructuredLogging.Should().BeTrue();
    }

    [TestMethod]
    public void DefaultHttpTimeoutSeconds_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new Options();

        // Act & Assert - Set custom value
        options.DefaultHttpTimeoutSeconds = 60;
        options.DefaultHttpTimeoutSeconds.Should().Be(60);

        // Act & Assert - Set to zero
        options.DefaultHttpTimeoutSeconds = 0;
        options.DefaultHttpTimeoutSeconds.Should().Be(0);

        // Act & Assert - Set negative value
        options.DefaultHttpTimeoutSeconds = -1;
        options.DefaultHttpTimeoutSeconds.Should().Be(-1);
    }

    [TestMethod]
    public void DefaultCacheExpirationMinutes_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new Options();

        // Act & Assert - Set custom value
        options.DefaultCacheExpirationMinutes = 30;
        options.DefaultCacheExpirationMinutes.Should().Be(30);

        // Act & Assert - Set to zero
        options.DefaultCacheExpirationMinutes = 0;
        options.DefaultCacheExpirationMinutes.Should().Be(0);

        // Act & Assert - Set negative value
        options.DefaultCacheExpirationMinutes = -1;
        options.DefaultCacheExpirationMinutes.Should().Be(-1);
    }

    #endregion

    #region Edge Case Tests

    [TestMethod]
    public void AllProperties_CanBeSetToExtremeValues()
    {
        // Arrange
        var options = new Options();

        // Act - Set all to minimum values
        options.EnableCorrelationId = false;
        options.EnableRequestId = false;
        options.EnableStructuredLogging = false;
        options.DefaultHttpTimeoutSeconds = int.MinValue;
        options.DefaultCacheExpirationMinutes = int.MinValue;

        // Assert
        options.EnableCorrelationId.Should().BeFalse();
        options.EnableRequestId.Should().BeFalse();
        options.EnableStructuredLogging.Should().BeFalse();
        options.DefaultHttpTimeoutSeconds.Should().Be(int.MinValue);
        options.DefaultCacheExpirationMinutes.Should().Be(int.MinValue);

        // Act - Set all to maximum values
        options.EnableCorrelationId = true;
        options.EnableRequestId = true;
        options.EnableStructuredLogging = true;
        options.DefaultHttpTimeoutSeconds = int.MaxValue;
        options.DefaultCacheExpirationMinutes = int.MaxValue;

        // Assert
        options.EnableCorrelationId.Should().BeTrue();
        options.EnableRequestId.Should().BeTrue();
        options.EnableStructuredLogging.Should().BeTrue();
        options.DefaultHttpTimeoutSeconds.Should().Be(int.MaxValue);
        options.DefaultCacheExpirationMinutes.Should().Be(int.MaxValue);
    }

    #endregion

    #region Integration and Configuration Tests

    [TestMethod]
    public void Options_ShouldSupportTypicalConfigurationScenarios()
    {
        // Scenario 1: Minimal logging configuration
        var minimalOptions = new Options
        {
            EnableCorrelationId = false,
            EnableRequestId = false,
            EnableStructuredLogging = false,
            DefaultHttpTimeoutSeconds = 10,
            DefaultCacheExpirationMinutes = 5
        };

        minimalOptions.EnableCorrelationId.Should().BeFalse();
        minimalOptions.EnableRequestId.Should().BeFalse();
        minimalOptions.EnableStructuredLogging.Should().BeFalse();
        minimalOptions.DefaultHttpTimeoutSeconds.Should().Be(10);
        minimalOptions.DefaultCacheExpirationMinutes.Should().Be(5);

        // Scenario 2: High performance configuration
        var performanceOptions = new Options
        {
            EnableCorrelationId = true,
            EnableRequestId = true,
            EnableStructuredLogging = true,
            DefaultHttpTimeoutSeconds = 120,
            DefaultCacheExpirationMinutes = 60
        };

        performanceOptions.EnableCorrelationId.Should().BeTrue();
        performanceOptions.EnableRequestId.Should().BeTrue();
        performanceOptions.EnableStructuredLogging.Should().BeTrue();
        performanceOptions.DefaultHttpTimeoutSeconds.Should().Be(120);
        performanceOptions.DefaultCacheExpirationMinutes.Should().Be(60);
    }

    [TestMethod]
    public void Properties_ShouldBeIndependent()
    {
        // Arrange
        var options = new Options();

        // Act - Modify one property at a time and verify others remain unchanged
        int originalHttpTimeout = options.DefaultHttpTimeoutSeconds;
        int originalCacheExpiration = options.DefaultCacheExpirationMinutes;

        options.EnableCorrelationId = false;

        // Assert - Other properties should remain unchanged
        options.EnableRequestId.Should().BeTrue();
        options.EnableStructuredLogging.Should().BeTrue();
        options.DefaultHttpTimeoutSeconds.Should().Be(originalHttpTimeout);
        options.DefaultCacheExpirationMinutes.Should().Be(originalCacheExpiration);

        // Act - Modify timeout
        options.DefaultHttpTimeoutSeconds = 100;

        // Assert - Other properties should remain unchanged
        options.EnableCorrelationId.Should().BeFalse(); // Our previous change
        options.EnableRequestId.Should().BeTrue();
        options.EnableStructuredLogging.Should().BeTrue();
        options.DefaultCacheExpirationMinutes.Should().Be(originalCacheExpiration);
    }

    #endregion

    #region Object Behavior Tests

    [TestMethod]
    public void Options_ShouldBeReferenceType()
    {
        // Arrange
        var options1 = new Options();
        Options options2 = options1;

        // Act
        options2.EnableCorrelationId = false;

        // Assert - Both references should point to same object
        options1.EnableCorrelationId.Should().BeFalse();
        ReferenceEquals(options1, options2).Should().BeTrue();
    }

    [TestMethod]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Arrange
        var options1 = new Options();
        var options2 = new Options();

        // Act
        options1.EnableCorrelationId = false;
        options1.DefaultHttpTimeoutSeconds = 60;

        // Assert - options2 should retain default values
        options2.EnableCorrelationId.Should().BeTrue();
        options2.DefaultHttpTimeoutSeconds.Should().Be(30);
        ReferenceEquals(options1, options2).Should().BeFalse();
    }

    #endregion
}
