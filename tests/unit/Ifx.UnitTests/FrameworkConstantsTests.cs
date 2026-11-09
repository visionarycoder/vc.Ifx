namespace Ifx.Tests;

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
        IfxConstants.Version.Should().Be("0.1.0");
    }

    [TestMethod]
    public void ConfigurationSection_ShouldHaveCorrectValue()
    {
        // Assert
        IfxConstants.ConfigurationSection.Should().Be("vc.IfxFramework");
    }

    #endregion

    #region Timeouts Constants Tests

    [TestMethod]
    public void TimeoutsDefaults_ShouldHaveCorrectValues()
    {
        // Assert
        IfxConstants.Timeouts.DefaultHttpTimeoutSeconds.Should().Be(30);
        IfxConstants.Timeouts.DefaultDatabaseTimeoutSeconds.Should().Be(30);
        IfxConstants.Timeouts.DefaultCacheExpirationMinutes.Should().Be(15);
    }

    [TestMethod]
    public void TimeoutsConstants_ShouldBePositiveValues()
    {
        // Assert
        IfxConstants.Timeouts.DefaultHttpTimeoutSeconds.Should().BePositive();
        IfxConstants.Timeouts.DefaultDatabaseTimeoutSeconds.Should().BePositive();
        IfxConstants.Timeouts.DefaultCacheExpirationMinutes.Should().BePositive();
    }

    #endregion

    #region Headers Constants Tests

    [TestMethod]
    public void HeaderNames_ShouldHaveCorrectValues()
    {
        // Assert
        IfxConstants.Headers.CorrelationId.Should().Be("X-Correlation-ID");
        IfxConstants.Headers.RequestId.Should().Be("X-Request-ID");
        IfxConstants.Headers.UserContext.Should().Be("X-User-Context");
        IfxConstants.Headers.ApiVersion.Should().Be("Api-Version");
    }

    [TestMethod]
    public void HeaderNames_ShouldNotBeNullOrEmpty()
    {
        // Assert
        IfxConstants.Headers.CorrelationId.Should().NotBeNullOrWhiteSpace();
        IfxConstants.Headers.RequestId.Should().NotBeNullOrWhiteSpace();
        IfxConstants.Headers.UserContext.Should().NotBeNullOrWhiteSpace();
        IfxConstants.Headers.ApiVersion.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void HeaderNames_ShouldFollowHTTPHeaderConventions()
    {
        // Assert - Headers should contain hyphens and follow standard HTTP header naming
        IfxConstants.Headers.CorrelationId.Should().Contain("-");
        IfxConstants.Headers.RequestId.Should().Contain("-");
        IfxConstants.Headers.UserContext.Should().Contain("-");
        IfxConstants.Headers.ApiVersion.Should().Contain("-");
    }

    #endregion

    #region Logging Constants Tests

    [TestMethod]
    public void LoggingTemplate_ShouldHaveCorrectValue()
    {
        // Arrange
        string expectedTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        // Assert
        IfxConstants.Logging.DefaultTemplate.Should().Be(expectedTemplate);
    }

    [TestMethod]
    public void LoggingPropertyNames_ShouldHaveCorrectValues()
    {
        // Assert
        IfxConstants.Logging.CorrelationIdProperty.Should().Be("CorrelationId");
        IfxConstants.Logging.RequestIdProperty.Should().Be("RequestId");
        IfxConstants.Logging.UserIdProperty.Should().Be("UserId");
    }

    [TestMethod]
    public void LoggingPropertyNames_ShouldNotBeNullOrEmpty()
    {
        // Assert
        IfxConstants.Logging.CorrelationIdProperty.Should().NotBeNullOrWhiteSpace();
        IfxConstants.Logging.RequestIdProperty.Should().NotBeNullOrWhiteSpace();
        IfxConstants.Logging.UserIdProperty.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void LoggingTemplate_ShouldContainRequiredPlaceholders()
    {
        // Arrange
        string template = IfxConstants.Logging.DefaultTemplate;

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
        string version = IfxConstants.Version;
        string configSection = IfxConstants.ConfigurationSection;

        // Timeout constants
        int httpTimeout = IfxConstants.Timeouts.DefaultHttpTimeoutSeconds;
        int dbTimeout = IfxConstants.Timeouts.DefaultDatabaseTimeoutSeconds;
        int cacheTimeout = IfxConstants.Timeouts.DefaultCacheExpirationMinutes;

        // Header constants
        string correlationHeader = IfxConstants.Headers.CorrelationId;
        string requestHeader = IfxConstants.Headers.RequestId;
        string userHeader = IfxConstants.Headers.UserContext;
        string versionHeader = IfxConstants.Headers.ApiVersion;

        // Logging constants
        string template = IfxConstants.Logging.DefaultTemplate;
        string correlationProp = IfxConstants.Logging.CorrelationIdProperty;
        string requestProp = IfxConstants.Logging.RequestIdProperty;
        string userProp = IfxConstants.Logging.UserIdProperty;

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
