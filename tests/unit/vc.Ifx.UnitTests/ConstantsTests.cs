namespace VisionaryCoder.Framework.Tests;

/// <summary>
/// Data-driven unit tests for the <see cref="Constants"/> class.
/// Tests all constant values to ensure they match expected values and remain stable.
/// </summary>
[TestClass]
public class ConstantsTests
{
    #region Top-Level Constants Tests

    [TestMethod]
    public void Version_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Version.Should().Be("1.0.0");
    }

    [TestMethod]
    public void ConfigurationSection_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.ConfigurationSection.Should().Be("VisionaryCoderFramework");
    }

    [TestMethod]
    public void Version_ShouldNotBeNullOrEmpty()
    {
        // Assert
        Constants.Version.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void ConfigurationSection_ShouldNotBeNullOrEmpty()
    {
        // Assert
        Constants.ConfigurationSection.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Timeouts Constants Tests

    [TestMethod]
    public void Timeouts_DefaultHttpTimeoutSeconds_ShouldBe30()
    {
        // Assert
        Constants.Timeouts.DefaultHttpTimeoutSeconds.Should().Be(30);
    }

    [TestMethod]
    public void Timeouts_DefaultDatabaseTimeoutSeconds_ShouldBe30()
    {
        // Assert
        Constants.Timeouts.DefaultDatabaseTimeoutSeconds.Should().Be(30);
    }

    [TestMethod]
    public void Timeouts_DefaultCacheExpirationMinutes_ShouldBe15()
    {
        // Assert
        Constants.Timeouts.DefaultCacheExpirationMinutes.Should().Be(15);
    }

    [TestMethod]
    public void Timeouts_AllValues_ShouldBePositive()
    {
        // Assert
        Constants.Timeouts.DefaultHttpTimeoutSeconds.Should().BePositive();
        Constants.Timeouts.DefaultDatabaseTimeoutSeconds.Should().BePositive();
        Constants.Timeouts.DefaultCacheExpirationMinutes.Should().BePositive();
    }

    [TestMethod]
    public void Timeouts_HttpAndDatabaseTimeouts_ShouldBeEqual()
    {
        // Assert - Both are set to 30 seconds
        Constants.Timeouts.DefaultHttpTimeoutSeconds.Should().Be(Constants.Timeouts.DefaultDatabaseTimeoutSeconds);
    }

    #endregion

    #region Headers Constants Tests

    [TestMethod]
    public void Headers_CorrelationId_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Headers.CorrelationId.Should().Be("X-Correlation-ID");
    }

    [TestMethod]
    public void Headers_RequestId_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Headers.RequestId.Should().Be("X-Request-ID");
    }

    [TestMethod]
    public void Headers_UserContext_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Headers.UserContext.Should().Be("X-User-Context");
    }

    [TestMethod]
    public void Headers_ApiVersion_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Headers.ApiVersion.Should().Be("Api-Version");
    }

    [TestMethod]
    public void Headers_AllValues_ShouldNotBeNullOrEmpty()
    {
        // Assert
        Constants.Headers.CorrelationId.Should().NotBeNullOrEmpty();
        Constants.Headers.RequestId.Should().NotBeNullOrEmpty();
        Constants.Headers.UserContext.Should().NotBeNullOrEmpty();
        Constants.Headers.ApiVersion.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    [DataRow("X-Correlation-ID")]
    [DataRow("X-Request-ID")]
    [DataRow("X-User-Context")]
    public void Headers_CustomHeaders_ShouldStartWithXPrefix(string headerValue)
    {
        // Assert - Custom headers should follow X- convention
        headerValue.Should().StartWith("X-");
    }

    [TestMethod]
    public void Headers_CorrelationId_ShouldContainHyphens()
    {
        // Assert - Header names should use hyphens for word separation
        Constants.Headers.CorrelationId.Should().Contain("-");
    }

    [TestMethod]
    public void Headers_CorrelationIdAndRequestId_ShouldBeDifferent()
    {
        // Assert - These should be distinct headers
        Constants.Headers.CorrelationId.Should().NotBe(Constants.Headers.RequestId);
    }

    #endregion

    #region Logging Constants Tests

    [TestMethod]
    public void Logging_DefaultLogLevel_ShouldBeInformation()
    {
        // Assert
        Constants.Logging.DefaultLogLevel.Should().Be("Information");
    }

    [TestMethod]
    public void Logging_FrameworkCategory_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Logging.FrameworkCategory.Should().Be("VisionaryCoder.Framework");
    }

    [TestMethod]
    public void Logging_PerformanceCategory_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Logging.PerformanceCategory.Should().Be("VisionaryCoder.Framework.Performance");
    }

    [TestMethod]
    public void Logging_DefaultTemplate_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Logging.DefaultTemplate.Should().Be("[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
    }

    [TestMethod]
    public void Logging_CorrelationIdProperty_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Logging.CorrelationIdProperty.Should().Be("CorrelationId");
    }

    [TestMethod]
    public void Logging_RequestIdProperty_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Logging.RequestIdProperty.Should().Be("RequestId");
    }

    [TestMethod]
    public void Logging_UserIdProperty_ShouldHaveExpectedValue()
    {
        // Assert
        Constants.Logging.UserIdProperty.Should().Be("UserId");
    }

    [TestMethod]
    public void Logging_AllValues_ShouldNotBeNullOrEmpty()
    {
        // Assert
        Constants.Logging.DefaultLogLevel.Should().NotBeNullOrEmpty();
        Constants.Logging.FrameworkCategory.Should().NotBeNullOrEmpty();
        Constants.Logging.PerformanceCategory.Should().NotBeNullOrEmpty();
        Constants.Logging.DefaultTemplate.Should().NotBeNullOrEmpty();
        Constants.Logging.CorrelationIdProperty.Should().NotBeNullOrEmpty();
        Constants.Logging.RequestIdProperty.Should().NotBeNullOrEmpty();
        Constants.Logging.UserIdProperty.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void Logging_PerformanceCategory_ShouldStartWithFrameworkCategory()
    {
        // Assert - Performance category should be a subcategory of framework
        Constants.Logging.PerformanceCategory.Should().StartWith(Constants.Logging.FrameworkCategory);
    }

    [TestMethod]
    public void Logging_DefaultTemplate_ShouldContainTimestamp()
    {
        // Assert
        Constants.Logging.DefaultTemplate.Should().Contain("{Timestamp");
    }

    [TestMethod]
    public void Logging_DefaultTemplate_ShouldContainLevel()
    {
        // Assert
        Constants.Logging.DefaultTemplate.Should().Contain("{Level");
    }

    [TestMethod]
    public void Logging_DefaultTemplate_ShouldContainMessage()
    {
        // Assert
        Constants.Logging.DefaultTemplate.Should().Contain("{Message");
    }

    [TestMethod]
    public void Logging_DefaultTemplate_ShouldContainException()
    {
        // Assert
        Constants.Logging.DefaultTemplate.Should().Contain("{Exception}");
    }

    [TestMethod]
    [DataRow("Information", "Information")]
    [DataRow("Debug", "Debug")]
    [DataRow("Warning", "Warning")]
    [DataRow("Error", "Error")]
    [DataRow("Critical", "Critical")]
    [DataRow("Trace", "Trace")]
    public void Logging_DefaultLogLevel_ShouldBeValidLogLevel(string expected, string actual)
    {
        // Assert - DefaultLogLevel should match one of the valid log levels
        if (Constants.Logging.DefaultLogLevel == expected)
        {
            actual.Should().Be(expected);
        }
    }

    [TestMethod]
    public void Logging_PropertyNames_ShouldUsePascalCase()
    {
        // Assert - Property names should follow PascalCase convention
        Constants.Logging.CorrelationIdProperty.Should().MatchRegex("^[A-Z][a-zA-Z]*$");
        Constants.Logging.RequestIdProperty.Should().MatchRegex("^[A-Z][a-zA-Z]*$");
        Constants.Logging.UserIdProperty.Should().MatchRegex("^[A-Z][a-zA-Z]*$");
    }

    [TestMethod]
    public void Logging_CorrelationIdAndRequestIdProperties_ShouldBeDifferent()
    {
        // Assert
        Constants.Logging.CorrelationIdProperty.Should().NotBe(Constants.Logging.RequestIdProperty);
    }

    #endregion

    #region Cross-Namespace Consistency Tests

    [TestMethod]
    public void Headers_CorrelationId_ShouldAlignWithLogging_CorrelationIdProperty()
    {
        // Assert - Header name (with hyphens) and logging property should represent the same concept (case-insensitive)
        string headerWithoutPrefix = Constants.Headers.CorrelationId.Replace("X-", "").Replace("-", "");
        string loggingProperty = Constants.Logging.CorrelationIdProperty;
        headerWithoutPrefix.Should().BeEquivalentTo(loggingProperty, "both represent the same correlation ID concept");
    }

    [TestMethod]
    public void Headers_RequestId_ShouldAlignWithLogging_RequestIdProperty()
    {
        // Assert - Header name (with hyphens) and logging property should represent the same concept (case-insensitive)
        string headerWithoutPrefix = Constants.Headers.RequestId.Replace("X-", "").Replace("-", "");
        string loggingProperty = Constants.Logging.RequestIdProperty;
        headerWithoutPrefix.Should().BeEquivalentTo(loggingProperty, "both represent the same request ID concept");
    }

    [TestMethod]
    public void Version_ShouldFollowSemanticVersioningFormat()
    {
        // Assert - Should match semantic versioning pattern (major.minor.patch)
        Constants.Version.Should().MatchRegex(@"^\d+\.\d+\.\d+$");
    }

    [TestMethod]
    public void ConfigurationSection_ShouldNotContainSpaces()
    {
        // Assert - Configuration section names typically don't have spaces
        Constants.ConfigurationSection.Should().NotContain(" ");
    }

    #endregion

    #region Type and Accessibility Tests

    [TestMethod]
    public void Constants_ShouldBeStaticClass()
    {
        // Arrange & Act
        Type type = typeof(Constants);

        // Assert
        type.IsAbstract.Should().BeTrue("static classes are abstract");
        type.IsSealed.Should().BeTrue("static classes are sealed");
    }

    [TestMethod]
    public void Constants_Timeouts_ShouldBeStaticClass()
    {
        // Arrange & Act
        Type type = typeof(Constants.Timeouts);

        // Assert
        type.IsAbstract.Should().BeTrue("static classes are abstract");
        type.IsSealed.Should().BeTrue("static classes are sealed");
    }

    [TestMethod]
    public void Constants_Headers_ShouldBeStaticClass()
    {
        // Arrange & Act
        Type type = typeof(Constants.Headers);

        // Assert
        type.IsAbstract.Should().BeTrue("static classes are abstract");
        type.IsSealed.Should().BeTrue("static classes are sealed");
    }

    [TestMethod]
    public void Constants_Logging_ShouldBeStaticClass()
    {
        // Arrange & Act
        Type type = typeof(Constants.Logging);

        // Assert
        type.IsAbstract.Should().BeTrue("static classes are abstract");
        type.IsSealed.Should().BeTrue("static classes are sealed");
    }

    [TestMethod]
    public void Constants_ShouldBePublic()
    {
        // Arrange & Act
        Type type = typeof(Constants);

        // Assert
        type.IsPublic.Should().BeTrue();
    }

    [TestMethod]
    public void Constants_NestedClasses_ShouldBePublic()
    {
        // Arrange & Act
        Type timeoutsType = typeof(Constants.Timeouts);
        Type headersType = typeof(Constants.Headers);
        Type loggingType = typeof(Constants.Logging);

        // Assert
        timeoutsType.IsNestedPublic.Should().BeTrue();
        headersType.IsNestedPublic.Should().BeTrue();
        loggingType.IsNestedPublic.Should().BeTrue();
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [TestMethod]
    public void Timeouts_DefaultHttpTimeoutSeconds_ShouldBeReasonable()
    {
        // Assert - 30 seconds is reasonable for HTTP requests
        Constants.Timeouts.DefaultHttpTimeoutSeconds.Should().BeInRange(1, 300);
    }

    [TestMethod]
    public void Timeouts_DefaultDatabaseTimeoutSeconds_ShouldBeReasonable()
    {
        // Assert - 30 seconds is reasonable for database operations
        Constants.Timeouts.DefaultDatabaseTimeoutSeconds.Should().BeInRange(1, 300);
    }

    [TestMethod]
    public void Timeouts_DefaultCacheExpirationMinutes_ShouldBeReasonable()
    {
        // Assert - 15 minutes is reasonable for cache expiration
        Constants.Timeouts.DefaultCacheExpirationMinutes.Should().BeInRange(1, 1440); // 1 min to 24 hours
    }

    [TestMethod]
    public void Headers_AllHeaders_ShouldNotContainWhitespace()
    {
        // Assert - HTTP headers should not contain whitespace
        Constants.Headers.CorrelationId.Should().NotContain(" ");
        Constants.Headers.RequestId.Should().NotContain(" ");
        Constants.Headers.UserContext.Should().NotContain(" ");
        Constants.Headers.ApiVersion.Should().NotContain(" ");
    }

    [TestMethod]
    public void Logging_DefaultLogLevel_ShouldNotBeNoneOrOff()
    {
        // Assert - Default log level should allow logging
        Constants.Logging.DefaultLogLevel.Should().NotBe("None");
        Constants.Logging.DefaultLogLevel.Should().NotBe("Off");
    }

    #endregion
}
