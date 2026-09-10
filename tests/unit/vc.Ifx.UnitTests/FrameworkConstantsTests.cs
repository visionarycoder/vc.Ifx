namespace VisionaryCoder.Framework.Tests;

/// <summary>
/// Unit tests for FrameworkConstants to ensure all constants have correct values.
/// </summary>
[TestClass]
public class FrameworkConstantsTests
{
    #region Main Constants Tests

    [TestMethod]
    public void Version_ShouldHaveCorrectValue()
    {
        // Assert
        Constants.Version.Should().Be("1.0.0");
    }

    [TestMethod]
    public void ConfigurationSection_ShouldHaveCorrectValue()
    {
        // Assert
        Constants.ConfigurationSection.Should().Be("VisionaryCoderFramework");
    }

    #endregion

    #region Timeouts Constants Tests

    [TestMethod]
    public void TimeoutsDefaults_ShouldHaveCorrectValues()
    {
        // Assert
        Constants.Timeouts.DefaultHttpTimeoutSeconds.Should().Be(30);
        Constants.Timeouts.DefaultDatabaseTimeoutSeconds.Should().Be(30);
        Constants.Timeouts.DefaultCacheExpirationMinutes.Should().Be(15);
    }

    [TestMethod]
    public void TimeoutsConstants_ShouldBePositiveValues()
    {
        // Assert
        Constants.Timeouts.DefaultHttpTimeoutSeconds.Should().BePositive();
        Constants.Timeouts.DefaultDatabaseTimeoutSeconds.Should().BePositive();
        Constants.Timeouts.DefaultCacheExpirationMinutes.Should().BePositive();
    }

    #endregion

    #region Headers Constants Tests

    [TestMethod]
    public void HeaderNames_ShouldHaveCorrectValues()
    {
        // Assert
        Constants.Headers.CorrelationId.Should().Be("X-Correlation-ID");
        Constants.Headers.RequestId.Should().Be("X-Request-ID");
        Constants.Headers.UserContext.Should().Be("X-User-Context");
        Constants.Headers.ApiVersion.Should().Be("Api-Version");
    }

    [TestMethod]
    public void HeaderNames_ShouldNotBeNullOrEmpty()
    {
        // Assert
        Constants.Headers.CorrelationId.Should().NotBeNullOrWhiteSpace();
        Constants.Headers.RequestId.Should().NotBeNullOrWhiteSpace();
        Constants.Headers.UserContext.Should().NotBeNullOrWhiteSpace();
        Constants.Headers.ApiVersion.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void HeaderNames_ShouldFollowHTTPHeaderConventions()
    {
        // Assert - Headers should contain hyphens and follow standard HTTP header naming
        Constants.Headers.CorrelationId.Should().Contain("-");
        Constants.Headers.RequestId.Should().Contain("-");
        Constants.Headers.UserContext.Should().Contain("-");
        Constants.Headers.ApiVersion.Should().Contain("-");
    }

    #endregion

    #region Logging Constants Tests

    [TestMethod]
    public void LoggingTemplate_ShouldHaveCorrectValue()
    {
        // Arrange
        string expectedTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        // Assert
        Constants.Logging.DefaultTemplate.Should().Be(expectedTemplate);
    }

    [TestMethod]
    public void LoggingPropertyNames_ShouldHaveCorrectValues()
    {
        // Assert
        Constants.Logging.CorrelationIdProperty.Should().Be("CorrelationId");
        Constants.Logging.RequestIdProperty.Should().Be("RequestId");
        Constants.Logging.UserIdProperty.Should().Be("UserId");
    }

    [TestMethod]
    public void LoggingPropertyNames_ShouldNotBeNullOrEmpty()
    {
        // Assert
        Constants.Logging.CorrelationIdProperty.Should().NotBeNullOrWhiteSpace();
        Constants.Logging.RequestIdProperty.Should().NotBeNullOrWhiteSpace();
        Constants.Logging.UserIdProperty.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void LoggingTemplate_ShouldContainRequiredPlaceholders()
    {
        // Arrange
        string template = Constants.Logging.DefaultTemplate;

        // Assert - Template should contain standard structured logging placeholders
        template.Should().Contain("{Timestamp");
        template.Should().Contain("{Level");
        template.Should().Contain("{SourceContext}");
        template.Should().Contain("{Message");
        template.Should().Contain("{NewLine}");
        template.Should().Contain("{Exception}");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void AllConstants_ShouldBeAccessible()
    {
        // This test ensures all nested classes and their constants are accessible
        // If compilation succeeds, the test passes

        // Main constants
        string version = Constants.Version;
        string configSection = Constants.ConfigurationSection;

        // Timeout constants
        int httpTimeout = Constants.Timeouts.DefaultHttpTimeoutSeconds;
        int dbTimeout = Constants.Timeouts.DefaultDatabaseTimeoutSeconds;
        int cacheTimeout = Constants.Timeouts.DefaultCacheExpirationMinutes;

        // Header constants
        string correlationHeader = Constants.Headers.CorrelationId;
        string requestHeader = Constants.Headers.RequestId;
        string userHeader = Constants.Headers.UserContext;
        string versionHeader = Constants.Headers.ApiVersion;

        // Logging constants
        string template = Constants.Logging.DefaultTemplate;
        string correlationProp = Constants.Logging.CorrelationIdProperty;
        string requestProp = Constants.Logging.RequestIdProperty;
        string userProp = Constants.Logging.UserIdProperty;

        // Assert that all values are not null (compilation test)
        version.Should().NotBeNull();
        configSection.Should().NotBeNull();
        httpTimeout.Should().BeGreaterThan(0);
        dbTimeout.Should().BeGreaterThan(0);
        cacheTimeout.Should().BeGreaterThan(0);
        correlationHeader.Should().NotBeNull();
        requestHeader.Should().NotBeNull();
        userHeader.Should().NotBeNull();
        versionHeader.Should().NotBeNull();
        template.Should().NotBeNull();
        correlationProp.Should().NotBeNull();
        requestProp.Should().NotBeNull();
        userProp.Should().NotBeNull();
    }

    #endregion
}
